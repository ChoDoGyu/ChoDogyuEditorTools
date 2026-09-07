using System;
using System.Collections.Generic;
using CDG.EditorTools.Validation.Navigation;
using CDG.EditorTools.Validation.Rules;
using CDG.EditorTools.Validation.Scopes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CDG.EditorTools.Validation.UI
{
    /// <summary>
    /// General Editor Tools의 Validation 기능을 실행하고 검사 결과를 확인하는 Editor Window입니다.
    /// </summary>
    internal sealed class ValidationWindow : EditorWindow
    {
        private const string MenuPath = "Tools/ChoDogyu/General Editor Tools/Validation";
        private const string WindowTitle = "CDG Validation";

        [SerializeField]
        private ValidationScopeType _selectedScope = ValidationScopeType.Selection;

        [SerializeField]
        private bool _includeMissingScript = true;

        [SerializeField]
        private bool _includeBrokenSerializedReference = true;

        [SerializeField]
        private string _resultSearchText = string.Empty;

        [SerializeField]
        private bool _showMissingScriptResults = true;

        [SerializeField]
        private bool _showBrokenSerializedReferenceResults = true;

        private readonly List<ValidationIssue> _issues = new List<ValidationIssue>();

        private EnumField _scopeField;
        private Toggle _missingScriptToggle;
        private Toggle _brokenReferenceToggle;
        private Button _validateButton;
        private Label _statusLabel;
        private VisualElement _resultsContainer;
        private bool _hasExecutedValidation;
        private ValidationWindowState _state = ValidationWindowState.Ready;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            ValidationWindow window = GetWindow<ValidationWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(520f, 420f);
            window.Show();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            root.style.paddingLeft = 12f;
            root.style.paddingRight = 12f;
            root.style.paddingTop = 12f;
            root.style.paddingBottom = 12f;

            CreateHeader(root);
            CreateScopeSection(root);
            CreateRuleSection(root);
            CreateExecutionSection(root);
            CreateResultSection(root);

            SetState(ValidationWindowState.Ready);
        }

        private void CreateHeader(VisualElement root)
        {
            Label title = new Label("General Editor Tools - Validation");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 16f;
            title.style.marginBottom = 12f;

            Label description = new Label("Unity 프로젝트의 Missing Script와 Broken Serialized Reference를 검사합니다.");
            description.style.whiteSpace = WhiteSpace.Normal;
            description.style.marginBottom = 16f;

            root.Add(title);
            root.Add(description);
        }

        private void CreateScopeSection(VisualElement root)
        {
            Label sectionTitle = CreateSectionTitle("Scope");

            _scopeField = new EnumField("검사 범위", _selectedScope);
            _scopeField.RegisterValueChangedCallback(evt => _selectedScope = (ValidationScopeType)evt.newValue);

            root.Add(sectionTitle);
            root.Add(_scopeField);
        }

        private void CreateRuleSection(VisualElement root)
        {
            Label sectionTitle = CreateSectionTitle("Rules");

            _missingScriptToggle = new Toggle("Missing Script")
            {
                value = _includeMissingScript
            };

            _missingScriptToggle.RegisterValueChangedCallback(evt =>
            {
                _includeMissingScript = evt.newValue;
                UpdateValidateButtonState();
            });

            _brokenReferenceToggle = new Toggle("Broken Serialized Reference")
            {
                value = _includeBrokenSerializedReference
            };

            _brokenReferenceToggle.RegisterValueChangedCallback(evt =>
            {
                _includeBrokenSerializedReference = evt.newValue;
                UpdateValidateButtonState();
            });

            root.Add(sectionTitle);
            root.Add(_missingScriptToggle);
            root.Add(_brokenReferenceToggle);
        }

        private void CreateExecutionSection(VisualElement root)
        {
            VisualElement container = new VisualElement();
            container.style.marginTop = 16f;
            container.style.marginBottom = 16f;

            _validateButton = new Button(ExecuteValidation)
            {
                text = "Validate"
            };

            _validateButton.tooltip = "선택한 범위와 규칙으로 Validation을 실행합니다.";

            _statusLabel = new Label();
            _statusLabel.style.marginTop = 8f;
            _statusLabel.style.whiteSpace = WhiteSpace.Normal;

            container.Add(_validateButton);
            container.Add(_statusLabel);

            root.Add(container);
        }

        private void CreateResultSection(VisualElement root)
        {
            Label sectionTitle = CreateSectionTitle("Results");

            TextField searchField = new TextField("검색")
            {
                value = _resultSearchText
            };

            searchField.style.marginBottom = 6f;

            VisualElement filterRow = new VisualElement();
            filterRow.style.flexDirection = FlexDirection.Row;
            filterRow.style.marginBottom = 8f;

            Label filterLabel = new Label("결과 필터");
            filterLabel.style.width = 90f;

            Toggle missingScriptFilter = new Toggle("Missing Script")
            {
                value = _showMissingScriptResults
            };

            Toggle brokenReferenceFilter = new Toggle("Broken Serialized Reference")
            {
                value = _showBrokenSerializedReferenceResults
            };

            brokenReferenceFilter.style.marginLeft = 12f;

            filterRow.Add(filterLabel);
            filterRow.Add(missingScriptFilter);
            filterRow.Add(brokenReferenceFilter);

            _resultsContainer = new ScrollView();
            _resultsContainer.style.flexGrow = 1f;
            _resultsContainer.style.minHeight = 160f;

            searchField.RegisterValueChangedCallback(evt =>
            {
                _resultSearchText = evt.newValue;

                if (_hasExecutedValidation)
                {
                    ShowResults();
                }
            });

            missingScriptFilter.RegisterValueChangedCallback(evt =>
            {
                _showMissingScriptResults = evt.newValue;

                if (_hasExecutedValidation)
                {
                    ShowResults();
                }
            });

            brokenReferenceFilter.RegisterValueChangedCallback(evt =>
            {
                _showBrokenSerializedReferenceResults = evt.newValue;

                if (_hasExecutedValidation)
                {
                    ShowResults();
                }
            });

            if (_hasExecutedValidation)
            {
                ShowResults();
            }
            else
            {
                ShowInitialResultMessage();
            }

            root.Add(sectionTitle);
            root.Add(searchField);
            root.Add(filterRow);
            root.Add(_resultsContainer);
        }

        private void ExecuteValidation()
        {
            _issues.Clear();
            _hasExecutedValidation = false;
            SetState(ValidationWindowState.Validating);

            EditorValidationProgress progress = new EditorValidationProgress();

            try
            {
                IValidationScope scope = CreateScope(_selectedScope, progress);
                IReadOnlyList<IValidationRule> rules = CreateRules();

                scope.Validate(rules, _issues);

                _hasExecutedValidation = true;
                SetState(ValidationWindowState.Completed);
                ShowResults();
            }
            catch (OperationCanceledException)
            {
                _issues.Clear();
                SetState(ValidationWindowState.Canceled);
                ShowCanceledMessage();
            }
            catch (Exception exception)
            {
                _issues.Clear();
                SetState(ValidationWindowState.Failed, exception.Message);
                ShowErrorMessage(exception.Message);
            }
            finally
            {
                progress.Clear();
                UpdateValidateButtonState();
            }
        }

        private IReadOnlyList<IValidationRule> CreateRules()
        {
            List<IValidationRule> rules = new List<IValidationRule>();

            if (_includeMissingScript)
            {
                rules.Add(new MissingScriptRule());
            }

            if (_includeBrokenSerializedReference)
            {
                rules.Add(new BrokenSerializedReferenceRule());
            }

            return rules;
        }

        private static IValidationScope CreateScope(ValidationScopeType scopeType, IValidationProgress progress)
        {
            switch (scopeType)
            {
                case ValidationScopeType.Selection:
                    return new SelectionValidationScope();

                case ValidationScopeType.LoadedScenes:
                    return new LoadedScenesValidationScope();

                case ValidationScopeType.ProjectPrefabs:
                    return new ProjectPrefabsValidationScope(progress);

                case ValidationScopeType.ProjectScenes:
                    return new ProjectScenesValidationScope(progress);

                case ValidationScopeType.Project:
                    return new ProjectValidationScope(progress);

                default:
                    throw new ArgumentOutOfRangeException(nameof(scopeType), scopeType, null);
            }
        }

        private void SetState(ValidationWindowState state, string detail = null)
        {
            _state = state;

            switch (state)
            {
                case ValidationWindowState.Ready:
                    _statusLabel.text = "Ready";
                    break;

                case ValidationWindowState.Validating:
                    _statusLabel.text = "Validating...";
                    break;

                case ValidationWindowState.Completed:
                    _statusLabel.text = $"Completed - {_issues.Count} issue(s)";
                    break;

                case ValidationWindowState.Canceled:
                    _statusLabel.text = "Canceled";
                    break;

                case ValidationWindowState.Failed:
                    _statusLabel.text = string.IsNullOrEmpty(detail) ? "Failed" : $"Failed - {detail}";
                    break;
            }

            UpdateInputState();
        }

        private void UpdateInputState()
        {
            bool isValidating = _state == ValidationWindowState.Validating;

            _scopeField?.SetEnabled(!isValidating);
            _missingScriptToggle?.SetEnabled(!isValidating);
            _brokenReferenceToggle?.SetEnabled(!isValidating);

            UpdateValidateButtonState();
        }

        private void UpdateValidateButtonState()
        {
            if (_validateButton == null)
            {
                return;
            }

            bool hasSelectedRule = _includeMissingScript || _includeBrokenSerializedReference;
            bool canValidate = _state != ValidationWindowState.Validating && hasSelectedRule;

            _validateButton.SetEnabled(canValidate);
        }

        private void ShowInitialResultMessage()
        {
            _resultsContainer.Clear();

            Label label = new Label("검사를 실행하면 결과가 여기에 표시됩니다.");
            label.style.marginTop = 8f;

            _resultsContainer.Add(label);
        }

        private void ShowResults()
        {
            _resultsContainer.Clear();

            if (_issues.Count == 0)
            {
                Label emptyLabel = new Label("문제가 발견되지 않았습니다.");
                emptyLabel.style.marginTop = 8f;
                _resultsContainer.Add(emptyLabel);
                return;
            }

            int visibleCount = 0;

            for (int i = 0; i < _issues.Count; i++)
            {
                if (ValidationIssueFilter.Matches(_issues[i], _resultSearchText, _showMissingScriptResults, _showBrokenSerializedReferenceResults))
                {
                    visibleCount++;
                }
            }

            Label summaryLabel = new Label($"총 {_issues.Count}개 중 {visibleCount}개 표시");
            summaryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            summaryLabel.style.marginTop = 4f;
            summaryLabel.style.marginBottom = 10f;
            _resultsContainer.Add(summaryLabel);

            if (visibleCount == 0)
            {
                Label noMatchLabel = new Label("현재 검색/필터 조건에 맞는 문제가 없습니다.");
                noMatchLabel.style.marginTop = 4f;
                _resultsContainer.Add(noMatchLabel);
                return;
            }

            int displayIndex = 1;

            for (int i = 0; i < _issues.Count; i++)
            {
                ValidationIssue issue = _issues[i];

                if (!ValidationIssueFilter.Matches(issue, _resultSearchText, _showMissingScriptResults, _showBrokenSerializedReferenceResults))
                {
                    continue;
                }

                _resultsContainer.Add(CreateIssueElement(issue, displayIndex));
                displayIndex++;
            }
        }

        private VisualElement CreateIssueElement(ValidationIssue issue, int index)
        {
            VisualElement container = new VisualElement();
            container.style.marginBottom = 12f;
            container.style.paddingLeft = 8f;
            container.style.paddingRight = 8f;
            container.style.paddingTop = 8f;
            container.style.paddingBottom = 8f;

            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 6f;

            Label header = new Label($"{index}. {GetIssueTypeDisplayName(issue.Type)}");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.flexGrow = 1f;

            Button goToButton = new Button(() => NavigateToIssue(issue))
            {
                text = "Go To"
            };

            goToButton.tooltip = "문제가 발견된 Unity Object로 이동합니다.";
            goToButton.style.width = 70f;

            headerRow.Add(header);
            headerRow.Add(goToButton);

            container.Add(headerRow);
            container.Add(CreateResultField("Asset", GetDisplayValue(issue.Location.AssetPath)));
            container.Add(CreateResultField("Object", GetDisplayValue(issue.Location.ObjectPath)));
            container.Add(CreateResultField("Component", GetDisplayValue(issue.Location.ComponentName)));
            container.Add(CreateResultField("Property", GetDisplayValue(issue.Location.PropertyPath)));
            container.Add(CreateResultField("Message", GetDisplayValue(issue.Message)));

            return container;
        }

        private void NavigateToIssue(ValidationIssue issue)
        {
            try
            {
                bool success = ValidationResultNavigator.TryNavigate(issue);
                _statusLabel.text = success ? "Navigation completed." : "Navigation target could not be found.";
            }
            catch (Exception exception)
            {
                _statusLabel.text = $"Navigation failed - {exception.Message}";
            }
        }

        private static VisualElement CreateResultField(string name, string value)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 3f;

            Label nameLabel = new Label(name);
            nameLabel.style.width = 90f;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            Label valueLabel = new Label(value);
            valueLabel.style.flexGrow = 1f;
            valueLabel.style.whiteSpace = WhiteSpace.Normal;

            row.Add(nameLabel);
            row.Add(valueLabel);

            return row;
        }

        private static string GetIssueTypeDisplayName(ValidationIssueType issueType)
        {
            switch (issueType)
            {
                case ValidationIssueType.MissingScript:
                    return "Missing Script";

                case ValidationIssueType.BrokenSerializedReference:
                    return "Broken Serialized Reference";

                default:
                    return issueType.ToString();
            }
        }

        private static string GetDisplayValue(string value)
        {
            return string.IsNullOrEmpty(value) ? "-" : value;
        }

        private void ShowCanceledMessage()
        {
            _resultsContainer.Clear();

            Label label = new Label("검사가 취소되었습니다. 부분 검사 결과는 표시하지 않습니다.");
            label.style.marginTop = 8f;

            _resultsContainer.Add(label);
        }

        private void ShowErrorMessage(string message)
        {
            _resultsContainer.Clear();

            HelpBox helpBox = new HelpBox(message, HelpBoxMessageType.Error);
            helpBox.style.marginTop = 8f;

            _resultsContainer.Add(helpBox);
        }

        private static Label CreateSectionTitle(string text)
        {
            Label label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginTop = 8f;
            label.style.marginBottom = 6f;

            return label;
        }
    }
}