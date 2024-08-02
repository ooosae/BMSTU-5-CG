using CourseCG.Models;
using System.Collections.ObjectModel;

namespace CourseCG.Models
{
    public class Scene
    {
        public ObservableCollection<Sphere> Spheres { get; set; }
        public Light Lights { get; set; }
    }
}
