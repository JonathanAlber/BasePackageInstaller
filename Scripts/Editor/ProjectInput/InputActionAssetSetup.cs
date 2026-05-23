#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.PackageInstaller.Editor.ProjectInput
{
    /// <summary>
    /// Finds, creates and configures the project's <see cref="InputActionAsset"/>.
    /// </summary>
    internal static class InputActionAssetSetup
    {
        private static readonly string[] SearchFolders = { "Assets" };

        public static bool TryEnsureAssetAtPath(string targetPath, string codeNamespace)
        {
            string assetPath = FindOrCreateAsset(targetPath);

            if (string.IsNullOrEmpty(assetPath))
                return false;

            ConfigureImporter(assetPath, codeNamespace);
            return true;
        }

        private static string FindOrCreateAsset(string targetPath)
        {
            string[] existing = AssetDatabase.FindAssets("t:InputActionAsset", SearchFolders);

            if (existing.Length == 0)
                return CreateNewAsset(targetPath);

            string existingPath = AssetDatabase.GUIDToAssetPath(existing[0]);

            if (existingPath == targetPath)
                return targetPath;

            DeleteGeneratedWrapper(existingPath);

            string moveError = AssetDatabase.MoveAsset(existingPath, targetPath);

            if (string.IsNullOrEmpty(moveError))
                return targetPath;

            Debug.LogError($"Could not move input asset to {targetPath}: {moveError}");
            return null;
        }

        private static void DeleteGeneratedWrapper(string assetPath)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            if (importer == null)
                return;

            SerializedObject so = new(importer);

            SerializedProperty generateProp = so.FindProperty("m_GenerateWrapperCode");
            SerializedProperty pathProp = so.FindProperty("m_WrapperCodePath");

            if (generateProp == null || !generateProp.boolValue)
                return;

            string wrapperPath = pathProp != null ? pathProp.stringValue : string.Empty;

            if (string.IsNullOrEmpty(wrapperPath))
                wrapperPath = Path.ChangeExtension(assetPath, ".cs");

            if (!File.Exists(wrapperPath))
                return;

            AssetDatabase.DeleteAsset(wrapperPath);
        }

        private static string CreateNewAsset(string targetPath)
        {
            string assetName = Path.GetFileNameWithoutExtension(targetPath);

            InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = assetName;
            asset.AddActionMap("Gameplay");

            File.WriteAllText(targetPath, asset.ToJson());
            Object.DestroyImmediate(asset);

            AssetDatabase.ImportAsset(targetPath);
            return targetPath;
        }

        private static void ConfigureImporter(string assetPath, string codeNamespace)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            if (importer == null)
                return;

            SerializedObject so = new(importer);

            SetBool(so, "m_GenerateWrapperCode", true);
            SetString(so, "m_WrapperCodePath", string.Empty);
            SetString(so, "m_WrapperClassName", string.Empty);
            SetString(so, "m_WrapperCodeNamespace", codeNamespace);

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static void SetBool(SerializedObject so, string propertyName, bool value)
        {
            SerializedProperty property = so.FindProperty(propertyName);

            if (property != null)
                property.boolValue = value;
        }

        private static void SetString(SerializedObject so, string propertyName, string value)
        {
            SerializedProperty property = so.FindProperty(propertyName);

            if (property != null)
                property.stringValue = value;
        }
    }
}
#endif