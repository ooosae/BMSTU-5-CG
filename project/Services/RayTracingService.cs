using CourseCG.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CourseCG.Services
{
    public static class RayTracingService
    {
        public static async Task<Color> TraceRayAsync(Scene scene, Vector3 camera, Vector3 direction, double tMin, double tMax, int recursionDepth)
        {
            IntersectionService.ClosestIntersection(scene, camera, direction, tMin, tMax, out double closestT, out Sphere? closestSphere);

            if (closestSphere == null)
            {
                return Colors.LightGray;
            }

            Vector3 intersectionPoint = camera + direction * closestT;
            Vector3 normal = intersectionPoint - closestSphere.Center;
            normal.Normalize();

            if (closestSphere.Texture != null)
            {
                Color textureColor = await GetColorFromTextureAsync(closestSphere, normal);
                normal = AdjustNormalWithTexture(closestSphere.Texture, normal, textureColor);
                normal.Normalize();
            }

            Vector3 viewDirection = -direction;
            double intensity = LightService.ComputeLighting(scene, intersectionPoint, normal, viewDirection, closestSphere.Specular);
            Color localColor = AdjustIntensity(closestSphere.Color, intensity);

            double reflectivity = closestSphere.Reflective;
            if (recursionDepth <= 0 || reflectivity <= 0)
            {
                return localColor;
            }

            Vector3 reflectionDirection = IntersectionService.ReflectRay(viewDirection, normal);
            Color reflectionColor = await TraceRayAsync(scene, intersectionPoint, reflectionDirection, 0.001, double.PositiveInfinity, recursionDepth - 1);
            Color finalColor = AdjustReflection(localColor, reflectionColor, reflectivity);

            return finalColor;
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

        private static async Task<Color> GetColorFromTextureAsync(Sphere sphere, Vector3 normal)
        {
            double u = 0;
            double v = 0;

            ComputeTextureCoordinates(normal, ref u, ref v);

            int x = (int)(u * sphere.Texture?.PixelWidth ?? 0);
            int y = (int)(v * sphere.Texture?.PixelHeight ?? 0);

            var pixels = new byte[4];

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                sphere.Texture?.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);
            });

            return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
        }

        private static void ComputeTextureCoordinates(Vector3 normal, ref double u, ref double v)
        {
            double phi = Math.Atan2(normal.Y, normal.X);
            double theta = Math.Acos(normal.Z);

            u = (phi + Math.PI) / (2.0 * Math.PI);
            v = theta / Math.PI;
        }

        private static Vector3 AdjustNormalWithTexture(BitmapSource texture, Vector3 normal, Color textureColor)
        {
            double Bu = textureColor.R / 255.0;
            double Bv = textureColor.G / 255.0;
            Vector3 Nb = new Vector3(Bu, Bv, 1);
            Nb.Normalize();

            Vector3 T = Vector3.CrossProduct(normal, Nb);
            T.Normalize();

            Vector3 B = Vector3.CrossProduct(normal, T);
            B.Normalize();

            Vector3 Nt = new Vector3(
                Vector3.DotProduct(T, Nb),
                Vector3.DotProduct(B, Nb),
                Vector3.DotProduct(normal, Nb));

            return normal + Nt;
        }
    }
}
