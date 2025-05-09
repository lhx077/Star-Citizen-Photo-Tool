using SCPhotoTool.Models;
using SCPhotoTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SCPhotoTool.ViewModels
{
    public class EditorViewModel : ViewModelBase
    {
        private readonly IPhotoLibraryService _photoLibraryService;
        private readonly IPhotoInfoService _photoInfoService;
        private readonly ISettingsService _settingsService;

        private Photo _currentPhoto;
        private BitmapImage _currentImage;
        private string _statusMessage = "请从照片库中选择照片进行编辑";
        private bool _isBusy;
        private int _brightness;
        private int _contrast;
        private int _saturation;
        private bool _hasUnsavedChanges;
        private Bitmap _originalBitmap;
        private Bitmap _editedBitmap;
        private string _selectedImagePath;
        private bool _showWatermark = true;
        private bool _isCropping;
        private CropArea _cropArea;
        private FilterType _selectedFilter;
        private CompositionType _selectedComposition;
        private FilmAspectRatio _selectedFilmAspectRatio;
        private ObservableCollection<string> _recentEdits;
        private string _title;
        private string _author;
        private string _date;
        private string _location;
        private string _cameraSettings;
        private string _description;

        public BitmapImage CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
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

        public int Brightness
        {
            get => _brightness;
            set
            {
                if (SetProperty(ref _brightness, value))
                {
                    ApplyImageEffects();
                }
            }
        }

        public int Contrast
        {
            get => _contrast;
            set
            {
                if (SetProperty(ref _contrast, value))
                {
                    ApplyImageEffects();
                }
            }
        }

        public int Saturation
        {
            get => _saturation;
            set
            {
                if (SetProperty(ref _saturation, value))
                {
                    ApplyImageEffects();
                }
            }
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set => SetProperty(ref _hasUnsavedChanges, value);
        }

        public bool HasLoadedPhoto => _currentPhoto != null;

        public string SelectedImagePath
        {
            get => _selectedImagePath;
            set => SetProperty(ref _selectedImagePath, value);
        }

        public bool ShowWatermark
        {
            get => _showWatermark;
            set => SetProperty(ref _showWatermark, value);
        }

        public bool IsCropping
        {
            get => _isCropping;
            set => SetProperty(ref _isCropping, value);
        }

        public CropArea CropArea
        {
            get => _cropArea;
            set => SetProperty(ref _cropArea, value);
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

        public ObservableCollection<string> RecentEdits
        {
            get => _recentEdits;
            set => SetProperty(ref _recentEdits, value);
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

        public string Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
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

        public List<FilterType> AvailableFilters { get; }
        public List<CompositionType> AvailableCompositions { get; }
        public List<FilmAspectRatio> AvailableFilmAspectRatios { get; }

        public ICommand SaveChangesCommand { get; }
        public ICommand ResetChangesCommand { get; }
        public ICommand OpenPhotoCommand { get; }
        public ICommand ExportPhotoCommand { get; }
        public ICommand OpenImageCommand { get; }
        public ICommand SaveImageCommand { get; }
        public ICommand StartCropCommand { get; }
        public ICommand ApplyCropCommand { get; }
        public ICommand CancelCropCommand { get; }
        public ICommand AddInfoCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand AddCompositionGuideCommand { get; }
        public ICommand AddFilmGuideCommand { get; }
        public ICommand OpenEditedImageCommand { get; }

        public EditorViewModel(IPhotoLibraryService photoLibraryService, IPhotoInfoService photoInfoService, ISettingsService settingsService)
        {
            _photoLibraryService = photoLibraryService;
            _photoInfoService = photoInfoService;
            _settingsService = settingsService;

            // 初始化集合
            RecentEdits = new ObservableCollection<string>();

            // 获取可用滤镜和构图类型
            AvailableFilters = new List<FilterType> {
                FilterType.BlackAndWhite,
                FilterType.Sepia, 
                FilterType.HighContrast,
                FilterType.Vintage,
                FilterType.ColdBlue,
                FilterType.WarmOrange
            };
            
            AvailableCompositions = new List<CompositionType> {
                CompositionType.RuleOfThirds,
                CompositionType.GoldenRatio,
                CompositionType.Diagonal,
                CompositionType.Grid
            };
            
            AvailableFilmAspectRatios = new List<FilmAspectRatio> {
                FilmAspectRatio.Standard,
                FilmAspectRatio.Widescreen,
                FilmAspectRatio.CinemaScope,
                FilmAspectRatio.IMAX,
                FilmAspectRatio.Anamorphic,
                FilmAspectRatio.Academy
            };
            
            // 设置默认值
            Brightness = 0;
            Contrast = 0;
            Saturation = 0;
            SelectedFilter = FilterType.BlackAndWhite;
            SelectedComposition = CompositionType.RuleOfThirds;
            SelectedFilmAspectRatio = FilmAspectRatio.Widescreen;
            
            // 初始化水印设置
            ShowWatermark = _settingsService.GetSetting<bool>("AddProgramWatermark", true);
            
            // 初始化裁剪区域
            CropArea = new CropArea(0, 0, 0, 0);

            // 初始化命令
            SaveChangesCommand = new RelayCommand(_ => SaveChangesAsync(), _ => CanSaveChanges());
            ResetChangesCommand = new RelayCommand(_ => ResetChanges(), _ => HasLoadedPhoto);
            OpenPhotoCommand = new RelayCommand(_ => OpenPhotoDialog());
            ExportPhotoCommand = new RelayCommand(_ => ExportPhoto(), _ => HasLoadedPhoto);
            OpenImageCommand = new RelayCommand(_ => OpenImageDialog());
            SaveImageCommand = new RelayCommand(_ => SaveImage(), _ => CanEditImage());
            StartCropCommand = new RelayCommand(_ => StartCrop(), _ => CanEditImage());
            ApplyCropCommand = new RelayCommand(_ => ApplyCrop(), _ => IsCropping);
            CancelCropCommand = new RelayCommand(_ => CancelCrop(), _ => IsCropping);
            AddInfoCommand = new RelayCommand(_ => AddInfoAsync(), _ => CanEditImage());
            ApplyFilterCommand = new RelayCommand(_ => ApplyFilterAsync(), _ => CanEditImage());
            AddCompositionGuideCommand = new RelayCommand(_ => AddCompositionGuideAsync(), _ => CanEditImage());
            AddFilmGuideCommand = new RelayCommand(_ => AddFilmGuideAsync(), _ => CanEditImage());
            OpenEditedImageCommand = new RelayCommand(p => OpenEditedImage(p as string));
        }

        public async Task LoadPhotoAsync(string photoId)
        {
            if (string.IsNullOrEmpty(photoId))
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "正在加载照片...";

                // 获取照片信息
                var photos = await _photoLibraryService.GetAllPhotosAsync();
                _currentPhoto = null;
                
                foreach (var photo in photos)
                {
                    if (photo.Id == photoId)
                    {
                        _currentPhoto = photo;
                        break;
                    }
                }

                if (_currentPhoto == null)
                {
                    StatusMessage = "找不到指定的照片";
                    return;
                }

                // 加载原始图像
                if (File.Exists(_currentPhoto.FilePath))
                {
                    _originalBitmap = new Bitmap(_currentPhoto.FilePath);
                    _editedBitmap = new Bitmap(_originalBitmap);
                    CurrentImage = BitmapToImageSource(_editedBitmap);
                    
                    // 重置编辑参数
                    Brightness = 0;
                    Contrast = 0;
                    Saturation = 0;
                    HasUnsavedChanges = false;
                    
                    StatusMessage = $"已加载: {_currentPhoto.Title}";
                }
                else
                {
                    StatusMessage = "照片文件不存在";
                    _currentPhoto = null;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载照片失败: {ex.Message}";
                _currentPhoto = null;
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(HasLoadedPhoto));
            }
        }

        private async Task SaveChangesAsync()
        {
            if (_currentPhoto == null || _editedBitmap == null)
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "正在保存更改...";

                // 保存编辑后的图像到原始文件
                _editedBitmap.Save(_currentPhoto.FilePath);

                // 更新照片库中的元数据
                Dictionary<string, string> metadata = new Dictionary<string, string>
                {
                    { "EditedBrightness", Brightness.ToString() },
                    { "EditedContrast", Contrast.ToString() },
                    { "EditedSaturation", Saturation.ToString() },
                    { "LastEdited", DateTime.Now.ToString() }
                };

                // 更新照片记录
                await _photoLibraryService.UpdatePhotoAsync(_currentPhoto.Id, _currentPhoto.Tags, _currentPhoto.Description);

                HasUnsavedChanges = false;
                StatusMessage = "更改已保存";
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

        private void ResetChanges()
        {
            if (_originalBitmap == null)
                return;

            Brightness = 0;
            Contrast = 0;
            Saturation = 0;

            // 重新创建编辑中的位图
            _editedBitmap?.Dispose();
            _editedBitmap = new Bitmap(_originalBitmap);
            CurrentImage = BitmapToImageSource(_editedBitmap);

            HasUnsavedChanges = false;
            StatusMessage = "已重置所有更改";
        }

        private void ApplyImageEffects()
        {
            if (_originalBitmap == null)
                return;

            try
            {
                // 应用图像效果处理
                // 注意：实际应用中，这里应该实现较为复杂的图像处理算法
                // 这里为了简单演示，仅采用简单方法

                // 重新创建编辑中的位图
                _editedBitmap?.Dispose();
                _editedBitmap = new Bitmap(_originalBitmap);

                // 在实际应用中，应该使用更高效的图像处理库
                // 这里仅为演示简单地调整每个像素
                for (int y = 0; y < _editedBitmap.Height; y++)
                {
                    for (int x = 0; x < _editedBitmap.Width; x++)
                    {
                        Color originalColor = _originalBitmap.GetPixel(x, y);
                        
                        // 简单的亮度、对比度和饱和度调整
                        float brightnessF = 1.0f + (Brightness / 100.0f);
                        float contrastF = 1.0f + (Contrast / 100.0f);
                        float saturationF = 1.0f + (Saturation / 100.0f);
                        
                        // 计算新的RGB值
                        int r = AdjustColorComponent(originalColor.R, brightnessF, contrastF);
                        int g = AdjustColorComponent(originalColor.G, brightnessF, contrastF);
                        int b = AdjustColorComponent(originalColor.B, brightnessF, contrastF);
                        
                        // 应用饱和度
                        if (saturationF != 1.0f)
                        {
                            float gray = (r * 0.3f + g * 0.59f + b * 0.11f);
                            r = (int)(gray + saturationF * (r - gray));
                            g = (int)(gray + saturationF * (g - gray));
                            b = (int)(gray + saturationF * (b - gray));
                            
                            r = Math.Max(0, Math.Min(255, r));
                            g = Math.Max(0, Math.Min(255, g));
                            b = Math.Max(0, Math.Min(255, b));
                        }
                        
                        _editedBitmap.SetPixel(x, y, Color.FromArgb(originalColor.A, r, g, b));
                    }
                }

                // 更新UI
                CurrentImage = BitmapToImageSource(_editedBitmap);
                HasUnsavedChanges = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"应用效果失败: {ex.Message}";
            }
        }

        private int AdjustColorComponent(int color, float brightness, float contrast)
        {
            // 亮度和对比度调整
            float value = color / 255.0f;
            value = (value - 0.5f) * contrast + 0.5f;
            value = value * brightness;
            value = Math.Max(0.0f, Math.Min(1.0f, value));
            
            return (int)(value * 255);
        }

        private void OpenPhotoDialog()
        {
            // 在实际应用中，这里应该打开一个图片选择对话框
            // 由于WPF应用中通常使用消息或导航服务来处理跨视图通信，
            // 这里只是一个简单的提示消息
            StatusMessage = "请从照片库中选择一张照片";
        }

        private void ExportPhoto()
        {
            if (_editedBitmap == null || _currentPhoto == null)
                return;
                
            try
            {
                // 在实际应用中，这里应该打开一个保存文件对话框
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string exportName = $"SC_Edited_{Path.GetFileNameWithoutExtension(_currentPhoto.FilePath)}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string exportPath = Path.Combine(desktopPath, exportName);
                
                _editedBitmap.Save(exportPath, ImageFormat.Png);
                
                StatusMessage = $"已导出到桌面: {exportName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"导出失败: {ex.Message}";
            }
        }

        private bool CanSaveChanges()
        {
            return HasLoadedPhoto && HasUnsavedChanges && !IsBusy;
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

        public void OpenImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                StatusMessage = "无效的图像路径";
                return;
            }
            
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(imagePath);
                image.EndInit();
                image.Freeze(); // 使图像可以跨线程访问
                
                // 更新当前图像和路径
                CurrentImage = image;
                SelectedImagePath = imagePath;
                
                // 重置裁剪状态
                IsCropping = false;
                
                // 更新状态
                StatusMessage = $"已加载图像: {Path.GetFileName(imagePath)}";
                
                // 尝试提取照片信息
                ExtractPhotoInfo(imagePath);
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载图像失败: {ex.Message}";
            }
        }

        private void OpenImageDialog()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp|所有文件|*.*",
                    Title = "选择要编辑的图像"
                };
                
                bool? result = dialog.ShowDialog();
                
                if (result == true)
                {
                    OpenImage(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"选择图像失败: {ex.Message}";
            }
        }

        private void SaveImage()
        {
            // 实现保存图像的功能
        }

        private void StartCrop()
        {
            if (CurrentImage == null)
                return;
                
            IsCropping = true;
            
            // 初始化裁剪区域为整个图像
            CropArea = new CropArea(0, 0, (int)CurrentImage.Width, (int)CurrentImage.Height);
            
            StatusMessage = "请调整裁剪区域";
        }

        private async void ApplyCrop()
        {
            if (!IsCropping || CurrentImage == null)
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = "正在裁剪图像...";
                
                var cropArea = new SCPhotoTool.Services.CropArea(
                    CropArea.X, CropArea.Y, CropArea.Width, CropArea.Height);
                    
                string newPath = await _photoInfoService.CropImageAsync(SelectedImagePath, cropArea);
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    OpenImage(newPath);
                    RecentEdits.Insert(0, newPath);
                    StatusMessage = $"图像已裁剪，保存为: {Path.GetFileName(newPath)}";
                }
                
                IsCropping = false;
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
            IsCropping = false;
            StatusMessage = "裁剪已取消";
        }

        private async void AddInfoAsync()
        {
            if (string.IsNullOrEmpty(SelectedImagePath))
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = "正在添加照片信息...";
                
                // 更新设置中的水印状态
                await _settingsService.SaveSettingAsync("AddProgramWatermark", ShowWatermark);
                
                var photoInfo = new SCPhotoTool.Services.PhotoInfo
                {
                    Title = Title,
                    Author = Author,
                    Date = Date,
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

        private async void ApplyFilterAsync()
        {
            if (string.IsNullOrEmpty(SelectedImagePath))
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = "正在应用滤镜...";
                
                // 更新设置中的水印状态
                await _settingsService.SaveSettingAsync("AddProgramWatermark", ShowWatermark);
                
                string newPath = await _photoInfoService.ApplyFilterAsync(SelectedImagePath, SelectedFilter);
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    OpenImage(newPath);
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

        private async void AddCompositionGuideAsync()
        {
            if (string.IsNullOrEmpty(SelectedImagePath))
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = "正在添加构图辅助线...";
                
                // 更新设置中的水印状态
                await _settingsService.SaveSettingAsync("AddProgramWatermark", ShowWatermark);
                
                string newPath = await _photoInfoService.AddCompositionGuideAsync(SelectedImagePath, SelectedComposition);
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    OpenImage(newPath);
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

        private async void AddFilmGuideAsync()
        {
            if (string.IsNullOrEmpty(SelectedImagePath))
                return;
                
            try
            {
                IsBusy = true;
                StatusMessage = "正在添加电影拍摄辅助线...";
                
                // 更新设置中的水印状态
                await _settingsService.SaveSettingAsync("AddProgramWatermark", ShowWatermark);
                
                string newPath = await _photoInfoService.AddFilmGuideAsync(SelectedImagePath, SelectedFilmAspectRatio);
                
                if (!string.IsNullOrEmpty(newPath))
                {
                    OpenImage(newPath);
                    RecentEdits.Insert(0, newPath);
                    StatusMessage = $"已添加电影拍摄辅助线，保存为: {Path.GetFileName(newPath)}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"添加电影拍摄辅助线失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OpenEditedImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                OpenImage(imagePath);
            }
        }

        private bool CanEditImage()
        {
            return !string.IsNullOrEmpty(SelectedImagePath) && !IsBusy;
        }

        private void ExtractPhotoInfo(string imagePath)
        {
            try
            {
                // 尝试从文件名中提取信息
                string fileName = Path.GetFileNameWithoutExtension(imagePath);
                
                // 尝试从EXIF信息中提取更多数据
                // 这里需要使用特定的库来读取EXIF数据
                
                // 这里只是简单设置标题为文件名
                Title = fileName;
                
                // 作者默认设置
                Author = Environment.UserName;
                
                // 日期默认为当前日期
                Date = DateTime.Now.ToString("yyyy-MM-dd");
                
                // 默认显示水印
                ShowWatermark = _settingsService.GetSetting<bool>("AddProgramWatermark", true);
            }
            catch
            {
                // 忽略提取照片信息时的错误
            }
        }
    }

    public class CropArea : ViewModelBase
    {
        private int _x;
        private int _y;
        private int _width;
        private int _height;
        
        public int X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }
        
        public int Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }
        
        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }
        
        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }
        
        public CropArea(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
} 