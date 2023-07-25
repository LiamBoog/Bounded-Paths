using System;
using System.Collections.Generic;
using BoundedPaths.Editor.Settings;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BoundedPaths.Editor
{
    [CustomEditor(typeof(BoundedPath))]
    public class BoundedPathEditor : UnityEditor.Editor
    {
        [InitializeOnLoad]
        private class PrefabMeshUpdater : AssetPostprocessor
        {
            private const string PREFAB_ASSET_EXTENSION = ".prefab";
        
            private static readonly HashSet<string> _exclusions = new ();
    
            static PrefabMeshUpdater()
            {
                EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
            }
    
            /// <summary>
            /// Attempt to update the procedural mesh on <see cref="BoundedPath"/>s in the asset described by the given GUID.
            /// Does nothing if the asset is not a prefab containing <see cref="BoundedPath"/>s or if the prefab has already had
            /// its meshes updated.
            ///
            /// When subscribed to <see cref="EditorApplication.projectWindowItemOnGUI"/>, this causes all prefabs containing a <see cref="BoundedPath"/>
            /// to have their meshes updated the first time they are rendered in the project view.
            /// </summary>
            /// <param name="guid">GUID of the asset being rendered in the project view.</param>
            /// <param name="selectionRect">Not used.</param>
            private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
            {
                if (_exclusions.Contains(guid)) 
                    return;
            
                try
                {
                    if (TryUpdateBoundedPathMeshes(AssetDatabase.GUIDToAssetPath(guid)))
                        _exclusions.Add(guid);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    _exclusions.Add(guid);
                }
            }
        
            /// <summary>
            /// Attempt to update the procedural mesh on <see cref="BoundedPath"/>s in all imported assets.
            /// Does nothing if the asset is not a prefab containing <see cref="BoundedPath"/>s or if the prefab has already had
            /// its meshes updated.
            /// </summary>
            /// <param name="importedAssets">Paths of assets that were imported.</param>
            /// <param name="deletedAssets">Not used.</param>
            /// <param name="movedAssets">Not used.</param>
            /// <param name="movedFromAssetPaths">Not used.</param>
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (string path in importedAssets)
                {
                    TryUpdateBoundedPathMeshes(path);
                }
            }

            /// <summary>
            /// Try to get all <see cref="BoundedPath"/>s in the given asset.
            /// </summary>
            /// <param name="assetPath">Path of the asset to search.</param>
            /// <param name="boundedPaths"><see cref="BoundedPath"/>s contained in the given asset.</param>
            /// <returns>True if the given asset was a prefab containing <see cref="BoundedPath"/>s, false otherwise.</returns>
            private static bool TryGetBoundedPathsInPrefab(string assetPath, out BoundedPath[] boundedPaths)
            {
                if (!assetPath.Contains(PREFAB_ASSET_EXTENSION))
                {
                    boundedPaths = null;
                    return false;
                }
    
                boundedPaths = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath).transform.GetComponentsInChildren<BoundedPath>();
                return boundedPaths.Length > 0;
            }
    
            /// <summary>
            /// Try to update meshes on all <see cref="BoundedPath"/>s in the given asset.
            /// </summary>
            /// <param name="assetPath">Path of the asset to update.</param>
            /// <returns>True if the given asset was a prefab containing <see cref="BoundedPath"/>s whose meshes were updated,
            /// false otherwise.</returns>
            private static bool TryUpdateBoundedPathMeshes(string assetPath)
            {
                if (!TryGetBoundedPathsInPrefab(assetPath, out BoundedPath[] boundedPaths))
                    return false;
            
                foreach (BoundedPath boundedPath in boundedPaths)
                {
                    if (boundedPath.Bounds == null)
                        continue;
                
                    boundedPath.UpdateMesh();
                }
    
                return true;
            }
        }

        private BoundedPath path;
        private PathBoundsEditor.IObserverContext boundsObserver;
        private bool isPrefab;
        private Mesh mesh;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private PathBounds pathBounds;

        /// <summary>
        /// Context menu item to create a <see cref="BoundedPath"/> easily in the inspector.
        /// </summary>
        [MenuItem("GameObject/" + nameof(BoundedPath))]
        public static void CreateNewBoundedPath(MenuCommand menuCommand)
        {
            GameObject path = new GameObject(nameof(BoundedPath));
            GameObjectUtility.SetParentAndAlign(path, menuCommand.context as GameObject);
            path.AddComponent<BoundedPath>();
            BoundedPathSettings.DefaultTransform.ApplyTo(path.transform);
            Undo.RegisterCreatedObjectUndo(path, $"Create {path.name}");
            Selection.activeObject = path;
        }

        /// <summary>
        /// Custom Reset context menu item. Resets the target <see cref="BoundedPath"/> to its default state.
        /// </summary>
        [MenuItem("CONTEXT/" + nameof(BoundedPath) + "/Reset")]
        private static void OnReset(MenuCommand menuCommand)
        {
            BoundedPath bounds = (BoundedPath) menuCommand.context;
            BoundedPathEditor editor = (BoundedPathEditor) CreateEditor(bounds);
            editor.ResetBoundedPath();
            DestroyImmediate(editor);
        }

        public void OnEnable()
        {
            path = (BoundedPath) target;
            GetTargetObjectFields();
            HideSubComponents();

            boundsObserver = PathBoundsEditor.GetObserverContext(pathBounds);
            boundsObserver.BoundsUpdated += OnBoundsUpdated;
            OnBoundsUpdated();
        
            CustomPrefabUtility.TryHandleExitPrefabMode(path.gameObject, OnExitPrefabMode);
        }

        private void OnDisable()
        {
            boundsObserver.BoundsUpdated -= OnBoundsUpdated;

            if (path != null) // the target was not destroyed
            {
                RecordMeshAndCenterLineChanges();
                return;
            }

            if (meshFilter != null && meshRenderer != null) // the target was removed from its parent GameObject
            {
                Undo.DestroyObjectImmediate(meshFilter);
                Undo.DestroyObjectImmediate(meshRenderer);
            }
        }

        /// <summary>
        /// Get references to the fields on the target that are needed for this editor to work.
        /// </summary>
        private void GetTargetObjectFields()
        {
            pathBounds = this.GetOrCreateUniqueComponent<PathBounds>(nameof(pathBounds), path.gameObject, ResetPathBounds);
            meshFilter = this.GetOrCreateUniqueComponent<MeshFilter>(nameof(meshFilter), path.gameObject, _ => {});
            meshRenderer = this.GetOrCreateUniqueComponent<MeshRenderer>(nameof(meshRenderer), path.gameObject, ResetMeshRenderer);
        }
    
        private void HideSubComponents()
        {
            meshFilter.hideFlags = HideFlags.HideInInspector;
            meshRenderer.hideFlags = HideFlags.HideInInspector;
        }

        /// <summary>
        /// Reset the given <see cref="PathBounds"/> to the default state defined for a <see cref="PathBounds"/>.
        /// </summary>
        /// <param name="bounds">The <see cref="PathBounds"/> to reset.</param>
        private void ResetPathBounds(PathBounds bounds)
        {
            PathBoundsEditor editor = (PathBoundsEditor) CreateEditor(bounds);
            editor.ResetPathBounds();
            DestroyImmediate(editor);
        }

        /// <summary>
        /// Reset the given <see cref="MeshRenderer"/> to the default state defined for a <see cref="BoundedPath"/>.
        /// </summary>
        /// <param name="renderer"></param>
        private void ResetMeshRenderer(MeshRenderer renderer)
        {
            Undo.RecordObject(renderer, "");
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            renderer.material = BoundedPathSettings.DefaultPathMaterial;
        }

        /// <summary>
        /// Reset the target to its default state.
        /// </summary>
        public void ResetBoundedPath()
        {
            ResetMeshRenderer(meshRenderer);
        }

        /// <summary>
        /// Update the target's procedural mesh.
        /// </summary>
        private void OnBoundsUpdated()
        {
            path.UpdateMesh();
        }

        /// <summary>
        /// Update the target's procedural mesh/centerline and save/record the changes. 
        /// </summary>
        private void RecordMeshAndCenterLineChanges()
        {
            Undo.RecordObject(path, "");
            PrefabUtility.RecordPrefabInstancePropertyModifications(path);
            Undo.RecordObject(meshFilter, "");
            PrefabUtility.RecordPrefabInstancePropertyModifications(meshFilter);
            path.UpdateMesh();
        }

        private void OnPreSceneGUI()
        {
            if (Event.current.type == EventType.MouseUp)
            {
                RecordMeshAndCenterLineChanges();
            }
        }

        /// <summary>
        /// Queue up an event handler that generates a new mesh for each <see cref="BoundedPath"/> in the given prefab instance
        /// before drawing the scene GUI. This is meant to be invoked once for each instance of a <see cref="BoundedPath"/>
        /// prefab after exiting prefab mode.
        /// </summary>
        /// <param name="instance">The prefab instance to generate the new meshes for.</param>
        private void OnExitPrefabMode(GameObject instance)
        {
            SceneView.beforeSceneGui += UpdatePrefabInstanceMeshes;
            
            void UpdatePrefabInstanceMeshes(SceneView sceneView)
            {
                foreach (BoundedPath boundedPath in instance.transform.GetComponentsInChildren<BoundedPath>())
                {
                    boundedPath.UpdateMesh();
                }
                SceneView.beforeSceneGui -= UpdatePrefabInstanceMeshes;
            }
        }
    }
}