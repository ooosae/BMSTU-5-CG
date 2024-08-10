using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CourseCG.Models
{
    public enum TextureType
    {
        HeightMap,
        NormalMap,
        ParallaxMap
    }

    public class Sphere
    {
        public double Radius { get; set; }
        public Vector3 Center { get; set; }
        public Color Color { get; set; }
        public double Specular { get; set; }
        public double Reflective { get; set; }
        public BitmapImage? Texture { get; set; }

        public TextureType TextureType { get; set; }

        public Sphere(Vector3 center, double radius, Color color, double specular, double reflective, BitmapImage? texture = null, TextureType textureType = 0)
        {
            Center = center;
            Radius = radius;
            Color = color;
            Specular = specular;
            Reflective = reflective;
            Texture = texture;
            TextureType = TextureType;
        }

        public Sphere()
        {
            Radius = 1.0;
            Center = new Vector3(0, 0, 0);
            Color = Colors.White;
            Specular = 0;
            Reflective = 0;
            Texture = null;
            TextureType = 0;
        }
    }
}
