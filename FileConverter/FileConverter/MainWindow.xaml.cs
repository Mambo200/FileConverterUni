﻿using System;
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
            Label_Statusbar.Content = "Select File...";

            string filePath = "";
            string savePath = "";
            int saveExtensionIndex = 0;
            string saveExtensionUser = "";
            System.Drawing.Imaging.ImageFormat saveImageFormat = null;

            // create select File dialog
            OpenFileDialog fileSelectionDialog = new OpenFileDialog
            {
                // set filter
                Filter = supportedPathFiles,
                Multiselect = false
            };




            // open file dialog
            if (fileSelectionDialog.ShowDialog() == true)
            {
                // stop when chosen file is an URL
                string temporaryInternetFilesDir = Environment.GetFolderPath(System.Environment.SpecialFolder.InternetCache);
                if (!string.IsNullOrEmpty(temporaryInternetFilesDir) &&
                fileSelectionDialog.FileName.StartsWith(temporaryInternetFilesDir, StringComparison.InvariantCultureIgnoreCase))
                {
                    Label_Statusbar.Content = "URLs are not supported";
                    return;
                }
                filePath = fileSelectionDialog.FileName;
            }
            // cancel
            else
            {
                Label_Statusbar.Content = "Idle";
                return;
            }

            // create save File dialog
            SaveFileDialog saveSelectionDialog = new SaveFileDialog
            {
                // set filter
                Filter = supportedSaveFiles,
                // user can choose own extension
                AddExtension = false
            };

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
                Label_Statusbar.Content = "Idle";
                return;
            }

            // set Extension
            saveImageFormat = ImageFormatToSave(saveExtensionUser, saveExtensionIndex);

            // check format
            if(saveImageFormat == null)
            {
                Label_Statusbar.Content = "Failure, wrong index or extension";
                return;
            }

            Label_Statusbar.Content = "Converting...";

            // save File
            SaveFile(filePath, savePath, saveImageFormat);

            Label_Statusbar.Content = "Convert complete";
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

            Label_Statusbar.Content = "Image with folder...";

            // choose folder with images
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                // if user choose a folder
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    folderPath = fbd.SelectedPath;
                    // top directory only or folder in folder
                    System.Windows.Forms.DialogResult dialogResult = System.Windows.Forms.MessageBox.Show(
                        "Only this folder (Yes) or Folder in Folder (No)?",
                        "Search option",
                        System.Windows.Forms.MessageBoxButtons.YesNoCancel
                        );

                    // save Searchoption
                    SearchOption sOpt;
                    if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                        sOpt = SearchOption.TopDirectoryOnly;
                    else if (dialogResult == System.Windows.Forms.DialogResult.No)
                        sOpt = SearchOption.AllDirectories;
                    else
                    {
                        Label_Statusbar.Content = "Idle";
                        return;
                    }

                    // search for files
                    files = GetAllPicturesPath(folderPath, sOpt);
                }
                // if user canceled
                else
                {
                    Label_Statusbar.Content = "Canceled";
                    return;
                }
            }

            Label_Statusbar.Content = "Image save folder...";

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
                    Label_Statusbar.Content = "Canceled";
                    return;
                }
            }

            // choose format
            string format = Microsoft.VisualBasic.Interaction.InputBox("Format you want to save.\n" +
                "Supported Formats:\n\n" +
                "JPG || PNG || BMP || " +
                "EMF || EXIF || GIF || " +
                "ICON || TIFF || WMF || " +
                "MEMORYBMP");
            saveImageFormat = ImageFormatToSave(format, 0);

            // check format
            if(saveImageFormat == null)
            {
                Label_Statusbar.Content = "Wrong format";
                return;
            }

            Label_Statusbar.Content = "Converting...";
            
            // save files
            for (int i = 0; i < files.GetLength(0); i++)
            {
                // show saved images in status bar
                Label_StatusFileOfFile.Content = i + 1 + " of " + (files.GetLength(0) + 1).ToString();
                // refresh window 
                System.Windows.Forms.Application.DoEvents();

                DirectoryInfo d = new DirectoryInfo(files[i]);

                // get filename
                string fileName = GetFileName(files[i]);

                Label_StatusCurrentFile.Content = fileName;
                System.Windows.Forms.Application.DoEvents();
                SaveFile(
                    files[i],
                    savePath + "\\" + fileName + "." + (saveImageFormat.ToString()).ToLower(),
                    saveImageFormat);

            }

            Label_Statusbar.Content = "Complete";
            Label_StatusCurrentFile.Content = "";
            Label_StatusFileOfFile.Content = "";
        }

        // -------------------------------------------------------------------------------------------------------------------------- //

        /// <summary>
        /// Get chosen Image Format from user (prefered) or index
        /// </summary>
        /// <param name="_userExtension">extension the User write</param>
        /// <param name="_index">Extension index from save dialog</param>
        /// <returns>Image Format when corrent extension, null when no Format coule be found</returns>
        private System.Drawing.Imaging.ImageFormat ImageFormatToSave(string _userExtension, int _index)
        {
            System.Drawing.Imaging.ImageFormat imageFormat = null;

            // get image format by user extension
            imageFormat = ImageFormatToSave(_userExtension);

            // check current image format
            if (imageFormat == null)
                // get image format by dialog index
                imageFormat = ImageFormatToSave(_index);

            return imageFormat;
        }

        /// <summary>
        /// Get chosen Format from user
        /// </summary>
        /// <param name="_userExtension">extension the user write</param>
        /// <returns>Image Format when corrent extension, null when no Format coule be found</returns>
        private System.Drawing.Imaging.ImageFormat ImageFormatToSave(string _userExtension)
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
            }

            return imageFormat;
        }

        /// <summary>
        /// Get chosen Format from index
        /// </summary>
        /// <param name="_index">Extension index from save dialog</param>
        /// <returns>Image Format when corrent extension, null when no Format coule be found</returns>
        private System.Drawing.Imaging.ImageFormat ImageFormatToSave(int _index)
        {
            System.Drawing.Imaging.ImageFormat imageFormat = null;

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
            // if format is same as user wants only copy
            if ((_filePath.ToLower()).EndsWith((_imageFormat.ToString()).ToLower()))
            {
                // copy file
                File.Copy(_filePath, _savePath, true);
            }
            else
            {
                // get image
                System.Drawing.Image image = System.Drawing.Image.FromFile(_filePath);
                // check pixels of image
                if(image.Height > 10000 || image.Width > 10000)
                {
                    image.Dispose();
                    // set new save format
                    string newSavePath = _savePath.TrimEnd(_imageFormat.ToString().ToArray());
                    newSavePath += GetExtension(_filePath);

                    // copy file
                    File.Copy(_filePath, newSavePath, true);
                    return;
                }
                // save image
                image.Save(_savePath, _imageFormat);
                // gibt Speicher frei
                image.Dispose();
            }
        }

        /// <summary>
        /// Get Extension of File
        /// </summary>
        /// <param name="_path">Path of File (NOT FOLDER!)</param>
        /// <returns>extension with dot</returns>
        private string GetExtension(string _path)
        {
            DirectoryInfo d = new DirectoryInfo(_path);
            return d.Extension;
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

        /// <summary>
        /// Get filename without extension
        /// </summary>
        /// <param name="_path">complete Path</param>
        /// <returns>Filename without extension</returns>
        private string GetFileName(string _path)
        {
            DirectoryInfo d = new DirectoryInfo(_path);
            string fileName = d.Name;
            fileName = d.Name.TrimEnd(d.Extension.ToCharArray());

            return fileName;

        }
    }
}
