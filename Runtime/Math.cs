using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BoundedPaths
{
    public static class Math
    {
        [BurstCompile]
        private struct ClosestPointJob : IJob
        {
            [ReadOnly] public Vector3 target;
            [ReadOnly] public NativeArray<Vector3> points;
            [WriteOnly] public NativeArray<int> closestPoint;
        
            public void Execute()
            {
                float smallestDistance = SquaredDistanceFromTarget2D(points[0]);
                closestPoint[0] = 0;
            
                int numPoints = points.Length;
                for (int i = 1; i < numPoints; i++)
                {
                    float distance = SquaredDistanceFromTarget2D(points[i]);
                    if (distance < smallestDistance)
                    {
                        closestPoint[0] = i;
                        smallestDistance = distance;
                    }
                }
            }
        
            private float SquaredDistanceFromTarget2D(Vector3 u) => (u.x - target.x) * (u.x - target.x) + (u.y - target.y) * (u.y - target.y);
        }
    
        /// <summary>
        /// Find the index of the point in <paramref name="points"/> that is nearest to the <paramref name="target"/>.
        /// </summary>
        /// <param name="points">The set of points from which to select the nearest point.</param>
        /// <param name="target">The point to which the chosen point should be nearest.</param>
        /// <returns>The index of the closest point in <paramref name="points"/> to the <paramref name="target"/>.</returns>
        public static int FindClosestPoint2D(Vector3[] points, Vector3 target)
        {
            using NativeArray<Vector3> nativePoints = new (points, Allocator.TempJob);
            using NativeArray<int> output = new(1, Allocator.TempJob);
            ClosestPointJob job = new () { target = target, points = nativePoints, closestPoint = output};
            job.Run();

            return output[0];
        }
    
        /// <summary>
        /// Determine whether a point, <paramref name="p"/> is inside the triangle whose vertices are
        /// (<paramref name="a"/>, <paramref name="b"/>, <paramref name="c"/>). This method is exclusive; i.e.,
        /// points on the edges or vertices of the triangle are not considered inside the triangle.
        /// </summary>
        /// <param name="p">The point to check against the triangle.</param>
        /// <param name="a">A vertex of the triangle.</param>
        /// <param name="b">A vertex of the triangle.</param>
        /// <param name="c">A vertex of the triangle.</param>
        /// <returns>True if the point is strictly inside the triangle, false otherwise.</returns>
        public static bool PointIsInsideTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            float u = ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y));
            if (u is <= 0f or > 1f)
                return false;
            
            float v = ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y));
            if (v is <= 0f or > 1f)
                return false;
            
            float w = 1f - u - v;
            return w is > 0f and < 1f;
        }
    
        /// <summary>
        /// Determine whether or not the line segments formed by the given points intersect.
        /// </summary>
        /// <param name="u1">One end of the first line segment.</param>
        /// <param name="u2">The other end of the first line segment.</param>
        /// <param name="v1">One end of the second line segment.</param>
        /// <param name="v2">The other end of the second line segment.</param>
        /// <returns>True if the line segments intersect, false otherwise.</returns>
        public static bool LinesIntersect(Vector2 u1, Vector2 u2, Vector2 v1, Vector2 v2)
        {
            int orientation1 = OrderedPointsOrientation(u1, u2, v1);
            int orientation2 = OrderedPointsOrientation(u1, u2, v2);
            int orientation3 = OrderedPointsOrientation(v1, v2, u1);
            int orientation4 = OrderedPointsOrientation(v1, v2, u2);

            if (orientation1 != orientation2 && orientation3 != orientation4)
                return true;

            if (orientation1 == 0 && OnSegment(u1, v1, u2))
                return true;
            if (orientation2 == 0 && OnSegment(u1, v2, u2))
                return true;
            if (orientation3 == 0 && OnSegment(v1, u1, v2))
                return true;
            if (orientation4 == 0 && OnSegment(v1, u2, v2))
                return true;

            return false;
        
            // 0 if p1, p2, p3 are collinear
            // 1 if clockwise
            // 2 if counter-clockwise
            int OrderedPointsOrientation(Vector2 p1, Vector2 p2, Vector2 p3)
            {
                float det = (p2.y - p1.y) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.y - p2.y);

                if (det == 0)
                    return 0;

                return det > 0 ? 1 : 2;
            }
        
            // Check if point is on line segment formed by p1 and p2
            bool OnSegment(Vector2 point, Vector2 p1, Vector2 p2)
            {
                return point.x <= Mathf.Max(p1.x, p2.x) && 
                       point.x >= Mathf.Min(p1.x, p2.x) && 
                       point.y <= Mathf.Max(p1.y, p2.y) && 
                       point.y >= Mathf.Min(p1.y, p2.y);
            }
        }
    
        /// <summary>
        /// Get the midpoint between 2 points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <returns>The midpoint between p1 and p2.</returns>
        public static Vector2 Midpoint(Vector2 p1, Vector2 p2)
        {
            return 0.5f * (p1 + p2);
        }

        /// <summary>
        /// Implements the modulo function.
        /// </summary>
        /// <param name="a">The value to apply the function to.</param>
        /// <param name="b">The value to use to compute the modulo.</param>
        /// <returns><paramref name="a"/> mod <paramref name="b"/>.</returns>
        public static int Mod(int a, int b)
        {
            int r = a % b;
            return r < 0 ? r + b : r;
        }
    }
}