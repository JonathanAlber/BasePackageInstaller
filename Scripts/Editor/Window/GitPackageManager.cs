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
    /// present to the latest remote version in a single action. Each package's current
    /// install status and version are shown in a table.
    /// </summary>
    public sealed class GitPackageManager : EditorWindow
    {
        private const string WindowTitle = "Git Package Manager";

        private const string Description = "Installs the selected git packages or updates them to the latest remote " +
                                           "version if they are already installed.";

        private const string InstallLabel = "Install Selected";
        private const string UpdateLabel = "Update Selected";
        private const string InstallOrUpdateLabel = "Install / Update Selected";
        private const string ProgressVerb = "Processing";
        private const string UnchangedPhrase = "is already up to date";
        private const string MissingVersion = "—";

        private const float ToggleWidth = 18f;
        private const float StatusWidth = 100f;
        private const float VersionWidth = 90f;

        private static readonly Color InstalledColor = new(0.40f, 0.78f, 0.40f);
        private static readonly Color NotInstalledColor = new(0.70f, 0.70f, 0.70f);

        private string _status;
        private bool _hasFailures;
        private Vector2 _scroll;

        private PackageEntry[] _packages;
        private bool[] _selected;

        private IReadOnlyDictionary<string, PackageStatus> _statuses = new Dictionary<string, PackageStatus>();
        private bool _statusChecked;

        private GUIStyle _installedStyle;
        private GUIStyle _notInstalledStyle;

        private PackageOperation _operation;
        private PackageStatusChecker _checker;

        [MenuItem("Tools/Git Package Manager", priority = -15)]
        public static void ShowWindow() => GetWindow<GitPackageManager>(WindowTitle);

        private void OnEnable()
        {
            RefreshPackages();

            _operation ??= new GitPackageOperation();
            _checker ??= new PackageStatusChecker();

            _operation.OnPackageStarted += HandlePackageStarted;
            _operation.OnPackageCompleted += HandlePackageCompleted;
            _operation.OnPackageFailed += HandlePackageFailed;
            _operation.OnAllPackagesCompleted += HandleAllPackagesCompleted;
            _checker.OnCompleted += HandleStatusesReady;

            // A package install can trigger a domain reload that re-creates this window and
            // its operation. Resume here so an interrupted run continues where it left off.
            _operation.Resume();

            RefreshStatuses();
        }

        private void OnDisable()
        {
            _operation.OnPackageStarted -= HandlePackageStarted;
            _operation.OnPackageCompleted -= HandlePackageCompleted;
            _operation.OnPackageFailed -= HandlePackageFailed;
            _operation.OnAllPackagesCompleted -= HandleAllPackagesCompleted;
            _checker.OnCompleted -= HandleStatusesReady;
        }

        private void OnFocus() => RefreshStatuses();

        private void OnGUI()
        {
            EnsureStyles();

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

        private static void DrawTableHeader()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(ToggleWidth));
            EditorGUILayout.LabelField("Package", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel, GUILayout.Width(StatusWidth));
            EditorGUILayout.LabelField("Version", EditorStyles.boldLabel, GUILayout.Width(VersionWidth));

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawProjectSetupSection()
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

        private void EnsureStyles()
        {
            if (_installedStyle != null)
                return;

            _installedStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = InstalledColor } };
            _notInstalledStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = NotInstalledColor } };
        }

        private void RefreshPackages()
        {
            _packages = new List<PackageEntry>(BasePackageRegistry.instance.SortedPackages).ToArray();
            _selected = new bool[_packages.Length];

            for (int i = 0; i < _selected.Length; i++)
                _selected[i] = true;
        }

        private void RefreshStatuses()
        {
            if (_checker == null || _checker.IsRunning)
                return;

            if (_operation is { IsRunning: true })
                return;

            _checker.Refresh();
        }

        private void DrawPackagesSection()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Git Packages", EditorStyles.boldLabel);

            if (GUILayout.Button("Refresh", GUILayout.Width(70)))
                RefreshStatuses();

            if (GUILayout.Button("Edit List", GUILayout.Width(80)))
                SettingsService.OpenProjectSettings(BasePackageSettingsProvider.Path);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            DrawTableHeader();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _packages.Length; i++)
                DrawPackageRow(i);

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

            if (GUILayout.Button(GetActionLabel(), GUILayout.Height(30)))
                StartOperation();

            EditorGUI.EndDisabledGroup();
        }

        private void DrawPackageRow(int index)
        {
            PackageStatus status = GetStatus(_packages[index]);

            EditorGUILayout.BeginHorizontal();

            _selected[index] = EditorGUILayout.Toggle(_selected[index], GUILayout.Width(ToggleWidth));

            EditorGUILayout.LabelField(_packages[index].Name);

            EditorGUILayout.LabelField(GetStatusText(status), GetStatusStyle(status), GUILayout.Width(StatusWidth));

            string version = status.IsInstalled ? status.Version : MissingVersion;

            EditorGUILayout.LabelField(version, GUILayout.Width(VersionWidth));

            EditorGUILayout.EndHorizontal();
        }

        private PackageStatus GetStatus(PackageEntry entry)
        {
            return _statuses.GetValueOrDefault(PackageStatusChecker.Normalize(entry.Url));
        }

        private string GetStatusText(PackageStatus status)
        {
            if (!_statusChecked)
                return "Checking…";

            return status.IsInstalled
                ? "Installed"
                : "Not installed";
        }

        private GUIStyle GetStatusStyle(PackageStatus status)
        {
            if (!_statusChecked)
                return EditorStyles.label;

            return status.IsInstalled
                ? _installedStyle
                : _notInstalledStyle;
        }

        private string GetActionLabel()
        {
            if (!_statusChecked)
                return InstallOrUpdateLabel;

            int installed = 0;
            int notInstalled = 0;

            for (int i = 0; i < _packages.Length; i++)
            {
                if (!_selected[i])
                    continue;

                if (GetStatus(_packages[i]).IsInstalled)
                    installed++;
                else
                    notInstalled++;
            }

            if (notInstalled == 0 && installed > 0)
                return UpdateLabel;

            if (installed == 0 && notInstalled > 0)
                return InstallLabel;

            return InstallOrUpdateLabel;
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

        private void HandleStatusesReady(IReadOnlyDictionary<string, PackageStatus> statuses)
        {
            _statuses = statuses;
            _statusChecked = true;

            Repaint();
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

            RefreshStatuses();

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