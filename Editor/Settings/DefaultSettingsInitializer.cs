using BoundedPaths.Editor.Settings;
using UnityEditor.Presets;
using UnityEngine;

namespace BoundedPaths.Editor
{
    internal class DefaultSettingsInitializer : ScriptableObject
    {
        [Header(nameof(BoundedPathSettings))]
        [SerializeField] private Material defaultPathBoundsMaterial;
        [SerializeField] private Preset defaultPathBoundsTransform;
        
        [Header(nameof(PathBoundsSettings))]
        [SerializeField] private Preset defaultPathBounds;

        private void Awake()
        {
            ResetDefaultSettings();
        }

        private void ResetDefaultSettings()
        {
            if (BoundedPathSettings.DefaultPathMaterial == null)
            {
                BoundedPathSettings.DefaultPathMaterial = defaultPathBoundsMaterial;
            }
            if (BoundedPathSettings.DefaultTransform == null)
            {
                BoundedPathSettings.DefaultTransform = defaultPathBoundsTransform;
            }
            if (PathBoundsSettings.DefaultBounds == null)
            {
                PathBoundsSettings.DefaultBounds = defaultPathBounds;
            }
        }
    }
}