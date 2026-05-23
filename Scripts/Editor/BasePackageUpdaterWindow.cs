#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Base.PackageInstaller.Editor
{
    /// <summary>
    /// Editor window for updating installed base packages.
    /// </summary>
    public class BasePackageUpdaterWindow : EditorWindow
    {
        private readonly bool[] _selected = Enumerable.Repeat(true, BasePackageRegistry.Packages.Length).ToArray();

        private string _status;
        private Vector2 _scroll;
        private PackageUpdater _updater;

        [MenuItem("Tools/Base Package Installer/Updater")]
        public static void ShowWindow() => GetWindow<BasePackageUpdaterWindow>("Base Package Updater");

        private void OnEnable()
        {
            _updater ??= new PackageUpdater();

            _updater.OnPackageStarted += HandlePackageStarted;
            _updater.OnPackageCompleted += HandlePackageUpdated;
            _updater.OnPackageFailed += HandlePackageFailed;
            _updater.OnAllPackagesCompleted += HandleAllPackagesUpdated;
        }

        private void OnDisable()
        {
            _updater.OnPackageStarted -= HandlePackageStarted;
            _updater.OnPackageCompleted -= HandlePackageUpdated;
            _updater.OnPackageFailed -= HandlePackageFailed;
            _updater.OnAllPackagesCompleted -= HandleAllPackagesUpdated;
        }

        private void OnGUI()
        {
            DrawNavigation();

            DrawPackagesSection();

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Base Package Updater", EditorStyles.boldLabel);

            EditorGUILayout.Space(4);

            EditorGUILayout.HelpBox(
                "Re-imports all registered Git packages to fetch the latest remote versions.",
                MessageType.Info);

            EditorGUILayout.Space(8);

            if (string.IsNullOrEmpty(_status))
                return;

            EditorGUILayout.Space(4);

            EditorGUILayout.HelpBox(_status, _updater.IsRunning
                ? MessageType.Info
                : MessageType.None);
        }

        private static void DrawNavigation()
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Open Installer", GUILayout.Width(140)))
                BasePackageInstallerWindow.ShowWindow();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
        }

        private void DrawPackagesSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Base Packages", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < BasePackageRegistry.Packages.Length; i++)
                _selected[i] = EditorGUILayout.ToggleLeft(BasePackageRegistry.Packages[i].Name, _selected[i]);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select All"))
                SetAllSelected(true);

            if (GUILayout.Button("Deselect All"))
                SetAllSelected(false);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUI.BeginDisabledGroup(_updater.IsRunning);

            if (GUILayout.Button("Update Selected", GUILayout.Height(30)))
                StartUpdate();

            EditorGUI.EndDisabledGroup();
        }

        private void SetAllSelected(bool value)
        {
            for (int i = 0; i < _selected.Length; i++)
                _selected[i] = value;
        }

        private void HandlePackageStarted(string url)
        {
            _status = $"Updating: {url.Split('/').Last()}...";
            Repaint();
        }

        private static void HandlePackageUpdated(string packageName)
        {
            Debug.Log($"{nameof(BasePackageUpdaterWindow)}: Updated {packageName} successfully.", null);
        }

        private void HandlePackageFailed(string error)
        {
            _status = $"Failed: {error}";

            Debug.LogError($"{nameof(BasePackageUpdaterWindow)}: {_status}", null);

            Repaint();
        }

        private void HandleAllPackagesUpdated()
        {
            _status = "All packages updated successfully.";
            Repaint();
        }

        private void StartUpdate()
        {
            List<string> urls = new();

            for (int i = 0; i < BasePackageRegistry.Packages.Length; i++)
                if (_selected[i])
                    urls.Add(BasePackageRegistry.Packages[i].Url);

            _updater.Run(urls);
        }
    }
}
#endif