#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Base.PackageInstaller.Editor.Data;
using Base.PackageInstaller.Editor.Operations;
using Base.PackageInstaller.Editor.ProjectInput;
using Base.PackageInstaller.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace Base.PackageInstaller.Editor.Window
{
    /// <summary>
    /// Editor window for managing base packages. Adds the selected packages as Git
    /// dependencies, installing any that are missing and updating any that are already
    /// present to the latest remote version in a single action.
    /// </summary>
    public sealed class BasePackageManagerWindow : EditorWindow
    {
        private const string WindowTitle = "Base Package Manager";
        private const string Description = "Installs the selected base packages or updates them to the latest remote " +
                                           "version if they are already installed.";

        private const string ActionLabel = "Install / Update Selected";
        private const string ProgressVerb = "Processing";
        private const string UnchangedPhrase = "is already up to date";

        private string _status;
        private bool _hasFailures;
        private Vector2 _scroll;

        private PackageEntry[] _packages;
        private bool[] _selected;

        private PackageOperation _operation;

        [MenuItem("Tools/Base Package Installer", priority = -15)]
        public static void ShowWindow() => GetWindow<BasePackageManagerWindow>(WindowTitle);

        private void OnEnable()
        {
            RefreshPackages();

            _operation ??= new GitPackageOperation();

            _operation.OnPackageStarted += HandlePackageStarted;
            _operation.OnPackageCompleted += HandlePackageCompleted;
            _operation.OnPackageFailed += HandlePackageFailed;
            _operation.OnAllPackagesCompleted += HandleAllPackagesCompleted;

            // A package install can trigger a domain reload that re-creates this window and
            // its operation. Resume here so an interrupted run continues where it left off.
            _operation.Resume();
        }

        private void OnDisable()
        {
            _operation.OnPackageStarted -= HandlePackageStarted;
            _operation.OnPackageCompleted -= HandlePackageCompleted;
            _operation.OnPackageFailed -= HandlePackageFailed;
            _operation.OnAllPackagesCompleted -= HandleAllPackagesCompleted;
        }

        private void OnGUI()
        {
            DrawPackagesSection();

            EditorGUILayout.Space(12);

            DrawProjectSetupSection();

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField(WindowTitle, EditorStyles.boldLabel);

            EditorGUILayout.Space(4);

            EditorGUILayout.HelpBox(Description, MessageType.Info);

            if (string.IsNullOrEmpty(_status))
                return;

            EditorGUILayout.Space(4);

            EditorGUILayout.HelpBox(_status, GetStatusMessageType());
        }

        private void RefreshPackages()
        {
            _packages = new List<PackageEntry>(BasePackageRegistry.instance.SortedPackages).ToArray();
            _selected = new bool[_packages.Length];

            for (int i = 0; i < _selected.Length; i++)
                _selected[i] = true;
        }

        private void DrawPackagesSection()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Base Packages", EditorStyles.boldLabel);

            if (GUILayout.Button("Edit List", GUILayout.Width(80)))
                SettingsService.OpenProjectSettings(BasePackageSettingsProvider.Path);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _packages.Length; i++)
                _selected[i] = EditorGUILayout.ToggleLeft(_packages[i].Name, _selected[i]);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select All"))
                SetAllSelected(true);

            if (GUILayout.Button("Deselect All"))
                SetAllSelected(false);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUI.BeginDisabledGroup(_operation.IsRunning);

            if (GUILayout.Button(ActionLabel, GUILayout.Height(30)))
                StartOperation();

            EditorGUI.EndDisabledGroup();
        }

        private void DrawProjectSetupSection()
        {
            EditorGUILayout.LabelField("Project Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            bool alreadySetUp = ProjectInputServiceSetup.IsSetUp;

            string label = alreadySetUp
                ? "ProjectInputService — already set up"
                : "Create ProjectInputService";

            EditorGUI.BeginDisabledGroup(alreadySetUp);

            if (GUILayout.Button(label, GUILayout.Height(30)))
                ProjectInputServiceSetup.Run();

            EditorGUI.EndDisabledGroup();
        }

        private void SetAllSelected(bool value)
        {
            for (int i = 0; i < _selected.Length; i++)
                _selected[i] = value;
        }

        private void StartOperation()
        {
            List<string> urls = new();

            for (int i = 0; i < _packages.Length; i++)
                if (_selected[i])
                    urls.Add(_packages[i].Url);

            _status = null;
            _hasFailures = false;

            _operation.Run(urls);
        }

        private MessageType GetStatusMessageType()
        {
            if (_operation.IsRunning)
                return MessageType.Info;

            return _hasFailures
                ? MessageType.Warning
                : MessageType.None;
        }

        private void HandlePackageStarted(string label)
        {
            _status = $"{ProgressVerb}: {label}...";
            Repaint();
        }

        private void HandlePackageCompleted(PackageResult result)
        {
            Debug.Log($"{WindowTitle}: {DescribeResult(result)}", null);
        }

        private void HandlePackageFailed(PackageResult result)
        {
            _hasFailures = true;

            Debug.LogWarning($"{WindowTitle}: {DescribeResult(result)}", null);
        }

        private void HandleAllPackagesCompleted(OperationSummary summary)
        {
            _hasFailures = summary.HasFailures;
            _status = BuildSummary(summary);

            if (summary.HasFailures)
                Debug.LogWarning($"{WindowTitle}: {_status}", null);
            else
                Debug.Log($"{WindowTitle}: {_status}", null);

            Repaint();
        }

        private string DescribeResult(PackageResult result)
        {
            if (!result.Success)
                return $"{result.Label} failed: {result.Error}";

            string resultName = string.IsNullOrEmpty(result.Name)
                ? result.Label
                : result.Name;

            if (string.IsNullOrEmpty(result.Version))
                return $"Installed {resultName}.";

            if (!result.Changed || result.PreviousVersion == result.Version)
                return $"{resultName} {UnchangedPhrase} ({result.Version}).";

            if (string.IsNullOrEmpty(result.PreviousVersion))
                return $"Installed {resultName} {result.Version}.";

            return $"Updated {resultName} {result.PreviousVersion} → {result.Version}.";
        }

        private string BuildSummary(OperationSummary summary)
        {
            StringBuilder builder = new();

            builder.Append($"Done. {summary.SuccessCount} ok");

            if (summary.ChangedCount > 0)
                builder.Append($", {summary.ChangedCount} changed");

            if (summary.UnchangedCount > 0)
                builder.Append($", {summary.UnchangedCount} unchanged");

            if (summary.FailedCount > 0)
                builder.Append($", {summary.FailedCount} failed");

            builder.Append('.');

            foreach (PackageResult result in summary.Results)
            {
                builder.Append('\n');
                builder.Append(DescribeResult(result));
            }

            return builder.ToString();
        }
    }
}
#endif