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

        public ICommand CaptureCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddToLibraryCommand { get; }
        public ICommand ClearCommand { get; }

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

            // 加载热键设置
            _currentHotkey = _settingsService.GetSetting<string>("ScreenshotHotkey", "PrintScreen");

            // 初始化命令
            CaptureCommand = new RelayCommand(_ => CaptureAsync());
            SaveCommand = new RelayCommand(_ => SaveScreenshotAsync(), _ => CanSaveScreenshot());
            AddToLibraryCommand = new RelayCommand(_ => AddToLibraryAsync(), _ => CanSaveScreenshot());
            ClearCommand = new RelayCommand(_ => ClearPreview(), _ => CanSaveScreenshot());

            // 订阅热键事件
            _screenshotService.HotkeyTriggered += OnHotkeyTriggered;

            // 加载设置
            LoadSettings();
        }

        private void LoadSettings()
        {
            CaptureGameWindow = _settingsService.GetSetting("CaptureGameWindow", true);
            AutoSave = _settingsService.GetSetting("AutoSave", true);
        }

        private void SaveSettings()
        {
            _settingsService.SaveSettingAsync("CaptureGameWindow", CaptureGameWindow);
            _settingsService.SaveSettingAsync("AutoSave", AutoSave);
        }

        private async void CaptureAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "正在截图...";

                // 根据设置选择截图方式
                if (CaptureGameWindow && _gameIntegrationService.IsConnected)
                {
                    _currentBitmap = await _screenshotService.CaptureGameWindowAsync();
                    StatusMessage = "游戏窗口截图完成";
                }
                else
                {
                    _currentBitmap = await _screenshotService.CaptureScreenAsync();
                    StatusMessage = "全屏截图完成";
                }

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
                
                StatusMessage = $"截图已保存至: {savedPath}";

                // 如果自动保存，也自动添加到库中
                if (AutoSave)
                {
                    await _photoLibraryService.AddPhotoAsync(savedPath, new[] { "截图", "Star Citizen" });
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
                
                // 添加到照片库
                Photo photo = await _photoLibraryService.AddPhotoAsync(savedPath, new[] { "截图", "Star Citizen" });
                
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
} 