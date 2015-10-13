using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Image_Viewer {
    public partial class MainWindow:Window {
        private OpenFileDialog openfiledialog = new OpenFileDialog();
        private static readonly List<string> acceptedFileTypes = new List<string>{".jpg",".jpeg",".bmp",".tiff",".tif",".png",".jpe",".gif",".ico"};
        private List<string> sortedImages = new List<string>();
        private BitmapImage currentImage = null;
        private int imageNumber = 0;
        private bool highResScaling = true;
        private bool isFullscreen = false;
        private int rotation = 0;
        private string[] presortedFiles;
        public MainWindow() {
            InitializeComponent();
        }
        public MainWindow(string[] commandLineFiles) {
            InitializeComponent();
            presortedFiles = commandLineFiles;
            SortFiles();
        }
        private void window_Drop(object sender,DragEventArgs e) {
            if(grid.AllowDrop && e.Data.GetDataPresent(DataFormats.FileDrop)) {
                presortedFiles = (string[])e.Data.GetData(DataFormats.FileDrop,false);
                SortFiles();
            }
        }
        private void window_DragEnter(object sender,DragEventArgs e) {
            if(grid.AllowDrop) {
                e.Effects = DragDropEffects.Copy;
            } else {
                e.Effects = DragDropEffects.None;
            }
        }
        private bool rightKeyDown = false;
        private bool leftKeyDown = false;
        private bool qIsDown = false;
        private bool rIsDown = false;
        private bool escIsDown = false;
        private bool f1IsDown = false;
        private bool spaceIsDown = false;
        private bool upArrowDown = false;
        private bool downArrowDown = false;
        private bool aIsDown = false;
        private bool dIsDown = false;
        private bool wIsDown = false;
        private bool eIsDown = false;
        private bool mIsDown = false;
        private void Window_KeyUp(object sender,KeyEventArgs e) {
            switch(e.Key) {
                case Key.Left:
                    leftKeyDown = false;
                    break;
                case Key.Right:
                    rightKeyDown = false;
                    break;
                case Key.Q:
                    qIsDown = false;
                    break;
                case Key.R:
                    rIsDown = false;
                    break;
                case Key.Escape:
                    escIsDown = false;
                    break;
                case Key.Space:
                    spaceIsDown = false;
                    break;
                case Key.Up:
                    upArrowDown = false;
                    break;
                case Key.Down:
                    downArrowDown = false;
                    break;
                case Key.A:
                    aIsDown = false;
                    break;
                case Key.D:
                    dIsDown = false;
                    break;
                case Key.W:
                    wIsDown = false;
                    break;
                case Key.E:
                    eIsDown = false;
                    break;
                case Key.M:
                    mIsDown = false;
                    break;
            }
        }
        private void Window_KeyDown(object sender,KeyEventArgs e) {
            switch(e.Key) {
                case Key.Left:
                    if(!leftKeyDown) {
                        leftKeyDown = true;
                        if(sortedImages.Count > 0) {
                            PreviousImage();
                            leftKeyDown = false;
                        }
                    }
                    break;
                case Key.Right:
                    if(!rightKeyDown) {
                        rightKeyDown = true;
                        if(sortedImages.Count > 1) {
                            NextImage();
                            rightKeyDown = false;
                        }
                    }
                    break;
                case Key.Q:
                    if(!qIsDown) {
                        qIsDown = true;
                        if(sortedImages.Count > 0 && !(isFullscreen && currentImage == null)) {
                            ToggleScalingMode();
                        }
                    }
                    break;
                case Key.R:
                    if(!rIsDown) {
                        rIsDown = true;
                        if(sortedImages.Count > 0) {
                            resetImages();
                        }
                    }
                    break;
                case Key.Escape:
                    if(!escIsDown) {
                        escIsDown = true;
                        if(isFullscreen) {
                            toggleFullscreen();
                        }
                    }
                    break;
                case Key.F1:
                    if(!f1IsDown) {
                        f1IsDown = true;
                        grid.AllowDrop = false;
                        showHelpDialog();
                        grid.AllowDrop = true;
                        f1IsDown = false;
                    }
                    break;
                case Key.Space:
                    if(!spaceIsDown) {
                        spaceIsDown = true;
                        toggleFullscreen();
                    }
                    break;
                case Key.Up:
                    if(!upArrowDown) {
                        upArrowDown = true;
                        imageBorder.child_MouseWheel(new object(),new MouseWheelEventArgs(Mouse.PrimaryDevice,0,1));
                    }
                    break;
                case Key.Down:
                    if(!downArrowDown) {
                        downArrowDown = true;
                        imageBorder.child_MouseWheel(new object(),new MouseWheelEventArgs(Mouse.PrimaryDevice,0,-1));
                    }
                    break;
                case Key.A:
                    if(!aIsDown) {
                        aIsDown = true;
                        Rotate(false);
                    }
                    break;
                case Key.D:
                    if(!dIsDown) {
                        dIsDown = true;
                        Rotate(true);
                    }
                    break;
                case Key.W:
                    if(!wIsDown) {
                        wIsDown = true;
                        addImagesFileDialog();
                    }
                    break;
                case Key.E:
                    if(!eIsDown) {
                        eIsDown = true;
                        if(!isFullscreen) {
                            imageBorder.Reset();
                        }
                    }
                    break;
                case Key.M:
                    if(!mIsDown) {
                        mIsDown = true;
                        if(!isFullscreen) {
                            toggleMenu();
                        }
                    }
                    break;
            }
        }
        private void next_Click(object sender,RoutedEventArgs e) {
            NextImage();
        }
        private void previous_Click(object sender,RoutedEventArgs e) {
            PreviousImage();
        }
        private void NextImage() {
            imageNumber++;
            if(imageNumber > sortedImages.Count - 1) {
                imageNumber = 0;
            }
            buttonUpdate();
            if(isFullscreen) {
                imageBorder.enabled = true;
            } else {
                imageBorder.enabled = false;
            }
            imageBorder.toggleEnabled();
            imageBorder.Reset();
            labelTextUpdate();
            ImageChange();
            rotation = 0;
        }
        private void PreviousImage() {
            imageNumber--;
            if(imageNumber < 0) {
                imageNumber = sortedImages.Count - 1;
            }
            buttonUpdate();
            if(isFullscreen) {
                imageBorder.enabled = true;
            } else {
                imageBorder.enabled = false;
            }
            imageBorder.toggleEnabled();
            imageBorder.Reset();
            labelTextUpdate();
            ImageChange();
            rotation = 0;
        }
        private bool isValidFileFormat(string path) {
            if(acceptedFileTypes.Contains(Path.GetExtension(path).ToLowerInvariant())) {
                return true;
            } else {
                return false;
            }
        }
        private void SortFiles() {
            try {
                Mouse.OverrideCursor = Cursors.Wait;
                grid.AllowDrop = false;
                addImages.IsEnabled = false;
                for(int i = 0;i < presortedFiles.Length;i++) {
                    if(File.Exists(presortedFiles[i]) || Directory.Exists(presortedFiles[i])) {
                        if(File.GetAttributes(presortedFiles[i]).HasFlag(FileAttributes.Directory)) {
                            string[] presortedFiles2 = Directory.GetFiles(presortedFiles[i],"*.*",SearchOption.AllDirectories);
                            for(int i2 = 0;i2 < presortedFiles2.Length;i2++) {
                                if(isValidFileFormat(presortedFiles2[i2])) {
                                    sortedImages.Add(presortedFiles2[i2]);
                                }
                            }
                        } else {
                            if(isValidFileFormat(presortedFiles[i])) {
                                sortedImages.Add(presortedFiles[i]);
                            }
                        }
                    }
                }
            } finally {
                Activate();
                wIsDown = false;
                grid.AllowDrop = true;
                addImages.IsEnabled = true;
                Mouse.OverrideCursor = null;
                if(sortedImages.Count > 0) {
                    ResizeMode = ResizeMode.CanResize;
                    dropImages.Visibility = Visibility.Hidden;
                    imageBorder.Visibility = Visibility.Visible;
                    Reset_Images.IsEnabled = true;
                    fullscreen.IsEnabled = true;
                    highResScaling = !highResScaling;
                    ToggleScalingMode();
                    if(rotation == 0) {
                        ImageChange();
                    }
                    if(sortedImages.Count > 0) {
                        buttonUpdate();
                        labelTextUpdate();
                    }
                } else {
                    resetImages();
                }
            }
        }
        private void Window_Closed(object sender,EventArgs e) {
            Application.Current.Shutdown();
        }
        private void buttonUpdate() {
            if(sortedImages.Count > 1) {
                previous.IsEnabled = true;
                next.IsEnabled = true;
            } else {
                previous.IsEnabled = false;
                next.IsEnabled = false;
            }
        }
        private void ToggleScalingMode() {
            highResScaling = !highResScaling;
            if(highResScaling) {
                RenderOptions.SetBitmapScalingMode(image,BitmapScalingMode.HighQuality);
            } else {
                RenderOptions.SetBitmapScalingMode(image,BitmapScalingMode.NearestNeighbor);
            }
            labelTextUpdate();
        }
        private void labelTextUpdate() {
            label.Content = "Image " + (imageNumber + 1) + " of " + sortedImages.Count + " | " + (highResScaling ? "High Quality" : "Nearest Neighbor");
        }
        private bool menuShown = false;
        private void menuImage_MouseUp(object sender,MouseButtonEventArgs e) {
            toggleMenu();
        }
        private void toggleMenu() {
            menuShown = !menuShown;
            if(menuShown) {
                menu.Visibility = Visibility.Visible;
            } else {
                menu.Visibility = Visibility.Hidden;
            }
        }
        private void addImages_Click(object sender,RoutedEventArgs e) {
            addImagesFileDialog();
        }
        private void Reset_Images_Click(object sender,RoutedEventArgs e) {
            if(sortedImages.Count > 0) {
                resetImages();
            }
        }
        private void addImagesFileDialog() {
            if(menuShown) {
                toggleMenu();
            }
            bool? result = openfiledialog.ShowDialog();
            wIsDown = false;
            if(result == true) {
                presortedFiles = openfiledialog.FileNames;
                SortFiles();
            }
        }
        private void resetImages() {
            if(isFullscreen) {
                toggleFullscreen();
            }
            if(menuShown) {
                toggleMenu();
            }
            imageNumber = 0;
            presortedFiles = null;
            sortedImages =  new List<string>();
            currentImage = null;
            image.Source = null;
            highResScaling = true;
            label.Content = "No Images Loaded";
            ResizeMode = ResizeMode.CanResize;
            dropImages.Visibility = Visibility.Visible;
            borderRectangle.Visibility = Visibility.Visible;
            imageBorder.Visibility = Visibility.Hidden;
            dropImages.Content = "Drop Images Anywhere";
            Title = "Image Viewer";
            imageBorder.releaseMouse();
            imageBorder.Reset();
            Reset_Images.IsEnabled = false;
            fullscreen.IsEnabled = false;
            buttonUpdate();
        }
        private void fullscreen_Click(object sender,RoutedEventArgs e) {
            toggleFullscreen();
        }
        private void Window_Loaded(object sender,RoutedEventArgs e) {
            openfiledialog.Title = "Select Images";
            openfiledialog.Multiselect = true;
            openfiledialog.CheckFileExists = true;
            openfiledialog.CheckPathExists = true;
            string filter = "Images|";
            foreach(string format in acceptedFileTypes) {
                filter += "*" + format + ";";
            }
            openfiledialog.Filter = filter;
        }
        private void Window_SizeChanged(object sender,SizeChangedEventArgs e) {
            imageBorder.Reset();
            if(menuShown) {
                toggleMenu();
            }
        }
        private WindowState previousWindowState;
        private void toggleFullscreen() {
            if(sortedImages.Count > 0) {
                isFullscreen = !isFullscreen;
                if(isFullscreen) {
                    if(menuShown) {
                        toggleMenu();
                    }
                    previousWindowState = WindowState;
                    ResizeMode = ResizeMode.NoResize;
                    WindowState = WindowState.Normal;
                    WindowState = WindowState.Maximized;
                    WindowStyle = WindowStyle.None;
                    imageBorder.Margin = new Thickness(0,0,0,0);
                    imageBorder.toggleEnabled();
                    imageBorder.Background = new SolidColorBrush(Colors.Black);
                    menuImage.Visibility = Visibility.Hidden;
                    Topmost = true;
                    dropImages.Background = new SolidColorBrush(Colors.Black);
                    dropImages.Foreground = new SolidColorBrush(Colors.White);
                    dropImages.Margin = new Thickness(0,0,0,0);
                    dropImages.FontSize = dropImages.FontSize * 2;
                } else {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    ResizeMode = ResizeMode.CanResize;
                    WindowState = previousWindowState;
                    imageBorder.Margin = new Thickness(0,45,0,55);
                    imageBorder.toggleEnabled();
                    imageBorder.Background = new SolidColorBrush(Color.FromRgb(247,247,247));
                    menuImage.Visibility = Visibility.Visible;
                    Topmost = false;
                    dropImages.Margin = new Thickness(110,75,110,85);
                    dropImages.Foreground = new SolidColorBrush(Color.FromRgb(100,100,100));
                    dropImages.Background = new SolidColorBrush(Colors.Transparent);
                    dropImages.FontSize = dropImages.FontSize / 2;
                }
            }
        }
        private void helpButton_Click(object sender,RoutedEventArgs e) {
            showHelpDialog();
        }
        private void Window_Deactivated(object sender,EventArgs e) {
            if(isFullscreen) {
                toggleFullscreen();
            }
            if(menuShown) {
                toggleMenu();
            }
        }
        private void showHelpDialog() {
            MessageBox.Show(Application.Current.MainWindow,"You can drag more images or folders onto the window at anytime\n\nUse the left and right arrow keys to select the next/previous image\n\nYou can scroll in based on the mouse position using your scroll wheel or the up and down arrows\n\nThe supported image formats are: .jpg, .jpeg, .bmp, .tiff, .tif, .png, .jpe, .gif, and .ico.\n\nR: Clears all the loaded files.\n\nQ: Toggles image interpolation mode.\n\nF1: Shows this help dialog.\n\nA: Rotates the image to the left by 90 degrees.\n\nD: Rotates the image to the right by 90 degrees.\n\nM: Toggles the menu.\n\nE: Resets image prior to any resizing or panning.\n\nW: Opens file selection dialog.\n\nSpace bar: Toggles fullscreen.\n\nEscape: Exits Fullscreen","Image Viewer");
        }
        private void Rotate(bool direction) {
            if(sortedImages.Count > 0 && currentImage != null) {
                if(direction) {
                    rotation++;
                    if(rotation > 3) {
                        rotation = 0;
                    }
                } else {
                    rotation--;
                    if(rotation < 0) {
                        rotation = 3;
                    }
                }
                if(isFullscreen) {
                    imageBorder.enabled = true;
                } else {
                    imageBorder.enabled = false;
                }
                imageBorder.toggleEnabled();
                image.Source = new TransformedBitmap(currentImage,new RotateTransform(rotation * 90));
            }
        }
        private void ImageChange() {
            if(File.Exists(sortedImages[imageNumber])){
                try {
                    currentImage = new BitmapImage(new Uri(sortedImages[imageNumber]));
                    image.Source = currentImage;
                    imageBorder.IsEnabled = true;
                    imageBorder.Visibility = Visibility.Visible;
                    dropImages.Visibility = Visibility.Hidden;
                    labelTextUpdate();
                    buttonUpdate();
                } catch {
                    currentImage = null;
                    imageBorder.Visibility = Visibility.Hidden;
                    dropImages.Visibility = Visibility.Visible;
                    dropImages.Content = "Could Not Load This Image";
                    imageBorder.IsEnabled = false;
                } finally {
                    Title = sortedImages[imageNumber];
                }
            }
        }
    }
}