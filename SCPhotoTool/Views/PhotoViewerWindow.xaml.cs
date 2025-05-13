using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SCPhotoTool.Views
{
    /// <summary>
    /// 照片查看器窗口交互逻辑
    /// </summary>
    public partial class PhotoViewerWindow : Window
    {
        private string _imagePath;
        private double[] zoomLevels = { 0.25, 0.5, 1.0, 1.5, 2.0, -1.0 }; // -1表示适应窗口
        
        /// <summary>
        /// 当前图像路径
        /// </summary>
        public string ImagePath 
        { 
            get => _imagePath; 
            private set => _imagePath = value; 
        }
        
        /// <summary>
        /// 初始化照片查看器窗口
        /// </summary>
        public PhotoViewerWindow(string imagePath)
        {
            InitializeComponent();
            
            // 设置数据上下文
            DataContext = this;
            
            // 保存图像路径
            ImagePath = imagePath;
            
            // 加载图像
            LoadImage(imagePath);
        }
        
        /// <summary>
        /// 加载图像
        /// </summary>
        private void LoadImage(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    NoImageText.Visibility = Visibility.Visible;
                    return;
                }
                
                // 加载图像
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(path);
                image.EndInit();
                
                // 设置到界面
                MainImage.Source = image;
                NoImageText.Visibility = Visibility.Collapsed;
                
                // 更新图像信息
                ImageInfoText.Text = $"{image.PixelWidth} x {image.PixelHeight}";
                
                // 默认缩放设置为100%
                ZoomComboBox.SelectedIndex = 2; // 100%
            }
            catch (Exception ex)
            {
                NoImageText.Text = $"加载图像失败: {ex.Message}";
                NoImageText.Visibility = Visibility.Visible;
            }
        }
        
        /// <summary>
        /// 设置图像缩放
        /// </summary>
        private void SetZoomLevel(int zoomIndex)
        {
            if (MainImage.Source == null)
                return;
                
            ZoomComboBox.SelectedIndex = zoomIndex;
                
            double zoomLevel = zoomLevels[zoomIndex];
            
            if (zoomLevel < 0) // 适应窗口
            {
                // 获取图像和查看器的大小
                double imageWidth = ((BitmapImage)MainImage.Source).PixelWidth;
                double imageHeight = ((BitmapImage)MainImage.Source).PixelHeight;
                
                // 计算ScrollViewer的可见区域
                System.Windows.Controls.ScrollViewer sv = 
                    MainImage.Parent as System.Windows.Controls.Grid != null ? 
                    (MainImage.Parent as System.Windows.Controls.Grid).Parent as System.Windows.Controls.ScrollViewer : null;
                    
                if (sv != null)
                {
                    double viewWidth = sv.ViewportWidth;
                    double viewHeight = sv.ViewportHeight;
                    
                    // 计算适应窗口的缩放比例
                    double scaleX = viewWidth / imageWidth;
                    double scaleY = viewHeight / imageHeight;
                    
                    // 使用较小的缩放比例，确保图像完全可见
                    double scale = Math.Min(scaleX, scaleY);
                    
                    // 应用缩放
                    ImageScale.ScaleX = scale;
                    ImageScale.ScaleY = scale;
                }
            }
            else // 固定缩放级别
            {
                ImageScale.ScaleX = zoomLevel;
                ImageScale.ScaleY = zoomLevel;
            }
        }
        
        /// <summary>
        /// 键盘事件处理
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close(); // ESC键关闭窗口
            }
            else if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                ZoomIn(); // +号放大
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                ZoomOut(); // -号缩小
            }
            else if (e.Key == Key.R)
            {
                RotateImage(); // R键旋转
            }
        }
        
        /// <summary>
        /// 鼠标滚轮事件处理
        /// </summary>
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ZoomIn(); // 向上滚动放大
            }
            else
            {
                ZoomOut(); // 向下滚动缩小
            }
        }
        
        /// <summary>
        /// 放大图像
        /// </summary>
        private void ZoomIn()
        {
            int currentIndex = ZoomComboBox.SelectedIndex;
            if (currentIndex < zoomLevels.Length - 2) // 不包括"适应窗口"选项
            {
                SetZoomLevel(currentIndex + 1);
            }
        }
        
        /// <summary>
        /// 缩小图像
        /// </summary>
        private void ZoomOut()
        {
            int currentIndex = ZoomComboBox.SelectedIndex;
            if (currentIndex > 0)
            {
                SetZoomLevel(currentIndex - 1);
            }
        }
        
        /// <summary>
        /// 旋转图像
        /// </summary>
        private void RotateImage()
        {
            ImageRotation.Angle = (ImageRotation.Angle + 90) % 360;
        }
        
        // 事件处理器
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
        }
        
        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
        }
        
        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            RotateImage();
        }
        
        private void ZoomComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetZoomLevel(ZoomComboBox.SelectedIndex);
        }

        /// <summary>
        /// 标题栏鼠标拖动事件处理
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 最小化按钮事件处理
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 最大化/还原按钮事件处理
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }
    }
} 