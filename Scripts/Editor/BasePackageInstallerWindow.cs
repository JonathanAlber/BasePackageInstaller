#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Base.PackageInstaller.Editor
{
    /// <summary>
    /// Editor window for installing base packages and setting up project-side services.
    /// </summary>
    public class BasePackageInstallerWindow : EditorWindow
    {
        private static readonly PackageEntry[] Packages =
        {
            new("Tools", "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/Tools"),
            new("Attributes", "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/Attributes"),
            new("Systems", "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/Systems"),
            new("UI", "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/UI"),
            new("Utility", "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/Utility"),
            new("ScreenShake", "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/ScreenShake"),
        };

        private readonly bool[] _selected = Enumerable.Repeat(true, Packages.Length).ToArray();
        private readonly PackageInstaller _installer = new();

        private Vector2 _scroll;
        private string _status;

        [MenuItem("Tools/Base Package Installer")]
        public static void ShowWindow() => GetWindow<BasePackageInstallerWindow>("Base Package Installer");

        private void OnEnable()
        {
            _installer.OnPackageStarted += HandlePackageStarted;
            _installer.OnPackageInstalled += HandlePackageInstalled;
            _installer.OnPackageFailed += HandlePackageFailed;
            _installer.OnAllPackagesInstalled += HandleAllPackagesInstalled;
        }

        private void OnDisable()
        {
            _installer.OnPackageStarted -= HandlePackageStarted;
            _installer.OnPackageInstalled -= HandlePackageInstalled;
            _installer.OnPackageFailed -= HandlePackageFailed;
            _installer.OnAllPackagesInstalled -= HandleAllPackagesInstalled;
        }

        private void OnGUI()
        {
            DrawPackagesSection();

            EditorGUILayout.Space(12);

            DrawProjectSetupSection();

            if (string.IsNullOrEmpty(_status))
                return;

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(_status, _installer.IsInstalling ? MessageType.Info : MessageType.None);
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

        private static void HandlePackageInstalled(string packageName)
        {
            Debug.Log($"{nameof(BasePackageInstallerWindow)}: Installed {packageName} successfully.");
        }

        private void DrawPackagesSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Base Packages", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < Packages.Length; i++)
                _selected[i] = EditorGUILayout.ToggleLeft(Packages[i].Name, _selected[i]);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select All"))
                SetAllSelected(true);

            if (GUILayout.Button("Deselect All"))
                SetAllSelected(false);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUI.BeginDisabledGroup(_installer.IsInstalling);

            if (GUILayout.Button("Install Selected", GUILayout.Height(30)))
                StartInstall();

            EditorGUI.EndDisabledGroup();
        }

        private void SetAllSelected(bool value)
        {
            for (int i = 0; i < _selected.Length; i++)
                _selected[i] = value;
        }

        private void StartInstall()
        {
            List<string> urls = new();

            for (int i = 0; i < Packages.Length; i++)
            {
                if (_selected[i])
                    urls.Add(Packages[i].Url);
            }

            _installer.Install(urls);
        }

        private void HandlePackageStarted(string url)
        {
            _status = $"Installing: {url.Split('/').Last()}...";
            Repaint();
        }

        private void HandlePackageFailed(string error)
        {
            _status = $"Failed: {error}";
            Debug.LogError($"{nameof(BasePackageInstallerWindow)}: {_status}");
            Repaint();
        }

        private void HandleAllPackagesInstalled()
        {
            _status = "All packages installed successfully.";
            Repaint();
        }
    }
}
#endif