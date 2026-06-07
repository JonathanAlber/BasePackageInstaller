#if UNITY_EDITOR
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Base.PackageInstaller.Editor.Operations
{
    /// <summary>
    /// Adds packages as Git dependencies. <see cref="Client.Add(string)"/> installs a package
    /// if missing and re-resolves it to the latest remote version if already present, so this
    /// single operation covers both installing and updating.
    /// </summary>
    public sealed class GitPackageOperation : PackageOperation
    {
        /// <inheritdoc/>
        protected override Request CreateRequest(string url) => Client.Add(url);
    }
}
#endif