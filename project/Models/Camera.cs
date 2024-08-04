namespace CourseCG.Models
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Camera(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Camera()
        {
            Position = new Vector3(0, 0.5, 8);
            Rotation = new Vector3(-0.1, 0, 1);
        }
    }
}
