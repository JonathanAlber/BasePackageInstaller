using System;
using Base.PackageInstaller.Editor.Data;

namespace Base.PackageInstaller.Editor.Operations.Persistence
{
    /// <summary>
    /// Serializable mirror of an <see cref="InstalledPackage"/> together with its package name.
    /// </summary>
    [Serializable]
    public struct SerializableSnapshotEntry
    {
        public string name;
        public string version;
        public string hash;
    }
}