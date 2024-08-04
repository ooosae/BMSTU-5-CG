using CourseCG.Models;

namespace CourseCG.Services
{
    public static class IntersectionService
    {
        public static void ClosestIntersection(Scene scene, Vector3 camera, Vector3 direction, double tMin, double tMax, out double closestT, out Sphere? closestSphere)
        {
            closestT = double.PositiveInfinity;
            closestSphere = null;

            foreach (var sphere in scene.Spheres)
            {
                IntersectRaySphere(camera, direction, sphere, out double t1, out double t2);

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

        private static void IntersectRaySphere(Vector3 camera, Vector3 direction, Sphere sphere, out double t1, out double t2)
        {
            Vector3 oc = camera - sphere.Center;
            double a = Vector3.DotProduct(direction, direction);
            double b = 2.0 * Vector3.DotProduct(oc, direction);
            double c = Vector3.DotProduct(oc, oc) - sphere.Radius * sphere.Radius;
            double discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                t1 = t2 = double.PositiveInfinity;
            }
            else
            {
                double sqrtDiscriminant = Math.Sqrt(discriminant);
                t1 = (-b - sqrtDiscriminant) / (2.0 * a);
                t2 = (-b + sqrtDiscriminant) / (2.0 * a);
            }
        }

        public static Vector3 ReflectRay(Vector3 direction, Vector3 normal)
        {
            double dotProduct = Vector3.DotProduct(direction, normal);
            return  normal * (2 * dotProduct) - direction;
        }
    }
}
