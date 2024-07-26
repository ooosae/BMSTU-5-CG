using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace CourseCG.Models
{
    public class Sphere
    {
        public double Radius { get; set; }
        public double XCenter { get; set; }
        public double YCenter { get; set; }
        public double ZCenter { get; set; }
        public Color Color { get; set; }
        public double Specular { get; set; }
        public double Reflective { get; set; }
        public BitmapImage Texture { get; set; }
    }
}
