using System.Linq;

#if UNITY_EDITOR
namespace Base.PackageInstaller.Editor.Data
{
    /// <summary>
    /// Shared registry of base packages.
    /// </summary>
    public static class BasePackageRegistry
    {
        public static readonly PackageEntry[] Packages =
        {
            new
            (
                "Tools",
                "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/Tools"
            ),
            new
            (
                "Attributes",
                "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/Attributes"
            ),
            new
            (
                "Systems",
                "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/Systems"
            ),
            new
            (
                "UI",
                "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/UI"
            ),
            new
            (
                "Utility",
                "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/Utility"
            ),
            new
            (
                "ScreenShake",
                "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/ScreenShake"
            ),
            new
            (
                "Save System",
                "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/SaveSystem"
            ),
            new
            (
                "Settings System",
                "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/Settings"
            ),
            new
            (
                "Localization",
                "https://github.com/JonathanAlber/BaseProjectPackages.git?path=BaseProject/Packages/Localization"
            ),
        };

        public static readonly PackageEntry[] SortedPackages = Packages.OrderBy(p => p.Name).ToArray();
    }
}
#endif