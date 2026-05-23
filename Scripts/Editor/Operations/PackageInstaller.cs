#if UNITY_EDITOR
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Base.PackageInstaller.Editor.Operations
{
    /// <summary>
    /// Installs packages sequentially by adding Git dependencies.
    /// </summary>
    public sealed class PackageInstaller : PackageOperation
    {
        /// <inheritdoc/>
        protected override Request CreateRequest(string url) => Client.Add(url);
    }
}
#endif