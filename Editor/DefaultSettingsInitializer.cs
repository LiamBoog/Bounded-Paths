using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BoundedPaths.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace BoundedPaths.Editor
{
    [InitializeOnLoad]
    public class DefaultSettingsInitializer : MonoBehaviour
    {
        static DefaultSettingsInitializer()
        {
            Debug.Log(BoundedPathSettings.DefaultPathMaterial);
            Debug.Log((AppDomain.CurrentDomain.BaseDirectory + $"{nameof(DefaultSettingsInitializer)}.cs", Directory.GetCurrentDirectory()));
        
        }
    }
}