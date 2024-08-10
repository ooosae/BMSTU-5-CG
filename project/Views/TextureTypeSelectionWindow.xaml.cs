using CourseCG.Models;
using System.Windows;

namespace CourseCG.Views
{
    public partial class TextureTypeSelectionWindow : Window
    {
        public TextureType SelectedTextureType { get; private set; }
        public bool IsTypeSelected { get; private set; }

        public TextureTypeSelectionWindow()
        {
            InitializeComponent();
            IsTypeSelected = false;
        }

        private void HeightMapButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTextureType = TextureType.HeightMap;
            IsTypeSelected = true;
            this.Close();
        }

        private void NormalMapButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTextureType = TextureType.NormalMap;
            IsTypeSelected = true;
            this.Close();
        }

        private void ParallaxMapButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTextureType = TextureType.ParallaxMap;
            IsTypeSelected = true;
            this.Close();
        }
    }
}
