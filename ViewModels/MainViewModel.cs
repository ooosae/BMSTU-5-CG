using CourseCG.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace CourseCG.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private Scene _scene;
        private Camera _camera;
        private Sphere _selectedSphere;
        private string _lightTypeToAdd;

        public event EventHandler<EventArgs> SceneChanged;

        public MainViewModel()
        {
            LoadScene();
            AddSphereCommand = new RelayCommand(_ => AddSphere());
            DeleteSphereCommand = new RelayCommand(parameter => DeleteSphere(parameter), parameter => CanDeleteSphere(parameter));
            ClearSceneCommand = new RelayCommand(_ => ClearScene());
            AddLightCommand = new RelayCommand(parameter => SetLightTypeAndAdd(parameter));
            DeleteLightCommand = new RelayCommand(parameter => DeleteLight(parameter), parameter => CanDeleteLight(parameter));
            MoveCameraUpCommand = new RelayCommand(_ => MoveCameraUp());
            MoveCameraDownCommand = new RelayCommand(_ => MoveCameraDown());
            MoveCameraLeftCommand = new RelayCommand(_ => MoveCameraLeft());
            MoveCameraRightCommand = new RelayCommand(_ => MoveCameraRight());
            MoveCameraCloserCommand = new RelayCommand(_ => MoveCameraCloser());
            MoveCameraFurtherCommand = new RelayCommand(_ => MoveCameraFurther());
            RotateCameraLeftCommand = new RelayCommand(_ => RotateCameraLeft());
            RotateCameraRightCommand = new RelayCommand(_ => RotateCameraRight());
            RotateCameraUpCommand = new RelayCommand(_ => RotateCameraUp());
            RotateCameraDownCommand = new RelayCommand(_ => RotateCameraDown());
            LoadTextureCommand = new RelayCommand<Sphere>(async sphere => await LoadTextureAsync(sphere));
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

        public Sphere SelectedSphere
        {
            get => _selectedSphere;
            set
            {
                if (SetProperty(ref _selectedSphere, value))
                {
                    ((RelayCommand)DeleteSphereCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string LightTypeToAdd
        {
            get => _lightTypeToAdd;
            set => SetProperty(ref _lightTypeToAdd, value);
        }

        public ICommand AddSphereCommand { get; }
        public ICommand DeleteSphereCommand { get; }
        public ICommand ClearSceneCommand { get; }
        public ICommand AddLightCommand { get; }
        public ICommand DeleteLightCommand { get; }
        public ICommand MoveCameraUpCommand { get; }
        public ICommand MoveCameraDownCommand { get; }
        public ICommand MoveCameraLeftCommand { get; }
        public ICommand MoveCameraRightCommand { get; }
        public ICommand MoveCameraCloserCommand { get; }
        public ICommand MoveCameraFurtherCommand { get; }
        public ICommand RotateCameraLeftCommand { get; }
        public ICommand RotateCameraRightCommand { get; }
        public ICommand RotateCameraUpCommand { get; }
        public ICommand RotateCameraDownCommand { get; }
        public ICommand LoadTextureCommand { get; }

        private void LoadScene()
        {
            Scene = new Scene
            {
                Spheres = new ObservableCollection<Sphere>
                {
                    new Sphere { Radius = 1, XCenter = 0, YCenter = -1, ZCenter = 3, Color = Colors.Red, Specular = 500, Reflective = 0.2 },
                    new Sphere { Radius = 1, XCenter = 2, YCenter = 0, ZCenter = 4, Color = Colors.Yellow, Specular = 500, Reflective = 0.3},
                    new Sphere { Radius = 1, XCenter = -2, YCenter = 0, ZCenter = 4, Color = Colors.Blue, Specular = 10, Reflective = 0.4},
                    new Sphere { Radius = 15, XCenter = 0, YCenter = -16, ZCenter = 0, Color = Colors.Green, Specular = 0, Reflective = 0}
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
                PosZ = 8,
                RotX = -0.1,
                RotY = 0,
                RotZ = 1
            };

            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SetLightTypeAndAdd(object parameter)
        {
            if (parameter is string lightType)
            {
                LightTypeToAdd = lightType;
                AddLight();
            }
        }

        private async void AddSphere()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string[] lines = await Task.Run(() => File.ReadAllLines(openFileDialog.FileName));
                foreach (string line in lines)
                {
                    string[] parts = line.Split(' ');
                    if (parts.Length == 7)
                    {
                        double.TryParse(parts[0], out double x);
                        double.TryParse(parts[1], out double y);
                        double.TryParse(parts[2], out double z);
                        double.TryParse(parts[3], out double radius);
                        Color color = (Color)ColorConverter.ConvertFromString(parts[4]);
                        double.TryParse(parts[5], out double specular);
                        double.TryParse(parts[6], out double reflective);

                        Sphere sphere = new Sphere
                        {
                            XCenter = x,
                            YCenter = y,
                            ZCenter = z,
                            Radius = radius,
                            Color = color,
                            Specular = specular,
                            Reflective = reflective
                        };

                        Scene.Spheres.Add(sphere);
                    }
                }
                SceneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool CanDeleteSphere(object parameter)
        {
            return parameter is Sphere;
        }

        private void DeleteSphere(object parameter)
        {
            if (parameter is Sphere sphere && Scene.Spheres.Contains(sphere))
            {
                Scene.Spheres.Remove(sphere);
                SceneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ClearScene()
        {
            Scene.Spheres.Clear();
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void AddLight()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(' ');
                    switch (LightTypeToAdd)
                    {
                        case "Ambient":
                            if (parts.Length == 1 && double.TryParse(parts[0], out double ambientIntensity))
                            {
                                Scene.Lights.Ambient.Add(new AmbientLight { Intensity = ambientIntensity });
                            }
                            break;
                        case "Point":
                            if (parts.Length == 4 && double.TryParse(parts[0], out double pointIntensity) &&
                                double.TryParse(parts[1], out double pointX) &&
                                double.TryParse(parts[2], out double pointY) &&
                                double.TryParse(parts[3], out double pointZ))
                            {
                                Scene.Lights.Point.Add(new PointLight
                                {
                                    Intensity = pointIntensity,
                                    PositionX = pointX,
                                    PositionY = pointY,
                                    PositionZ = pointZ
                                });
                            }
                            break;
                        case "Directional":
                            if (parts.Length == 4 && double.TryParse(parts[0], out double dirIntensity) &&
                                double.TryParse(parts[1], out double dirX) &&
                                double.TryParse(parts[2], out double dirY) &&
                                double.TryParse(parts[3], out double dirZ))
                            {
                                Scene.Lights.Directional.Add(new DirectionalLight
                                {
                                    Intensity = dirIntensity,
                                    DirectionX = dirX,
                                    DirectionY = dirY,
                                    DirectionZ = dirZ
                                });
                            }
                            break;
                    }
                }
                SceneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task LoadTextureAsync(Sphere sphere)
        {
            if (sphere == null)
                return;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg)|*.png;*.jpg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string texturePath = openFileDialog.FileName;
                BitmapImage bitmap = new BitmapImage();

                await Task.Run(() =>
                {
                    using (var stream = new FileStream(texturePath, FileMode.Open, FileAccess.Read))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                            bitmap.Freeze();
                        });
                    }
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    sphere.Texture = bitmap;
                });

                SceneChanged?.Invoke(this, EventArgs.Empty);
            }
        }


        private bool CanDeleteLight(object parameter)
        {
            return parameter is AmbientLight || parameter is PointLight || parameter is DirectionalLight;
        }

        private void DeleteLight(object parameter)
        {
            switch (parameter)
            {
                case AmbientLight ambientLight:
                    if (Scene.Lights.Ambient.Contains(ambientLight))
                    {
                        Scene.Lights.Ambient.Remove(ambientLight);
                    }
                    break;
                case PointLight pointLight:
                    if (Scene.Lights.Point.Contains(pointLight))
                    {
                        Scene.Lights.Point.Remove(pointLight);
                    }
                    break;
                case DirectionalLight directionalLight:
                    if (Scene.Lights.Directional.Contains(directionalLight))
                    {
                        Scene.Lights.Directional.Remove(directionalLight);
                    }
                    break;
            }
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraUp()
        {
            Camera.PosY += 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraDown()
        {
            Camera.PosY -= 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraLeft()
        {
            Camera.PosX -= 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraRight()
        {
            Camera.PosX += 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraCloser()
        {
            Camera.PosZ -= 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraFurther()
        {
            Camera.PosZ += 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RotateCameraLeft()
        {
            Camera.RotY -= 0.1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RotateCameraRight()
        {
            Camera.RotY += 0.1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RotateCameraUp()
        {
            Camera.RotX += 0.1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RotateCameraDown()
        {
            Camera.RotX -= 0.1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
