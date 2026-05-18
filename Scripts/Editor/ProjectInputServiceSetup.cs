#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Base.PackageInstaller.Editor
{
    /// <summary>
    /// Sets up the project's input service: configures the input action asset
    /// and writes <c>ProjectInputService.cs</c> into the project.
    /// </summary>
    public static class ProjectInputServiceSetup
    {
        private const string AssetsPrefix = "Assets/";

        private const string AssetFolder = "Assets/Input";
        private const string AssetName = "PlayerInputActions";

        private const string ServiceFolder = "Assets/Generated/Input";
        private const string ServiceFileName = "ProjectInputService.cs";

        private static readonly string ServicePath = $"{ServiceFolder}/{ServiceFileName}";
        private static readonly string AssetPath = $"{AssetFolder}/{AssetName}.inputactions";

        public static bool IsSetUp => File.Exists(AssetPath) && File.Exists(ServicePath);

        public static void Run()
        {
            EnsureFolder(AssetFolder);
            EnsureFolder(ServiceFolder);

            string assetNamespace = FolderToNamespace(AssetFolder);
            string serviceNamespace = FolderToNamespace(ServiceFolder);

            if (!InputActionAssetSetup.TryEnsureAssetAtPath(AssetPath, assetNamespace))
                return;

            if (!File.Exists(ServicePath))
                WriteServiceFile(serviceNamespace, assetNamespace);

            AssetDatabase.Refresh();
            Debug.Log("ProjectInputService setup complete.");
        }

        private static void WriteServiceFile(string serviceNamespace, string assetNamespace)
        {
            string code = ProjectInputServiceCodeTemplate.Render(serviceNamespace, assetNamespace);
            File.WriteAllText(ServicePath, code);
            AssetDatabase.ImportAsset(ServicePath);
        }

        private static string FolderToNamespace(string folder)
        {
            if (folder.StartsWith(AssetsPrefix))
                folder = folder[AssetsPrefix.Length..];

            string ns = folder.Replace('/', '.');

            return ns;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string[] parts = path.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";

                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }
        }
    }
}
#endif