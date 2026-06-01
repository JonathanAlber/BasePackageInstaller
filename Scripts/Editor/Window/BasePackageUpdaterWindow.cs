#if UNITY_EDITOR
using Base.PackageInstaller.Editor.Operations;
using UnityEditor;

namespace Base.PackageInstaller.Editor.Window
{
    /// <summary>
    /// Editor window for updating installed base packages.
    /// </summary>
    public sealed class BasePackageUpdaterWindow : BasePackageWindow
    {
        /// <inheritdoc/>
        protected override string Title => "Base Package Updater";

        /// <inheritdoc/>
        protected override string Description =>
            "Re-imports the selected Git packages to fetch the latest remote versions.";

        /// <inheritdoc/>
        protected override string ActionLabel => "Update Selected";

        /// <inheritdoc/>
        protected override string VerbContinuous => "Updating";

        /// <inheritdoc/>
        protected override string VerbPast => "Updated";

        /// <inheritdoc/>
        protected override string UnchangedPhrase => "is already up to date";

        /// <inheritdoc/>
        protected override string OtherWindowLabel => "Open Installer";

        [MenuItem("Tools/Base Packages/Updater", priority = 3)]
        public static void ShowWindow() => GetWindow<BasePackageUpdaterWindow>("Base Package Updater");

        /// <inheritdoc/>
        protected override PackageOperation CreateOperation() => new PackageUpdater();

        /// <inheritdoc/>
        protected override void OpenOtherWindow() => BasePackageInstallerWindow.ShowWindow();
    }
}
#endif