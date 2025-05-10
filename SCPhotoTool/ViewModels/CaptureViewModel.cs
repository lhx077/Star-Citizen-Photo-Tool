using SCPhotoTool.Models;
using SCPhotoTool.Services;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Windows;
using System.Collections.ObjectModel;

namespace SCPhotoTool.ViewModels
{
    public class CaptureViewModel : ViewModelBase
    {
        private readonly IScreenshotService _screenshotService;
        private readonly IPhotoLibraryService _photoLibraryService;
        private readonly ISettingsService _settingsService;
        private readonly IGameIntegrationService _gameIntegrationService;

        private BitmapImage _previewImage;
        private string _statusMessage = "准备就绪";
        private bool _isBusy;
        private Bitmap _currentBitmap;
        private string _currentHotkey;
        private bool _captureGameWindow = true;
        private bool _autoSave = true;
        private bool _autoRemoveTitleBar = true;
        private bool _addGameInfoWatermark = false;
        private string _watermarkText = "Star Citizen Alpha 4.1 - SC Photo Tool";
        private CaptureMode _captureMode = CaptureMode.GameWindow;
        private ObservableCollection<string> _recentScreenshots;

        public BitmapImage PreviewImage
        {
            get => _previewImage;
            set => SetProperty(ref _previewImage, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string CurrentHotkey
        {
            get => _currentHotkey;
            set => SetProperty(ref _currentHotkey, value);
        }

        public bool CaptureGameWindow
        {
            get => _captureGameWindow;
            set => SetProperty(ref _captureGameWindow, value);
        }

        public bool AutoSave
        {
            get => _autoSave;
            set => SetProperty(ref _autoSave, value);
        }
        
        public bool AutoRemoveTitleBar
        {
            get => _autoRemoveTitleBar;
            set => SetProperty(ref _autoRemoveTitleBar, value);
        }
        
        public bool AddGameInfoWatermark
        {
            get => _addGameInfoWatermark;
            set => SetProperty(ref _addGameInfoWatermark, value);
        }
        
        public string WatermarkText
        {
            get => _watermarkText;
            set => SetProperty(ref _watermarkText, value);
        }
        
        public CaptureMode CaptureMode
        {
            get => _captureMode;
            set => SetProperty(ref _captureMode, value);
        }
        
        public ObservableCollection<string> RecentScreenshots
        {
            get => _recentScreenshots;
            set => SetProperty(ref _recentScreenshots, value);
        }
        
        public bool IsGameConnected => _gameIntegrationService.IsConnected;
        
        public string GameVersionInfo => $"当前游戏版本: {_gameIntegrationService.GameVersion}";

        public ICommand CaptureCommand { get; }
        public ICommand CaptureSelectedAreaCommand { get; }
        public ICommand CaptureFullScreenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddToLibraryCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand OpenGameCommand { get; }
        public ICommand RefreshGameStatusCommand { get; }
        public ICommand OpenRecentCommand { get; }

        public CaptureViewModel(
            IScreenshotService screenshotService,
            IPhotoLibraryService photoLibraryService,
            ISettingsService settingsService,
            IGameIntegrationService gameIntegrationService)
        {
            _screenshotService = screenshotService;
            _photoLibraryService = photoLibraryService;
            _settingsService = settingsService;
            _gameIntegrationService = gameIntegrationService;

            // 初始化集合
            _recentScreenshots = new ObservableCollection<string>();

            // 加载热键设置
            _currentHotkey = _settingsService.GetSetting<string>("ScreenshotHotkey", "PrintScreen");

            // 初始化命令
            CaptureCommand = new RelayCommand(_ => CaptureAsync());
            CaptureSelectedAreaCommand = new RelayCommand(_ => CaptureSelectedAreaAsync());
            CaptureFullScreenCommand = new RelayCommand(_ => CaptureFullScreenAsync());
            SaveCommand = new RelayCommand(_ => SaveScreenshotAsync(), _ => CanSaveScreenshot());
            AddToLibraryCommand = new RelayCommand(_ => AddToLibraryAsync(), _ => CanSaveScreenshot());
            ClearCommand = new RelayCommand(_ => ClearPreview(), _ => CanSaveScreenshot());
            OpenGameCommand = new RelayCommand(_ => OpenGame());
            RefreshGameStatusCommand = new RelayCommand(_ => RefreshGameStatus());
            OpenRecentCommand = new RelayCommand(p => OpenRecentScreenshot(p as string));

            // 订阅热键事件
            _screenshotService.HotkeyTriggered += OnHotkeyTriggered;
            
            // 订阅游戏状态变更事件
            _gameIntegrationService.GameConnectionStatusChanged += OnGameConnectionStatusChanged;

            // 加载设置
            LoadSettings();
            
            // 尝试连接游戏
            _gameIntegrationService.Connect();
        }
        
        private void OnGameConnectionStatusChanged(object sender, bool isConnected)
        {
            // 更新UI状态
            OnPropertyChanged(nameof(IsGameConnected));
            OnPropertyChanged(nameof(GameVersionInfo));
            
            if (isConnected)
            {
                StatusMessage = $"已连接到游戏: {_gameIntegrationService.GameVersion}";
            }
            else
            {
                StatusMessage = "未连接到游戏";
            }
        }

        private void LoadSettings()
        {
            CaptureGameWindow = _settingsService.GetSetting("CaptureGameWindow", true);
            AutoSave = _settingsService.GetSetting("AutoSave", true);
            AutoRemoveTitleBar = _settingsService.GetSetting("AutoRemoveTitleBar", true);
            AddGameInfoWatermark = _settingsService.GetSetting("AddGameInfoWatermark", false);
            WatermarkText = _settingsService.GetSetting("WatermarkText", "Star Citizen Alpha 4.1 - SC Photo Tool");
            CaptureMode = _settingsService.GetSetting("CaptureMode", CaptureMode.GameWindow);
        }

        private void SaveSettings()
        {
            _settingsService.SaveSettingAsync("CaptureGameWindow", CaptureGameWindow);
            _settingsService.SaveSettingAsync("AutoSave", AutoSave);
            _settingsService.SaveSettingAsync("AutoRemoveTitleBar", AutoRemoveTitleBar);
            _settingsService.SaveSettingAsync("AddGameInfoWatermark", AddGameInfoWatermark);
            _settingsService.SaveSettingAsync("WatermarkText", WatermarkText);
            _settingsService.SaveSettingAsync("CaptureMode", CaptureMode);
        }

        private async void CaptureAsync()
        {
            // 根据当前模式选择截图方式
            switch (CaptureMode)
            {
                case CaptureMode.GameWindow:
                    await CaptureGameWindowAsync();
                    break;
                case CaptureMode.SelectedArea:
                    await CaptureSelectedAreaAsync();
                    break;
                case CaptureMode.FullScreen:
                    await CaptureFullScreenAsync();
                    break;
            }
        }
        
        private async Task CaptureGameWindowAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "正在截取游戏窗口...";

                // 检查游戏是否连接
                if (!_gameIntegrationService.IsConnected && !_gameIntegrationService.Connect())
                {
                    StatusMessage = "未找到游戏窗口，改为全屏截图";
                    await CaptureFullScreenAsync();
                    return;
                }

                _currentBitmap = await _screenshotService.CaptureGameWindowAsync();
                
                // 如果需要添加水印
                if (AddGameInfoWatermark)
                {
                    string watermark = WatermarkText;
                    
                    // 如果水印文本包含占位符，则替换为实际游戏版本
                    if (watermark.Contains("{version}"))
                    {
                        watermark = watermark.Replace("{version}", _gameIntegrationService.GameVersion);
                    }
                    
                    _currentBitmap = _screenshotService.AddWatermark(_currentBitmap, watermark);
                }
                
                StatusMessage = "游戏窗口截图完成";

                // 更新预览
                PreviewImage = BitmapToImageSource(_currentBitmap);

                // 如果设置了自动保存，则保存截图
                if (AutoSave)
                {
                    await SaveScreenshotAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"截图失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private async Task CaptureSelectedAreaAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "请选择截图区域...";

                _currentBitmap = await _screenshotService.CaptureSelectedAreaAsync();
                
                // 如果需要添加水印
                if (AddGameInfoWatermark)
                {
                    _currentBitmap = _screenshotService.AddWatermark(_currentBitmap, WatermarkText);
                }
                
                StatusMessage = "区域截图完成";

                // 更新预览
                PreviewImage = BitmapToImageSource(_currentBitmap);

                // 如果设置了自动保存，则保存截图
                if (AutoSave)
                {
                    await SaveScreenshotAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"截图失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private async Task CaptureFullScreenAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "正在截取全屏...";

                _currentBitmap = await _screenshotService.CaptureScreenAsync();
                
                // 如果需要添加水印
                if (AddGameInfoWatermark)
                {
                    _currentBitmap = _screenshotService.AddWatermark(_currentBitmap, WatermarkText);
                }
                
                StatusMessage = "全屏截图完成";

                // 更新预览
                PreviewImage = BitmapToImageSource(_currentBitmap);

                // 如果设置了自动保存，则保存截图
                if (AutoSave)
                {
                    await SaveScreenshotAsync();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"截图失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveScreenshotAsync()
        {
            if (_currentBitmap == null || IsBusy)
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "正在保存...";

                string savedPath = await _screenshotService.SaveScreenshotAsync(_currentBitmap);
                
                // 添加到最近截图列表
                if (!_recentScreenshots.Contains(savedPath))
                {
                    _recentScreenshots.Insert(0, savedPath);
                    
                    // 限制列表大小
                    while (_recentScreenshots.Count > 10)
                    {
                        _recentScreenshots.RemoveAt(_recentScreenshots.Count - 1);
                    }
                }
                
                StatusMessage = $"截图已保存至: {savedPath}";

                // 如果自动保存，也自动添加到库中
                if (AutoSave)
                {
                    // 添加游戏版本标签
                    var tags = new[] { "截图", "Star Citizen" };
                    if (_gameIntegrationService.IsConnected)
                    {
                        tags = new[] { "截图", "Star Citizen", _gameIntegrationService.GameVersion };
                    }
                    
                    await _photoLibraryService.AddPhotoAsync(savedPath, tags);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddToLibraryAsync()
        {
            if (_currentBitmap == null || IsBusy)
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "正在添加到库...";

                // 先保存图片
                string savedPath = await _screenshotService.SaveScreenshotAsync(_currentBitmap);
                
                // 添加游戏版本标签
                var tags = new[] { "截图", "Star Citizen" };
                if (_gameIntegrationService.IsConnected)
                {
                    tags = new[] { "截图", "Star Citizen", _gameIntegrationService.GameVersion };
                }
                
                // 添加到照片库
                Photo photo = await _photoLibraryService.AddPhotoAsync(savedPath, tags);
                
                StatusMessage = "截图已添加到照片库";
            }
            catch (Exception ex)
            {
                StatusMessage = $"添加到库失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearPreview()
        {
            _currentBitmap?.Dispose();
            _currentBitmap = null;
            PreviewImage = null;
            StatusMessage = "准备就绪";
        }

        private bool CanSaveScreenshot()
        {
            return _currentBitmap != null && !IsBusy;
        }

        private void OnHotkeyTriggered(object sender, EventArgs e)
        {
            // 在UI线程上执行截图
            Application.Current.Dispatcher.Invoke(CaptureAsync);
        }
        
        private void OpenGame()
        {
            try
            {
                // 尝试连接游戏
                if (_gameIntegrationService.Connect())
                {
                    StatusMessage = $"已连接到游戏: {_gameIntegrationService.GameVersion}";
                }
                else
                {
                    // 如果无法连接，尝试启动游戏
                    StatusMessage = "尝试启动游戏...";
                    // 这里添加启动游戏的逻辑
                    
                    StatusMessage = "请启动星际公民后再尝试连接";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"连接游戏失败: {ex.Message}";
            }
        }
        
        private void RefreshGameStatus()
        {
            // 刷新游戏连接状态
            if (_gameIntegrationService.IsConnected)
            {
                // 如果当前已连接，断开后重新连接
                _gameIntegrationService.Disconnect();
            }
            
            if (_gameIntegrationService.Connect())
            {
                StatusMessage = $"已连接到游戏: {_gameIntegrationService.GameVersion}";
            }
            else
            {
                StatusMessage = "未找到游戏";
            }
        }
        
        private void OpenRecentScreenshot(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;
            
            try
            {
                // 使用默认图片查看器打开
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"打开图片失败: {ex.Message}";
            }
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // 重要：使图像可以跨线程访问
                return bitmapImage;
            }
        }
    }
    
    public enum CaptureMode
    {
        GameWindow,
        SelectedArea,
        FullScreen
    }
} 