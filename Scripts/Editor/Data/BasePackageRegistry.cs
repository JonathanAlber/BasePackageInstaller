#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Base.PackageInstaller.Editor.Data
{
    /// <summary>
    /// Editor-only registry of base packages, persisted per project in
    /// <c>ProjectSettings/BasePackageRegistry.asset</c> so it can be version controlled.
    /// <para>
    /// Seeded with <see cref="BasePackageDefaults"/> on first creation; consumers can then
    /// add, remove or edit entries via Project Settings → "Base Packages".
    /// </para>
    /// </summary>
    [FilePath("ProjectSettings/BasePackageRegistry.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class BasePackageRegistry : ScriptableSingleton<BasePackageRegistry>
    {
        [SerializeField] private bool seeded;
        [SerializeField] private List<PackageEntry> packages = new();

        /// <summary>The registered packages in declaration order.</summary>
        public IReadOnlyList<PackageEntry> Packages
        {
            get
            {
                EnsureSeeded();
                return packages;
            }
        }

        /// <summary>The registered packages sorted alphabetically by name.</summary>
        public IReadOnlyList<PackageEntry> SortedPackages => Packages.OrderBy(entry => entry.Name).ToArray();

        /// <summary>Writes the registry back to disk after edits.</summary>
        public void Persist() => Save(true);

        private void EnsureSeeded()
        {
            if (seeded)
                return;

            if (packages.Count == 0)
                packages.AddRange(BasePackageDefaults.Create());

            seeded = true;
            Save(true);
        }
    }
}
#endif