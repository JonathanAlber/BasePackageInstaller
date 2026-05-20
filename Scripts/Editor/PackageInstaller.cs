#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Base.PackageInstaller.Editor
{
    /// <summary>
    /// Queues and installs UPM packages sequentially.
    /// </summary>
    public class PackageInstaller
    {
        /// <summary>
        /// Invoked when the installation of a package starts, with the package URL as an argument.
        /// </summary>
        public event Action<string> OnPackageStarted;

        /// <summary>
        /// Invoked when a package is successfully installed, with the package name as an argument.
        /// </summary>
        public event Action<string> OnPackageInstalled;

        /// <summary>
        /// Invoked when a package installation fails, with the error message as an argument.
        /// </summary>
        public event Action<string> OnPackageFailed;

        /// <summary>
        /// Invoked when all packages in the queue have been processed, regardless of success or failure.
        /// </summary>
        public event Action OnAllPackagesInstalled;

        /// <summary>
        /// Indicates whether the installer is currently processing a queue of packages.
        /// </summary>
        public bool IsInstalling { get; private set; }

        private readonly Queue<string> _queue = new();

        private AddRequest _currentRequest;

        public void Install(IEnumerable<string> packageUrls)
        {
            if (IsInstalling)
                return;

            _queue.Clear();

            foreach (string url in packageUrls)
                _queue.Enqueue(url);

            if (_queue.Count == 0)
                return;

            IsInstalling = true;
            ProcessNext();
        }

        private void ProcessNext()
        {
            if (_queue.Count == 0)
            {
                IsInstalling = false;
                OnAllPackagesInstalled?.Invoke();
                return;
            }

            string url = _queue.Dequeue();
            OnPackageStarted?.Invoke(url);

            _currentRequest = Client.Add(url);
            EditorApplication.update += OnProgress;
        }

        private void OnProgress()
        {
            if (_currentRequest is not { IsCompleted: true })
                return;

            EditorApplication.update -= OnProgress;

            if (_currentRequest.Status == StatusCode.Failure)
            {
                IsInstalling = false;
                OnPackageFailed?.Invoke(_currentRequest.Error?.message ?? "Unknown error");
                return;
            }

            OnPackageInstalled?.Invoke(_currentRequest.Result.name);
            ProcessNext();
        }
    }
}
#endif