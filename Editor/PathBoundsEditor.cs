using System;
using System.Collections.Generic;
using System.Linq;
using BoundedPaths.Editor.Settings;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;
using Debug = UnityEngine.Debug;

namespace BoundedPaths.Editor
{
    [CustomEditor(typeof(PathBounds))]
    public class PathBoundsEditor : UnityEditor.Editor
    {
        public interface IObserverContext
        {
            /// <summary>
            /// Raised whenever the target <see cref="PathBounds"/>' boundaries are modified.
            /// </summary>
            public event Action BoundsUpdated;
        }
    
        private class ObserverContext : IObserverContext
        {
            private struct SelectionInfo
            {
                /// <summary>
                /// The number of currently selected <see cref="BezierKnot"/>s belonging to the target object.
                /// </summary>
                public int knotCount;
                /// <summary>
                /// The number of <see cref="Spline"/>s on the target object with at least one <see cref="BezierKnot"/> selected.
                /// </summary>
                public int splineCount;
            }
        
            private readonly PathBounds target;
            private readonly Spline inner;
            private readonly Spline outer;
            private SelectionInfo selection;
            private Action eventHandlers;
        
            public event Action BoundsUpdated
            {
                add
                {
                    if (eventHandlers == null)
                    {
                        eventHandlers = value;
                        SplineSelection.changed += UpdateSelectionInfo;
                        Spline.Changed += OnSplineChanged;
                        BoundariesUpdated += OnBoundariesUpdated;
                        return;
                    }
                
                    eventHandlers += value;
                }
                remove
                {
                    eventHandlers -= value;
                
                    if (eventHandlers == null)
                    {
                        SplineSelection.changed -= UpdateSelectionInfo;
                        Spline.Changed -= OnSplineChanged;
                        BoundariesUpdated -= OnBoundariesUpdated;
                    }
                }
            }

            /// <summary>
            /// Whether a single <see cref="BezierKnot"/> is being dragged on the <see cref="target"/>.
            /// </summary>
            private bool DraggingSingleKnot => selection.knotCount <= 1 || DirectManipulation.IsDragging;

            public ObserverContext(PathBounds target, Spline innerPath, Spline outerPath)
            {
                this.target = target;
                inner = innerPath;
                outer = outerPath;
                UpdateSelectionInfo();
            }

            /// <summary>
            /// Determines which boundary (or both) should be updated, if any, and updates it.
            /// </summary>
            /// <param name="spline">The <see cref="Spline"/> that changed.</param>
            /// <param name="i">Index of the changed <paramref name="spline"/> in its <see cref="SplineContainer"/>.</param>
            /// <param name="modification">The type of modification to the <paramref name="spline"/>.</param>
            private void OnSplineChanged(Spline spline, int i, SplineModification modification)
            {
                bool isInnerSpline = spline == inner;
                bool isOuterSpline = spline == outer;
                if (!(isInnerSpline || isOuterSpline))
                    return;

                if (!DraggingSingleKnot)
                {
                    // When multiple knots are modified together, Spline.Changed is raised for each one. So we wait until 
                    // the event is raised for each knot before updating the boundaries.
                    int counter = 1;
                    Spline.Changed -= OnSplineChanged;
                    Spline.Changed += OnEditMultipleKnots;
                    return;

                    void OnEditMultipleKnots(Spline spline, int i, SplineModification modification)
                    {
                        counter++;
                        if (counter < selection.knotCount)
                            return;
                    
                        UpdateBoundaries(selection.splineCount > 1);
                        eventHandlers?.Invoke();
                        Spline.Changed -= OnEditMultipleKnots;
                        Spline.Changed += OnSplineChanged;
                    }
                }
            
                UpdateBoundaries(false);
                eventHandlers?.Invoke();
            
                void UpdateBoundaries(bool updateBoth)
                {
                    Spline singleSpline = isInnerSpline && !isOuterSpline ? inner : outer;
                    target.UpdateBoundaries(updateBoth, singleSpline);
                }
            }

            /// <summary>
            /// Raises the <see cref="BoundsUpdated"/> event when the <see cref="target"/>'s boundaries are updated from the editor.
            /// </summary>
            private void OnBoundariesUpdated(PathBounds modifiedBounds)
            {
                if (modifiedBounds != target)
                    return;
            
                eventHandlers?.Invoke();
            }
        
            /// <summary>
            /// Update the <see cref="selection"/> to reflect the selected knots/splines for the <see cref="target"/>. 
            /// </summary>
            private void UpdateSelectionInfo()
            {
                HashSet<Spline> selectedSplines = new ();
                int selectedKnots = 0;
                CountSelectedBezierKnots(new [] { inner, outer }, spline =>
                {
                    selectedKnots++;
                    selectedSplines.Add(spline);
                });

                selection = new SelectionInfo { knotCount = selectedKnots, splineCount = selectedSplines.Count };
            }
        }

        private PathBounds bounds;
        private SplineContainer splineContainer;
        private SerializedProperty numSamples;
        private Spline inner;
        private Spline outer;

        /// <summary>
        /// Raised whenever the boundaries of a <see cref="PathBounds"/> are updated in the editor. The input is the
        /// <see cref="PathBounds"/> whose boundaries were updated.
        /// </summary>
        private static event Action<PathBounds> BoundariesUpdated; 

        /// <summary>
        /// Custom Reset context menu item. Resets the target <see cref="PathBounds"/> to its default state.
        /// </summary>
        [MenuItem("CONTEXT/" + nameof(PathBounds) + "/Reset")]
        private static void OnReset(MenuCommand menuCommand)
        {
            PathBounds bounds = (PathBounds) menuCommand.context;
            PathBoundsEditor editor = (PathBoundsEditor) CreateEditor(bounds);
            editor.ResetPathBounds();
            DestroyImmediate(editor);
        }

        /// <summary>
        /// Get an <see cref="IObserverContext"/> for the given <see cref="PathBounds"/>.
        /// </summary>
        /// <param name="target">The <see cref="PathBounds"/> the returned context will observe.</param>
        /// <returns>An <see cref="IObserverContext"/> that can be used to observe changes to the given <paramref name="target"/>.</returns>
        public static IObserverContext GetObserverContext(PathBounds target)
        {
            PathBoundsEditor editor = (PathBoundsEditor) CreateEditor(target);
            ObserverContext output = new ObserverContext(target, editor.inner, editor.outer);
            DestroyImmediate(editor);
        
            return output;
        }

        /// <summary>
        /// Count the number of selected <see cref="BezierKnot"/>s in the given set of <see cref="Spline"/>s,
        /// and invoke <paramref name="onKnotFound"/> for each one.
        /// </summary>
        /// <param name="boundaries">The boundaries in which to look for selected <see cref="BezierKnot"/>s.</param>
        /// <param name="onKnotFound">Invoked when a selected knot is found, with the knot's containing <see cref="Spline"/>.</param>
        private static void CountSelectedBezierKnots(IEnumerable<Spline> boundaries, Action<Spline> onKnotFound)
        {
            foreach (SelectableSplineElement element in SplineSelection.selection)
            {
                Spline parentSpline = element.target?.Splines[element.targetIndex];
                if (element.tangentIndex == -1 && boundaries.Any(spline => spline == parentSpline))
                {
                    onKnotFound(parentSpline);
                }
            }
        }
    
        private void OnEnable()
        {
            bounds = (PathBounds) target;
            GetTargetObjectFields();
            HideSubComponents();
        }
    
        private void OnDisable()
        {
            if (target != null) // the target was not destroyed
            {
                SerializeBoundaries();
                return;
            }
        
            if (splineContainer != null) // the target was removed from its parent GameObject
            { 
                Undo.DestroyObjectImmediate(splineContainer);
            }
        }

        /// <summary>
        /// Get references to the fields on the target that are needed for this editor to work.
        /// </summary>
        private void GetTargetObjectFields()
        {
            numSamples = serializedObject.FindProperty(nameof(numSamples));
            splineContainer = this.GetOrCreateComponent<SplineContainer>(nameof(splineContainer), bounds.gameObject, ResetSplineContainer);
            inner = splineContainer.Splines[PathBounds.INNER];
            outer = splineContainer.Splines[PathBounds.OUTER];
        }
    
        private void HideSubComponents()
        {
            splineContainer.hideFlags = HideFlags.NotEditable;
        }

        /// <summary>
        /// Reset the given <see cref="SplineContainer"/> to the default state defined for a <see cref="PathBounds"/>.
        /// </summary>
        /// <param name="container">The container to reset.</param>
        private void ResetSplineContainer(SplineContainer container)
        {
            Undo.RecordObject(container, "");
            PrefabUtility.RecordPrefabInstancePropertyModifications(container);
            PathBoundsSettings.DefaultBounds.ApplyTo(container);
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(container, false);
        
            SerializeBoundaries();
        }

        /// <summary>
        /// Reset the target to its default state.
        /// </summary>
        public void ResetPathBounds()
        {
            ResetSplineContainer(splineContainer);
        }

        private void OnPreSceneGUI()
        {
            DisallowDestructiveDelete();
            if (Event.current.type == EventType.MouseUp)
            {        
                SerializeBoundaries();
            }
        }

        /// <summary>
        /// Interrupts a deletion of multiple <see cref="SelectableSplineElement"/>s if it would either delete one of the
        /// target's boundaries or reduce it to a single <see cref="BezierKnot"/>.
        /// </summary>
        private void DisallowDestructiveDelete()
        {
            // You can only delete elements when the active context is SplineToolContext
            if (ToolManager.activeContextType != typeof(SplineToolContext))
                return;

            Event current = Event.current;
            if (current.type != EventType.ExecuteCommand || current.commandName != "SoftDelete")
                return;
        
            Dictionary<Spline, int> selectedKnotCounts = new ();
            CountSelectedBezierKnots(new [] { inner, outer }, spline =>
            {
                if (!selectedKnotCounts.TryAdd(spline, 1))
                {
                    selectedKnotCounts[spline]++;
                }
            });

            foreach (KeyValuePair<Spline, int> pair in selectedKnotCounts)
            {
                int unselectedKnots = pair.Key.Count - pair.Value;
                switch (unselectedKnots)
                {
                    case 0:
                        Debug.LogWarning("You're trying to delete one of the path's boundaries; I can't allow that.");
                        current.Use();
                        return;
                    case 1:
                        Debug.LogWarning($"You're trying to delete {pair.Value} knots from a path boundary with {pair.Key.Count} knots. " +
                                         "A path boundary must have at least 2 knots.");
                        current.Use();
                        return;
                }
            }
        }

        /// <summary>
        /// Update the target's boundaries and save/record the changes.
        /// </summary>
        private void SerializeBoundaries()
        {
            Undo.RecordObject(bounds, "");
            PrefabUtility.RecordPrefabInstancePropertyModifications(bounds);
            UpdateBoundaries();
        }

        /// <summary>
        /// Update the target's boundaries and raise <see cref="BoundariesUpdated"/>.
        /// </summary>
        private void UpdateBoundaries()
        {
            bounds.UpdateBoundaries();
            BoundariesUpdated?.Invoke(bounds);
        }
        public override void OnInspectorGUI()
        {
            DrawNumSamplesProperty(UpdateBoundaries);
        }

        /// <summary>
        /// Draw the <see cref="numSamples"/> property in the inspector and invoke the given callback if its value changes.
        /// </summary>
        /// <param name="onNumSamplesChanged">Invoked when <see cref="numSamples"/>' value is modified in the inspector.</param>
        private void DrawNumSamplesProperty(Action onNumSamplesChanged)
        {
            int previous = numSamples.intValue;
            int newValue = EditorGUILayout.IntField(numSamples.displayName, previous);
            numSamples.intValue = newValue < PathBounds.MIN_NUM_SAMPLES ? previous : newValue;
            serializedObject.ApplyModifiedProperties();

            if (numSamples.intValue == previous)
                return;
        
            onNumSamplesChanged();
        }
    }
}