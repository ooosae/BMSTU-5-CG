using CourseCG.Models;
using System;

namespace CourseCG.Services
{
    public static class IntersectionService
    {
        public static void ClosestIntersection(Scene scene, double[] camera, double[] direction, double tMin, double tMax, ref double closestT, ref Sphere closestSphere)
        {
            foreach (var sphere in scene.Spheres)
            {
                double t1, t2;
                IntersectRaySphere(camera, direction, sphere, out t1, out t2);
                if (tMin <= t1 && t1 <= tMax && t1 < closestT)
                {
                    closestT = t1;
                    closestSphere = sphere;
                }
                if (tMin <= t2 && t2 <= tMax && t2 < closestT)
                {
                    closestT = t2;
                    closestSphere = sphere;
                }
            }
        }

        public static void IntersectRaySphere(double[] camera, double[] direction, Sphere sphere, out double t1, out double t2)
        {
            double r = sphere.Radius;
            double cx = camera[0] - sphere.XCenter;
            double cy = camera[1] - sphere.YCenter;
            double cz = camera[2] - sphere.ZCenter;

            double a = direction[0] * direction[0] + direction[1] * direction[1] + direction[2] * direction[2];
            double b = 2 * (cx * direction[0] + cy * direction[1] + cz * direction[2]);
            double c = cx * cx + cy * cy + cz * cz - r * r;

            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                t1 = double.PositiveInfinity;
                t2 = double.PositiveInfinity;
            }
            else
            {
                t1 = (-b + Math.Sqrt(discriminant)) / (2.0 * a);
                t2 = (-b - Math.Sqrt(discriminant)) / (2.0 * a);
            }
        }

        public static void ReflectRay(double[] direction, double[] normal, double[] reflected)
        {
            double dotProduct = DotProduct(direction, normal);
            reflected[0] = 2 * normal[0] * dotProduct - direction[0];
            reflected[1] = 2 * normal[1] * dotProduct - direction[1];
            reflected[2] = 2 * normal[2] * dotProduct - direction[2];
        }

        public static void NormalizeVector(double[] vector)
        {
            double length = Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1] + vector[2] * vector[2]);
            if (length > 1e-6)
            {
                vector[0] /= length;
                vector[1] /= length;
                vector[2] /= length;
            }
        }


        public static double DotProduct(double[] v1, double[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }
    }
}
