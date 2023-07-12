using UnityEditor;
using Vector2 = UnityEngine.Vector2;

namespace BoundedPaths.Editor.Settings
{
    public abstract class SettingsSingleton<T> : ScriptableSingleton<T>  where T : ScriptableSingleton<T>
    {
        protected abstract class CustomSettingsProvider : SettingsProvider
        {
            private Vector2 scrollPosition;
            protected CustomSettingsProvider(string path, SettingsScope scopes) : base(path, scopes) {}

            public override void OnGUI(string searchContext)
            {
                scrollPosition = CustomEditorUtility.DrawSettingsScrollView(scrollPosition, OnSettingsScrollViewGUI);
            }

            /// <summary>
            /// Override this to draw a custom editor in a scroll view with settings menu styling.
            /// </summary>
            protected abstract void OnSettingsScrollViewGUI();
        }
    }
}