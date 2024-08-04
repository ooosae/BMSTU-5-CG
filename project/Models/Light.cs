using System.Collections.ObjectModel;

namespace CourseCG.Models
{
    public abstract class Light
    {
        public double Intensity { get; set; }
    }

    public class PointLight : Light
    {
        public Vector3 Position { get; set; }

        public PointLight(Vector3 position, double intensity)
        {
            Position = position;
            Intensity = intensity;
        }
    }

    public class AmbientLight : Light
    {
        public AmbientLight(double intensity)
        {
            Intensity = intensity;
        }
    }

    public class DirectionalLight : Light
    {
        public Vector3 Direction { get; set; }

        public DirectionalLight(Vector3 direction, double intensity)
        {
            Direction = direction;
            Intensity = intensity;
        }
    }

    public class LightCollection
    {
        public ObservableCollection<PointLight> PointLights { get; set; }
        public ObservableCollection<AmbientLight> AmbientLights { get; set; }
        public ObservableCollection<DirectionalLight> DirectionalLights { get; set; }

        public LightCollection()
        {
            PointLights = new ObservableCollection<PointLight>();
            AmbientLights = new ObservableCollection<AmbientLight>();
            DirectionalLights = new ObservableCollection<DirectionalLight>();
        }
    }
}
