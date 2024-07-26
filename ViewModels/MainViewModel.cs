using CourseCG.Models;
using CourseCG.Services;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace CourseCG.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private Scene _scene;
        private Camera _camera;

        public MainViewModel()
        {
            LoadScene();
        }

        public Scene Scene
        {
            get => _scene;
            set => SetProperty(ref _scene, value);
        }

        public Camera Camera
        {
            get => _camera;
            set => SetProperty(ref _camera, value);
        }

        private void LoadScene()
        {
            Scene = new Scene
            {
                Spheres = new ObservableCollection<Sphere>
                {
                   new Sphere { Radius = 1, XCenter = 0, YCenter = -1, ZCenter = 3, Color = Colors.Red, Specular = 500, Reflective = 0.2 },
                   new Sphere { Radius = 1, XCenter = 2, YCenter = 0, ZCenter = 4, Color = Colors.Yellow, Specular = 500, Reflective = 0.3},
                   new Sphere { Radius = 1, XCenter = -2, YCenter = 0, ZCenter = 4, Color = Colors.Blue, Specular = 10, Reflective = 0.4},
                   new Sphere {Radius = 5000, XCenter = 0, YCenter = -5001, ZCenter = 0, Color = Colors.Green, Specular = 0, Reflective = 0}
                },
                Lights = new Light
                {
                    Ambient = new ObservableCollection<AmbientLight>
                    {
                        new AmbientLight { Intensity = 0.2 }
                    },
                    Point = new ObservableCollection<PointLight>
                    {
                        new PointLight { Intensity = 0.6, PositionX = 2, PositionY = 1, PositionZ = 0 }
                    },
                    Directional = new ObservableCollection<DirectionalLight>
                    {
                        new DirectionalLight { Intensity = 0.2, DirectionX = 1, DirectionY = 4, DirectionZ = 4 }
                    }
                }
            };

            Camera = new Camera
            {
                PosX = 0,
                PosY = 0.5,
                PosZ = 10,
                RotX = -1,
                RotY = 1,
                RotZ = -1
            };
        }
    }
}
