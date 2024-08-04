namespace CourseCG.Models
{
    public class Vector3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3() { }

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);

        public void Normalize()
        {
            double length = Length();
            if (length > 1e-6)
            {
                X /= length;
                Y /= length;
                Z /= length;
            }
        }
        public static Vector3 operator -(Vector3 v1, Vector3 v2) =>
            new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);

        public static Vector3 operator -(Vector3 v1) =>
            new Vector3(-v1.X, -v1.Y, -v1.Z);

        public static Vector3 operator +(Vector3 v1, Vector3 v2) =>
            new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

        public static Vector3 operator *(Vector3 v, double scalar) =>
            new Vector3(v.X * scalar, v.Y * scalar, v.Z * scalar);

        public static double DotProduct(Vector3 v1, Vector3 v2) =>
            v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;

        public static Vector3 CrossProduct(Vector3 v1, Vector3 v2) =>
            new Vector3(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
    }
}
