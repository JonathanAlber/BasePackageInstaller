#if UNITY_EDITOR
namespace Base.PackageInstaller.Editor.Data
{
    /// <summary>
    /// The default base packages seeded into a fresh <see cref="BasePackageRegistry"/>.
    /// Other projects can edit the registry afterward; these are only the starting set.
    /// </summary>
    internal static class BasePackageDefaults
    {
        private const string BaseUrl =
            "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/";

        /// <summary>
        /// Creates a fresh copy of the default entries.
        /// </summary>
        /// <returns>The default package entries.</returns>
        public static PackageEntry[] Create() => new[]
        {
            new PackageEntry("Tools", $"{BaseUrl}Tools"),
            new PackageEntry("Attributes", $"{BaseUrl}Attributes"),
            new PackageEntry("Systems", $"{BaseUrl}Systems"),
            new PackageEntry("UI", $"{BaseUrl}UI"),
            new PackageEntry("Utility", $"{BaseUrl}Utility"),
            new PackageEntry("ScreenShake", $"{BaseUrl}ScreenShake"),
            new PackageEntry("Save System", $"{BaseUrl}SaveSystem"),
            new PackageEntry("Settings System", $"{BaseUrl}Settings"),
            new PackageEntry("Localization", $"{BaseUrl}Localization"),
            new PackageEntry("Memory Profiler", $"{BaseUrl}MemoryProfiler")
        };
    }
}
#endif