using System;
using System.IO;
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
using Microsoft.Win32;

namespace FileConverter
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string supportedPathFiles =
             "All supportet files|*.Bmp;*.Emf;*.Exif;*.Gif;*.Icon;*.Jpeg;*.Jpg;*.MemoryBmp;*.Png;*.Tiff;*.Wmf|" +
                "Bitmap|*.Bmp|" +
                "Erweiterte Metadatei|*.Emf|" +
                "Exchangeable Image File|*.Exif|" +
                "Graphics Interchange Format|*.Gif|" +
                "Windows Symbolformat|*.Icon|" +
                "Joint Photographic Experts Group|*.Jpeg;*.Jpg|" +
                "Bitmap im Speicher|*.MemoryBmp|" +
                "Portable Network Graphics (W3C)|*.Png|" +
                "Tagged Image File Format|*.Tiff|" +
                "Windows-Metadatei|*.Wmf"
            ;

        readonly string supportedSaveFiles =
                "Bitmap|*.Bmp|" +
                "Erweiterte Metadatei|*.Emf|" +
                "Exchangeable Image File|*.Exif|" +
                "Graphics Interchange Format|*.Gif|" +
                "Windows Symbolformat|*.Icon|" +
                "Joint Photographic Experts Group|*.Jpeg;*.Jpg|" +
                "Bitmap im Speicher|*.MemoryBmp|" +
                "Portable Network Graphics (W3C)|*.Png|" +
                "Tagged Image File Format|*.Tiff|" +
                "Windows-Metadatei|*.Wmf"
            ;
                


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_SelectFile_Click(object sender, RoutedEventArgs e)
        {
            Label_Staturbar.Content = "Select File...";

            string filePath = "";
            string savePath = "";
            int saveExtensionIndex = 0;
            string saveExtensionUser = "";
            System.Drawing.Imaging.ImageFormat saveImageFormat = null;

            // select File
            OpenFileDialog fileSelectionDialog = new OpenFileDialog();
            // set filter
            fileSelectionDialog.Filter = supportedPathFiles;
            fileSelectionDialog.Multiselect = false;

            // open file dialog
            if (fileSelectionDialog.ShowDialog() == true)
            {
                // stop when chosen file is an URL
                string temporaryInternetFilesDir = Environment.GetFolderPath(System.Environment.SpecialFolder.InternetCache);
                if (!string.IsNullOrEmpty(temporaryInternetFilesDir) &&
                fileSelectionDialog.FileName.StartsWith(temporaryInternetFilesDir, StringComparison.InvariantCultureIgnoreCase))
                {
                    Label_Staturbar.Content = "URLs are not supported";
                    return;
                }
                filePath = fileSelectionDialog.FileName;
            }
            // cancel
            else
            {
                Label_Staturbar.Content = "Idle";
                return;
            }

            // save File
            SaveFileDialog saveSelectionDialog = new SaveFileDialog();
            // set filter
            saveSelectionDialog.Filter = supportedSaveFiles;
            fileSelectionDialog.Multiselect = false;
            saveSelectionDialog.AddExtension = false;

            // open file dialog
            if (saveSelectionDialog.ShowDialog() == true)
            {
                savePath = saveSelectionDialog.FileName;
                saveExtensionIndex = saveSelectionDialog.FilterIndex;
                string[] saveSplitExtension = saveSelectionDialog.FileName.Split('.');
                saveExtensionUser = saveSplitExtension[saveSplitExtension.GetLength(0) - 1].ToLower();
            }
            // cancel
            else
            {
                Label_Staturbar.Content = "Idle";
                return;
            }

            // set Extension
            saveImageFormat = ImageFormatToSave(saveExtensionUser, saveExtensionIndex);

            // check format
            if(saveImageFormat == null)
            {
                Label_Staturbar.Content = "Failure, wrong index or extension";
                return;
            }
            
            // save File
            SaveFile(filePath, savePath, saveImageFormat);

            Label_Staturbar.Content = "Convert complete";
        }

        private void Button_SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = "";
            string[] files;
            string savePath = "";
            System.Drawing.Imaging.ImageFormat saveImageFormat = null;

            // select File
            OpenFileDialog fileSelectionDialog = new OpenFileDialog();
            // set filter
            fileSelectionDialog.Filter = supportedPathFiles;
            fileSelectionDialog.Multiselect = false;

            Label_Staturbar.Content = "Choose Folder with images...";

            // choose folder with images
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                // if user choose a folder
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    folderPath = fbd.SelectedPath;
                    files = GetAllPicturesPath(folderPath, SearchOption.TopDirectoryOnly);
                }
                // if user canceled
                else
                {
                    Label_Staturbar.Content = "Canceled";
                    return;
                }
            }

            Label_Staturbar.Content = "Choose Folder to save images...";

            // choose folder to save images
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                // if user choose a folder
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    savePath = fbd.SelectedPath;
                }
                // if user canceled
                else
                {
                    Label_Staturbar.Content = "Canceled";
                    return;
                }
            }

            // choose format
            string format = Microsoft.VisualBasic.Interaction.InputBox("Format you want to save.\nExample:\nbmp\npng\ngif");
            saveImageFormat = ImageFormatToSave(format, 0);

            // check format
            if(saveImageFormat == null)
            {
                Label_Staturbar.Content = "Failure, wrong format";
                return;
            }

            Label_Staturbar.Content = "Converting...";
            // save files
            for (int i = 0; i < files.GetLength(0); i++)
            {
                Label_StatusFileOfFile.Content = i + 1 + " of " + (files.GetLength(0) + 1).ToString();
                // show saved images in status bar
                System.Windows.Forms.Application.DoEvents();

                string fileName = "";
                DirectoryInfo d = new DirectoryInfo(files[i]);
                
                // get filename
                foreach (char c in d.Name)
                {
                    if (c == '.')
                    {
                        break;
                    }

                    fileName += c;
                }
                Label_StatusCurrentFile.Content = fileName;
                System.Windows.Forms.Application.DoEvents();
                SaveFile(
                    files[i],
                    savePath + "\\" + fileName + "." + (saveImageFormat.ToString()).ToLower(),
                    saveImageFormat);

            }

            Label_Staturbar.Content = "Complete";
            Label_StatusCurrentFile.Content = "";
            Label_StatusFileOfFile.Content = "";
        }

        // -------------------------------------------------------------------------------------------------------------------------- //

        /// <summary>
        /// Get chosen Image Format from user (prefered) or index
        /// </summary>
        /// <param name="_userExtension">extension the User write</param>
        /// <param name="_index">Extension index</param>
        /// <returns></returns>
        private System.Drawing.Imaging.ImageFormat ImageFormatToSave(string _userExtension, int _index)
        {
            System.Drawing.Imaging.ImageFormat imageFormat = null;

            switch (_userExtension.ToLower())
            {
                case "bmp":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                    break;
                case "emf":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Emf;
                    break;
                case "exif":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Exif;
                    break;
                case "gif":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Gif;
                    break;
                case "icon":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Icon;
                    break;
                case "jpeg":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
                case "jpg":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
                case "memorybmp":
                    imageFormat = System.Drawing.Imaging.ImageFormat.MemoryBmp;
                    break;
                case "png":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                    break;
                case "tiff":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Tiff;
                    break;
                case "wmf":
                    imageFormat = System.Drawing.Imaging.ImageFormat.Wmf;
                    break;
                default:
                    // extension from index
                    switch (_index)
                    {
                        case 1:
                            imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                            break;
                        case 2:
                            imageFormat = System.Drawing.Imaging.ImageFormat.Emf;
                            break;
                        case 3:
                            imageFormat = System.Drawing.Imaging.ImageFormat.Exif;
                            break;
                        case 4:
                            imageFormat = System.Drawing.Imaging.ImageFormat.Gif;
                            break;
                        case 5:
                            imageFormat = System.Drawing.Imaging.ImageFormat.Icon;
                            break;
                        case 6:
                            imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case 7:
                            imageFormat = System.Drawing.Imaging.ImageFormat.MemoryBmp;
                            break;
                        case 8:
                            imageFormat = System.Drawing.Imaging.ImageFormat.Png;
                            break;
                        case 9:
                            imageFormat = System.Drawing.Imaging.ImageFormat.Tiff;
                            break;
                        case 10:
                            imageFormat = System.Drawing.Imaging.ImageFormat.Wmf;
                            break;
                    }
                    break;
            }
            return imageFormat;
        }

        /// <summary>
        /// Save Image (System.Drawing)
        /// </summary>
        /// <param name="_filePath">path of existing Image (NOT FOLDER!)</param>
        /// <param name="_savePath">path of Save to Image (NOT FOLDER!)</param>
        /// <param name="_imageFormat">Image format</param>
        private void SaveFile(string _filePath, string _savePath, System.Drawing.Imaging.ImageFormat _imageFormat)
        {
            // get image
            System.Drawing.Image image = System.Drawing.Image.FromFile(_filePath);
            // save image
            image.Save(_savePath, _imageFormat);
            // gibt Speicher frei
            image.Dispose();
        }

        /// <summary>
        /// Get all pictures in Folder
        /// </summary>
        /// <param name="_path">path of Folder</param>
        /// <param name="_searchOption">search only in this folder or wiht all directories below</param>
        /// <returns></returns>
        private string[] GetAllPicturesPath(string _path, SearchOption _searchOption)
        {
            string[] supportetSaveFilesSplit = supportedSaveFiles.Split('|');
            List<string> filesList = new List<string>();

            for (int i = 1; i < supportetSaveFilesSplit.GetLength(0); i++)
            {
                if (i%2 == 0)
                    continue;

                filesList.AddRange(Directory.GetFiles(_path, supportetSaveFilesSplit[i], _searchOption));

            }

            return filesList.ToArray();
        }


    }
}
