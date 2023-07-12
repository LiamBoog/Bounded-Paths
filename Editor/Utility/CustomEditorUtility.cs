using System;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BoundedPaths.Editor
{
    public static class CustomEditorUtility
    {
        /// <summary>
        /// Draw the given content in a scroll view with settings menu styling.
        /// </summary>
        /// <param name="scrollPosition">Scroll position for the scroll view.</param>
        /// <param name="content">Content to be drawn in the scroll view.</param>
        /// <returns>The new scroll position of the scroll view.</returns>
        public static Vector2 DrawSettingsScrollView(Vector2 scrollPosition, Action content)
        {
            GUIStyle scrollViewStyle = new() { padding = new RectOffset(10, 0, 10, 10) };
            EditorGUIUtility.labelWidth = 250f;
            Vector2 output = EditorGUILayout.BeginScrollView(scrollPosition, scrollViewStyle);
            content();
            EditorGUILayout.EndScrollView();

            return output;
        }

        /// <summary>
        /// Draw a drag-and-drop object which accepts objects of the given type.  
        /// </summary>
        /// <param name="label">Label for the object field.</param>
        /// <param name="initialValue">The initial value to display in the object field.</param>
        /// <typeparam name="T">The type of object that will be accepted by the object field.</typeparam>
        /// <returns>The input <see cref="Object"/> of type <typeparamref name="T"/>./></returns>
        public static T ObjectField<T>(string label, T initialValue) where T : Object
        {
            Object output = EditorGUILayout.ObjectField(label, initialValue, typeof(T), false);
            return output as T;
        }

        /// <summary>
        /// Draw a drag-and-drop object field which accepts <see cref="Preset"/>s for <see cref="Object"/>s of the specified type.
        /// </summary>
        /// <param name="label">Label for the object field.</param>
        /// <param name="initialValue">The initial value to display in the object field.</param>
        /// <typeparam name="T">The type of <see cref="Object"/> the accepted <see cref="Preset"/> applies to.</typeparam>
        /// <returns>The input <see cref="Preset"/> for <see cref="Object"/>s of type <typeparamref name="T"/>.</returns>
        public static Preset PresetField<T>(string label, Preset initialValue) where T : Object
        {
            Preset output = ObjectField(label, initialValue);
            if (output == null || output.GetPresetType().GetManagedTypeName() == typeof(T).FullName)
                return output;
        
            return initialValue;
        }
    }
}