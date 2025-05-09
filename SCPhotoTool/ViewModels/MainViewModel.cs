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
        public ICommand ShowAboutWindowCommand { get; }
        public ICommand ConnectToGameCommand { get; }

        private readonly CaptureViewModel _captureViewModel;
        private readonly LibraryViewModel _libraryViewModel;
        private readonly PhotoEditorViewModel _editorViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        public MainViewModel(
            IScreenshotService screenshotService, 
            IPhotoLibraryService photoLibraryService,
            ISettingsService settingsService,
            IGameIntegrationService gameIntegrationService,
            CaptureViewModel captureViewModel,
            LibraryViewModel libraryViewModel,
            PhotoEditorViewModel editorViewModel,
            SettingsViewModel settingsViewModel)
        {
            _screenshotService = screenshotService;
            _photoLibraryService = photoLibraryService;
            _settingsService = settingsService;
            _gameIntegrationService = gameIntegrationService;

            _captureViewModel = captureViewModel;
            _libraryViewModel = libraryViewModel;
            _editorViewModel = editorViewModel;
            _settingsViewModel = settingsViewModel;

            // 默认显示截图工具视图
            CurrentViewModel = _captureViewModel;

            // 初始化命令
            NavigateToScreenshotCommand = new RelayCommand(_ => NavigateToScreenshot());
            NavigateToLibraryCommand = new RelayCommand(_ => NavigateToLibrary());
            NavigateToEditorCommand = new RelayCommand(_ => NavigateToEditor());
            NavigateToSettingsCommand = new RelayCommand(_ => NavigateToSettings());
            ShowAboutWindowCommand = new RelayCommand(_ => ShowAboutWindow());
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

        private void ConnectToGame()
        {
            _gameIntegrationService.Connect();
            UpdateGameConnectionStatus();
        }

        private void UpdateGameConnectionStatus()
        {
            GameConnectionStatus = _gameIntegrationService.IsConnected 
                ? $"已连接 - {_gameIntegrationService.GameVersion}" 
                : "未连接";
        }

        private void ShowAboutWindow()
        {
            var aboutWindow = App.Services.GetService(typeof(AboutWindow)) as AboutWindow;
            if (aboutWindow != null)
            {
                aboutWindow.Owner = App.Current.MainWindow;
                aboutWindow.ShowDialog();
            }
        }
    }
} 