using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace SCPhotoTool
{
    public partial class MainWindow : Window
    {
        private Random random = new Random();
        private DispatcherTimer starAnimationTimer;
        private List<Ellipse> stars = new List<Ellipse>();
        private DispatcherTimer _resizeTimer;
        
        public ViewModels.MainViewModel MainViewModel { get; }
        
        public MainWindow()
        {
            InitializeComponent();
            
            // 初始化星空背景
            InitializeStarryBackground();
            
            // 启动星星闪烁动画（使用较低的频率）
            starAnimationTimer = new DispatcherTimer();
            starAnimationTimer.Interval = TimeSpan.FromMilliseconds(200); // 降低频率，减少卡顿
            starAnimationTimer.Tick += StarAnimationTimer_Tick;
            starAnimationTimer.Start();
            
            // 获取视图模型
            try
            {
                // 尝试从静态服务获取
                MainViewModel = App.Services?.GetService<ViewModels.MainViewModel>();
                
                // 如果获取失败，尝试从应用程序实例获取
                if (MainViewModel == null && Application.Current is App app)
                {
                    MainViewModel = app.ServiceProvider.GetService<ViewModels.MainViewModel>();
                    
                    if (MainViewModel == null)
                    {
                        // 无法获取视图模型
                        MessageBox.Show("无法初始化应用程序服务。应用程序将以有限功能运行。", 
                                    "初始化警告", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            // 设置数据上下文
            DataContext = this;
            
            // 窗口大小改变时重新创建星空（添加延迟优化）
            this.SizeChanged += (s, e) => 
            {
                if (_resizeTimer != null)
                {
                    _resizeTimer.Stop();
                }
                else
                {
                    _resizeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
                    _resizeTimer.Tick += (sender, args) => 
                    {
                        _resizeTimer.Stop();
                        InitializeStarryBackground();
                    };
                }
                _resizeTimer.Start();
            };
        }
        
        private void InitializeStarryBackground()
        {
            // 清空现有的星星
            if (StarCanvas == null)
                return;
                
            StarCanvas.Children.Clear();
            stars.Clear();
            
            // 添加新的星星（减少数量）
            int starCount = 80; // 减少星星数量，提高性能
            
            for (int i = 0; i < starCount; i++)
            {
                double x = random.NextDouble() * this.ActualWidth;
                double y = random.NextDouble() * this.ActualHeight;
                double size = random.NextDouble() * 2 + 1; // 1-3 像素大小
                double opacity = random.NextDouble() * 0.5 + 0.3; // 0.3-0.8 不透明度
                
                var star = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = new SolidColorBrush(Colors.White),
                    Opacity = opacity
                };
                
                System.Windows.Controls.Canvas.SetLeft(star, x);
                System.Windows.Controls.Canvas.SetTop(star, y);
                System.Windows.Controls.Canvas.SetZIndex(star, -1);
                
                StarCanvas.Children.Add(star);
                stars.Add(star);
            }
        }
        
        private void StarAnimationTimer_Tick(object sender, EventArgs e)
        {
            // 只对部分星星进行闪烁动画，减少性能开销
            int animateCount = Math.Min(10, stars.Count);
            for (int i = 0; i < animateCount; i++)
            {
                int index = random.Next(stars.Count);
                if (index < stars.Count)
                {
                    // 随机改变星星的不透明度，制造闪烁效果
                    double newOpacity = random.NextDouble() * 0.5 + 0.3; // 0.3-0.8
                    stars[index].Opacity = newOpacity;
                }
            }
        }
        
        // 标题栏拖动
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        
        // 窗口控制方法
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        
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
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 