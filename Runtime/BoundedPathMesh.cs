using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BoundedPaths
{
    public class BoundedPathMesh
    {
        [BurstCompile]
        private struct TriangulateMeshJob : IJob
        {
            private struct Triangle
            {
                public int A { get; set; }
                public int B { get; set; }
                public int C { get; set; }

                public void Flip() => (A, B) = (B, A);
            }
    
            private struct BoundaryIndexToggle
            {
                public IntStream Near { get; set; }
                public IntStream Far { get; set; }

                public void Swap() => (Near, Far) = (Far, Near);
            }
        
            public NativeArray<Vector3> vertices;
            public NativeArray<int> triangles;
            public NativeArray<Vector2> centerLine;
            public int startOfSecondBoundary;

            private BoundaryIndexToggle boundaryIndex;
            private int triangleIndex;
            private int centerLineIndex;

            public void Execute()
            {
                InitializeIndexVariables();

                int numTriangles = vertices.Length - 2;
                for (int i = 0; i < numTriangles; i++)
                {
                    bool useNearVertex = UseNearVertex();
                    Triangle triangle = new ()
                    {
                        A = boundaryIndex.Near,
                        B = boundaryIndex.Far,
                        C = useNearVertex ? ++boundaryIndex.Near : ++boundaryIndex.Far
                    };
                    WriteTriangleIndices(triangle);

                    Vector2 p1 = vertices[useNearVertex ? triangle.B : triangle.A];
                    Vector2 p2 = vertices[triangle.C];
                    centerLine[centerLineIndex++] = Math.Midpoint(p1, p2);

                    if (!useNearVertex)
                    {
                        boundaryIndex.Swap();
                    }
                }
            }

            private void InitializeIndexVariables()
            {
                boundaryIndex = new ()
                {
                    Near = new (0, startOfSecondBoundary),
                    Far = new (startOfSecondBoundary, vertices.Length)
                };
                triangleIndex = 0;
                centerLineIndex = 0;
            }

            private bool UseNearVertex()
            {
                if (boundaryIndex.Far.IsMax)
                    return true;

                IntStream near = boundaryIndex.Near;
                IntStream far = boundaryIndex.Far;

                Vector2 nearVertex = vertices[near];
                Vector2 nextNearVertex = vertices[near + 1];
                Vector2 farVertex = vertices[far];
                Vector2 nextFarVertex = vertices[far + 1];

                Vector2 nearEdge = nextNearVertex - nearVertex;
                Vector2 nearHypotenuse = nextNearVertex - farVertex;
                Vector2 farEdge = nextFarVertex - farVertex;
                Vector2 farHypotenuse = nextFarVertex - nearVertex;

                if (Vector2.Dot(nearEdge.normalized, nearHypotenuse.normalized) < Vector2.Dot(farEdge.normalized, farHypotenuse.normalized))
                {
                    return !Math.PointIsInsideTriangle(vertices[near - 1], nearVertex, farVertex, nextNearVertex)
                           && !Math.LinesIntersect(farVertex, nextNearVertex, nearVertex, vertices[near - 1]);
                }

                return Math.PointIsInsideTriangle(vertices[far - 1], nearVertex, farVertex, nextFarVertex)
                       || Math.LinesIntersect(nextFarVertex, nearVertex, farVertex, vertices[near - 1]);
            }

            private void WriteTriangleIndices(Triangle triangle)
            {
                if (TriangleIsBackFace(triangle))
                {
                    triangle.Flip();
                }

                triangles[triangleIndex++] = triangle.A;
                triangles[triangleIndex++] = triangle.B;
                triangles[triangleIndex++] = triangle.C;
            }

            private bool TriangleIsBackFace(Triangle triangle)
            {
                Vector3 a = vertices[triangle.A];
                Vector3 b = vertices[triangle.B];
                Vector3 c = vertices[triangle.C];

                return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) > 0f;
            }
        }
    
        private readonly Mesh mesh;

        public BoundedPathMesh(Vector3[] firstBoundary, Vector3[] secondBoundary)
        {
            mesh = GenerateMeshBurst(firstBoundary, secondBoundary, out Vector2[] _);
        }
    
        public BoundedPathMesh(Vector3[] firstBoundary, Vector3[] secondBoundary, out Vector2[] centerLine)
        {
            mesh = GenerateMeshBurst(firstBoundary, secondBoundary, out centerLine);
        }

        public static Mesh GenerateMeshBurst(Vector3[] a, Vector3[] b, out Vector2[] centerLine)
        {
            using NativeArray<Vector3> vertices = new (ConstructVertexArray(a, b), Allocator.TempJob);
            using NativeArray<int> triangles = new (ConstructTriangleList(vertices.Length), Allocator.TempJob);
            using NativeArray<Vector2> centerLinePoints = new (vertices.Length - 2, Allocator.TempJob);
            TriangulateMeshJob job = new () { vertices = vertices, triangles = triangles, centerLine = centerLinePoints, startOfSecondBoundary = a.Length };
            job.Schedule().Complete();

            centerLine = centerLinePoints.ToArray();
            return new Mesh { vertices = vertices.ToArray(), triangles = triangles.ToArray() };
        }

        /// <summary>
        /// Construct an ILazyArray which contains all the vertices used by the mesh.
        /// Assumes the first and last element are identical in each input array.
        /// </summary>
        /// <param name="a">Either the inner or outer boundary of the mesh.</param>
        /// <param name="b">Either the outer or inner boundary of the mesh.</param>
        /// <returns>An ILazyArray containing all the points in a and b, in the following order:
        ///     - All the points in b
        ///     - The point in a nearest to b[0]
        ///     - All the points in a after that point (except the last point)
        ///     - All the points in a before that point
        ///     - The point in a nearest to b[0]</returns>
        private static Vector3[] ConstructVertexArray(Vector3[] a, Vector3[] b)
        {
            IEnumerable<Vector3> output = b;

            int startIndex = Math.FindClosestPoint2D(a, b[0]);
            output = output.Concat(a.Skip(startIndex).Take(a.Length - 1 - startIndex));
            output = output.Concat(a.Take(startIndex));
            output = output.Append(a[startIndex]);

            return output.ToArray();
        }

        /// <summary>
        /// Construct an empty array with the appropriate number of elements for the given number of mesh vertices.
        /// </summary>
        /// <param name="numVertices">The number of vertices in the mesh.</param>
        private static int[] ConstructTriangleList(int numVertices)
        {
            return new int[3 * (numVertices - 2)];
        }

        // Do this since the Mesh class is sealed:((
        public static implicit operator Mesh(BoundedPathMesh pathMesh)
        {
            return pathMesh.mesh;
        }
    }
}