#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Base.PackageInstaller.Editor.Data;
using UnityEngine;

namespace Base.PackageInstaller.Editor.Operations.Persistence
{
    /// <summary>
    /// Serializable snapshot of a running package operation.
    /// Used to persist progress across editor domain reloads so a run can resume
    /// after a package installation triggers a recompile.
    /// </summary>
    /// <remarks>
    /// This type intentionally mirrors the immutable <see cref="Data"/> structs into
    /// serializable form so those structs stay decoupled from the persistence layer.
    /// </remarks>
    [Serializable]
    public sealed class PackageOperationState
    {
        [SerializeField] private string[] remainingUrls;
        [SerializeField] private SerializableResult[] results;
        [SerializeField] private SerializableSnapshotEntry[] snapshot;
        [SerializeField] private bool hasSnapshot;

        /// <summary>The package URLs that still need to be processed, head first.</summary>
        public string[] RemainingUrls => remainingUrls ?? Array.Empty<string>();

        /// <summary>True if the pre-operation installed snapshot has been captured.</summary>
        public bool HasSnapshot => hasSnapshot;

        /// <summary>
        /// Builds a serializable state from the live operation data.
        /// </summary>
        /// <param name="remaining">The not-yet-completed URLs, head first.</param>
        /// <param name="results">The results gathered so far.</param>
        /// <param name="snapshot">The pre-operation installed packages keyed by name.</param>
        /// <param name="hasSnapshot">Whether the snapshot has already been captured.</param>
        /// <returns>A state object ready to be serialized.</returns>
        public static PackageOperationState Create(IEnumerable<string> remaining, IReadOnlyList<PackageResult> results,
            IReadOnlyDictionary<string, InstalledPackage> snapshot,bool hasSnapshot)
        {
            return new PackageOperationState
            {
                remainingUrls = new List<string>(remaining).ToArray(),
                results = ToSerializable(results),
                snapshot = ToSerializable(snapshot),
                hasSnapshot = hasSnapshot
            };
        }

        /// <summary>
        /// Rebuilds the gathered results in their original order.
        /// </summary>
        /// <returns>The results restored from the serialized form.</returns>
        public List<PackageResult> GetResults()
        {
            List<PackageResult> restored = new();

            if (results == null)
                return restored;

            foreach (SerializableResult result in results)
                restored.Add(new PackageResult(result.label, result.name, result.version,
                    result.previousVersion, result.changed, result.success, result.error));

            return restored;
        }

        /// <summary>
        /// Rebuilds the pre-operation installed snapshot keyed by package name.
        /// </summary>
        /// <returns>The snapshot restored from the serialized form.</returns>
        public Dictionary<string, InstalledPackage> GetSnapshot()
        {
            Dictionary<string, InstalledPackage> restored = new();

            if (snapshot == null)
                return restored;

            foreach (SerializableSnapshotEntry entry in snapshot)
                restored[entry.name] = new InstalledPackage(entry.version, entry.hash);

            return restored;
        }

        private static SerializableResult[] ToSerializable(IReadOnlyList<PackageResult> source)
        {
            SerializableResult[] target = new SerializableResult[source.Count];

            for (int i = 0; i < source.Count; i++)
            {
                PackageResult result = source[i];

                target[i] = new SerializableResult
                {
                    label = result.Label,
                    name = result.Name,
                    version = result.Version,
                    previousVersion = result.PreviousVersion,
                    changed = result.Changed,
                    success = result.Success,
                    error = result.Error
                };
            }

            return target;
        }

        private static SerializableSnapshotEntry[] ToSerializable(IReadOnlyDictionary<string, InstalledPackage> source)
        {
            SerializableSnapshotEntry[] target = new SerializableSnapshotEntry[source.Count];

            int i = 0;

            foreach (KeyValuePair<string, InstalledPackage> pair in source)
            {
                target[i++] = new SerializableSnapshotEntry
                {
                    name = pair.Key,
                    version = pair.Value.Version,
                    hash = pair.Value.Hash
                };
            }

            return target;
        }
    }
}
#endif