using SCPhotoTool.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace SCPhotoTool.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IScreenshotService _screenshotService;
        
        private string _screenshotDirectory;
        private string _screenshotFileNameFormat;
        private int _screenshotQuality;
        private string _selectedHotkey;
        private bool _isBusy;
        private string _statusMessage = "就绪";
        private bool _addProgramWatermark = true;
        
        public string ScreenshotDirectory
        {
            get => _screenshotDirectory;
            set => SetProperty(ref _screenshotDirectory, value);
        }

        public string ScreenshotFileNameFormat
        {
            get => _screenshotFileNameFormat;
            set => SetProperty(ref _screenshotFileNameFormat, value);
        }

        public int ScreenshotQuality
        {
            get => _screenshotQuality;
            set => SetProperty(ref _screenshotQuality, value);
        }

        public string SelectedHotkey
        {
            get => _selectedHotkey;
            set
            {
                if (SetProperty(ref _selectedHotkey, value))
                {
                    UpdateHotkey();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        
        public bool AddProgramWatermark
        {
            get => _addProgramWatermark;
            set => SetProperty(ref _addProgramWatermark, value);
        }

        public ObservableCollection<string> AvailableHotkeys { get; } = new ObservableCollection<string>();

        public ICommand SaveSettingsCommand { get; }
        public ICommand BrowseDirectoryCommand { get; }
        public ICommand ResetSettingsCommand { get; }

        public SettingsViewModel(ISettingsService settingsService, IScreenshotService screenshotService)
        {
            _settingsService = settingsService;
            _screenshotService = screenshotService;

            // 初始化命令
            SaveSettingsCommand = new RelayCommand(_ => SaveSettingsAsync());
            BrowseDirectoryCommand = new RelayCommand(_ => BrowseDirectory());
            ResetSettingsCommand = new RelayCommand(_ => ResetSettings());

            // 加载设置
            LoadSettings();
            
            // 初始化可用热键列表
            InitializeHotkeyOptions();
        }

        private void LoadSettings()
        {
            try
            {
                ScreenshotDirectory = _settingsService.GetScreenshotDirectory();
                ScreenshotFileNameFormat = _settingsService.GetScreenshotFileNameFormat();
                ScreenshotQuality = _settingsService.GetScreenshotQuality();
                SelectedHotkey = _settingsService.GetSetting<string>("ScreenshotHotkey", "PrintScreen");
                AddProgramWatermark = _settingsService.GetSetting<bool>("AddProgramWatermark", true);
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载设置失败: {ex.Message}";
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "正在保存设置...";

                // 验证截图目录是否存在
                if (!Directory.Exists(ScreenshotDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(ScreenshotDirectory);
                    }
                    catch
                    {
                        StatusMessage = "截图目录无效，请选择有效目录";
                        return;
                    }
                }

                // 验证质量设置
                if (ScreenshotQuality < 0 || ScreenshotQuality > 100)
                {
                    StatusMessage = "质量设置必须在0-100之间";
                    return;
                }

                // 保存设置
                await _settingsService.SetScreenshotDirectoryAsync(ScreenshotDirectory);
                await _settingsService.SetScreenshotFileNameFormatAsync(ScreenshotFileNameFormat);
                await _settingsService.SetScreenshotQualityAsync(ScreenshotQuality);
                await _settingsService.SaveSettingAsync("ScreenshotHotkey", SelectedHotkey);
                await _settingsService.SaveSettingAsync("AddProgramWatermark", AddProgramWatermark);

                // 更新热键
                _screenshotService.SetHotkey(SelectedHotkey);

                StatusMessage = "设置已保存";
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存设置失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void BrowseDirectory()
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "选择截图保存目录";
                
                if (!string.IsNullOrEmpty(ScreenshotDirectory) && Directory.Exists(ScreenshotDirectory))
                {
                    folderDialog.SelectedPath = ScreenshotDirectory;
                }

                DialogResult result = folderDialog.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    ScreenshotDirectory = folderDialog.SelectedPath;
                }
            }
        }

        private void ResetSettings()
        {
            var result = System.Windows.MessageBox.Show("确定要重置所有设置到默认值吗？", "确认重置", 
                System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // 重置到默认值
                ScreenshotDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    "Star Citizen Photos");
                ScreenshotFileNameFormat = "SC_Photo_{0:yyyyMMdd_HHmmss}";
                ScreenshotQuality = 90;
                SelectedHotkey = "PrintScreen";
                AddProgramWatermark = true;

                // 保存默认设置
                SaveSettingsAsync();
            }
        }

        private void InitializeHotkeyOptions()
        {
            // 添加常用热键选项
            AvailableHotkeys.Add("PrintScreen");
            AvailableHotkeys.Add("F8");
            AvailableHotkeys.Add("F12");
            AvailableHotkeys.Add("Ctrl+P");
            AvailableHotkeys.Add("Ctrl+S");
            AvailableHotkeys.Add("Alt+S");
        }

        private void UpdateHotkey()
        {
            try
            {
                _settingsService.SaveSettingAsync("ScreenshotHotkey", SelectedHotkey);
                _screenshotService.SetHotkey(SelectedHotkey);
                StatusMessage = $"已设置截图热键为: {SelectedHotkey}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"更新热键失败: {ex.Message}";
            }
        }
    }
} 