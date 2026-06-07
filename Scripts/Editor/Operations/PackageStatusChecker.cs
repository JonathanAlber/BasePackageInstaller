#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Base.PackageInstaller.Editor.Data;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Base.PackageInstaller.Editor.Operations
{
    /// <summary>
    /// Queries the project's installed packages and maps them to registry entries by Git URL,
    /// so the window can show which packages are installed and at what version.
    /// </summary>
    public sealed class PackageStatusChecker
    {
        /// <summary>
        /// Invoked when a refresh finishes. The dictionary is keyed by normalized Git URL.
        /// </summary>
        public event Action<IReadOnlyDictionary<string, PackageStatus>> OnCompleted;

        /// <summary>True while a query is in progress.</summary>
        public bool IsRunning { get; private set; }

        private ListRequest _request;

        /// <summary>
        /// Normalizes a Git URL so registry entries and installed packages compare equal.
        /// </summary>
        /// <param name="url">The URL to normalize.</param>
        /// <returns>The trimmed URL without a trailing slash.</returns>
        public static string Normalize(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            return url.Trim().TrimEnd('/');
        }

        /// <summary>
        /// Starts an offline query of installed packages. Does nothing if one is already running.
        /// </summary>
        public void Refresh()
        {
            if (IsRunning)
                return;

            IsRunning = true;
            _request = Client.List(offlineMode: true, includeIndirectDependencies: false);

            EditorApplication.update += Poll;
        }

        private void Poll()
        {
            if (_request is not { IsCompleted: true })
                return;

            EditorApplication.update -= Poll;

            Dictionary<string, PackageStatus> statuses = new();

            if (_request.Status == StatusCode.Success && _request.Result != null)
                foreach (PackageInfo info in _request.Result)
                    AddGitPackage(statuses, info);

            _request = null;
            IsRunning = false;

            OnCompleted?.Invoke(statuses);
        }

        private static void AddGitPackage(IDictionary<string, PackageStatus> statuses, PackageInfo info)
        {
            if (info.source != PackageSource.Git || string.IsNullOrEmpty(info.packageId))
                return;

            // A git package id has the form "name@url"; everything after the first '@' is the source URL.
            int separator = info.packageId.IndexOf('@');

            if (separator < 0)
                return;

            string url = Normalize(info.packageId[(separator + 1)..]);

            if (url.Length == 0)
                return;

            statuses[url] = new PackageStatus(true, info.version);
        }
    }
}
#endif