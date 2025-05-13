using SCPhotoTool.Services;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SCPhotoTool.Views
{
    /// <summary>
    /// 区域选择窗口的交互逻辑
    /// </summary>
    public partial class AreaSelectWindow : Window
    {
        private bool _isSelecting = false;
        private System.Windows.Point _startPoint;
        private System.Windows.Point _currentPoint;
        private double _dpiScaleX = 1.0;
        private double _dpiScaleY = 1.0;
        
        /// <summary>
        /// 用户选择的区域
        /// </summary>
        public AreaSelectedInfo SelectedArea { get; private set; }
        
        /// <summary>
        /// 初始化区域选择窗口
        /// </summary>
        /// <param name="screenBitmap">屏幕截图用作背景</param>
        public AreaSelectWindow(Bitmap screenBitmap)
        {
            InitializeComponent();
            
            // 设置窗口尺寸为全屏
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
            
            // 获取DPI缩放因子
            GetDpiScale();
            
            // 设置背景图像
            BackgroundImage.Source = ConvertBitmapToBitmapSource(screenBitmap);
            
            // 初始化选择区域
            SelectedArea = null;
            
            // 确保窗口覆盖整个屏幕
            WindowState = WindowState.Maximized;
        }
        
        /// <summary>
        /// 获取DPI缩放因子
        /// </summary>
        private void GetDpiScale()
        {
            try
            {
                PresentationSource source = PresentationSource.FromVisual(this);
                if (source != null && source.CompositionTarget != null)
                {
                    _dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                    _dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
                    System.Diagnostics.Debug.WriteLine($"DPI缩放: X={_dpiScaleX}, Y={_dpiScaleY}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取DPI缩放失败: {ex.Message}");
                _dpiScaleX = 1.0;
                _dpiScaleY = 1.0;
            }
        }
        
        /// <summary>
        /// 开始区域选择
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isSelecting = true;
                _startPoint = e.GetPosition(this);
                _currentPoint = _startPoint;
                
                // 显示选择边框
                UpdateSelectionUI();
            }
        }
        
        /// <summary>
        /// 更新选择区域
        /// </summary>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isSelecting && e.LeftButton == MouseButtonState.Pressed)
            {
                _currentPoint = e.GetPosition(this);
                UpdateSelectionUI();
            }
        }
        
        /// <summary>
        /// 完成选择
        /// </summary>
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                _currentPoint = e.GetPosition(this);
                UpdateSelectionUI();
            }
        }
        
        /// <summary>
        /// 键盘处理（Enter确认，Esc取消）
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 确认选择
                if (SelectionBorder.Visibility == Visibility.Visible)
                {
                    // 计算选区坐标
                    double left = Canvas.GetLeft(SelectionBorder);
                    double top = Canvas.GetTop(SelectionBorder);
                    double width = SelectionBorder.Width;
                    double height = SelectionBorder.Height;
                    
                    // 如果选区太小，则取消选择
                    if (width < 10 || height < 10)
                    {
                        MessageBox.Show("选择区域太小，请重新选择", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    
                    // 保存选择区域，应用DPI缩放
                    SelectedArea = new AreaSelectedInfo(
                        (int)Math.Round(left),
                        (int)Math.Round(top),
                        (int)Math.Round(width),
                        (int)Math.Round(height));
                    
                    DialogResult = true;
                    Close();
                }
            }
            else if (e.Key == Key.Escape)
            {
                // 取消选择
                DialogResult = false;
                Close();
            }
        }
        
        /// <summary>
        /// 更新选择区域UI
        /// </summary>
        private void UpdateSelectionUI()
        {
            // 计算选区坐标
            double left = Math.Min(_startPoint.X, _currentPoint.X);
            double top = Math.Min(_startPoint.Y, _currentPoint.Y);
            double width = Math.Abs(_currentPoint.X - _startPoint.X);
            double height = Math.Abs(_currentPoint.Y - _startPoint.Y);
            
            // 更新选区边框
            SelectionBorder.Visibility = Visibility.Visible;
            Canvas.SetLeft(SelectionBorder, left);
            Canvas.SetTop(SelectionBorder, top);
            SelectionBorder.Width = width;
            SelectionBorder.Height = height;
            
            // 更新选区标记点
            UpdateMarkers(left, top, width, height);
            
            // 更新信息文本
            SelectionInfoText.Text = $"尺寸: {(int)width} x {(int)height} | 位置: ({(int)left}, {(int)top})";
        }
        
        /// <summary>
        /// 更新选区标记点位置
        /// </summary>
        private void UpdateMarkers(double left, double top, double width, double height)
        {
            // 左上角
            TopLeftMarker.Visibility = Visibility.Visible;
            Canvas.SetLeft(TopLeftMarker, left - 4);
            Canvas.SetTop(TopLeftMarker, top - 4);
            
            // 右上角
            TopRightMarker.Visibility = Visibility.Visible;
            Canvas.SetLeft(TopRightMarker, left + width - 4);
            Canvas.SetTop(TopRightMarker, top - 4);
            
            // 左下角
            BottomLeftMarker.Visibility = Visibility.Visible;
            Canvas.SetLeft(BottomLeftMarker, left - 4);
            Canvas.SetTop(BottomLeftMarker, top + height - 4);
            
            // 右下角
            BottomRightMarker.Visibility = Visibility.Visible;
            Canvas.SetLeft(BottomRightMarker, left + width - 4);
            Canvas.SetTop(BottomRightMarker, top + height - 4);
        }
        
        /// <summary>
        /// 将Bitmap转换为BitmapSource
        /// </summary>
        private BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            var handle = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(
                    handle,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                // 释放GDI+位图句柄
                DeleteObject(handle);
            }
        }
        
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
} 