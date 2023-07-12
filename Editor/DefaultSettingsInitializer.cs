using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BoundedPaths.Editor.Settings;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DefaultSettingsInitializer : MonoBehaviour
{
    static DefaultSettingsInitializer()
    {
        Debug.Log(BoundedPathSettings.DefaultPathMaterial);
        Debug.Log((Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Directory.GetCurrentDirectory()));
        
    }
}
