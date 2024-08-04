using CourseCG.Services;
using CourseCG.ViewModels;
using CourseCG.Models;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CourseCG.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            _viewModel.SceneChanged += async (sender, e) => await RenderSceneAsync();
            RenderSceneAsync().ConfigureAwait(false);
        }

        private async Task RenderSceneAsync()
        {
            int width = (int)RenderImage.Width;
            int height = (int)RenderImage.Height;

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);

            int[] pixels = new int[width * height];

            await Task.Run(() =>
            {
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        Vector3 defaultDirection = new Vector3((x * 1.0 / width - 0.5) * 2, (-y * 1.0 / height + 0.5) * 2, -1);
                        Vector3 rotatedDirectionY = Transformation.RotateY(defaultDirection, _viewModel.Camera.Rotation.Y);
                        Vector3 finalDirection = Transformation.RotateX(rotatedDirectionY, _viewModel.Camera.Rotation.X);
                        finalDirection.Normalize();

                        Color color = RayTracingService.TraceRayAsync(
                            _viewModel.Scene,
                            new Vector3(_viewModel.Camera.Position.X, _viewModel.Camera.Position.Y, _viewModel.Camera.Position.Z),
                            finalDirection, 0.001, double.PositiveInfinity, 3).Result;
                        int pixelColor = (color.R << 16) | (color.G << 8) | color.B;

                        pixels[y * width + x] = pixelColor;
                    }
                });
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);
                RenderImage.Source = bitmap;
            });
        }
    }
}
