using Microsoft.Extensions.DependencyInjection;
using SCPhotoTool.Services;
using SCPhotoTool.ViewModels;
using SCPhotoTool.Views;
using System;
using System.Windows;

namespace SCPhotoTool
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        
        public ServiceProvider ServiceProvider => _serviceProvider;
        
        public static ServiceProvider Services { get; private set; }

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            Services = _serviceProvider;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // 注册服务
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IScreenshotService, ScreenshotService>();
            services.AddSingleton<IGameIntegrationService, GameIntegrationService>();
            services.AddSingleton<IPhotoLibraryService, PhotoLibraryService>();
            services.AddSingleton<IPhotoInfoService, PhotoInfoService>();
            
            // 注册视图模型
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<CaptureViewModel>();
            services.AddSingleton<LibraryViewModel>();
            services.AddSingleton<EditorViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<PhotoEditorViewModel>();
            services.AddSingleton<AboutViewModel>();
            
            // 注册视图
            services.AddTransient<PhotoEditorView>();
            services.AddTransient<SettingsView>();
            
            // 注册主窗口
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            try
            {
                // 加载资源字典
                LoadResourceDictionaries();
                
                // 创建并显示主窗口
                CreateAndShowMainWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用程序启动错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }
        
        private void LoadResourceDictionaries()
        {
            // 确保资源被正确加载
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/SCPhotoTool;component/Resources/Converters.xaml", UriKind.Relative)
            };
            
            if (!Application.Current.Resources.MergedDictionaries.Contains(resourceDictionary))
            {
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
        }
        
        private void CreateAndShowMainWindow()
        {
            // 确保主窗口在创建前所有服务都已正确初始化
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            
            // 显式设置DataContext
            mainWindow.DataContext = mainViewModel;
            
            // 设置为应用程序主窗口并显示
            Current.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
} 