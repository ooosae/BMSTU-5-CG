using CourseCG.Models;
using System;
using System.Diagnostics;
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
                return Colors.LightBlue;
            }

            Vector3 intersectionPoint = camera + direction * closestT;
            Vector3 normal = intersectionPoint - closestSphere.Center;
            normal.Normalize();

            if (closestSphere.Texture != null)
            {
   
                switch (closestSphere.TextureType)
                {
                    case TextureType.HeightMap:
                        normal = AdjustNormalWithHeightMap(closestSphere.Texture, normal, intersectionPoint);
                        break;
                    case TextureType.NormalMap:
                        Color textureColor = await GetColorFromTextureAsync(closestSphere, normal);
                        normal = AdjustNormalWithNormalMap(closestSphere.Texture, normal, textureColor);
                        break;
                    case TextureType.ParallaxMap:
                        normal = AdjustNormalWithParallaxMap(closestSphere.Texture, normal, intersectionPoint);
                        break;
                }
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

        private static Vector3 AdjustNormalWithHeightMap(BitmapSource heightMap, Vector3 normal, Vector3 intersectionPoint, double heightMapIntensity = 10.0)
        {
            double u = 0;
            double v = 0;
            ComputeTextureCoordinates(normal, ref u, ref v);

            int width = heightMap.PixelWidth;
            int height = heightMap.PixelHeight;

            int x = (int)(u * width);
            int y = (int)(v * height);

            x = Math.Max(0, Math.Min(width - 1, x));
            y = Math.Max(0, Math.Min(height - 1, y));

            byte[] currentPixel = new byte[4];
            heightMap.CopyPixels(new Int32Rect(x, y, 1, 1), currentPixel, 4, 0);

            double h = currentPixel[0] / 255.0;

            Vector3 gradient = new Vector3(0, 0, 1.0);

            if (x > 0 && x < width - 1)
            {
                byte[] leftPixel = new byte[4];
                byte[] rightPixel = new byte[4];

                heightMap.CopyPixels(new Int32Rect(x - 1, y, 1, 1), leftPixel, 4, 0);
                heightMap.CopyPixels(new Int32Rect(x + 1, y, 1, 1), rightPixel, 4, 0);

                double hLeft = leftPixel[0] / 255.0;
                double hRight = rightPixel[0] / 255.0;

                gradient.X = (hRight - hLeft) * heightMapIntensity;
            }

            if (y > 0 && y < height - 1)
            {
                byte[] topPixel = new byte[4];
                byte[] bottomPixel = new byte[4];

                heightMap.CopyPixels(new Int32Rect(x, y - 1, 1, 1), topPixel, 4, 0);
                heightMap.CopyPixels(new Int32Rect(x, y + 1, 1, 1), bottomPixel, 4, 0);

                double hTop = topPixel[0] / 255.0;
                double hBottom = bottomPixel[0] / 255.0;

                gradient.Y = (hBottom - hTop) * heightMapIntensity;
            }

            Vector3 perturbedNormal = normal + new Vector3(-gradient.X, -gradient.Y, 1.0);
            perturbedNormal.Normalize();

            return perturbedNormal;
        }


        private static Vector3 AdjustNormalWithNormalMap(BitmapSource texture, Vector3 normal, Color textureColor, double normalMapIntensity = 0.45)
        {
            double x = ((textureColor.R) / 255.0) * 2.0 - 1.0;
            double y = ((textureColor.G) / 255.0) * 2.0 - 1.0;

            double z = Math.Sqrt(Math.Max(0.0, 1.0 - Math.Clamp(x * x + y * y, 0.0, 1.0)));

            Vector3 normalMapNormal = new Vector3(x, y, z);
            normalMapNormal.Normalize();

            return Vector3.Lerp(normal, normalMapNormal, normalMapIntensity);
        }


        private static Vector3 AdjustNormalWithParallaxMap(BitmapSource parallaxMap, Vector3 normal, Vector3 viewDirection, double heightMapIntensity = 10.0)
        {
            double u = 0;
            double v = 0;
            ComputeTextureCoordinates(normal, ref u, ref v);

            int width = parallaxMap.PixelWidth;
            int height = parallaxMap.PixelHeight;

            int x = (int)(u * width) % width;
            int y = (int)(v * height) % height;

            x = (x + width) % width;
            y = (y + height) % height;

            byte[] currentPixel = new byte[4];
            parallaxMap.CopyPixels(new Int32Rect(x, y, 1, 1), currentPixel, 4, 0);
            double heightValue = currentPixel[0] / 255.0;

            Vector3 viewDirXY = new Vector3(viewDirection.X, viewDirection.Y, 0);
            viewDirXY.Normalize();

            double parallaxScale = 0.05;

            Vector3 parallaxOffset = viewDirXY * (heightValue * heightMapIntensity * parallaxScale);

            parallaxOffset.X = (parallaxOffset.X + width) % width;
            parallaxOffset.Y = (parallaxOffset.Y + height) % height;

            double uAdj = (u - parallaxOffset.X / width + 1.0) % 1.0;
            double vAdj = (v - parallaxOffset.Y / height + 1.0) % 1.0;

            int adjX = (int)(uAdj * width) % width;
            int adjY = (int)(vAdj * height) % height;

            adjX = (adjX + width) % width;
            adjY = (adjY + height) % height;

            byte[] adjustedPixel = new byte[4];
            parallaxMap.CopyPixels(new Int32Rect(adjX, adjY, 1, 1), adjustedPixel, 4, 0);

            double hX = 0.0;
            double hY = 0.0;

            if (adjX > 0 && adjX < width - 1)
            {
                byte[] leftPixel = new byte[4];
                byte[] rightPixel = new byte[4];

                parallaxMap.CopyPixels(new Int32Rect((adjX - 1 + width) % width, adjY, 1, 1), leftPixel, 4, 0);
                parallaxMap.CopyPixels(new Int32Rect((adjX + 1) % width, adjY, 1, 1), rightPixel, 4, 0);

                double hLeft = leftPixel[0] / 255.0;
                double hRight = rightPixel[0] / 255.0;

                hX = (hRight - hLeft) * heightMapIntensity;
            }

            if (adjY > 0 && adjY < height - 1)
            {
                byte[] topPixel = new byte[4];
                byte[] bottomPixel = new byte[4];

                parallaxMap.CopyPixels(new Int32Rect(adjX, (adjY - 1 + height) % height, 1, 1), topPixel, 4, 0);
                parallaxMap.CopyPixels(new Int32Rect(adjX, (adjY + 1) % height, 1, 1), bottomPixel, 4, 0);

                double hTop = topPixel[0] / 255.0;
                double hBottom = bottomPixel[0] / 255.0;

                hY = (hBottom - hTop) * heightMapIntensity;
            }

            Vector3 perturbedNormal = new Vector3(-hX, -hY, 1.0);
            perturbedNormal.Normalize();

            return normal + perturbedNormal;
        }
    }
}
