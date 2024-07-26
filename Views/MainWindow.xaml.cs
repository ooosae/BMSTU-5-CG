using CourseCG.Services;
using CourseCG.ViewModels;
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
            RenderScene();
        }

        private void RenderScene()
        {
            int width = (int)RenderImage.Width;
            int height = (int)RenderImage.Height;

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double[] direction = { (x * 1.0 / width - 0.5) * _viewModel.Camera.RotX,
                       (-y * 1.0 / height + 0.5) * _viewModel.Camera.RotY,
                        _viewModel.Camera.RotZ };

                    IntersectionService.NormalizeVector(direction);
                    Color color = RayTracingService.TraceRay(_viewModel.Scene,
                        new double[] { _viewModel.Camera.PosX, _viewModel.Camera.PosY, _viewModel.Camera.PosZ },
                        direction, 0.001, double.PositiveInfinity, 3);
                    bitmap.SetPixelColor(x, y, color);
                }
            }

            RenderImage.Source = bitmap;
        }
    }

    public static class BitmapExtensions
    {
        public static void SetPixelColor(this WriteableBitmap wb, int x, int y, Color color)
        {
            int colorData = (color.B << 16) | (color.G << 8) | color.R;

            wb.Lock();
            IntPtr buffer = wb.BackBuffer + y * wb.BackBufferStride + x * 4;
            System.Runtime.InteropServices.Marshal.WriteInt32(buffer, colorData);
            wb.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            wb.Unlock();
        }
    }
}
