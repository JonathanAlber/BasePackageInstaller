#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Base.PackageInstaller.Editor.Data
{
    /// <summary>
    /// Represents a package entry with a friendly name and a Git URL.
    /// Serializable so it can be edited in the package registry inspector.
    /// </summary>
    [Serializable]
    public sealed class PackageEntry
    {
        [SerializeField] private string name;
        [SerializeField] private string url;

        /// <summary>The friendly name shown in the window.</summary>
        public string Name => name;

        /// <summary>The Git URL the package is added from.</summary>
        public string Url => url;

        public PackageEntry(string name, string url)
        {
            this.name = name;
            this.url = url;
        }
    }
}
#endif