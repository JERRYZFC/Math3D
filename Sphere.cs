﻿// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Ara3D
{
    /// <summary>
    /// Describes a sphere in 3D-space for bounding operations.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Sphere : IEquatable<Sphere>
    {
        // TODO: convert this to use a Vector4
        #region Public Fields

        /// <summary>
        /// The sphere center.
        /// </summary>
        [DataMember]
        public Vector3 Center;

        /// <summary>
        /// The sphere radius.
        /// </summary>
        [DataMember]
        public float Radius;

        #endregion

        #region Internal Properties

        internal string DebugDisplayString => string.Concat(
                    "Center( ", Center.ToString(), " )  \r\n",
                    "Radius( ", Radius.ToString(), " )"
                    );

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a bounding sphere with the specified center and radius.  
        /// </summary>
        /// <param name="center">The sphere center.</param>
        /// <param name="radius">The sphere radius.</param>
        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        #endregion

        #region Public Methods

        #region Contains

        /// <summary>
        /// Test if a bounding box is fully inside, outside, or just intersecting the sphere.
        /// </summary>
        /// <param name="box">The box for testing.</param>
        /// <returns>The containment type.</returns>
        public ContainmentType Contains(Box box)
        {
            //check if all corner is in sphere
            bool inside = true;
            foreach (Vector3 corner in box.GetCorners())
            {
                if (Contains(corner) == ContainmentType.Disjoint)
                {
                    inside = false;
                    break;
                }
            }

            if (inside)
                return ContainmentType.Contains;

            //check if the distance from sphere center to cube face < radius
            double dmin = 0;

            if (Center.X < box.Min.X)
				dmin += (Center.X - box.Min.X) * (Center.X - box.Min.X);

			else if (Center.X > box.Max.X)
					dmin += (Center.X - box.Max.X) * (Center.X - box.Max.X);

			if (Center.Y < box.Min.Y)
				dmin += (Center.Y - box.Min.Y) * (Center.Y - box.Min.Y);

			else if (Center.Y > box.Max.Y)
				dmin += (Center.Y - box.Max.Y) * (Center.Y - box.Max.Y);

			if (Center.Z < box.Min.Z)
				dmin += (Center.Z - box.Min.Z) * (Center.Z - box.Min.Z);

			else if (Center.Z > box.Max.Z)
				dmin += (Center.Z - box.Max.Z) * (Center.Z - box.Max.Z);

			if (dmin <= Radius * Radius) 
				return ContainmentType.Intersects;
            
            //else disjoint
            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Test if a bounding box is fully inside, outside, or just intersecting the sphere.
        /// </summary>
        /// <param name="box">The box for testing.</param>
        /// <param name="result">The containment type as an output parameter.</param>
        public void Contains(ref Box box, out ContainmentType result)
        {
            result = Contains(box);
        }

        /// <summary>
        /// Test if a frustum is fully inside, outside, or just intersecting the sphere.
        /// </summary>
        /// <param name="frustum">The frustum for testing.</param>
        /// <returns>The containment type.</returns>
        public ContainmentType Contains(Frustum frustum)
        {
            //check if all corner is in sphere
            bool inside = true;

            Vector3[] corners = frustum.GetCorners();
            foreach (Vector3 corner in corners)
            {
                if (Contains(corner) == ContainmentType.Disjoint)
                {
                    inside = false;
                    break;
                }
            }
            if (inside)
                return ContainmentType.Contains;

            //check if the distance from sphere center to frustrum face < radius
            double dmin = 0;
            //TODO : calcul dmin

            if (dmin <= Radius * Radius)
                return ContainmentType.Intersects;

            //else disjoint
            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Test if a frustum is fully inside, outside, or just intersecting the sphere.
        /// </summary>
        /// <param name="frustum">The frustum for testing.</param>
        /// <param name="result">The containment type as an output parameter.</param>
        public void Contains(ref Frustum frustum,out ContainmentType result)
        {
            result = Contains(frustum);
        }

        /// <summary>
        /// Test if a sphere is fully inside, outside, or just intersecting the sphere.
        /// </summary>
        /// <param name="sphere">The other sphere for testing.</param>
        /// <returns>The containment type.</returns>
        public ContainmentType Contains(Sphere sphere)
        {
            Contains(ref sphere, out ContainmentType result);
            return result;
        }

        /// <summary>
        /// Test if a sphere is fully inside, outside, or just intersecting the sphere.
        /// </summary>
        /// <param name="sphere">The other sphere for testing.</param>
        /// <param name="result">The containment type as an output parameter.</param>
        public void Contains(ref Sphere sphere, out ContainmentType result)
        {
            float sqDistance = Vector3.DistanceSquared(sphere.Center, Center);

            if (sqDistance > (sphere.Radius + Radius) * (sphere.Radius + Radius))
                result = ContainmentType.Disjoint;

            else if (sqDistance <= (Radius - sphere.Radius) * (Radius - sphere.Radius))
                result = ContainmentType.Contains;

            else
                result = ContainmentType.Intersects;
        }

        /// <summary>
        /// Test if a point is fully inside, outside, or just intersecting the sphere.
        /// </summary>
        /// <param name="point">The vector in 3D-space for testing.</param>
        /// <returns>The containment type.</returns>
        public ContainmentType Contains(Vector3 point)
        {
            Contains(ref point, out ContainmentType result);
            return result;
        }

        /// <summary>
        /// Test if a point is fully inside, outside, or just intersecting the sphere.
        /// </summary>
        /// <param name="point">The vector in 3D-space for testing.</param>
        /// <param name="result">The containment type as an output parameter.</param>
        public void Contains(ref Vector3 point, out ContainmentType result)
        {
            float sqRadius = Radius * Radius;
            float sqDistance = Vector3.DistanceSquared(point, Center);
            
            if (sqDistance > sqRadius)
                result = ContainmentType.Disjoint;

            else if (sqDistance < sqRadius)
                result = ContainmentType.Contains;

            else 
                result = ContainmentType.Intersects;
        }

        #endregion

        #region CreateFromBoundingBox

        /// <summary>
        /// Creates the smallest <see cref="Sphere"/> that can contain a specified <see cref="Box"/>.
        /// </summary>
        /// <param name="box">The box to create the sphere from.</param>
        /// <returns>The new <see cref="Sphere"/>.</returns>
        public static Sphere CreateFromBoundingBox(Box box)
        {
            CreateFromBoundingBox(ref box, out Sphere result);
            return result;
        }

        /// <summary>
        /// Creates the smallest <see cref="Sphere"/> that can contain a specified <see cref="Box"/>.
        /// </summary>
        /// <param name="box">The box to create the sphere from.</param>
        /// <param name="result">The new <see cref="Sphere"/> as an output parameter.</param>
        public static void CreateFromBoundingBox(ref Box box, out Sphere result)
        {
            // Find the center of the box.
            Vector3 center = new Vector3((box.Min.X + box.Max.X) / 2.0f,
                                         (box.Min.Y + box.Max.Y) / 2.0f,
                                         (box.Min.Z + box.Max.Z) / 2.0f);

            // Find the distance between the center and one of the corners of the box.
            float radius = Vector3.Distance(center, box.Max);

            result = new Sphere(center, radius);
        }

        #endregion

        /// <summary>
        /// Creates the smallest <see cref="Sphere"/> that can contain a specified <see cref="Frustum"/>.
        /// </summary>
        /// <param name="frustum">The frustum to create the sphere from.</param>
        /// <returns>The new <see cref="Sphere"/>.</returns>
        public static Sphere CreateFromFrustum(Frustum frustum)
        {
            return CreateFromPoints(frustum.GetCorners());
        }

        /// <summary>
        /// Creates the smallest <see cref="Sphere"/> that can contain a specified list of points in 3D-space. 
        /// </summary>
        /// <param name="points">List of point to create the sphere from.</param>
        /// <returns>The new <see cref="Sphere"/>.</returns>
        public static Sphere CreateFromPoints(IEnumerable<Vector3> points)
        {
            if (points == null )
                throw new ArgumentNullException(nameof(points));

            // From "Real-Time Collision Detection" (Page 89)

            var minx = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var maxx = -minx;
            var miny = minx;
            var maxy = -minx;
            var minz = minx;
            var maxz = -minx;

            // Find the most extreme points along the principle axis.
            var numPoints = 0;           
            foreach (var pt in points)
            {
                ++numPoints;

                if (pt.X < minx.X) 
                    minx = pt;
                if (pt.X > maxx.X) 
                    maxx = pt;
                if (pt.Y < miny.Y) 
                    miny = pt;
                if (pt.Y > maxy.Y) 
                    maxy = pt;
                if (pt.Z < minz.Z) 
                    minz = pt;
                if (pt.Z > maxz.Z) 
                    maxz = pt;
            }

            if (numPoints == 0)
                throw new ArgumentException("You should have at least one point in points.");

            var sqDistX = Vector3.DistanceSquared(maxx, minx);
            var sqDistY = Vector3.DistanceSquared(maxy, miny);
            var sqDistZ = Vector3.DistanceSquared(maxz, minz);

            // Pick the pair of most distant points.
            var min = minx;
            var max = maxx;
            if (sqDistY > sqDistX && sqDistY > sqDistZ) 
            {
                max = maxy;
                min = miny;
            }
            if (sqDistZ > sqDistX && sqDistZ > sqDistY) 
            {
                max = maxz;
                min = minz;
            }
            
            var center = (min + max) * 0.5f;
            var radius = Vector3.Distance(max, center);
            
            // Test every point and expand the sphere.
            // The current bounding sphere is just a good approximation and may not enclose all points.            
            // From: Mathematics for 3D Game Programming and Computer Graphics, Eric Lengyel, Third Edition.
            // Page 218
            float sqRadius = radius * radius;
            foreach (var pt in points)
            {
                Vector3 diff = (pt-center);
                float sqDist = diff.LengthSquared();
                if (sqDist > sqRadius)
                {
                    float distance = (float)Math.Sqrt(sqDist); // equal to diff.Length();
                    Vector3 direction = diff / distance;
                    Vector3 G = center - radius * direction;
                    center = (G + pt) / 2;
                    radius = Vector3.Distance(pt, center);
                    sqRadius = radius * radius;
                }
            }

            return new Sphere(center, radius);
        }

        /// <summary>
        /// Creates the smallest <see cref="Sphere"/> that can contain two spheres.
        /// </summary>
        /// <param name="original">First sphere.</param>
        /// <param name="additional">Second sphere.</param>
        /// <returns>The new <see cref="Sphere"/>.</returns>
        public static Sphere CreateMerged(Sphere original, Sphere additional)
        {
            CreateMerged(ref original, ref additional, out Sphere result);
            return result;
        }

        /// <summary>
        /// Creates the smallest <see cref="Sphere"/> that can contain two spheres.
        /// </summary>
        /// <param name="original">First sphere.</param>
        /// <param name="additional">Second sphere.</param>
        /// <param name="result">The new <see cref="Sphere"/> as an output parameter.</param>
        public static void CreateMerged(ref Sphere original, ref Sphere additional, out Sphere result)
        {
            Vector3 ocenterToaCenter = Vector3.Subtract(additional.Center, original.Center);
            float distance = ocenterToaCenter.Length();
            if (distance <= original.Radius + additional.Radius)//intersect
            {
                if (distance <= original.Radius - additional.Radius)//original contain additional
                {
                    result = original;
                    return;
                }
                if (distance <= additional.Radius - original.Radius)//additional contain original
                {
                    result = additional;
                    return;
                }
            }
            //else find center of new sphere and radius
            float leftRadius = Math.Max(original.Radius - distance, additional.Radius);
            float Rightradius = Math.Max(original.Radius + distance, additional.Radius);
            ocenterToaCenter = ocenterToaCenter + (((leftRadius - Rightradius) / (2 * ocenterToaCenter.Length())) * ocenterToaCenter);//oCenterToResultCenter

            result = new Sphere
            {
                Center = original.Center + ocenterToaCenter,
                Radius = (leftRadius + Rightradius) / 2
            };
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Sphere"/>.
        /// </summary>
        /// <param name="other">The <see cref="Sphere"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Sphere other)
        {
            return Center == other.Center && Radius == other.Radius;
        }

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Sphere)
                return Equals((Sphere)obj);

            return false;
        }

        /// <summary>
        /// Gets the hash code of this <see cref="Sphere"/>.
        /// </summary>
        /// <returns>Hash code of this <see cref="Sphere"/>.</returns>
        public override int GetHashCode()
        {
            return Center.GetHashCode() + Radius.GetHashCode();
        }

        #region Intersects

        /// <summary>
        /// Gets whether or not a specified <see cref="Box"/> intersects with this sphere.
        /// </summary>
        /// <param name="box">The box for testing.</param>
        /// <returns><c>true</c> if <see cref="Box"/> intersects with this sphere; <c>false</c> otherwise.</returns>
        public bool Intersects(Box box)
        {
			return box.Intersects(this);
        }

        /// <summary>
        /// Gets whether or not a specified <see cref="Box"/> intersects with this sphere.
        /// </summary>
        /// <param name="box">The box for testing.</param>
        /// <param name="result"><c>true</c> if <see cref="Box"/> intersects with this sphere; <c>false</c> otherwise. As an output parameter.</param>
        public void Intersects(ref Box box, out bool result)
        {
            box.Intersects(ref this, out result);
        }

        /*
        TODO : Make the public bool Intersects(BoundingFrustum frustum) overload

        public bool Intersects(BoundingFrustum frustum)
        {
            if (frustum == null)
                throw new NullReferenceException();

            throw new NotImplementedException();
        }

        */

        /// <summary>
        /// Gets whether or not the other <see cref="Sphere"/> intersects with this sphere.
        /// </summary>
        /// <param name="sphere">The other sphere for testing.</param>
        /// <returns><c>true</c> if other <see cref="Sphere"/> intersects with this sphere; <c>false</c> otherwise.</returns>
        public bool Intersects(Sphere sphere)
        {
            Intersects(ref sphere, out bool result);
            return result;
        }

        /// <summary>
        /// Gets whether or not the other <see cref="Sphere"/> intersects with this sphere.
        /// </summary>
        /// <param name="sphere">The other sphere for testing.</param>
        /// <param name="result"><c>true</c> if other <see cref="Sphere"/> intersects with this sphere; <c>false</c> otherwise. As an output parameter.</param>
        public void Intersects(ref Sphere sphere, out bool result)
        {
            float sqDistance = Vector3.DistanceSquared(sphere.Center, Center);

            if (sqDistance > (sphere.Radius + Radius) * (sphere.Radius + Radius))
                result = false;
            else
                result = true;
        }

        /// <summary>
        /// Gets whether or not a specified <see cref="Plane"/> intersects with this sphere.
        /// </summary>
        /// <param name="plane">The plane for testing.</param>
        /// <returns>Type of intersection.</returns>
        public PlaneIntersectionType Intersects(Plane plane)
        {
            // TODO: we might want to inline this for performance reasons
            Intersects(ref plane, out PlaneIntersectionType result);
            return result;
        }

        /// <summary>
        /// Gets whether or not a specified <see cref="Plane"/> intersects with this sphere.
        /// </summary>
        /// <param name="plane">The plane for testing.</param>
        /// <param name="result">Type of intersection as an output parameter.</param>
        public void Intersects(ref Plane plane, out PlaneIntersectionType result)
        {
            var distance = Vector3.Dot(plane.Normal, Center);
            distance += plane.D;
            if (distance > Radius)
                result = PlaneIntersectionType.Front;
            else if (distance < -Radius)
                result = PlaneIntersectionType.Back;
            else
                result = PlaneIntersectionType.Intersecting;
        }

        /// <summary>
        /// Gets whether or not a specified <see cref="Ray"/> intersects with this sphere.
        /// </summary>
        /// <param name="ray">The ray for testing.</param>
        /// <returns>Distance of ray intersection or <c>null</c> if there is no intersection.</returns>
        public float? Intersects(Ray ray)
        {
            return ray.Intersects(this);
        }

        /// <summary>
        /// Gets whether or not a specified <see cref="Ray"/> intersects with this sphere.
        /// </summary>
        /// <param name="ray">The ray for testing.</param>
        /// <param name="result">Distance of ray intersection or <c>null</c> if there is no intersection as an output parameter.</param>
        public void Intersects(ref Ray ray, out float? result)
        {
            ray.Intersects(ref this, out result);
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Sphere"/> in the format:
        /// {Center:[<see cref="Center"/>] Radius:[<see cref="Radius"/>]}
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Sphere"/>.</returns>
        public override string ToString()
        {
            return "{Center:" + Center + " Radius:" + Radius + "}";
        }

        #region Transform

        /// <summary>
        /// Creates a new <see cref="Sphere"/> that contains a transformation of translation and scale from this sphere by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <returns>Transformed <see cref="Sphere"/>.</returns>
        public Sphere Transform(Matrix4x4 matrix)
        {
            Sphere sphere = new Sphere
            {
                Center = Vector3.Transform(Center, matrix),
                Radius = Radius * ((float)Math.Sqrt((double)Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13), Math.Max(((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23), ((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33)))))
            };
            return sphere;
        }

        /// <summary>
        /// Creates a new <see cref="Sphere"/> that contains a transformation of translation and scale from this sphere by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="result">Transformed <see cref="Sphere"/> as an output parameter.</param>
        public void Transform(ref Matrix4x4 matrix, out Sphere result)
        {
            result.Center = Vector3.Transform(Center, matrix);
            result.Radius = Radius * ((float)Math.Sqrt((double)Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13), Math.Max(((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23), ((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33)))));
        }

        #endregion

        /// <summary>
        /// Deconstruction method for <see cref="Sphere"/>.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void Deconstruct(out Vector3 center, out float radius)
        {
            center = Center;
            radius = Radius;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Compares whether two <see cref="Sphere"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="Sphere"/> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="Sphere"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator == (Sphere a, Sphere b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Compares whether two <see cref="Sphere"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="Sphere"/> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="Sphere"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
        public static bool operator != (Sphere a, Sphere b)
        {
            return !a.Equals(b);
        }

        #endregion
    }
}
