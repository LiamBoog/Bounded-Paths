using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoundedPaths
{
    [Serializable]
    public struct ProceduralMaterialInfo
    {
        [Serializable]
        public struct MaterialProperties
        {
            [Serializable]
            private class MaterialProperty<T> where T : struct
            {
                [SerializeField] private int propertyId;
                [SerializeField] private T value;
    
                public MaterialProperty(int propertyId, T value)
                {
                    this.propertyId = propertyId;
                    this.value = value;
                }
    
                /// <summary>
                /// Apply this <see cref="MaterialProperty{T}"/> to the given <see cref="Material"/>.
                /// </summary>
                public void Apply(Material material)
                {
                    switch (value)
                    {
                        case float floatValue:
                            material.SetFloat(propertyId, floatValue);
                            return;
                        case int intValue:
                            material.SetInt(propertyId, intValue);
                            return;
                    }
                }
            }
    
            [SerializeField] private List<MaterialProperty<int>> intProperties;
            [SerializeField] private List<MaterialProperty<float>> floatProperties;
    
            /// <summary>
            /// Add a property value to this <see cref="MaterialProperties"/>.
            /// </summary>
            /// <param name="propertyId">Id of the property whose value this entry represents.</param>
            /// <param name="value">Value of the property modification.</param>
            public void Add(int propertyId, int value)
            {
                intProperties ??= new();
                intProperties.Add(new(propertyId, value));
            }
    
            /// <summary>
            /// Add a property value to this <see cref="MaterialProperties"/>.
            /// </summary>
            /// <param name="propertyId">Id of the property whose value this entry represents.</param>
            /// <param name="value">Value of the property modification.</param>
            public void Add(int propertyId, float value)
            {
                floatProperties ??= new();
                floatProperties.Add(new(propertyId, value));
            }
    
            /// <summary>
            /// Apply all current properties to the given <see cref="Material"/>.
            /// </summary>
            public void Apply(Material material)
            {
                foreach (MaterialProperty<int> property in intProperties)
                {
                    property.Apply(material);
                }
    
                foreach (MaterialProperty<float> property in floatProperties)
                {
                    property.Apply(material);
                }
            }
        }
    
        [SerializeField] private Material baseMaterial;
        [SerializeField] private MaterialProperties proceduralModifications;

        public ProceduralMaterialInfo(Material baseMaterial, MaterialProperties proceduralModifications)
        {
            this.baseMaterial = baseMaterial;
            this.proceduralModifications = proceduralModifications;
        }

        /// <summary>
        /// Get a procedural <see cref="Material"/> by applying all current properties to the base material.
        /// </summary>
        public Material GetProceduralMaterial()
        {
            Material output = new(baseMaterial);
            proceduralModifications.Apply(output);
            return output;
        }
    }
}