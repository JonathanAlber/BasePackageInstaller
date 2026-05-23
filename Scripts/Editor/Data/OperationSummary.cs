#if UNITY_EDITOR
using System.Collections.Generic;

namespace Base.PackageInstaller.Editor.Data
{
    /// <summary>
    /// Summary of a completed run over multiple packages.
    /// </summary>
    public readonly struct OperationSummary
    {
        /// <summary>The result of every package that was processed.</summary>
        public readonly IReadOnlyList<PackageResult> Results;

        /// <summary>The number of packages that succeeded.</summary>
        public readonly int SuccessCount;

        /// <summary>The number of packages that failed.</summary>
        public readonly int FailedCount;

        /// <summary>The number of packages whose installed content changed.</summary>
        public readonly int ChangedCount;

        /// <summary>The number of packages that were already up to date.</summary>
        public readonly int UnchangedCount;

        public OperationSummary(IReadOnlyList<PackageResult> results, int successCount,
            int failedCount, int changedCount, int unchangedCount)
        {
            Results = results;
            SuccessCount = successCount;
            FailedCount = failedCount;
            ChangedCount = changedCount;
            UnchangedCount = unchangedCount;
        }

        /// <summary>True if at least one package failed.</summary>
        public bool HasFailures => FailedCount > 0;
    }
}
#endif