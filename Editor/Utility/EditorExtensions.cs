using System;
using UnityEditor;
using UnityEngine;

namespace BoundedPaths.Editor
{
    public static class EditorExtensions
    {
        /// <summary>
        /// Try to find a <see cref="Component"/> through a <see cref="SerializedProperty"/>. If none is found but one
        /// exists on the given <paramref name="parent"/>, it is assigned to the target object through a
        /// <see cref="SerializedProperty"/>. If no component exists on the <paramref name="parent"/>, create one
        /// and initialize it with the given <paramref name="initializer"/>. 
        /// </summary>
        /// <param name="propertyName">The name of the <see cref="SerializedProperty"/> used to find/assign the desired <see cref="Component"/>.</param>
        /// <param name="parent">The <see cref="GameObject"/> the desired component should be attached to.</param>
        /// <param name="initializer">Method used to initialize the desired <see cref="Component"/> after creating it.</param>
        /// <returns>An existing <see cref="Component"/> of type <typeparamref name="T"/>, or a new one, initialized with <paramref name="initializer"/>.</returns>
        public static T GetOrCreateUniqueComponent<T>(this UnityEditor.Editor editor, string propertyName, GameObject parent, Action<T> initializer) where T : Component
        {
            SerializedObject serializedObject = editor.serializedObject;
            SerializedProperty serializedProperty = serializedObject.FindProperty(propertyName);
            T output = (T) serializedProperty.objectReferenceValue;
            if (output != null)
                return output;
        
            bool createdComponent = TryAddComponent(out output);
            serializedProperty.objectReferenceValue = output;
            serializedObject.ApplyModifiedProperties();
            if (createdComponent)
            {
                initializer(output);
            }
        
            return output;
        
            bool TryAddComponent(out T component)
            {
                if (parent.GetComponent<T>() is { } existingComponent && existingComponent != null)
                {
                    component = existingComponent;
                    return false;
                }

                component = Undo.AddComponent<T>(parent);
                return true;
            }
        }

        /// <summary>
        /// Try to find a <see cref="Component"/> through a <see cref="SerializedProperty"/>. If none is found, create one
        /// and initialize it with the given <paramref name="initializer"/>, then assign it to the target object through a
        /// <see cref="SerializedProperty"/>. 
        /// </summary>
        /// <param name="propertyName">The name of the <see cref="SerializedProperty"/> used to find/assign the desired <see cref="Component"/>.</param>
        /// <param name="parent">The <see cref="GameObject"/> the desired component should be attached to.</param>
        /// <param name="initializer">Method used to initialize the desired <see cref="Component"/> after creating it.</param>
        /// <returns>An existing <see cref="Component"/> of type <typeparamref name="T"/>, or a new one, initialized with <paramref name="initializer"/>.</returns>
        public static T GetOrCreateComponent<T>(this UnityEditor.Editor editor, string propertyName, GameObject parent, Action<T> initializer) where T : Component
        {
            SerializedObject serializedObject = editor.serializedObject;
            SerializedProperty serializedProperty = serializedObject.FindProperty(propertyName);
            T output = (T) serializedProperty.objectReferenceValue;
            if (output != null)
                return output;
        
            output = Undo.AddComponent<T>(parent);
            serializedProperty.objectReferenceValue = output;
            serializedObject.ApplyModifiedProperties();
            initializer(output);
        
            return output;
        }

        /// <summary>
        /// Try to find a <see cref="Component"/> through a <see cref="SerializedProperty"/>. If none is found, create a
        /// <see cref="GameObject"/> with a <see cref="Component"/> of type <typeparamref name="T"/> on it, initialized
        /// with the given <paramref name="initializer"/> and whose parent <see cref="Transform"/> is <paramref name="parent"/>. 
        /// </summary>
        /// <param name="propertyName">The name of the <see cref="SerializedProperty"/> use to find/assign the desired <see cref="Component"/>.</param>
        /// <param name="parent">The parent <see cref="Transform"/> of the desired <see cref="Component"/>.</param>
        /// <param name="initializer">Method used to initialize the desired <see cref="Component"/>.</param>
        /// <returns>And existing <see cref="Component"/> of type <typeparamref name="T"/>, or a new one, initialized with <paramref name="initializer"/>.</returns>
        public static T GetOrCreateComponentInChild<T>(this UnityEditor.Editor editor, string propertyName, Transform parent, Action<T> initializer) where T : Component
        {
            SerializedObject serializedObject = editor.serializedObject;
            SerializedProperty serializedProperty = serializedObject.FindProperty(propertyName);
            T output = (T) serializedProperty.objectReferenceValue;
            if (output != null)
                return output;
        
            GameObject child = new();
            child.transform.SetParent(parent, false);
            output = child.AddComponent<T>();

            serializedProperty.objectReferenceValue = output;
            serializedObject.ApplyModifiedProperties();

            initializer(output);

            return output;
        }
    }
}