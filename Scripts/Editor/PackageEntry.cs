#if UNITY_EDITOR
namespace Base.PackageInstaller.Editor
{
    /// <summary>
    /// Represents a package entry with a name and a Git URL for installation.
    /// </summary>
    public readonly struct PackageEntry
    {
        public readonly string Name;
        public readonly string Url;

        public PackageEntry(string name, string url) { Name = name; Url = url; }
    }
}
#endif