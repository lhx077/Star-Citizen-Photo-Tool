using Microsoft.Win32;
using SCPhotoTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SCPhotoTool.ViewModels
{
    public class PhotoEditorViewModel : ViewModelBase
    {
        private readonly IPhotoInfoService _photoInfoService;
        private readonly ISettingsService _settingsService;
        
        private string _selectedImagePath;
        private BitmapImage _currentImage;
        private bool _isBusy;
        private string _statusMessage = "就绪";
        
        private string _title;
        private string _author;
        private string _location;
        private string _date;
        private string _cameraSettings;
        private string _description;
        private bool _showWatermark;
        
        private FilterType _selectedFilter;
        private CompositionType _selectedComposition;
        private FilmAspectRatio _selectedFilmAspectRatio;
        private CropArea _cropArea;
        private bool _isCropping;
        private ObservableCollection<string> _recentEdits = new ObservableCollection<string>();
        
        public string SelectedImagePath
        {
            get => _selectedImagePath;
            set
            {
                if (SetProperty(ref _selectedImagePath, value))
                {
                    LoadImage(value);
                }
            }
        }
        
        public BitmapImage CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
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
        
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        
        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }
        
        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }
        
        public string Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }
        
        public string CameraSettings
        {
            get => _cameraSettings;
            set => SetProperty(ref _cameraSettings, value);
        }
        
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        
        public bool ShowWatermark
        {
            get => _showWatermark;
            set => SetProperty(ref _showWatermark, value);
        }
        
        public FilterType SelectedFilter
        {
            get => _selectedFilter;
            set => SetProperty(ref _selectedFilter, value);
        }
        
        public CompositionType SelectedComposition
        {
            get => _selectedComposition;
            set => SetProperty(ref _selectedComposition, value);
        }
        
        public FilmAspectRatio SelectedFilmAspectRatio
        {
            get => _selectedFilmAspectRatio;
            set => SetProperty(ref _selectedFilmAspectRatio, value);
        }
        
        public CropArea CropArea
        {
            get => _cropArea;
            set => SetProperty(ref _cropArea, value);
        }
        
        public bool IsCropping
        {
            get => _isCropping;
            set => SetProperty(ref _isCropping, value);
        }
        
        public ObservableCollection<string> RecentEdits
        {
            get => _recentEdits;
            set => SetProperty(ref _recentEdits, value);
        }
        
        public Array AvailableFilters => Enum.GetValues(typeof(FilterType));
        
        public Array AvailableCompositions => Enum.GetValues(typeof(CompositionType));
        
        public Array AvailableFilmAspectRatios => Enum.GetValues(typeof(FilmAspectRatio));
        
        public ICommand OpenImageCommand { get; }
        public ICommand AddInfoCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand AddCompositionGuideCommand { get; }
        public ICommand SaveImageCommand { get; }
        public ICommand OpenEditedImageCommand { get; }
        public ICommand StartCropCommand { get; }
        public ICommand ApplyCropCommand { get; }
        public ICommand CancelCropCommand { get; }
        public ICommand AddFilmGuideCommand { get; }
        
        public PhotoEditorViewModel(IPhotoInfoService photoInfoService, ISettingsService settingsService)
        {
            _photoInfoService = photoInfoService;
            _settingsService = settingsService;
            
            // 初始化命令
            OpenImageCommand = new RelayCommand(_ => OpenImage());
            AddInfoCommand = new RelayCommand(_ => AddPhotoInfo(), _ => !string.IsNullOrEmpty(SelectedImagePath));
            ApplyFilterCommand = new RelayCommand(_ => ApplyFilter(), _ => !string.IsNullOrEmpty(SelectedImagePath));
            AddCompositionGuideCommand = new RelayCommand(_ => AddCompositionGuide(), _ => !string.IsNullOrEmpty(SelectedImagePath));
            SaveImageCommand = new RelayCommand(_ => SaveImage(), _ => CurrentImage != null);
            OpenEditedImageCommand = new RelayCommand(path => OpenEditedImage((string)path), _ => true);
            StartCropCommand = new RelayCommand(_ => StartCrop(), _ => CurrentImage != null && !IsCropping);
            ApplyCropCommand = new RelayCommand(_ => ApplyCrop(), _ => IsCropping && CropArea != null);
            CancelCropCommand = new RelayCommand(_ => CancelCrop(), _ => IsCropping);
            AddFilmGuideCommand = new RelayCommand(_ => AddFilmGuide(), _ => !string.IsNullOrEmpty(SelectedImagePath));
            
            // 初始化默认值
            Author = _settingsService.GetSetting<string>("DefaultAuthor", "");
            Date = DateTime.Now.ToString("yyyy-MM-dd");
            ShowWatermark = _settingsService.GetSetting<bool>("AddProgramWatermark", true);
            SelectedFilmAspectRatio = FilmAspectRatio.Widescreen; // 默认16:9
            IsCropping = false;
        }
        
        private void OpenImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择照片",
                Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp|所有文件|*.*"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedImagePath = openFileDialog.FileName;
            }
        }
        
        private void LoadImage(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                CurrentImage = null;
                return;
            }
            
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                bitmap.Freeze(); // 提高性能
                
                CurrentImage = bitmap;
                StatusMessage = $"已加载图片: {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载图片失败: {ex.Message}";
            }
        }
        
        private async void AddPhotoInfo()
        {
            if (string.IsNullOrEmpty(SelectedImagePath))
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = "正在添加照片信息...";
                
                // 更新设置中的水印状态
                await _settingsService.SaveSettingAsync("AddProgramWatermark", ShowWatermark);
                
                var photoInfo = new PhotoInfo
                {
                    Title = Title,
                    Author = Author,
                    DateTaken = DateTime.TryParse(Date, out var dateTaken) ? dateTaken : DateTime.Now,
                    Location = Location,
                    CameraSettings = CameraSettings,
                    Description = Description
                };
                
                string newPath = await _photoInfoService.AddPhotoInfoAsync(SelectedImagePath, photoInfo);
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    RecentEdits.Insert(0, newPath);
                    StatusMessage = $"已添加照片信息，保存为: {Path.GetFileName(newPath)}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"添加照片信息失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private async void ApplyFilter()
        {
            if (string.IsNullOrEmpty(SelectedImagePath))
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = $"正在应用滤镜 {SelectedFilter}...";
                
                // 更新设置中的水印状态
                await _settingsService.SaveSettingAsync("AddProgramWatermark", ShowWatermark);
                
                string newPath = await _photoInfoService.ApplyFilterAsync(SelectedImagePath, SelectedFilter);
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    RecentEdits.Insert(0, newPath);
                    StatusMessage = $"已应用滤镜，保存为: {Path.GetFileName(newPath)}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"应用滤镜失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private async void AddCompositionGuide()
        {
            if (string.IsNullOrEmpty(SelectedImagePath))
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = $"正在添加构图辅助线 {SelectedComposition}...";
                
                // 更新设置中的水印状态
                await _settingsService.SaveSettingAsync("AddProgramWatermark", ShowWatermark);
                
                string newPath = await _photoInfoService.ShowCompositionGuideAsync(SelectedImagePath, SelectedComposition);
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    RecentEdits.Insert(0, newPath);
                    StatusMessage = $"已添加构图辅助线，保存为: {Path.GetFileName(newPath)}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"添加构图辅助线失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private void SaveImage()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "保存照片",
                Filter = "JPG图像|*.jpg|PNG图像|*.png|BMP图像|*.bmp",
                DefaultExt = ".jpg"
            };
            
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 使用当前加载的图像创建一个新文件
                    BitmapEncoder encoder;
                    string extension = Path.GetExtension(saveFileDialog.FileName).ToLower();
                    
                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            encoder = new JpegBitmapEncoder();
                            break;
                        case ".png":
                            encoder = new PngBitmapEncoder();
                            break;
                        case ".bmp":
                            encoder = new BmpBitmapEncoder();
                            break;
                        default:
                            encoder = new JpegBitmapEncoder();
                            break;
                    }
                    
                    encoder.Frames.Add(BitmapFrame.Create(CurrentImage));
                    
                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                    
                    StatusMessage = $"图像已保存为: {Path.GetFileName(saveFileDialog.FileName)}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"保存图像失败: {ex.Message}";
                }
            }
        }
        
        private void OpenEditedImage(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                SelectedImagePath = path;
            }
        }
        
        private void StartCrop()
        {
            if (CurrentImage == null)
                return;
                
            // 开始裁剪模式
            IsCropping = true;
            
            // 默认裁剪区域为整个图像
            CropArea = new CropArea(0, 0, (int)CurrentImage.Width, (int)CurrentImage.Height);
            
            StatusMessage = "进入裁剪模式，请选择裁剪区域后点击应用裁剪";
        }
        
        private async void ApplyCrop()
        {
            if (CurrentImage == null || CropArea == null || !IsCropping)
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = "正在裁剪图像...";
                
                // 更新设置中的水印状态
                await _settingsService.SaveSettingAsync("AddProgramWatermark", ShowWatermark);
                
                // 创建Services.CropArea实例并传递属性值
                var cropArea = new SCPhotoTool.Services.CropArea(
                    CropArea.X, CropArea.Y, CropArea.Width, CropArea.Height);
                
                string newPath = await _photoInfoService.CropImageAsync(SelectedImagePath, cropArea);
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    RecentEdits.Insert(0, newPath);
                    StatusMessage = $"已裁剪图像，保存为: {Path.GetFileName(newPath)}";
                }
                
                // 退出裁剪模式
                IsCropping = false;
                CropArea = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"裁剪图像失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private void CancelCrop()
        {
            // 退出裁剪模式
            IsCropping = false;
            CropArea = null;
            StatusMessage = "已取消裁剪操作";
        }
        
        private async void AddFilmGuide()
        {
            if (string.IsNullOrEmpty(SelectedImagePath))
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = $"正在添加电影短片辅助线 {SelectedFilmAspectRatio}...";
                
                // 更新设置中的水印状态
                await _settingsService.SaveSettingAsync("AddProgramWatermark", ShowWatermark);
                
                string newPath = await _photoInfoService.CropToAspectRatioAsync(SelectedImagePath, SelectedFilmAspectRatio);
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    RecentEdits.Insert(0, newPath);
                    StatusMessage = $"已按电影比例裁剪图像，保存为: {Path.GetFileName(newPath)}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"添加电影短片辅助线失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}