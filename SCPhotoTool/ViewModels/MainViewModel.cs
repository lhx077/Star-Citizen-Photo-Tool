using SCPhotoTool.Services;
using SCPhotoTool.Views;
using System.Windows.Input;
using System;

namespace SCPhotoTool.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IScreenshotService _screenshotService;
        private readonly IPhotoLibraryService _photoLibraryService;
        private readonly ISettingsService _settingsService;
        private readonly IGameIntegrationService _gameIntegrationService;

        private ViewModelBase _currentViewModel;
        private readonly CaptureViewModel _captureViewModel;
        private readonly LibraryViewModel _libraryViewModel;
        private readonly EditorViewModel _editorViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly AboutViewModel _aboutViewModel;
        private string _gameConnectionStatus = "未连接";

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public string GameConnectionStatus
        {
            get => _gameConnectionStatus;
            set => SetProperty(ref _gameConnectionStatus, value);
        }

        public ICommand NavigateToScreenshotCommand { get; }
        public ICommand NavigateToLibraryCommand { get; }
        public ICommand NavigateToEditorCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToAboutCommand { get; }
        public ICommand ConnectToGameCommand { get; }

        public MainViewModel(
            IScreenshotService screenshotService, 
            IPhotoLibraryService photoLibraryService,
            ISettingsService settingsService,
            IGameIntegrationService gameIntegrationService,
            CaptureViewModel captureViewModel,
            LibraryViewModel libraryViewModel,
            EditorViewModel editorViewModel,
            SettingsViewModel settingsViewModel,
            AboutViewModel aboutViewModel)
        {
            _screenshotService = screenshotService;
            _photoLibraryService = photoLibraryService;
            _settingsService = settingsService;
            _gameIntegrationService = gameIntegrationService;

            _captureViewModel = captureViewModel;
            _libraryViewModel = libraryViewModel;
            _editorViewModel = editorViewModel;
            _settingsViewModel = settingsViewModel;
            _aboutViewModel = aboutViewModel;

            // 默认显示截图工具视图
            CurrentViewModel = _captureViewModel;

            // 初始化命令
            NavigateToScreenshotCommand = new RelayCommand(_ => NavigateToScreenshot());
            NavigateToLibraryCommand = new RelayCommand(_ => NavigateToLibrary());
            NavigateToEditorCommand = new RelayCommand(_ => NavigateToEditor());
            NavigateToSettingsCommand = new RelayCommand(_ => NavigateToSettings());
            NavigateToAboutCommand = new RelayCommand(_ => NavigateToAbout());
            ConnectToGameCommand = new RelayCommand(_ => ConnectToGame());

            // 初始化游戏连接状态
            UpdateGameConnectionStatus();
        }

        private void NavigateToScreenshot()
        {
            CurrentViewModel = _captureViewModel;
        }

        private void NavigateToLibrary()
        {
            CurrentViewModel = _libraryViewModel;
        }

        private void NavigateToEditor()
        {
            CurrentViewModel = _editorViewModel;
        }

        private void NavigateToSettings()
        {
            CurrentViewModel = _settingsViewModel;
        }

        private void NavigateToAbout()
        {
            CurrentViewModel = _aboutViewModel;
        }

        private void ConnectToGame()
        {
            // 这里实现游戏连接逻辑
            // 暂时只更新状态
            GameConnectionStatus = GameConnectionStatus == "已连接" ? "未连接" : "已连接";
        }

        private void UpdateGameConnectionStatus()
        {
            GameConnectionStatus = _gameIntegrationService.IsConnected 
                ? $"已连接 - {_gameIntegrationService.GameVersion}" 
                : "未连接";
        }
    }
} 