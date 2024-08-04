using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CourseCG.Models
{
    public class Scene
    {
        public ObservableCollection<Sphere> Spheres { get; set; }
        public LightCollection Lights { get; set; }

        public Scene()
        {
            Spheres = new ObservableCollection<Sphere>
                {
                    new Sphere(new Vector3(0, -1, 3 ), 1, Colors.Red, 500, 0.2),
                    new Sphere(new Vector3(2, 0, 4), 1, Colors.Yellow, 500, 0.3),
                    new Sphere(new Vector3(-2, 0,  4), 1, Colors.Blue, 10, 0.4),
                    new Sphere(new Vector3(0, -16,  0), 15, Colors.Green, 0, 0)
                };

            Lights = new LightCollection
            {
                AmbientLights = new ObservableCollection<AmbientLight>
                {
                    new AmbientLight(0.2)
                },
                PointLights = new ObservableCollection<PointLight>
                {
                    new PointLight(new Vector3(2, 1, 0), 0.6)
                },
                DirectionalLights = new ObservableCollection<DirectionalLight>
                {
                    new DirectionalLight(new Vector3(1, 4, 4), 0.2)
                }
            };
        }
    }
}
