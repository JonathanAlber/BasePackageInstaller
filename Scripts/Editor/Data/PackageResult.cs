#if UNITY_EDITOR
namespace Base.PackageInstaller.Editor.Data
{
    /// <summary>
    /// Outcome of a single package operation.
    /// </summary>
    public readonly struct PackageResult
    {
        /// <summary>The friendly label taken from the package URL.</summary>
        public readonly string Label;

        /// <summary>The resolved package name (falls back to the label if unknown).</summary>
        public readonly string Name;

        /// <summary>The resolved package version (empty if unknown).</summary>
        public readonly string Version;

        /// <summary>The version installed before the operation (empty if it was not installed before).</summary>
        public readonly string PreviousVersion;

        /// <summary>True if the installed content changed (a new install or a new version/commit).</summary>
        public readonly bool Changed;

        /// <summary>True if the operation succeeded.</summary>
        public readonly bool Success;

        /// <summary>The error message if the operation failed; otherwise null.</summary>
        public readonly string Error;

        public PackageResult(string label, string name, string version, string previousVersion,
            bool changed, bool success, string error)
        {
            Label = label;
            Name = name;
            Version = version;
            PreviousVersion = previousVersion;
            Changed = changed;
            Success = success;
            Error = error;
        }
    }
}
#endif