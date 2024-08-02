using CourseCG.Models;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CourseCG.Services
{
    public static class RayTracingService
    {
        public static async Task<Color> TraceRayAsync(Scene scene, double[] camera, double[] direction, double tMin, double tMax, int recursionDepth)
        {
            double closestT = double.PositiveInfinity;
            Sphere? closestSphere = null;
            IntersectionService.ClosestIntersection(scene, camera, direction, tMin, tMax, ref closestT, ref closestSphere);
            if (closestSphere == null)
            {
                return Colors.LightGray;
            }

            double[] intersectionPoint = { camera[0] + closestT * direction[0], camera[1] + closestT * direction[1], camera[2] + closestT * direction[2] };
            double[] normal = { intersectionPoint[0] - closestSphere.XCenter, intersectionPoint[1] - closestSphere.YCenter, intersectionPoint[2] - closestSphere.ZCenter };
            IntersectionService.NormalizeVector(normal);

            if (closestSphere.Texture != null)
            {
                Color textureColor = await GetColorFromTextureAsync(closestSphere, normal);
                normal = AdjustNormalWithTexture(closestSphere.Texture, normal, textureColor);
                IntersectionService.NormalizeVector(normal);
            }

            double[] viewDirection = { -direction[0], -direction[1], -direction[2] };
            double intensity = LightService.ComputeLighting(scene, intersectionPoint, normal, viewDirection, closestSphere.Specular);
            Color localColor = AdjustIntensity(closestSphere.Color, intensity);

            double reflectivity = closestSphere.Reflective;
            if (recursionDepth <= 0 || reflectivity <= 0)
            {
                return localColor;
            }

            double[] reflectionDirection = new double[3];
            IntersectionService.ReflectRay(viewDirection, normal, reflectionDirection);

            Color reflectionColor = await TraceRayAsync(scene, intersectionPoint, reflectionDirection, 0.001, double.PositiveInfinity, recursionDepth - 1);
            Color finalColor = AdjustReflection(localColor, reflectionColor, reflectivity);

            return finalColor;
        }

        private static async Task<Color> GetColorFromTextureAsync(Sphere sphere, double[] normal)
        {
            double u = 0;
            double v = 0;

            ComputeTextureCoordinates(normal, ref u, ref v);

            int x = (int)(u * sphere.Texture?.PixelWidth??0);
            int y = (int)(v * sphere.Texture?.PixelHeight??0);

            var pixels = new byte[4];

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                sphere.Texture?.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);
            });

            return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
        }

        private static Color AdjustIntensity(Color color, double intensity)
        {
            byte r = (byte)Math.Min(255, Math.Max(0, color.R * intensity));
            byte g = (byte)Math.Min(255, Math.Max(0, color.G * intensity));
            byte b = (byte)Math.Min(255, Math.Max(0, color.B * intensity));
            return Color.FromRgb(r, g, b);
        }

        private static Color AdjustReflection(Color localColor, Color reflectionColor, double reflectivity)
        {
            byte r = (byte)Math.Min(255, Math.Max(0, localColor.R * (1 - reflectivity) + reflectionColor.R * reflectivity));
            byte g = (byte)Math.Min(255, Math.Max(0, localColor.G * (1 - reflectivity) + reflectionColor.G * reflectivity));
            byte b = (byte)Math.Min(255, Math.Max(0, localColor.B * (1 - reflectivity) + reflectionColor.B * reflectivity));
            return Color.FromRgb(r, g, b);
        }

        private static void ComputeTextureCoordinates(double[] normal, ref double u, ref double v)
        {
            double phi = Math.Atan2(normal[1], normal[0]);
            double theta = Math.Acos(normal[2]);

            u = (phi + Math.PI) / (2.0 * Math.PI);
            v = theta / Math.PI;
        }

        private static double[] AdjustNormalWithTexture(BitmapSource texture, double[] normal, Color textureColor)
        {
            double Bu = textureColor.R / 255.0;
            double Bv = textureColor.G / 255.0;
            double[] Nb = { Bu, Bv, 1 };
            IntersectionService.NormalizeVector(Nb);

            double[] T = new double[3];
            IntersectionService.Cross(normal, Nb, ref T);
            IntersectionService.NormalizeVector(T);

            double[] B = new double[3];
            IntersectionService.Cross(normal, T, ref B);
            IntersectionService.NormalizeVector(B);

            double[] Nt = { IntersectionService.DotProduct(T, Nb), IntersectionService.DotProduct(B, Nb), IntersectionService.DotProduct(normal, Nb) };
            return new double[] { normal[0] + Nt[0], normal[1] + Nt[1], normal[2] + Nt[2] };
        }
    }
}
