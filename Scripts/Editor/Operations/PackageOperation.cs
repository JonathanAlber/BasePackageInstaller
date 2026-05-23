#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Base.PackageInstaller.Editor.Data;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Base.PackageInstaller.Editor.Operations
{
    /// <summary>
    /// Base class for sequential package operations.
    /// </summary>
    public abstract class PackageOperation
    {
        /// <summary>
        /// Invoked when a package operation starts.
        /// The string parameter is the friendly label of the package being processed.
        /// </summary>
        public event Action<string> OnPackageStarted;

        /// <summary>
        /// Invoked when a package operation completes successfully.
        /// The parameter describes the resolved package and version.
        /// </summary>
        public event Action<PackageResult> OnPackageCompleted;

        /// <summary>
        /// Invoked when a package operation fails.
        /// The run continues with the next package after this is raised.
        /// </summary>
        public event Action<PackageResult> OnPackageFailed;

        /// <summary>
        /// Invoked when all package operations have finished.
        /// The summary reports how many packages succeeded, changed, or failed.
        /// </summary>
        public event Action<OperationSummary> OnAllPackagesCompleted;

        /// <summary>
        /// Indicates whether a package operation is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        private readonly Queue<string> _queue = new();
        private readonly List<PackageResult> _results = new();
        private readonly Dictionary<string, InstalledPackage> _installed = new();

        private Request _currentRequest;
        private string _currentLabel;
        private ListRequest _listRequest;

        /// <summary>
        /// Starts processing the given package URLs sequentially.
        /// If an operation is already running, this method does nothing.
        /// </summary>
        /// <param name="packageUrls">The URLs of the packages to process.</param>
        public void Run(IEnumerable<string> packageUrls)
        {
            if (IsRunning)
                return;

            _queue.Clear();
            _results.Clear();
            _installed.Clear();

            foreach (string url in packageUrls)
                _queue.Enqueue(url);

            if (_queue.Count == 0)
                return;

            IsRunning = true;

            // Snapshot the currently installed packages first so we can report
            // version changes and detect packages that are already up to date.
            _listRequest = Client.List(offlineMode: true, includeIndirectDependencies: false);

            EditorApplication.update += OnSnapshotProgress;
        }

        /// <summary>
        /// Creates a package manager request for the given URL.
        /// </summary>
        /// <param name="url">The URL of the package to process.</param>
        /// <returns>A request object representing the package operation.</returns>
        protected abstract Request CreateRequest(string url);

        private void OnSnapshotProgress()
        {
            if (_listRequest is not { IsCompleted: true })
                return;

            EditorApplication.update -= OnSnapshotProgress;

            if (_listRequest.Status == StatusCode.Success && _listRequest.Result != null)
                foreach (PackageInfo info in _listRequest.Result)
                    _installed[info.name] = new InstalledPackage(info.version, info.git?.hash);

            _listRequest = null;

            ProcessNext();
        }

        private void ProcessNext()
        {
            if (_queue.Count == 0)
            {
                Finish();
                return;
            }

            string url = _queue.Dequeue();

            _currentLabel = GetLabel(url);

            OnPackageStarted?.Invoke(_currentLabel);

            _currentRequest = CreateRequest(url);

            EditorApplication.update += OnProgress;
        }

        private void OnProgress()
        {
            if (_currentRequest is not { IsCompleted: true })
                return;

            EditorApplication.update -= OnProgress;

            PackageResult result;

            try
            {
                result = BuildResult();
            }
            catch (Exception exception)
            {
                result = Failure(exception.Message);
            }

            _results.Add(result);

            if (result.Success)
                OnPackageCompleted?.Invoke(result);
            else
                OnPackageFailed?.Invoke(result);

            ProcessNext();
        }

        private PackageResult BuildResult()
        {
            if (_currentRequest.Status == StatusCode.Failure)
                return Failure(_currentRequest.Error?.message ?? "Unknown error");

            if (_currentRequest is not AddRequest { Result: { } info })
                return new PackageResult(_currentLabel, _currentLabel, string.Empty,
                    string.Empty, true, true, null);

            _installed.TryGetValue(info.name, out InstalledPackage previous);

            bool wasInstalled = previous.Version != null || previous.Hash != null;
            bool changed = !wasInstalled || HasChanged(previous, info);

            return new PackageResult(_currentLabel, info.name, info.version, previous.Version ?? string.Empty,
                changed, true, null);
        }

        private PackageResult Failure(string error)
        {
            return new PackageResult(_currentLabel, _currentLabel, string.Empty,
                string.Empty, false, false, error);
        }

        private void Finish()
        {
            EditorApplication.update -= OnProgress;

            _currentRequest = null;

            int success = 0;
            int failed = 0;
            int changed = 0;
            int unchanged = 0;

            foreach (PackageResult result in _results)
            {
                if (!result.Success)
                {
                    failed++;
                    continue;
                }

                success++;

                if (result.Changed)
                    changed++;
                else
                    unchanged++;
            }

            OperationSummary summary = new(_results.ToArray(), success, failed, changed, unchanged);

            IsRunning = false;

            OnAllPackagesCompleted?.Invoke(summary);
        }

        private static bool HasChanged(InstalledPackage previous, PackageInfo info)
        {
            if (!string.IsNullOrEmpty(previous.Hash) && info.git != null)
                return previous.Hash != info.git.hash;

            return previous.Version != info.version;
        }

        private static string GetLabel(string url) => url.Split('/').Last();
    }
}
#endif