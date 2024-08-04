using CourseCG.Models;

namespace CourseCG.Services
{
    public static class Transformation
    {
        public static Vector3 RotateY(Vector3 vector, double angle)
        {
            double radians = angle * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            double x = vector.X * cos - vector.Z * sin;
            double z = vector.X * sin + vector.Z * cos;

            return new Vector3(x, vector.Y, z);
        }

        public static Vector3 RotateX(Vector3 vector, double angle)
        {
            double radians = angle * Math.PI / 180.0;
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            double y = vector.Y * cos - vector.Z * sin;
            double z = vector.Y * sin + vector.Z * cos;

            return new Vector3(vector.X, y, z);
        }
    }
}
