#if UNITY_EDITOR
namespace Base.PackageInstaller.Editor.Data
{
    /// <summary>
    /// Represents a package entry with a name, package id and Git URL.
    /// </summary>
    public readonly struct PackageEntry
    {
        public readonly string Name;
        public readonly string Url;

        public PackageEntry(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}
#endif