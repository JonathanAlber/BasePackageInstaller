#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Base.PackageInstaller.Editor
{
    /// <summary>
    /// A custom Unity Editor window for installing a set of predefined packages from Git URLs.
    /// /// </summary>
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
        private readonly Queue<string> _installQueue = new();

        private AddRequest _currentRequest;
        private Vector2 _scroll;
        private bool _installing;
        private string _status;

        [MenuItem("Tools/Base Package Installer")]
        public static void ShowWindow() => GetWindow<BasePackageInstallerWindow>("Base Package Installer");

        private void OnGUI()
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
                SetAll(true);

            if (GUILayout.Button("Deselect All"))
                SetAll(false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUI.BeginDisabledGroup(_installing);
            if (GUILayout.Button("Install Selected", GUILayout.Height(30)))
                StartInstall();
            EditorGUI.EndDisabledGroup();

            if (string.IsNullOrEmpty(_status))
                return;

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(_status, _installing ? MessageType.Info : MessageType.None);
        }

        private void SetAll(bool value)
        {
            for (int i = 0; i < _selected.Length; i++)
                _selected[i] = value;
        }

        private void StartInstall()
        {
            _installQueue.Clear();
            for (int i = 0; i < Packages.Length; i++)
                if (_selected[i])
                    _installQueue.Enqueue(Packages[i].Url);

            if (_installQueue.Count == 0)
                return;

            _installing = true;
            ProcessNext();
        }

        private void ProcessNext()
        {
            if (_installQueue.Count == 0)
            {
                _installing = false;
                _status = "All packages installed successfully.";
                Repaint();
                return;
            }

            string url = _installQueue.Dequeue();
            _status = $"Installing: {url.Split('/').Last()}...";
            _currentRequest = Client.Add(url);
            EditorApplication.update += OnProgress;
            Repaint();
        }

        private void OnProgress()
        {
            if (_currentRequest is not { IsCompleted: true })
                return;

            EditorApplication.update -= OnProgress;

            if (_currentRequest.Status == StatusCode.Failure)
            {
                _installing = false;
                _status = $"Failed: {_currentRequest.Error?.message}";
                Debug.LogError($"{nameof(BasePackageInstallerWindow)}: {_status}");
                Repaint();
                return;
            }

            Debug.Log($"{nameof(BasePackageInstallerWindow)}: Installed {_currentRequest.Result.name} successfully.");
            ProcessNext();
        }
    }
}
#endif