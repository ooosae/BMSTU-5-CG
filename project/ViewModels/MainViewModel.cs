using CourseCG.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace CourseCG.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private Scene _scene = new Scene();
        private Camera _camera = new Camera();
        private Sphere _selectedSphere = new Sphere();
        private string _lightTypeToAdd = string.Empty;

        public event EventHandler<EventArgs>? SceneChanged;

        public MainViewModel()
        {
            SceneChanged?.Invoke(this, EventArgs.Empty);

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

        private void SetLightTypeAndAdd(object? parameter)
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

                        Sphere sphere = new Sphere(new Vector3(x, y, z), radius, color, specular, reflective);
                        Scene.Spheres.Add(sphere);
                    }
                }
                SceneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool CanDeleteSphere(object? parameter)
        {
            return parameter is Sphere;
        }

        private void DeleteSphere(object? parameter)
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
                                Scene.Lights.AmbientLights?.Add(new AmbientLight(ambientIntensity));
                            }
                            break;
                        case "Point":
                            if (parts.Length == 4 && double.TryParse(parts[0], out double pointIntensity) &&
                                double.TryParse(parts[1], out double pointX) &&
                                double.TryParse(parts[2], out double pointY) &&
                                double.TryParse(parts[3], out double pointZ))
                            {
                                Scene.Lights.PointLights?.Add(new PointLight(new Vector3(pointX, pointY, pointZ), pointIntensity));
                            }
                            break;
                        case "Directional":
                            if (parts.Length == 4 && double.TryParse(parts[0], out double dirIntensity) &&
                                double.TryParse(parts[1], out double dirX) &&
                                double.TryParse(parts[2], out double dirY) &&
                                double.TryParse(parts[3], out double dirZ))
                            {
                                Scene.Lights.DirectionalLights?.Add(new DirectionalLight(new Vector3(dirX, dirY, dirZ), dirIntensity));
                            }
                            break;
                    }
                }
                SceneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task LoadTextureAsync(Sphere? sphere)
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


        private bool CanDeleteLight(object? parameter)
        {
            return parameter is AmbientLight || parameter is PointLight || parameter is DirectionalLight;
        }

        private void DeleteLight(object? parameter)
        {
            switch (parameter)
            {
                case AmbientLight ambientLight:
                    if (Scene.Lights.AmbientLights != null && Scene.Lights.AmbientLights.Contains(ambientLight))
                    {
                        Scene.Lights.AmbientLights.Remove(ambientLight);
                    }
                    break;
                case PointLight pointLight:
                    if (Scene.Lights.PointLights != null && Scene.Lights.PointLights.Contains(pointLight))
                    {
                        Scene.Lights.PointLights.Remove(pointLight);
                    }
                    break;
                case DirectionalLight directionalLight:
                    if (Scene.Lights.DirectionalLights != null && Scene.Lights.DirectionalLights.Contains(directionalLight))
                    {
                        Scene.Lights.DirectionalLights.Remove(directionalLight);
                    }
                    break;
            }
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraUp()
        {
            Camera.Position.Y += 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraDown()
        {
            Camera.Position.Y -= 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraLeft()
        {
            Camera.Position.X -= 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraRight()
        {
            Camera.Position.X += 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraCloser()
        {
            Camera.Position.Z -= 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MoveCameraFurther()
        {
            Camera.Position.Z += 1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RotateCameraLeft()
        {
            Camera.Rotation.Y -= 0.1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RotateCameraRight()
        {
            Camera.Rotation.Y += 0.1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RotateCameraUp()
        {
            Camera.Rotation.X += 0.1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RotateCameraDown()
        {
            Camera.Rotation.X -= 0.1;
            SceneChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
