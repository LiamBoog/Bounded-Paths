using System;
using System.Collections;
using System.Collections.Generic;
using BoundedPaths.Editor.Settings;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Windows;

namespace BoundedPaths.Editor
{
    [CreateAssetMenu]
    internal class DefaultSettingsContainer : ScriptableObject
    {
        [SerializeField] private Material defaultPathBoundsMaterial;
        [SerializeField] private Preset defaultPathBoundsTransform;
        [SerializeField] private Preset defaultPathBounds;

        private void Awake()
        {
            Debug.Log("awake");
        }

        private void OnEnable()
        {
            Debug.Log((defaultPathBoundsMaterial, defaultPathBoundsTransform, defaultPathBounds));
        }
    }
}