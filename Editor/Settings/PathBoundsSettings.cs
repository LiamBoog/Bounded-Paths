using UnityEditor;
using UnityEngine;
using UnityEditor.Presets;
using UnityEngine.Splines;

namespace BoundedPaths.Editor.Settings
{
    [FilePath("ProjectSettings/" + nameof(PathBoundsSettings) + ".asset", FilePathAttribute.Location.ProjectFolder)]
    internal class PathBoundsSettings : SettingsSingleton<PathBoundsSettings>
    {
        private class PathBoundsSettingsProvider : CustomSettingsProvider
        {
            private PathBoundsSettingsProvider(string path, SettingsScope scopes) : base(path, scopes) {}

            protected override void OnSettingsScrollViewGUI()
            {
                DefaultBounds = CustomEditorUtility.PresetField<SplineContainer>($"Default {nameof(PathBounds)} Preset", DefaultBounds);
            }

            [SettingsProvider]
            public static SettingsProvider CreatePathBoundsSettings()
            {
                string pathBoundsName = ObjectNames.NicifyVariableName(nameof(PathBounds));
                return new PathBoundsSettingsProvider($"{BoundedPathSettings.BoundedPathSettingsPath}/{pathBoundsName} Settings", SettingsScope.Project);
            }
        }
        
        [SerializeField] private Preset defaultBounds;

        public static Preset DefaultBounds
        {
            get => instance.defaultBounds;
            set
            {
                instance.defaultBounds = value;
                instance.Save(true);
            }
        }
    }
}