namespace Base.PackageInstaller.Editor.Data
{
    /// <summary>
    /// Represents a package installed in the project, with its version and hash.
    /// </summary>
    public readonly struct InstalledPackage
    {
        public readonly string Version;
        public readonly string Hash;

        public InstalledPackage(string version, string hash)
        {
            Version = version;
            Hash = hash;
        }
    }
}