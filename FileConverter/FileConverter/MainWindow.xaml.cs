using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

namespace FileConverter
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_SelectFile_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile("E:\\Tobias\\Bilder\\Test\\uncanny instinct.jpg");
            image.Save("E:\\Tobias\\Bilder\\Test\\uncanny instinct.png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
