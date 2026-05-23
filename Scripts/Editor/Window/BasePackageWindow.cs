#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base.PackageInstaller.Editor.Data;
using Base.PackageInstaller.Editor.Operations;
using UnityEditor;
using UnityEngine;

namespace Base.PackageInstaller.Editor.Window
{
    /// <summary>
    /// Shared editor window logic for installing and updating base packages.
    /// </summary>
    public abstract class BasePackageWindow : EditorWindow
    {
        private readonly bool[] _selected = Enumerable.Repeat(true, BasePackageRegistry.Packages.Length).ToArray();

        private string _status;
        private bool _hasFailures;
        private Vector2 _scroll;

        private PackageOperation _operation;

        /// <summary>The bold header shown above the help box.</summary>
        protected abstract string Title { get; }

        /// <summary>The help text shown below the header.</summary>
        protected abstract string Description { get; }

        /// <summary>The label of the main action button (e.g. "Install Selected").</summary>
        protected abstract string ActionLabel { get; }

        /// <summary>Present-continuous verb used in progress messages (e.g. "Installing").</summary>
        protected abstract string VerbContinuous { get; }

        /// <summary>Past-tense verb used in result messages (e.g. "Installed").</summary>
        protected abstract string VerbPast { get; }

        /// <summary>Phrase used when a package did not change (e.g. "already up to date").</summary>
        protected abstract string UnchangedPhrase { get; }

        /// <summary>The label of the button that opens the other window.</summary>
        protected abstract string OtherWindowLabel { get; }

        /// <summary>
        /// Creates the package operation backing this window.
        /// </summary>
        /// <returns>A new package operation instance.</returns>
        protected abstract PackageOperation CreateOperation();

        /// <summary>
        /// Opens the companion window (the updater opens the installer and vice versa).
        /// </summary>
        protected abstract void OpenOtherWindow();

        private void OnEnable()
        {
            _operation ??= CreateOperation();

            _operation.OnPackageStarted += HandlePackageStarted;
            _operation.OnPackageCompleted += HandlePackageCompleted;
            _operation.OnPackageFailed += HandlePackageFailed;
            _operation.OnAllPackagesCompleted += HandleAllPackagesCompleted;
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
            DrawNavigation();

            DrawPackagesSection();

            EditorGUILayout.Space(12);

            DrawExtraSections();

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);

            EditorGUILayout.Space(4);

            EditorGUILayout.HelpBox(Description, MessageType.Info);

            if (string.IsNullOrEmpty(_status))
                return;

            EditorGUILayout.Space(4);

            EditorGUILayout.HelpBox(_status, GetStatusMessageType());
        }

        /// <summary>
        /// Draws window-specific sections. Override to add extra controls.
        /// </summary>
        protected virtual void DrawExtraSections()
        {
        }

        private void DrawNavigation()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(OtherWindowLabel, GUILayout.Width(140)))
                OpenOtherWindow();

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

            EditorGUI.BeginDisabledGroup(_operation.IsRunning);

            if (GUILayout.Button(ActionLabel, GUILayout.Height(30)))
                StartOperation();

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

            for (int i = 0; i < BasePackageRegistry.Packages.Length; i++)
                if (_selected[i])
                    urls.Add(BasePackageRegistry.Packages[i].Url);

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
            _status = $"{VerbContinuous}: {label}...";
            Repaint();
        }

        private void HandlePackageCompleted(PackageResult result)
        {
            Debug.Log($"{GetType().Name}: {DescribeResult(result)}", null);
        }

        private void HandlePackageFailed(PackageResult result)
        {
            _hasFailures = true;

            Debug.LogWarning($"{GetType().Name}: {DescribeResult(result)}", null);
        }

        private void HandleAllPackagesCompleted(OperationSummary summary)
        {
            _hasFailures = summary.HasFailures;
            _status = BuildSummary(summary);

            if (summary.HasFailures)
                Debug.LogWarning($"{GetType().Name}: {_status}", null);
            else
                Debug.Log($"{GetType().Name}: {_status}", null);

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
                return $"{VerbPast} {resultName}.";

            if (!result.Changed)
                return $"{resultName} {UnchangedPhrase} ({result.Version}).";

            if (string.IsNullOrEmpty(result.PreviousVersion))
                return $"{VerbPast} {resultName} {result.Version}.";

            return $"{VerbPast} {resultName} {result.PreviousVersion} → {result.Version}.";
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