using CourseCG.Models;
using System.Diagnostics;
using System.Windows.Media;

namespace CourseCG.Services
{
    public static class RayTracingService
    {
        public static Color TraceRay(Scene scene, double[] camera, double[] direction, double tMin, double tMax, int recursionDepth)
        {
            Console.WriteLine("TraceRay called");
            double closestT = double.PositiveInfinity;
            Sphere closestSphere = null;
            IntersectionService.ClosestIntersection(scene, camera, direction, tMin, tMax, ref closestT, ref closestSphere);
            if (closestSphere == null)
            {
                Debug.WriteLine("No intersection found");
                return Colors.LightGray;
            }
            else
            {
                Console.WriteLine($"Intersection found at t = {closestT}");
            }

            double[] intersectionPoint = { camera[0] + closestT * direction[0], camera[1] + closestT * direction[1], camera[2] + closestT * direction[2] };
            double[] normal = { intersectionPoint[0] - closestSphere.XCenter, intersectionPoint[1] - closestSphere.YCenter, intersectionPoint[2] - closestSphere.ZCenter };
            IntersectionService.NormalizeVector(normal);

            double[] viewDirection = { -direction[0], -direction[1], -direction[2] };
            double intensity = LightService.ComputeLighting(scene, intersectionPoint, normal, viewDirection, closestSphere.Specular);
            Color localColor = AdjustIntensity(closestSphere.Color, intensity);

            double reflectivity = closestSphere.Reflective;
            if (recursionDepth <= 0 || reflectivity <= 0)
                return localColor;

            double[] reflectionDirection = new double[3];
            IntersectionService.ReflectRay(viewDirection, normal, reflectionDirection);
            Color reflectionColor = TraceRay(scene, intersectionPoint, reflectionDirection, 0.001, double.PositiveInfinity, recursionDepth - 1);
            return AdjustReflection(localColor, reflectionColor, reflectivity);
        }

        private static Color AdjustIntensity(Color color, double intensity)
        {
            byte r = (byte)(color.R * intensity);
            byte g = (byte)(color.G * intensity);
            byte b = (byte)(color.B * intensity);
            return Color.FromRgb(r, g, b);
        }

        private static Color AdjustReflection(Color localColor, Color reflectionColor, double reflectivity)
        {
            byte r = (byte)(localColor.R * (1 - reflectivity) + reflectionColor.R * reflectivity);
            byte g = (byte)(localColor.G * (1 - reflectivity) + reflectionColor.G * reflectivity);
            byte b = (byte)(localColor.B * (1 - reflectivity) + reflectionColor.B * reflectivity);
            return Color.FromRgb(r, g, b);
        }
    }
}
