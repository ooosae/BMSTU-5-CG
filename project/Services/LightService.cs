using CourseCG.Models;

namespace CourseCG.Services
{
    public static class LightService
    {
        public static double ComputeLighting(Scene scene, double[] point, double[] normal, double[] view, double specular)
        {
            double intensity = 0;

            foreach (var ambient in scene.Lights.Ambient)
                intensity += ambient.Intensity;

            foreach (var pointLight in scene.Lights.Point)
            {
                double[] lightDir = { pointLight.PositionX - point[0], pointLight.PositionY - point[1], pointLight.PositionZ - point[2] };
                IntersectionService.NormalizeVector(lightDir);
                intensity += ComputeDiffuseSpecular(scene, point, normal, view, lightDir, pointLight.Intensity, specular);
            }

            foreach (var directional in scene.Lights.Directional)
            {
                double[] lightDir = { directional.DirectionX, directional.DirectionY, directional.DirectionZ };
                IntersectionService.NormalizeVector(lightDir);
                intensity += ComputeDiffuseSpecular(scene, point, normal, view, lightDir, directional.Intensity, specular);
            }

            return intensity;
        }

        private static double ComputeDiffuseSpecular(Scene scene, double[] point, double[] normal, double[] view, double[] lightDir, double lightIntensity, double specular)
        {
            double diffuseIntensity = IntersectionService.DotProduct(normal, lightDir) * lightIntensity;
            if (diffuseIntensity < 0) diffuseIntensity = 0;

            double specularIntensity = 0;
            if (specular >= 0)
            {
                double[] reflectDir = new double[3];
                IntersectionService.ReflectRay(lightDir, normal, reflectDir);
                double spec = IntersectionService.DotProduct(reflectDir, view);
                if (spec > 0)
                {
                    specularIntensity = Math.Pow(spec, specular) * lightIntensity;
                }
            }
            return Math.Max(0, Math.Min(1, diffuseIntensity + specularIntensity));
        }
    }
}
