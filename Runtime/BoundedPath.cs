using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BoundedPaths
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class BoundedPath : MonoBehaviour
    {
        [BurstCompile]
        private struct GaussianSmooth : IJobParallelFor
        {
            [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<Vector2> points;
            [WriteOnly] public NativeArray<Vector2> smoothPoints;
        
            private static readonly float[] _kernel = { 0.021f, 0.228f, 0.502f, 0.228f, 0.021f };

            private int GetIndex(int i) => Math.Mod(i, points.Length);

            public void Execute(int index)
            {
                smoothPoints[index] = _kernel[0] * points[GetIndex(index - 2)] + _kernel[1] * points[GetIndex(index - 1)] 
                                                                               + _kernel[2] * points[GetIndex(index)] + _kernel[3] * points[GetIndex(index + 1)]
                                                                               + _kernel[4] * points[GetIndex(index + 2)];
            }
        }
    
        [BurstCompile]
        private struct ConvertSpace : IJobParallelFor
        {
            public Matrix4x4 transformation;
            [ReadOnly] public NativeArray<Vector2> points;
            [WriteOnly] public NativeArray<Vector2> convertedPoints;

            public void Execute(int index) => convertedPoints[index] = transformation * points[index];
        }
    
        private const int SMOOTHING_BATCH_SIZE = 32;
        private const int SPACE_CONVERSION_BATCH_SIZE = 64;
    
        [HideInInspector, SerializeField] private MeshFilter meshFilter;
        [HideInInspector, SerializeField] private MeshRenderer meshRenderer;
        [HideInInspector, SerializeField] private PathBounds pathBounds;
        [HideInInspector, SerializeField] private Vector2[] centerLine;

        public PathBounds Bounds => pathBounds;

        private void Start()
        {
            if (pathBounds == null) // this happens sometimes in prefab mode
                return;

            UpdateMesh();
        }

        /// <summary>
        /// Generate a new <see cref="BoundedPathMesh"/> and centerline from the current state of <see cref="Bounds"/>' boundaries.
        /// </summary>
        public void UpdateMesh()
        {
            meshFilter.sharedMesh = new BoundedPathMesh(pathBounds.InnerBoundary, pathBounds.OuterBoundary, out centerLine);
            centerLine = SmoothCurve(centerLine);
        }

        /// <summary>
        /// Compute the center line of the BoundedPath.
        /// </summary>
        /// <param name="space">The space of the returned points.</param>
        /// <returns>A set of points representing the center line of the BoundedPath. Points are in local or world space
        /// depending on the given space parameter.</returns>
        public Vector2[] GetCenterLine(Space space)
        {
            if (space == Space.Self)
                return centerLine;

            using NativeArray<Vector2> points = new(centerLine, Allocator.TempJob);
            using NativeArray<Vector2> convertedPoints = new(centerLine.Length, Allocator.TempJob);
            ConvertSpace job = new() { transformation = transform.localToWorldMatrix, points = points, convertedPoints = convertedPoints };
            job.Schedule(points.Length, SPACE_CONVERSION_BATCH_SIZE).Complete();

            return convertedPoints.ToArray();
        }

        /// <summary>
        /// Smooth the given curve using a Gaussian kernel.
        /// </summary>
        /// <param name="curve">The curve to smooth.</param>
        /// <returns>A smoother version of the given curve.</returns>
        private Vector2[] SmoothCurve(Vector2[] curve)
        {
            using NativeArray<Vector2> smoothPoints = new(curve, Allocator.TempJob);
            int numPasses = Mathf.Max(2, curve.Length / 100);
            for (int i = 0; i < numPasses; i++)
            {
                using NativeArray<Vector2> points = new(smoothPoints, Allocator.TempJob);
                GaussianSmooth job = new() { points = points, smoothPoints = smoothPoints };
                job.Schedule(points.Length, SMOOTHING_BATCH_SIZE).Complete();
            }

            return smoothPoints.ToArray();
        }
    }
}