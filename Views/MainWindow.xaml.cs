using CourseCG.Services;
using CourseCG.ViewModels;
using System;
using System.Threading.Tasks;
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

            double cosY = Math.Cos(_viewModel.Camera.RotY);
            double sinY = Math.Sin(_viewModel.Camera.RotY);
            double cosX = Math.Cos(_viewModel.Camera.RotX);
            double sinX = Math.Sin(_viewModel.Camera.RotX);

            int[] pixels = new int[width * height];

            await Task.Run(() =>
            {
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        double[] defaultDirection = { (x * 1.0 / width - 0.5) * 2, (-y * 1.0 / height + 0.5) * 2, -1 };

                        double[] rotatedDirectionY = {
                            defaultDirection[0] * cosY - defaultDirection[2] * sinY,
                            defaultDirection[1],
                            defaultDirection[0] * sinY + defaultDirection[2] * cosY
                        };

                        double[] finalDirection = {
                            rotatedDirectionY[0],
                            rotatedDirectionY[1] * cosX - rotatedDirectionY[2] * sinX,
                            rotatedDirectionY[1] * sinX + rotatedDirectionY[2] * cosX
                        };

                        IntersectionService.NormalizeVector(finalDirection);

                        Color color = RayTracingService.TraceRayAsync(
                            _viewModel.Scene,
                            new double[] { _viewModel.Camera.PosX, _viewModel.Camera.PosY, _viewModel.Camera.PosZ },
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
