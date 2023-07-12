using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace BoundedPaths.Editor.Settings
{
    [FilePath("ProjectSettings/" + nameof(BoundedPathSettings) + ".asset", FilePathAttribute.Location.PreferencesFolder)]
    public class BoundedPathSettings : SettingsSingleton<BoundedPathSettings>
    {
        private class BoundedPathSettingsProvider : CustomSettingsProvider
        {
            private BoundedPathSettingsProvider(string path, SettingsScope scopes) : base(path, scopes) {}

            protected override void OnSettingsScrollViewGUI()
            {
                DefaultPathMaterial = CustomEditorUtility.ObjectField(
                    $"Default {nameof(BoundedPath)} Material", DefaultPathMaterial);
                DefaultTransform = CustomEditorUtility.PresetField<Transform>(
                    $"Default {nameof(PathBounds)} {nameof(Transform)} Preset", DefaultTransform);
            }

            [SettingsProvider]
            public static SettingsProvider CreateBoundedPathSettings()
            {
                return new BoundedPathSettingsProvider($"{BoundedPathSettingsPath}/{GetNicePathname()} Settings", SettingsScope.Project);
            }
        }
    
        [SerializeField] private Material defaultPathMaterial;
        [SerializeField] private Preset defaultTransform;
    
        public static Material DefaultPathMaterial
        {
            get => instance.defaultPathMaterial;
            set
            {
                instance.defaultPathMaterial = value;
                instance.Save(true);
            }
        }
        public static Preset DefaultTransform
        {
            get => instance.defaultTransform;
            set
            {
                instance.defaultTransform = value;
                instance.Save(true);
            }
        }
        public static string BoundedPathSettingsPath => $"Project/{GetNicePathname()}";
        private static string GetNicePathname() => ObjectNames.NicifyVariableName(nameof(BoundedPath));
    }
}