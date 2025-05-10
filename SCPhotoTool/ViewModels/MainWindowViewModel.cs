using System.Windows.Input;
using System.Diagnostics;
using SCPhotoTool.Services;

namespace SCPhotoTool.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly CaptureViewModel _captureViewModel;
        private readonly LibraryViewModel _libraryViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly EditorViewModel _editorViewModel;
        private readonly IGameIntegrationService _gameIntegrationService;
        private readonly AboutViewModel _aboutViewModel;
        
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

        public ICommand NavigateToLibraryCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToEditorCommand { get; }
        public ICommand NavigateToScreenshotCommand { get; }
        public ICommand NavigateToAboutCommand { get; }
        public ICommand ConnectToGameCommand { get; }

        public MainWindowViewModel(
            CaptureViewModel captureViewModel,
            LibraryViewModel libraryViewModel,
            SettingsViewModel settingsViewModel,
            EditorViewModel editorViewModel,
            IGameIntegrationService gameIntegrationService,
            AboutViewModel aboutViewModel)
        {
            _captureViewModel = captureViewModel;
            _libraryViewModel = libraryViewModel;
            _settingsViewModel = settingsViewModel;
            _editorViewModel = editorViewModel;
            _gameIntegrationService = gameIntegrationService;
            _aboutViewModel = aboutViewModel;
            
            // 初始视图为截图视图
            _currentViewModel = _captureViewModel;
            
            // 订阅游戏连接状态变更事件
            _gameIntegrationService.GameConnectionStatusChanged += OnGameConnectionStatusChanged;
            
            // 初始化命令
            NavigateToLibraryCommand = new RelayCommand(_ => CurrentViewModel = _libraryViewModel);
            NavigateToSettingsCommand = new RelayCommand(_ => CurrentViewModel = _settingsViewModel);
            NavigateToEditorCommand = new RelayCommand(_ => CurrentViewModel = _editorViewModel);
            NavigateToScreenshotCommand = new RelayCommand(_ => CurrentViewModel = _captureViewModel);
            NavigateToAboutCommand = new RelayCommand(_ => CurrentViewModel = _aboutViewModel);
            ConnectToGameCommand = new RelayCommand(_ => ConnectToGame());
            
            // 尝试连接游戏
            ConnectToGame();
        }
        
        private void ConnectToGame()
        {
            if (_gameIntegrationService.Connect())
            {
                GameConnectionStatus = "已连接";
            }
            else
            {
                GameConnectionStatus = "未连接";
            }
        }
        
        private void OnGameConnectionStatusChanged(object sender, bool isConnected)
        {
            GameConnectionStatus = isConnected ? "已连接" : "未连接";
        }
    }
} 