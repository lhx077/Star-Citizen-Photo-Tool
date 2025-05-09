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
        private ServiceProvider serviceProvider;
        
        public static ServiceProvider Services { get; private set; }

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
            Services = serviceProvider;
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
            services.AddTransient<MainViewModel>();
            services.AddTransient<CaptureViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<EditorViewModel>();
            services.AddTransient<LibraryViewModel>();
            services.AddTransient<PhotoEditorViewModel>();
            
            // 注册视图
            services.AddTransient<PhotoEditorView>();
            services.AddTransient<SettingsView>();
            services.AddTransient<AboutWindow>();
            
            // 注册主窗口
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow.DataContext = serviceProvider.GetService<MainViewModel>();
            mainWindow.Show();
        }
    }
} 