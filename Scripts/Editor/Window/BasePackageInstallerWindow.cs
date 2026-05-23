#if UNITY_EDITOR
using Base.PackageInstaller.Editor.Operations;
using Base.PackageInstaller.Editor.ProjectInput;
using UnityEditor;
using UnityEngine;

namespace Base.PackageInstaller.Editor.Window
{
    /// <summary>
    /// Editor window for installing base packages.
    /// </summary>
    public sealed class BasePackageInstallerWindow : BasePackageWindow
    {
        /// <inheritdoc/>
        protected override string Title => "Base Package Installer";

        /// <inheritdoc/>
        protected override string Description =>
            "Adds the selected base packages as Git dependencies.";

        /// <inheritdoc/>
        protected override string ActionLabel => "Install Selected";

        /// <inheritdoc/>
        protected override string VerbContinuous => "Installing";

        /// <inheritdoc/>
        protected override string VerbPast => "Installed";

        /// <inheritdoc/>
        protected override string UnchangedPhrase => "already installed";

        /// <inheritdoc/>
        protected override string OtherWindowLabel => "Open Updater";

        [MenuItem("Tools/Base Package Installer/Installer")]
        public static void ShowWindow() => GetWindow<BasePackageInstallerWindow>("Base Package Installer");

        /// <inheritdoc/>
        protected override PackageOperation CreateOperation() => new Operations.PackageInstaller();

        /// <inheritdoc/>
        protected override void OpenOtherWindow() => BasePackageUpdaterWindow.ShowWindow();

        /// <inheritdoc/>
        protected override void DrawExtraSections()
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
    }
}
#endif