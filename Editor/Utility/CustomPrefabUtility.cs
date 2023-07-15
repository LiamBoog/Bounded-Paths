using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BoundedPaths.Editor
{
    public static class CustomPrefabUtility
    {
        /// <summary>
        /// Perform some action once for each instance of the prefab referenced by the given PrefabStage when it closes.
        /// </summary>
        /// <param name="stage">The PrefabStage upon closing which will trigger the given callback.</param>
        /// <param name="callback">Hook used to perform the desired action on each prefab instance.</param>
        private static void HandleExitPrefabMode(PrefabStage stage, Action<GameObject> callback)
        {
            PrefabStage.prefabStageClosing += InvokeCallBackForPrefabInstances;

            void InvokeCallBackForPrefabInstances(PrefabStage currentStage)
            {
                if (currentStage != stage)
                {
                    return;
                }
            
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(stage.assetPath);
                foreach (GameObject instance in PrefabUtility.FindAllInstancesOfPrefab(prefabAsset))
                {
                    callback?.Invoke(instance);
                }

                // Unsubscribe so this method only triggers once
                PrefabStage.prefabStageClosing -= InvokeCallBackForPrefabInstances;
            }
        }

        /// <summary>
        /// Perform some action once for each instance of the given prefab when exiting prefab mode
        /// iff the given GameObject is:
        ///     - A prefab
        ///     - Open in prefab mode
        /// </summary>
        /// <param name="prefab">A GameObject which may or may not be a prefab open in prefab mode.</param>
        /// <param name="callback">Hook used to perform the desired action for each prefab instance.</param>
        public static void TryHandleExitPrefabMode(GameObject prefab, Action<GameObject> callback)
        {
            if (PrefabStageUtility.GetPrefabStage(prefab) is not { } currentPrefabStage)
                return;
        
            HandleExitPrefabMode(currentPrefabStage, callback);
        }
    }
}