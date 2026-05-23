#if UNITY_EDITOR
namespace Base.PackageInstaller.Editor.ProjectInput
{
    /// <summary>
    /// Template for the generated <c>ProjectInputService.cs</c> file.
    /// </summary>
    internal static class ProjectInputServiceCodeTemplate
    {
        private const string NamespaceToken = "__NAMESPACE__";
        private const string AssetNamespaceToken = "__ASSET_NAMESPACE__";

        private const string Template = @"using Base.SystemsCorePackage.Services;
using Input;
    
namespace __NAMESPACE__
{
    /// <summary>
    /// Project-specific input service. Owns the generated <see cref=""PlayerInputActions""/>
    /// instance and exposes it through <see cref=""Actions""/>.
    /// </summary>
    public class ProjectInputService : GameServiceBehaviour
    {
        /// <summary>
        /// The instance of the generated project specific input actions class.
        /// </summary>
        public PlayerInputActions Actions { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            Actions = new PlayerInputActions();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Actions?.Dispose();
        }
    }
}
";

        /// <summary>
        /// Renders the template, replacing tokens with the provided namespaces.
        /// </summary>
        /// <param name="serviceNamespace">The namespace for the generated service class.</param>
        /// <param name="assetNamespace">The namespace for the generated input actions class.</param>
        /// <returns>The rendered code.</returns>
        public static string Render(string serviceNamespace, string assetNamespace)
        {
            return Template
                .Replace(NamespaceToken, serviceNamespace)
                .Replace(AssetNamespaceToken, assetNamespace);
        }    }
}
#endif