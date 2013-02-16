﻿/*
* Copyright (c) 2007-2010 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;

namespace CryEngine
{
    /*
     * This class is organized so that the least complex objects come first so that the least
     * complex objects will have the most methods in most cases. Note that not all shapes exist
     * at this time and not all shapes have a corresponding struct. Only the objects that have
     * a corresponding struct should come first in naming and in parameter order. The order of
     * complexity is as follows:
     * 
     * 1. Point
     * 2. Ray
     * 3. Segment
     * 4. Plane
     * 5. Triangle
     * 6. Polygon
     * 7. Box
     * 8. Sphere
     * 9. Ellipsoid
     * 10. Cylinder
     * 11. Cone
     * 12. Capsule
     * 13. Torus
     * 14. Polyhedron
     * 15. Frustum
    */

    /// <summary>
    /// Contains static methods to help in determining intersections, containment, etc.
    /// </summary>
    public static class Collision
    {
        /// <summary>
        /// Determines the closest point between a point and a triangle.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="vertex1">The first vertex to test.</param>
        /// <param name="vertex2">The second vertex to test.</param>
        /// <param name="vertex3">The third vertex to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointPointTriangle(ref Vec3 point, ref Vec3 vertex1, ref Vec3 vertex2, ref Vec3 vertex3, out Vec3 result)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 136

            // Check if P in vertex region outside A
            Vec3 ab = vertex2 - vertex1;
            Vec3 ac = vertex3 - vertex1;
            Vec3 ap = point - vertex1;

            float d1 = Vec3.Dot(ab, ap);
            float d2 = Vec3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f)
                result = vertex1; // Barycentric coordinates (1,0,0)

            // Check if P in vertex region outside B
            Vec3 bp = point - vertex2;
            float d3 = Vec3.Dot(ab, bp);
            float d4 = Vec3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3)
                result = vertex2; // barycentric coordinates (0,1,0)

            // Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);
                result = vertex1 + v * ab; // Barycentric coordinates (1-v,v,0)
            }

            // Check if P in vertex region outside C
            Vec3 cp = point - vertex3;
            float d5 = Vec3.Dot(ab, cp);
            float d6 = Vec3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6)
                result = vertex3; // Barycentric coordinates (0,0,1)

            // Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);
                result = vertex1 + w * ac; // Barycentric coordinates (1-w,0,w)
            }

            // Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                result = vertex2 + w * (vertex3 - vertex2); // Barycentric coordinates (0,1-w,w)
            }

            // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float v2 = vb * denom;
            float w2 = vc * denom;
            result = vertex1 + ab * v2 + ac * w2; // = u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
        }

        /// <summary>
        /// Determines the closest point between a <see cref="CryEngine.Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointPlanePoint(ref Plane plane, ref Vec3 point, out Vec3 result)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 126

            float dot = plane.Normal.Dot(point);
            float t = dot - plane.D;

            result = point - (t * plane.Normal);
        }

        /// <summary>
        /// Determines the closest point between a <see cref="CryEngine.BoundingBox"/> and a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointBoxPoint(ref BoundingBox box, ref Vec3 point, out Vec3 result)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 130

            Vec3 temp;
            Vec3.Max(ref point, ref box.Minimum, out temp);
            Vec3.Min(ref temp, ref box.Maximum, out result);
        }

        /// <summary>
        /// Determines the closest point between a <see cref="CryEngine.BoundingSphere"/> and a point.
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects;
        /// or, if the point is directly in the center of the sphere, contains <see cref="CryEngine.Vec3"/>.</param>
        public static void ClosestPointSpherePoint(ref BoundingSphere sphere, ref Vec3 point, out Vec3 result)
        {
            // Source: Jorgy343
            // Reference: None

            // Get the unit direction from the sphere's center to the point.
            result = point - sphere.Center;
            result.Normalize();

            // Multiply the unit direction by the sphere's radius to get a vector
            // the length of the sphere.
            result *= sphere.Radius;

            // Add the sphere's center to the direction to get a point on the sphere.
            result += sphere.Center;
        }

        /// <summary>
        /// Determines the closest point between a <see cref="CryEngine.BoundingSphere"/> and a <see cref="CryEngine.BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects;
        /// or, if the point is directly in the center of the sphere, contains <see cref="CryEngine.Vec3"/>.</param>
        /// <remarks>
        /// If the two spheres are overlapping, but not directly ontop of each other, the closest point
        /// is the 'closest' point of intersection. This can also be considered is the deepest point of
        /// intersection.
        /// </remarks>
        public static void ClosestPointSphereSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2, out Vec3 result)
        {
            // Source: Jorgy343
            // Reference: None

            // Get the unit direction from the first sphere's center to the second sphere's center.
            result = sphere2.Center - sphere1.Center;
            result.Normalize();

            // Multiply the unit direction by the first sphere's radius to get a vector
            // the length of the first sphere.
            result *= sphere1.Radius;

            // Add the first sphere's center to the direction to get a point on the first sphere.
            result += sphere1.Center;
        }

        /// <summary>
        /// Determines the distance between a <see cref="CryEngine.Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistancePlanePoint(ref Plane plane, ref Vec3 point)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 127

            float dot = plane.Normal.Dot(point);
            return dot - plane.D;
        }

        /// <summary>
        /// Determines the distance between a <see cref="CryEngine.BoundingBox"/> and a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceBoxPoint(ref BoundingBox box, ref Vec3 point)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 131

            float distance = 0f;

            if (point.X < box.Minimum.X)
                distance += (box.Minimum.X - point.X) * (box.Minimum.X - point.X);
            if (point.X > box.Maximum.X)
                distance += (point.X - box.Maximum.X) * (point.X - box.Maximum.X);

            if (point.Y < box.Minimum.Y)
                distance += (box.Minimum.Y - point.Y) * (box.Minimum.Y - point.Y);
            if (point.Y > box.Maximum.Y)
                distance += (point.Y - box.Maximum.Y) * (point.Y - box.Maximum.Y);

            if (point.Z < box.Minimum.Z)
                distance += (box.Minimum.Z - point.Z) * (box.Minimum.Z - point.Z);
            if (point.Z > box.Maximum.Z)
                distance += (point.Z - box.Maximum.Z) * (point.Z - box.Maximum.Z);

            return (float)Math.Sqrt(distance);
        }

        /// <summary>
        /// Determines the distance between a <see cref="CryEngine.BoundingBox"/> and a <see cref="CryEngine.BoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceBoxBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            float distance = 0f;

            // Distance for X.
            if (box1.Minimum.X > box2.Maximum.X)
            {
                float delta = box2.Maximum.X - box1.Minimum.X;
                distance += delta * delta;
            }
            else if (box2.Minimum.X > box1.Maximum.X)
            {
                float delta = box1.Maximum.X - box2.Minimum.X;
                distance += delta * delta;
            }

            // Distance for Y.
            if (box1.Minimum.Y > box2.Maximum.Y)
            {
                float delta = box2.Maximum.Y - box1.Minimum.Y;
                distance += delta * delta;
            }
            else if (box2.Minimum.Y > box1.Maximum.Y)
            {
                float delta = box1.Maximum.Y - box2.Minimum.Y;
                distance += delta * delta;
            }

            // Distance for Z.
            if (box1.Minimum.Z > box2.Maximum.Z)
            {
                float delta = box2.Maximum.Z - box1.Minimum.Z;
                distance += delta * delta;
            }
            else if (box2.Minimum.Z > box1.Maximum.Z)
            {
                float delta = box1.Maximum.Z - box2.Minimum.Z;
                distance += delta * delta;
            }

            return (float)Math.Sqrt(distance);
        }

        /// <summary>
        /// Determines the distance between a <see cref="CryEngine.BoundingSphere"/> and a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceSpherePoint(ref BoundingSphere sphere, ref Vec3 point)
        {
            // Source: Jorgy343
            // Reference: None

            float distance = sphere.Center.GetDistance(point);
            distance -= sphere.Radius;

            return MathHelpers.Max(distance, 0f);
        }

        /// <summary>
        /// Determines the distance between a <see cref="CryEngine.BoundingSphere"/> and a <see cref="CryEngine.BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceSphereSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2)
        {
            // Source: Jorgy343
            // Reference: None

            float distance = sphere1.Center.GetDistance(sphere2.Center);
            distance -= sphere1.Radius + sphere2.Radius;

            return MathHelpers.Max(distance, 0f);
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Ray"/> and a point.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersect.</returns>
        public static bool RayIntersectsPoint(ref Ray ray, ref Vec3 point)
        {
            // Source: RayIntersectsSphere
            // Reference: None

            Vec3 m = ray.Position - point;

            // Same thing as RayIntersectsSphere except that the radius of the sphere (point)
            // is the epsilon for zero.
            float b = Vec3.Dot(m, ray.Direction);
            float c = Vec3.Dot(m, m) - MathHelpers.ZeroTolerance;

            if (c > 0f && b > 0f)
                return false;

            float discriminant = b * b - c;

            if (discriminant < 0f)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Ray"/> and a <see cref="CryEngine.Ray"/>.
        /// </summary>
        /// <param name="ray1">The first ray to test.</param>
        /// <param name="ray2">The second ray to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="CryEngine.Vec3"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersect.</returns>
        /// <remarks>
        /// This method performs a ray vs ray intersection test based on the following formula
        /// from Goldman.
        /// <code>s = det([o_2 - o_1, d_2, d_1 x d_2]) / ||d_1 x d_2||^2</code>
        /// <code>t = det([o_2 - o_1, d_1, d_1 x d_2]) / ||d_1 x d_2||^2</code>
        /// Where o_1 is the position of the first ray, o_2 is the position of the second ray,
        /// d_1 is the normalized direction of the first ray, d_2 is the normalized direction
        /// of the second ray, det denotes the determinant of a matrix, x denotes the cross
        /// product, [ ] denotes a matrix, and || || denotes the length or magnitude of a vector.
        /// </remarks>
        public static bool RayIntersectsRay(ref Ray ray1, ref Ray ray2, out Vec3 point)
        {
            // Source: Real-Time Rendering, Third Edition
            // Reference: Page 780

            Vec3 cross = ray1.Direction.Cross(ray2.Direction);
            float denominator = cross.Length;

            // Lines are parallel.
            if (Math.Abs(denominator) < MathHelpers.ZeroTolerance)
            {
                // Lines are parallel and on top of each other.
                if (Math.Abs(ray2.Position.X - ray1.Position.X) < MathHelpers.ZeroTolerance &&
                    Math.Abs(ray2.Position.Y - ray1.Position.Y) < MathHelpers.ZeroTolerance &&
                    Math.Abs(ray2.Position.Z - ray1.Position.Z) < MathHelpers.ZeroTolerance)
                {
                    point = Vec3.Zero;
                    return true;
                }
            }

            denominator = denominator * denominator;

            // 3x3 matrix for the first ray.
            float m11 = ray2.Position.X - ray1.Position.X;
            float m12 = ray2.Position.Y - ray1.Position.Y;
            float m13 = ray2.Position.Z - ray1.Position.Z;
            float m21 = ray2.Direction.X;
            float m22 = ray2.Direction.Y;
            float m23 = ray2.Direction.Z;
            float m31 = cross.X;
            float m32 = cross.Y;
            float m33 = cross.Z;

            // Determinant of first matrix.
            float dets =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            // 3x3 matrix for the second ray.
            m21 = ray1.Direction.X;
            m22 = ray1.Direction.Y;
            m23 = ray1.Direction.Z;

            // Determinant of the second matrix.
            float dett =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            // t values of the point of intersection.
            float s = dets / denominator;
            float t = dett / denominator;

            // The points of intersection.
            Vec3 point1 = ray1.Position + (s * ray1.Direction);
            Vec3 point2 = ray2.Position + (t * ray2.Direction);

            // If the points are not equal, no intersection has occured.
            if (Math.Abs(point2.X - point1.X) > MathHelpers.ZeroTolerance ||
                Math.Abs(point2.Y - point1.Y) > MathHelpers.ZeroTolerance ||
                Math.Abs(point2.Z - point1.Z) > MathHelpers.ZeroTolerance)
            {
                point = Vec3.Zero;
                return false;
            }

            point = point1;
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Ray"/> and a <see cref="CryEngine.Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="plane">The plane to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersect.</returns>
        public static bool RayIntersectsPlane(ref Ray ray, ref Plane plane, out float distance)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 175

            float direction = plane.Normal.Dot(ray.Direction);

            if (Math.Abs(direction) < MathHelpers.ZeroTolerance)
            {
                distance = 0f;
                return false;
            }

            float position = plane.Normal.Dot(ray.Position);
            distance = (plane.D - position) / direction;

            if (distance < 0f)
            {
                if (distance < -MathHelpers.ZeroTolerance)
                {
                    distance = 0;
                    return false;
                }

                distance = 0f;
            }

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Ray"/> and a <see cref="CryEngine.Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="plane">The plane to test</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="CryEngine.Vec3"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsPlane(ref Ray ray, ref Plane plane, out Vec3 point)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 175

            float distance;
            if (!RayIntersectsPlane(ref ray, ref plane, out distance))
            {
                point = Vec3.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triagnle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// This method tests if the ray intersects either the front or back of the triangle.
        /// If the ray is parallel to the triangle's plane, no intersection is assumed to have
        /// happened. If the intersection of the ray and the triangle is behind the origin of
        /// the ray, no intersection is assumed to have happened. In both cases of assumptions,
        /// this method returns false.
        /// </remarks>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Vec3 vertex1, ref Vec3 vertex2, ref Vec3 vertex3, out float distance)
        {
            // Source: Fast Minimum Storage Ray / Triangle Intersection
            // Reference: http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf

            // Compute vectors along two edges of the triangle.
            Vec3 edge1, edge2;

            // Edge 1
            edge1.X = vertex2.X - vertex1.X;
            edge1.Y = vertex2.Y - vertex1.Y;
            edge1.Z = vertex2.Z - vertex1.Z;

            // Edge2
            edge2.X = vertex3.X - vertex1.X;
            edge2.Y = vertex3.Y - vertex1.Y;
            edge2.Z = vertex3.Z - vertex1.Z;

            // Cross product of ray direction and edge2 - first part of determinant.
            Vec3 directioncrossedge2;
            directioncrossedge2.X = (ray.Direction.Y * edge2.Z) - (ray.Direction.Z * edge2.Y);
            directioncrossedge2.Y = (ray.Direction.Z * edge2.X) - (ray.Direction.X * edge2.Z);
            directioncrossedge2.Z = (ray.Direction.X * edge2.Y) - (ray.Direction.Y * edge2.X);

            // Compute the determinant.
            // Dot product of edge1 and the first part of determinant.
            float determinant = (edge1.X * directioncrossedge2.X) + (edge1.Y * directioncrossedge2.Y) + (edge1.Z * directioncrossedge2.Z);

            // If the ray is parallel to the triangle plane, there is no collision.
            // This also means that we are not culling, the ray may hit both the
            // back and the front of the triangle.
            if (determinant > -MathHelpers.ZeroTolerance && determinant < MathHelpers.ZeroTolerance)
            {
                distance = 0f;
                return false;
            }

            float inversedeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vec3 distanceVector;
            distanceVector.X = ray.Position.X - vertex1.X;
            distanceVector.Y = ray.Position.Y - vertex1.Y;
            distanceVector.Z = ray.Position.Z - vertex1.Z;

            float triangleU = (distanceVector.X * directioncrossedge2.X) + (distanceVector.Y * directioncrossedge2.Y) + (distanceVector.Z * directioncrossedge2.Z);
            triangleU *= inversedeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0f || triangleU > 1f)
            {
                distance = 0f;
                return false;
            }

            // Calculate the V parameter of the intersection point.
            Vec3 distancecrossedge1;
            distancecrossedge1.X = (distanceVector.Y * edge1.Z) - (distanceVector.Z * edge1.Y);
            distancecrossedge1.Y = (distanceVector.Z * edge1.X) - (distanceVector.X * edge1.Z);
            distancecrossedge1.Z = (distanceVector.X * edge1.Y) - (distanceVector.Y * edge1.X);

            float triangleV = ((ray.Direction.X * distancecrossedge1.X) + (ray.Direction.Y * distancecrossedge1.Y)) + (ray.Direction.Z * distancecrossedge1.Z);
            triangleV *= inversedeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0f || triangleU + triangleV > 1f)
            {
                distance = 0f;
                return false;
            }

            // Compute the distance along the ray to the triangle.
            float raydistance = (edge2.X * distancecrossedge1.X) + (edge2.Y * distancecrossedge1.Y) + (edge2.Z * distancecrossedge1.Z);
            raydistance *= inversedeterminant;

            // Is the triangle behind the ray origin?
            if (raydistance < 0f)
            {
                distance = 0f;
                return false;
            }

            distance = raydistance;
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Ray"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triagnle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="CryEngine.Vec3"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsTriangle(ref Ray ray, ref Vec3 vertex1, ref Vec3 vertex2, ref Vec3 vertex3, out Vec3 point)
        {
            float distance;
            if (!RayIntersectsTriangle(ref ray, ref vertex1, ref vertex2, ref vertex3, out distance))
            {
                point = Vec3.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Ray"/> and a <see cref="CryEngine.BoundingBox"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref Ray ray, ref BoundingBox box, out float distance)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 179

            distance = 0f;
            float tmax = float.MaxValue;

            if (Math.Abs(ray.Direction.X) < MathHelpers.ZeroTolerance)
            {
                if (ray.Position.X < box.Minimum.X || ray.Position.X > box.Maximum.X)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.X;
                float t1 = (box.Minimum.X - ray.Position.X) * inverse;
                float t2 = (box.Maximum.X - ray.Position.X) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = MathHelpers.Max(t1, distance);
                tmax = MathHelpers.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            if (Math.Abs(ray.Direction.Y) < MathHelpers.ZeroTolerance)
            {
                if (ray.Position.Y < box.Minimum.Y || ray.Position.Y > box.Maximum.Y)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.Y;
                float t1 = (box.Minimum.Y - ray.Position.Y) * inverse;
                float t2 = (box.Maximum.Y - ray.Position.Y) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = MathHelpers.Max(t1, distance);
                tmax = MathHelpers.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            if (Math.Abs(ray.Direction.Z) < MathHelpers.ZeroTolerance)
            {
                if (ray.Position.Z < box.Minimum.Z || ray.Position.Z > box.Maximum.Z)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.Z;
                float t1 = (box.Minimum.Z - ray.Position.Z) * inverse;
                float t2 = (box.Maximum.Z - ray.Position.Z) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = MathHelpers.Max(t1, distance);
                tmax = MathHelpers.Min(t2, tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Ray"/> and a <see cref="CryEngine.Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="CryEngine.Vec3"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsBox(ref Ray ray, ref BoundingBox box, out Vec3 point)
        {
            float distance;
            if (!RayIntersectsBox(ref ray, ref box, out distance))
            {
                point = Vec3.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Ray"/> and a <see cref="CryEngine.BoundingSphere"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection,
        /// or 0 if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref Ray ray, ref BoundingSphere sphere, out float distance)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 177

            Vec3 m = ray.Position - sphere.Center;

            float b = Vec3.Dot(m, ray.Direction);
            float c = Vec3.Dot(m, m) - (sphere.Radius * sphere.Radius);

            if (c > 0f && b > 0f)
            {
                distance = 0f;
                return false;
            }

            float discriminant = b * b - c;

            if (discriminant < 0f)
            {
                distance = 0f;
                return false;
            }

            distance = -b - (float)Math.Sqrt(discriminant);

            if (distance < 0f)
                distance = 0f;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Ray"/> and a <see cref="CryEngine.BoundingSphere"/>. 
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection,
        /// or <see cref="CryEngine.Vec3"/> if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool RayIntersectsSphere(ref Ray ray, ref BoundingSphere sphere, out Vec3 point)
        {
            float distance;
            if (!RayIntersectsSphere(ref ray, ref sphere, out distance))
            {
                point = Vec3.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsPoint(ref Plane plane, ref Vec3 point)
        {
            float distance = plane.Normal.Dot(point);
            distance += plane.D;

            if (distance > 0f)
                return PlaneIntersectionType.Front;

            if (distance < 0f)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Plane"/> and a <see cref="CryEngine.Plane"/>.
        /// </summary>
        /// <param name="plane1">The first plane to test.</param>
        /// <param name="plane2">The second plane to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool PlaneIntersectsPlane(ref Plane plane1, ref Plane plane2)
        {
            Vec3 direction = plane1.Normal.Cross(plane2.Normal);

            // If direction is the zero vector, the planes are parallel and possibly
            // coincident. It is not an intersection. The dot product will tell us.
            float denominator = direction.Dot(direction);

            if (Math.Abs(denominator) < MathHelpers.ZeroTolerance)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Plane"/> and a <see cref="CryEngine.Plane"/>.
        /// </summary>
        /// <param name="plane1">The first plane to test.</param>
        /// <param name="plane2">The second plane to test.</param>
        /// <param name="line">When the method completes, contains the line of intersection
        /// as a <see cref="CryEngine.Ray"/>, or a zero ray if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// Although a ray is set to have an origin, the ray returned by this method is really
        /// a line in three dimensions which has no real origin. The ray is considered valid when
        /// both the positive direction is used and when the negative direction is used.
        /// </remarks>
        public static bool PlaneIntersectsPlane(ref Plane plane1, ref Plane plane2, out Ray line)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 207

            Vec3 direction = plane1.Normal.Cross(plane2.Normal);

            // If direction is the zero vector, the planes are parallel and possibly
            // coincident. It is not an intersection. The dot product will tell us.
            float denominator = direction.Dot(direction);

            // We assume the planes are normalized, therefore the denominator
            // only serves as a parallel and coincident check. Otherwise we need
            // to deivide the point by the denominator.
            if (Math.Abs(denominator) < MathHelpers.ZeroTolerance)
            {
                line = new Ray();
                return false;
            }

            Vec3 temp = plane1.D * plane2.Normal - plane2.D * plane1.Normal;
            Vec3 point = temp.Cross(direction);

            line.Position = point;
            line.Direction = direction;
            line.Direction.Normalize();

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Plane"/> and a triangle.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triagnle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsTriangle(ref Plane plane, ref Vec3 vertex1, ref Vec3 vertex2, ref Vec3 vertex3)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 207

            PlaneIntersectionType test1 = PlaneIntersectsPoint(ref plane, ref vertex1);
            PlaneIntersectionType test2 = PlaneIntersectsPoint(ref plane, ref vertex2);
            PlaneIntersectionType test3 = PlaneIntersectsPoint(ref plane, ref vertex3);

            if (test1 == PlaneIntersectionType.Front && test2 == PlaneIntersectionType.Front && test3 == PlaneIntersectionType.Front)
                return PlaneIntersectionType.Front;

            if (test1 == PlaneIntersectionType.Back && test2 == PlaneIntersectionType.Back && test3 == PlaneIntersectionType.Back)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Plane"/> and a <see cref="CryEngine.BoundingBox"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="box">The box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsBox(ref Plane plane, ref BoundingBox box)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 161

            Vec3 min;
            Vec3 max;

            max.X = (plane.Normal.X >= 0.0f) ? box.Minimum.X : box.Maximum.X;
            max.Y = (plane.Normal.Y >= 0.0f) ? box.Minimum.Y : box.Maximum.Y;
            max.Z = (plane.Normal.Z >= 0.0f) ? box.Minimum.Z : box.Maximum.Z;
            min.X = (plane.Normal.X >= 0.0f) ? box.Maximum.X : box.Minimum.X;
            min.Y = (plane.Normal.Y >= 0.0f) ? box.Maximum.Y : box.Minimum.Y;
            min.Z = (plane.Normal.Z >= 0.0f) ? box.Maximum.Z : box.Minimum.Z;

            float distance = plane.Normal.Dot(max);

            if (distance + plane.D > 0.0f)
                return PlaneIntersectionType.Front;

            distance = Vec3.Dot(plane.Normal, min);

            if (distance + plane.D < 0.0f)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.Plane"/> and a <see cref="CryEngine.BoundingSphere"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static PlaneIntersectionType PlaneIntersectsSphere(ref Plane plane, ref BoundingSphere sphere)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 160

            float distance = plane.Normal.Dot(sphere.Center);
            distance += plane.D;

            if (distance > sphere.Radius)
                return PlaneIntersectionType.Front;

            if (distance < -sphere.Radius)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        /* This implentation is wrong
        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.BoundingBox"/> and a triangle.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triagnle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsTriangle(ref BoundingBox box, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            if (BoxContainsPoint(ref box, ref vertex1) == ContainmentType.Contains)
                return true;

            if (BoxContainsPoint(ref box, ref vertex2) == ContainmentType.Contains)
                return true;

            if (BoxContainsPoint(ref box, ref vertex3) == ContainmentType.Contains)
                return true;

            return false;
        }
        */

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.BoundingBox"/> and a <see cref="CryEngine.BoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            if (box1.Minimum.X > box2.Maximum.X || box2.Minimum.X > box1.Maximum.X)
                return false;

            if (box1.Minimum.Y > box2.Maximum.Y || box2.Minimum.Y > box1.Maximum.Y)
                return false;

            if (box1.Minimum.Z > box2.Maximum.Z || box2.Minimum.Z > box1.Maximum.Z)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.BoundingBox"/> and a <see cref="CryEngine.BoundingSphere"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool BoxIntersectsSphere(ref BoundingBox box, ref BoundingSphere sphere)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 166

            Vec3 vector;
            Vec3.Clamp(ref sphere.Center, ref box.Minimum, ref box.Maximum, out vector);
            float distance = sphere.Center.GetDistanceSquared(vector);

            return distance <= sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.BoundingSphere"/> and a triangle.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triagnle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool SphereIntersectsTriangle(ref BoundingSphere sphere, ref Vec3 vertex1, ref Vec3 vertex2, ref Vec3 vertex3)
        {
            // Source: Real-Time Collision Detection by Christer Ericson
            // Reference: Page 167

            Vec3 point;
            ClosestPointPointTriangle(ref sphere.Center, ref vertex1, ref vertex2, ref vertex3, out point);
            Vec3 v = point - sphere.Center;

            float dot = v.Dot(v);

            return dot <= sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Determines whether there is an intersection between a <see cref="CryEngine.BoundingSphere"/> and a <see cref="CryEngine.BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">First sphere to test.</param>
        /// <param name="sphere2">Second sphere to test.</param>
        /// <returns>Whether the two objects intersected.</returns>
        public static bool SphereIntersectsSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2)
        {
            float radiisum = sphere1.Radius + sphere2.Radius;
            return sphere1.Center.GetDistanceSquared(sphere2.Center) <= radiisum * radiisum;
        }

        /// <summary>
        /// Determines whether a <see cref="CryEngine.BoundingBox"/> contains a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static bool BoxContainsPoint(ref BoundingBox box, ref Vec3 point)
        {
            return (box.Minimum.X <= point.X && box.Maximum.X >= point.X &&
                box.Minimum.Y <= point.Y && box.Maximum.Y >= point.Y &&
                box.Minimum.Z <= point.Z && box.Maximum.Z >= point.Z);
        }

        /* This implentation is wrong
        /// <summary>
        /// Determines whether a <see cref="CryEngine.BoundingBox"/> contains a triangle.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triagnle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsTriangle(ref BoundingBox box, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
        {
            ContainmentType test1 = BoxContainsPoint(ref box, ref vertex1);
            ContainmentType test2 = BoxContainsPoint(ref box, ref vertex2);
            ContainmentType test3 = BoxContainsPoint(ref box, ref vertex3);

            if (test1 == ContainmentType.Contains && test2 == ContainmentType.Contains && test3 == ContainmentType.Contains)
                return ContainmentType.Contains;

            if (test1 == ContainmentType.Contains || test2 == ContainmentType.Contains || test3 == ContainmentType.Contains)
                return ContainmentType.Intersects;

            return ContainmentType.Disjoint;
        }
        */

        /// <summary>
        /// Determines whether a <see cref="CryEngine.BoundingBox"/> contains a <see cref="CryEngine.BoundingBox"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsBox(ref BoundingBox box1, ref BoundingBox box2)
        {
            if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
                return ContainmentType.Disjoint;

            if (box1.Maximum.Y < box2.Minimum.Y || box1.Minimum.Y > box2.Maximum.Y)
                return ContainmentType.Disjoint;

            if (box1.Maximum.Z < box2.Minimum.Z || box1.Minimum.Z > box2.Maximum.Z)
                return ContainmentType.Disjoint;

            if (box1.Minimum.X <= box2.Minimum.X && (box2.Maximum.X <= box1.Maximum.X &&
                box1.Minimum.Y <= box2.Minimum.Y && box2.Maximum.Y <= box1.Maximum.Y) &&
                box1.Minimum.Z <= box2.Minimum.Z && box2.Maximum.Z <= box1.Maximum.Z)
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines whether a <see cref="CryEngine.BoundingBox"/> contains a <see cref="CryEngine.BoundingSphere"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType BoxContainsSphere(ref BoundingBox box, ref BoundingSphere sphere)
        {
            Vec3 vector;
            Vec3.Clamp(ref sphere.Center, ref box.Minimum, ref box.Maximum, out vector);
            float distance = sphere.Center.GetDistanceSquared(vector);

            if (distance > sphere.Radius * sphere.Radius)
                return ContainmentType.Disjoint;

            if ((((box.Minimum.X + sphere.Radius <= sphere.Center.X) && (sphere.Center.X <= box.Maximum.X - sphere.Radius)) && ((box.Maximum.X - box.Minimum.X > sphere.Radius) &&
                (box.Minimum.Y + sphere.Radius <= sphere.Center.Y))) && (((sphere.Center.Y <= box.Maximum.Y - sphere.Radius) && (box.Maximum.Y - box.Minimum.Y > sphere.Radius)) &&
                (((box.Minimum.Z + sphere.Radius <= sphere.Center.Z) && (sphere.Center.Z <= box.Maximum.Z - sphere.Radius)) && (box.Maximum.X - box.Minimum.X > sphere.Radius))))
            {
                return ContainmentType.Contains;
            }

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines whether a <see cref="CryEngine.BoundingSphere"/> contains a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static bool SphereContainsPoint(ref BoundingSphere sphere, ref Vec3 point)
        {
            return (point.GetDistanceSquared(sphere.Center) <= sphere.Radius * sphere.Radius);
        }

        /// <summary>
        /// Determines whether a <see cref="CryEngine.BoundingSphere"/> contains a triangle.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="vertex1">The first vertex of the triangle to test.</param>
        /// <param name="vertex2">The second vertex of the triagnle to test.</param>
        /// <param name="vertex3">The third vertex of the triangle to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsTriangle(ref BoundingSphere sphere, ref Vec3 vertex1, ref Vec3 vertex2, ref Vec3 vertex3)
        {
            // Source: Jorgy343
            // Reference: None

            if (SphereContainsPoint(ref sphere, ref vertex1) && SphereContainsPoint(ref sphere, ref vertex2) && SphereContainsPoint(ref sphere, ref vertex3))
                return ContainmentType.Contains;

            if (SphereIntersectsTriangle(ref sphere, ref vertex1, ref vertex2, ref vertex3))
                return ContainmentType.Intersects;

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines whether a <see cref="CryEngine.BoundingSphere"/> contains a <see cref="CryEngine.BoundingBox"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="box">The box to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsBox(ref BoundingSphere sphere, ref BoundingBox box)
        {
            Vec3 vector;

            if (!BoxIntersectsSphere(ref box, ref sphere))
                return ContainmentType.Disjoint;

            float radiussquared = sphere.Radius * sphere.Radius;
            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Maximum.Z;

            if (vector.LengthSquared > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Maximum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Maximum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared > radiussquared)
                return ContainmentType.Intersects;

            vector.X = sphere.Center.X - box.Minimum.X;
            vector.Y = sphere.Center.Y - box.Minimum.Y;
            vector.Z = sphere.Center.Z - box.Minimum.Z;

            if (vector.LengthSquared > radiussquared)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines whether a <see cref="CryEngine.BoundingSphere"/> contains a <see cref="CryEngine.BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <returns>The type of containment the two objects have.</returns>
        public static ContainmentType SphereContainsSphere(ref BoundingSphere sphere1, ref BoundingSphere sphere2)
        {
            float distance = sphere1.Center.GetDistance(sphere2.Center);

            if (sphere1.Radius + sphere2.Radius < distance)
                return ContainmentType.Disjoint;

            if (sphere1.Radius - sphere2.Radius < distance)
                return ContainmentType.Intersects;

            return ContainmentType.Contains;
        }
    }
}