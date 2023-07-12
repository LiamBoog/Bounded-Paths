using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace BoundedPaths
{
    [DisallowMultipleComponent]
    public class PathBounds : MonoBehaviour
    {
        public const int INNER = 0;
        public const int OUTER = 1;
        public const int MIN_NUM_SAMPLES = 10;
        // In testing, this was best for high sample counts and irrelevant for low ones
        private const int SAMPLING_BATCH_SIZE = 128;

        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private int numSamples = 300;
        [SerializeField] private Vector3[] innerBoundary;
        [SerializeField] private Vector3[] outerBoundary;

        public Vector3[] InnerBoundary => innerBoundary;
        public Vector3[] OuterBoundary => outerBoundary;

        /// <summary>
        /// Resample the boundaries.
        /// </summary>
        /// <param name="updateBoth">Whether to resample both boundaries.</param>
        /// <param name="singleSpline">Which boundary to sample, if updateBoth is false.</param>
        public void UpdateBoundaries(bool updateBoth = true, Spline singleSpline = null)
        {
            IReadOnlyList<Spline> splines = splineContainer.Splines;
            innerBoundary = updateBoth || singleSpline == splines[INNER] ? SampleBoundary(splines[INNER]) : innerBoundary;
            outerBoundary = updateBoth || singleSpline == splines[OUTER] ? SampleBoundary(splines[OUTER]) : outerBoundary;
        
            Vector3[] SampleBoundary(Spline spline)
            {
                using NativeSpline nativeSpline = new NativeSpline(spline, Matrix4x4.identity, Allocator.TempJob);
                using NativeArray<float3> points = new NativeArray<float3>(numSamples, Allocator.TempJob);
                GetPosition job = new GetPosition { Spline = nativeSpline, Positions = points };
                job.Schedule(points.Length, SAMPLING_BATCH_SIZE).Complete();

                return points.Reinterpret<Vector3>().ToArray();
            }
        }
    }
}