using CourseCG.Models;

namespace CourseCG.Services
{
    public static class LightService
    {
        public static double ComputeLighting(Scene scene, Vector3 point, Vector3 normal, Vector3 view, double specular)
        {
            double intensity = 0;

            foreach (var ambient in scene.Lights.AmbientLights)
                intensity += ambient.Intensity;

            foreach (var pointLight in scene.Lights.PointLights)
            {
                Vector3 lightDir = pointLight.Position - point;
                lightDir.Normalize();
                intensity += ComputeDiffuseSpecular(point, normal, view, lightDir, pointLight.Intensity, specular);
            }

            foreach (var directional in scene.Lights.DirectionalLights)
            {
                Vector3 lightDir = new Vector3(directional.Direction.X, directional.Direction.Y, directional.Direction.Z);
                lightDir.Normalize();
                intensity += ComputeDiffuseSpecular(point, normal, view, lightDir, directional.Intensity, specular);
            }

            return intensity;
        }

        private static double ComputeDiffuseSpecular(Vector3 point, Vector3 normal, Vector3 view, Vector3 lightDir, double lightIntensity, double specular)
        {
            double diffuseIntensity = Vector3.DotProduct(normal, lightDir) * lightIntensity;
            diffuseIntensity = Math.Max(diffuseIntensity, 0);

            double specularIntensity = 0;
            if (specular >= 0)
            {
                Vector3 reflectDir = IntersectionService. ReflectRay(lightDir, normal);
                double spec = Vector3.DotProduct(reflectDir, view);
                if (spec > 0)
                {
                    specularIntensity = Math.Pow(spec, specular) * lightIntensity;
                }
            }

            return Math.Max(0, Math.Min(1, diffuseIntensity + specularIntensity));
        }
    }
}
