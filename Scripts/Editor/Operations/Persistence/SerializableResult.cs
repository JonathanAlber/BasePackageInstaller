using System;
using Base.PackageInstaller.Editor.Data;

namespace Base.PackageInstaller.Editor.Operations.Persistence
{
    /// <summary>
    /// Serializable mirror of <see cref="PackageResult"/>.
    /// </summary>
    [Serializable]
    public struct SerializableResult
    {
        public string label;
        public string name;
        public string version;
        public string previousVersion;
        public bool changed;
        public bool success;
        public string error;
    }
}