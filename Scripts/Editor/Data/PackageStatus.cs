#if UNITY_EDITOR
namespace Base.PackageInstaller.Editor.Data
{
    /// <summary>
    /// The current install status of a registry package in the project.
    /// </summary>
    public readonly struct PackageStatus
    {
        /// <summary>True if the package is currently installed.</summary>
        public readonly bool IsInstalled;

        /// <summary>The installed version, or empty if not installed.</summary>
        public readonly string Version;

        public PackageStatus(bool isInstalled, string version)
        {
            IsInstalled = isInstalled;
            Version = version;
        }
    }
}
#endif