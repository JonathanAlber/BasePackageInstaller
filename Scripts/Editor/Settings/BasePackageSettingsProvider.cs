#if UNITY_EDITOR
using System.Collections.Generic;
using Base.PackageInstaller.Editor.Data;
using UnityEditor;

namespace Base.PackageInstaller.Editor.Settings
{
    /// <summary>
    /// Exposes the <see cref="BasePackageRegistry"/> under Project Settings → "Base Packages"
    /// so packages can be added, removed or edited per project.
    /// </summary>
    internal static class BasePackageSettingsProvider
    {
        private const string SettingsPath = "Project/Custom Tools/Git Packages";
        private const string PackagesProperty = "packages";

        /// <summary>
        /// The settings path used to open this page programmatically.
        /// </summary>
        public static string Path => SettingsPath;

        [SettingsProvider]
        private static SettingsProvider Create()
        {
            SerializedObject serializedObject = new(BasePackageRegistry.instance);
            SerializedProperty packagesProperty = serializedObject.FindProperty(PackagesProperty);

            return new SettingsProvider(SettingsPath, SettingsScope.Project)
            {
                label = "Git Packages",
                keywords = new HashSet<string> { "package", "git", "installer", "updater", "base" },
                guiHandler = _ => DrawGui(serializedObject, packagesProperty)
            };
        }

        private static void DrawGui(SerializedObject serializedObject, SerializedProperty packagesProperty)
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Packages available in the Base Package Manager window. " +
                "Name is the label shown; URL is the Git dependency to add.",
                MessageType.Info);

            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(packagesProperty, true);

            if (!EditorGUI.EndChangeCheck())
                return;

            serializedObject.ApplyModifiedProperties();
            BasePackageRegistry.instance.Persist();
        }
    }
}
#endif