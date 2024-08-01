using System.Collections.ObjectModel;

namespace CourseCG.Models
{
    public class PointLight
    {
        public double Intensity { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
    }

    public class AmbientLight
    {
        public double Intensity { get; set; }
    }

    public class DirectionalLight
    {
        public double Intensity { get; set; }
        public double DirectionX { get; set; }
        public double DirectionY { get; set; }
        public double DirectionZ { get; set; }
    }

    public class Light
    {
        public ObservableCollection<PointLight>? Point { get; set; }
        public ObservableCollection<AmbientLight>? Ambient { get; set; }
        public ObservableCollection<DirectionalLight>? Directional { get; set; }
    }
}
