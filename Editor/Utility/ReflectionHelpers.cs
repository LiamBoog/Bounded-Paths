using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoundedPaths.Editor;
using UnityEditor.Splines;
using UnityEngine.Splines;

namespace BoundedPaths.Editor
{
    /// <summary>
    /// Stand-in for <see cref="UnityEditor.Splines.SelectableSplineElement"/> that provides access to internal properties using reflection.
    /// </summary>
    public readonly struct SelectableSplineElement
    {
        private static readonly Type selectiableSplineElement = GetSelectableSplineElementType();
        private static readonly FieldInfo _target = GetTargetField();
        private static readonly FieldInfo _targetIndex = GetTargetIndexField();
        private static readonly FieldInfo _tangentIndex = GetTangentIndexField();

        private readonly object selectableSplineElement;

        /// <summary>
        /// The <see cref="ISplineContainer"/> of the <see cref="Spline"/> this element belongs to.
        /// </summary>
        public ISplineContainer target => _target.GetValue(selectableSplineElement) as ISplineContainer;
        /// <summary>
        /// The index of the <see cref="Spline"/> this element belongs to in that <see cref="Spline"/>'s containing <see cref="ISplineContainer"/>.
        /// </summary>
        public int targetIndex => (int) _targetIndex.GetValue(selectableSplineElement);
        /// <summary>
        /// This element's index in the <see cref="Spline"/> it belongs to, if it's a <see cref="SelectableTangent"/>, -1 otherwise.
        /// </summary>
        public int tangentIndex => (int) _tangentIndex.GetValue(selectableSplineElement);

        public SelectableSplineElement(object underlyingObject)
        {
            selectableSplineElement = underlyingObject;
        }

        private static Type GetSelectableSplineElementType()
        {
            string typeName = typeof(EditorSplineUtility).AssemblyQualifiedName.Replace(nameof(EditorSplineUtility), nameof(SelectableSplineElement));
            return Type.GetType(typeName);
        }

        private static FieldInfo GetTargetField() => selectiableSplineElement.GetField(nameof(target));
    
        private static FieldInfo GetTargetIndexField() => selectiableSplineElement.GetField(nameof(targetIndex));
    
        private static FieldInfo GetTangentIndexField() => selectiableSplineElement.GetField(nameof(tangentIndex));
    }
}

/// <summary>
/// Stand-in for <see cref="UnityEditor.Splines.SplineSelection"/> that provides access to internal properties/events using reflection.
/// </summary>
public static class SplineSelection
{
    private static readonly Type splineSelectionType = GetSplineSelectionType();
    private static readonly PropertyInfo _selection = GetSelectionProperty();
    private static readonly EventInfo _changed = GetSelectionChangedEvent();
    
    /// <summary>
    /// Raised whenever the current selection of <see cref="SelectableSplineElement"/>s changes. 
    /// </summary>
    public static event Action changed
    {
        add => _changed.AddEventHandler(null, value);
        remove => _changed.RemoveEventHandler(null, value);
    }

    /// <summary>
    /// The currently selected <see cref="SelectableSplineElement"/>s.
    /// </summary>
    public static IEnumerable<SelectableSplineElement> selection => 
        ((IEnumerable) _selection.GetValue(null)).Cast<object>().Select(obj => new SelectableSplineElement(obj));

    private static Type GetSplineSelectionType()
    {
        string typeName = typeof(EditorSplineUtility).AssemblyQualifiedName.Replace(nameof(EditorSplineUtility), nameof(SplineSelection));
        return Type.GetType(typeName);
    }
    
    private static PropertyInfo GetSelectionProperty() => splineSelectionType.GetProperty(nameof(selection), BindingFlags.Static | BindingFlags.NonPublic);
    
    private static EventInfo GetSelectionChangedEvent() => splineSelectionType.GetEvent(nameof(changed), BindingFlags.Static | BindingFlags.Public);
}

/// <summary>
/// Stand-in for <see cref="UnityEditor.Splines.DirectManipulation"/> that provides access to internal properties using reflection.
/// </summary>
public static class DirectManipulation
{
    private static readonly PropertyInfo _isDragging = GetIsDraggingProperty();

    /// <summary>
    /// Whether a <see cref="SelectableSplineElement"/> is being directly dragged
    /// (i.e., wasn't first selected then moved using the Move Tool).
    /// </summary>
    public static bool IsDragging => (bool) _isDragging.GetValue(null); 
    
    private static PropertyInfo GetIsDraggingProperty()
    {
        string typeName = typeof(EditorSplineUtility).AssemblyQualifiedName.Replace(nameof(EditorSplineUtility), nameof(DirectManipulation));
        return Type.GetType(typeName).GetProperty(nameof(IsDragging));
    }
}
