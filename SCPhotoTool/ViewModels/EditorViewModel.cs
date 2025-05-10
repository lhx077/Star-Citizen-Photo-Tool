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
using System.Runtime.InteropServices;

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
                FilterType.WarmOrange,
                FilterType.SpaceNebula,
                FilterType.StarLight,
                FilterType.CosmicGlow,
                FilterType.EpicSpace,
                FilterType.CinematicSpace,
                FilterType.SciFiTech,
                FilterType.DeepShadows,
                FilterType.VibrantColors,
                FilterType.MattePainting,
                FilterType.AlienWorld,
                FilterType.SpaceExploration,
                FilterType.PlanetSurface,
                FilterType.HollywoodAction,
                FilterType.FilmNoir,
                FilterType.SummerBlockbuster,
                FilterType.Dystopian,
                FilterType.WesternDesert
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

        private void ApplyFilter(Bitmap bitmap, FilterType filter)
        {
            if (bitmap == null)
                return;

            // 锁定位图进行处理
            BitmapData bmpData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            // 获取位图数据的指针
            IntPtr ptr = bmpData.Scan0;
            
            // 计算位图的字节数
            int bytes = bmpData.Stride * bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            
            // 将位图数据复制到字节数组
            Marshal.Copy(ptr, rgbValues, 0, bytes);
            
            // 处理每个像素
            for (int i = 0; i < rgbValues.Length; i += 4)
            {
                byte blue = rgbValues[i];
                byte green = rgbValues[i + 1];
                byte red = rgbValues[i + 2];
                // Alpha值在i+3
                
                switch (filter)
                {
                    case FilterType.BlackAndWhite:
                        byte gray = (byte)(0.299 * red + 0.587 * green + 0.114 * blue);
                        rgbValues[i] = gray;      // Blue
                        rgbValues[i + 1] = gray;  // Green
                        rgbValues[i + 2] = gray;  // Red
                        break;
                        
                    case FilterType.Sepia:
                        byte newRed = (byte)Math.Min(255, (red * 0.393) + (green * 0.769) + (blue * 0.189));
                        byte newGreen = (byte)Math.Min(255, (red * 0.349) + (green * 0.686) + (blue * 0.168));
                        byte newBlue = (byte)Math.Min(255, (red * 0.272) + (green * 0.534) + (blue * 0.131));
                        rgbValues[i] = newBlue;
                        rgbValues[i + 1] = newGreen;
                        rgbValues[i + 2] = newRed;
                        break;
                        
                    case FilterType.HighContrast:
                        rgbValues[i] = (byte)(blue < 128 ? blue * 0.5 : Math.Min(255, blue * 1.5));
                        rgbValues[i + 1] = (byte)(green < 128 ? green * 0.5 : Math.Min(255, green * 1.5));
                        rgbValues[i + 2] = (byte)(red < 128 ? red * 0.5 : Math.Min(255, red * 1.5));
                        break;
                        
                    case FilterType.Vintage:
                        rgbValues[i] = (byte)Math.Min(255, blue * 0.8);
                        rgbValues[i + 1] = (byte)Math.Min(255, green * 0.9);
                        rgbValues[i + 2] = (byte)Math.Min(255, red * 1.2);
                        break;
                        
                    case FilterType.ColdBlue:
                        rgbValues[i] = (byte)Math.Min(255, blue * 1.2);
                        rgbValues[i + 1] = (byte)Math.Min(255, green * 1.0);
                        rgbValues[i + 2] = (byte)Math.Min(255, red * 0.8);
                        break;
                        
                    case FilterType.WarmOrange:
                        rgbValues[i] = (byte)Math.Min(255, blue * 0.7);
                        rgbValues[i + 1] = (byte)Math.Min(255, green * 0.9);
                        rgbValues[i + 2] = (byte)Math.Min(255, red * 1.3);
                        break;
                        
                    case FilterType.SpaceNebula:
                        // 增强蓝色和紫色调，创造星云效果
                        rgbValues[i] = (byte)Math.Min(255, blue * 1.5);  // 增强蓝色
                        rgbValues[i + 1] = (byte)Math.Min(255, green * 0.6);  // 降低绿色
                        rgbValues[i + 2] = (byte)Math.Min(255, red * 1.3);  // 增强红色，创造紫色调
                        
                        // 增加亮度对比度，使星云更加突出
                        byte luminance = (byte)(0.2126 * rgbValues[i + 2] + 0.7152 * rgbValues[i + 1] + 0.0722 * rgbValues[i]);
                        if (luminance > 180)
                        {
                            // 增亮明亮区域，更好地显示星云亮部
                            rgbValues[i] = (byte)Math.Min(255, rgbValues[i] * 1.3);
                            rgbValues[i + 1] = (byte)Math.Min(255, rgbValues[i + 1] * 1.2);
                            rgbValues[i + 2] = (byte)Math.Min(255, rgbValues[i + 2] * 1.2);
                        }
                        break;
                        
                    case FilterType.StarLight:
                        // 增强亮点，模拟星光
                        byte lightValue = (byte)(0.2126 * red + 0.7152 * green + 0.0722 * blue);
                        if (lightValue > 180) // 如果是亮点
                        {
                            // 创造星光膨胀效果，使亮点更明亮
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.8);
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 1.8);
                            rgbValues[i + 2] = (byte)Math.Min(255, red * 1.8);
                        }
                        else if (lightValue < 50) // 深色部分
                        {
                            // 降低暗部亮度，增强对比度
                            rgbValues[i] = (byte)(blue * 0.7);
                            rgbValues[i + 1] = (byte)(green * 0.7);
                            rgbValues[i + 2] = (byte)(red * 0.7);
                        }
                        else // 中间调
                        {
                            // 在中间调添加轻微的蓝色调
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.1);
                            rgbValues[i + 1] = (byte)(green * 0.95);
                            rgbValues[i + 2] = (byte)(red * 0.95);
                        }
                        break;
                        
                    case FilterType.CosmicGlow:
                        // 创造宇宙辉光效果
                        byte brightness = (byte)((red + green + blue) / 3);
                        
                        // 根据亮度分层处理
                        if (brightness > 200) // 高光区域
                        {
                            // 增强亮部，创造辉光
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.4 + 20);
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 1.3 + 15);
                            rgbValues[i + 2] = (byte)Math.Min(255, red * 1.3 + 15);
                        }
                        else if (brightness < 50) // 阴影区域
                        {
                            // 增强深色部分的蓝色调
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.2);
                            rgbValues[i + 1] = (byte)(green * 0.85);
                            rgbValues[i + 2] = (byte)(red * 0.85);
                        }
                        else // 中间调
                        {
                            // 中间调添加紫色偏移
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.15 + 5);
                            rgbValues[i + 1] = (byte)(green * 0.9);
                            rgbValues[i + 2] = (byte)Math.Min(255, red * 1.05 + 3);
                        }
                        break;
                        
                    case FilterType.EpicSpace:
                        // 史诗感太空效果
                        // 强化对比度和颜色
                        byte avgBrightness = (byte)((red + green + blue) / 3);

                        // 创建电影级的颜色分级效果
                        if (avgBrightness > 180) // 高光区域 - 偏蓝色调
                        {
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.4);
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 1.2);
                            rgbValues[i + 2] = (byte)Math.Min(255, red * 1.0);
                        }
                        else if (avgBrightness < 60) // 阴影区域 - 偏深蓝色
                        {
                            rgbValues[i] = (byte)Math.Min(255, blue * 0.8);
                            rgbValues[i + 1] = (byte)(green * 0.6);
                            rgbValues[i + 2] = (byte)(red * 0.5);
                        }
                        else // 中间调 - 增强对比度
                        {
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.2);
                            rgbValues[i + 1] = (byte)(green * 0.9);
                            rgbValues[i + 2] = (byte)(red * 0.8);
                        }
                        
                        // 额外增强蓝色通道，创造经典的太空蓝效果
                        if (blue > 100)
                        {
                            rgbValues[i] = (byte)Math.Min(255, rgbValues[i] * 1.1 + 5);
                        }
                        break;
                        
                    case FilterType.CinematicSpace:
                        // 电影级太空效果，色调映射技术
                        // 获取亮度
                        float redF = red / 255.0f;
                        float greenF = green / 255.0f;
                        float blueF = blue / 255.0f;
                        
                        // 对暗部应用S形曲线，压暗阴影
                        if (redF < 0.4f) redF = redF * 0.75f;
                        if (greenF < 0.4f) greenF = greenF * 0.75f;
                        if (blueF < 0.4f) blueF = blueF * 0.75f;
                        
                        // 对亮部应用S形曲线，增强高光
                        if (redF > 0.7f) redF = Math.Min(1.0f, redF * 1.2f);
                        if (greenF > 0.7f) greenF = Math.Min(1.0f, greenF * 1.15f);
                        if (blueF > 0.7f) blueF = Math.Min(1.0f, blueF * 1.25f);
                        
                        // 添加电影感色彩（略偏冷蓝色调）
                        redF *= 0.95f;
                        greenF *= 0.97f;
                        blueF *= 1.05f;
                        
                        // 将结果转换回字节
                        rgbValues[i] = (byte)(blueF * 255);
                        rgbValues[i + 1] = (byte)(greenF * 255);
                        rgbValues[i + 2] = (byte)(redF * 255);
                        break;

                    case FilterType.SciFiTech:
                        // 科幻技术感
                        // 强化蓝色和青色调
                        rgbValues[i] = (byte)Math.Min(255, blue * 1.3);
                        rgbValues[i + 1] = (byte)Math.Min(255, green * 1.2);
                        rgbValues[i + 2] = (byte)(red * 0.8);
                        
                        // 提高对比度
                        byte techBrightness = (byte)(0.2126 * rgbValues[i + 2] + 0.7152 * rgbValues[i + 1] + 0.0722 * rgbValues[i]);
                        if (techBrightness > 128)
                        {
                            // 增亮高光区域
                            rgbValues[i] = (byte)Math.Min(255, rgbValues[i] * 1.1 + 10);
                            rgbValues[i + 1] = (byte)Math.Min(255, rgbValues[i + 1] * 1.1 + 5);
                        }
                        break;
                        
                    case FilterType.DeepShadows:
                        // 加深阴影，增强对比度
                        byte avgValue = (byte)((red + green + blue) / 3);
                        if (avgValue < 80)
                        {
                            // 深色区域，加强深度
                            rgbValues[i] = (byte)(blue * 0.4);
                            rgbValues[i + 1] = (byte)(green * 0.4);
                            rgbValues[i + 2] = (byte)(red * 0.4);
                        }
                        else if (avgValue > 200)
                        {
                            // 亮区，略微增强
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.1);
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 1.1);
                            rgbValues[i + 2] = (byte)Math.Min(255, red * 1.1);
                        }
                        // 中间调保持不变
                        break;
                        
                    case FilterType.VibrantColors:
                        // 鲜艳色彩，增强全局饱和度
                        float maxValue = Math.Max(Math.Max(red, green), blue);
                        float minValue = Math.Min(Math.Min(red, green), blue);
                        float delta = (maxValue - minValue) / 255.0f;
                        float saturationIncrease = 1.5f;  // 饱和度增强系数
                        
                        if (delta > 0.05f)  // 只处理有一定饱和度的像素
                        {
                            float lumR = 0.213f, lumG = 0.715f, lumB = 0.072f;
                            float lumaValue = (lumR * red + lumG * green + lumB * blue) / 255.0f;
                            float satMult = saturationIncrease;
                            float satR = (1.0f - satMult) * lumaValue + satMult * (red / 255.0f);
                            float satG = (1.0f - satMult) * lumaValue + satMult * (green / 255.0f);
                            float satB = (1.0f - satMult) * lumaValue + satMult * (blue / 255.0f);
                            
                            rgbValues[i] = (byte)Math.Min(255, satB * 255);
                            rgbValues[i + 1] = (byte)Math.Min(255, satG * 255);
                            rgbValues[i + 2] = (byte)Math.Min(255, satR * 255);
                        }
                        break;
                        
                    case FilterType.SpaceExploration:
                        // 太空探索滤镜 - 增强星际飞船和站台的科技感
                        // 调整色调，强化金属感
                        float metalFactor = 0.8f;
                        float blueBoost = 1.2f;
                        
                        // 增强亮部对比度，让金属表面更加突出
                        if ((red + green + blue) / 3 > 150)
                        {
                            rgbValues[i] = (byte)Math.Min(255, blue * blueBoost);
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 1.1f);
                            rgbValues[i + 2] = (byte)Math.Min(255, red * metalFactor);
                        }
                        else
                        {
                            // 阴影区域减弱红色，增强蓝色
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.1f);
                            rgbValues[i + 1] = (byte)(green * 0.9f);
                            rgbValues[i + 2] = (byte)(red * 0.8f);
                        }
                        break;
                        
                    case FilterType.PlanetSurface:
                        // 行星表面滤镜 - 适合地形和景观
                        // 增强红色和棕色调，适合火星等类似场景
                        
                        // 获取亮度
                        byte planetBrightness = (byte)((red + green + blue) / 3);
                        if (red > green && red > blue) // 红色区域
                        {
                            // 增强红色调和对比度
                            rgbValues[i] = (byte)(blue * 0.7);
                            rgbValues[i + 1] = (byte)(green * 0.8);
                            rgbValues[i + 2] = (byte)Math.Min(255, red * 1.3);
                        }
                        else if (green > red && green > blue) // 绿色区域 - 可能是植被
                        {
                            // 增强绿色调
                            rgbValues[i] = (byte)(blue * 0.8);
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 1.2);
                            rgbValues[i + 2] = (byte)(red * 0.9);
                        }
                        else // 其他区域
                        {
                            // 轻微调整为更温暖的色调
                            rgbValues[i] = (byte)(blue * 0.9);
                            rgbValues[i + 1] = (byte)(green * 1.0);
                            rgbValues[i + 2] = (byte)Math.Min(255, red * 1.1);
                        }
                        break;
                        
                    case FilterType.MattePainting:
                        // 创造绘画感
                        float avg2 = (red + green + blue) / 3.0f;
                        // 对暗部增加蓝色调，亮部增加黄色调
                        if (avg2 < 100)
                        {
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.1);
                            rgbValues[i + 1] = (byte)(green * 0.95);
                            rgbValues[i + 2] = (byte)(red * 0.9);
                        }
                        else
                        {
                            rgbValues[i] = (byte)(blue * 0.9);
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 1.05);
                            rgbValues[i + 2] = (byte)Math.Min(255, red * 1.1);
                        }
                        break;
                        
                    case FilterType.AlienWorld:
                        // 创造外星世界感
                        // 交换颜色通道，增加异域感
                        rgbValues[i] = (byte)Math.Min(255, green * 0.9);
                        rgbValues[i + 1] = (byte)Math.Min(255, red * 0.9);
                        rgbValues[i + 2] = (byte)Math.Min(255, blue * 1.1);
                        break;
                        
                    case FilterType.HollywoodAction:
                        // 好莱坞动作片滤镜 - 高对比度、蓝橙配色
                        float actionBrightness = (red + green + blue) / 3.0f;
                        
                        // 增强橙色和蓝色(蓝橙配色是好莱坞大片的标志性色调)
                        if (red > blue && red > green) // 偏红/橙色区域
                        {
                            // 增强橙色，降低绿色和蓝色
                            rgbValues[i] = (byte)Math.Min(255, blue * 0.7); // 降低蓝色
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 0.8); // 降低绿色
                            rgbValues[i + 2] = (byte)Math.Min(255, red * 1.3); // 增强红色
                        }
                        else if (blue > red && blue > green) // 偏蓝色区域
                        {
                            // 增强蓝色，降低其他颜色
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.3); // 增强蓝色
                            rgbValues[i + 1] = (byte)(green * 0.85); // 降低绿色
                            rgbValues[i + 2] = (byte)(red * 0.85); // 降低红色
                        }
                        
                        // 增强对比度
                        if (actionBrightness < 80) // 暗部
                        {
                            // 压暗暗部，提高对比度
                            rgbValues[i] = (byte)(rgbValues[i] * 0.7);
                            rgbValues[i + 1] = (byte)(rgbValues[i + 1] * 0.7);
                            rgbValues[i + 2] = (byte)(rgbValues[i + 2] * 0.7);
                        }
                        else if (actionBrightness > 200) // 亮部
                        {
                            // 增亮亮部
                            rgbValues[i] = (byte)Math.Min(255, rgbValues[i] * 1.15);
                            rgbValues[i + 1] = (byte)Math.Min(255, rgbValues[i + 1] * 1.15);
                            rgbValues[i + 2] = (byte)Math.Min(255, rgbValues[i + 2] * 1.15);
                        }
                        break;
                        
                    case FilterType.FilmNoir:
                        // 黑色电影滤镜 - 高对比度黑白效果，暗部加强
                        // 计算黑白值
                        byte noirGray = (byte)(red * 0.3 + green * 0.59 + blue * 0.11);
                        
                        // 增强对比度
                        if (noirGray < 90)
                        {
                            noirGray = (byte)(noirGray * 0.6); // 加深暗部
                        }
                        else if (noirGray > 190)
                        {
                            noirGray = (byte)Math.Min(255, noirGray * 1.2); // 增亮高光
                        }
                        else
                        {
                            // 中间调微调
                            noirGray = (byte)(noirGray * 0.95);
                        }
                        
                        // 设置最终黑白值，带轻微冷色调
                        rgbValues[i] = (byte)Math.Min(255, noirGray * 1.05); // 蓝色稍强
                        rgbValues[i + 1] = noirGray;
                        rgbValues[i + 2] = (byte)(noirGray * 0.95); // 红色稍弱
                        break;
                        
                    case FilterType.SummerBlockbuster:
                        // 夏季大片滤镜 - 鲜艳色彩，蓝天和绿地增强
                        // 获取亮度
                        float summerBrightness = (red + green + blue) / 3.0f;
                        
                        // 根据颜色分布应用不同效果
                        if (blue > red && blue > green && blue > 100) // 天空/水面区域
                        {
                            // 蓝天增强，使天空更蓝更通透
                            rgbValues[i] = (byte)Math.Min(255, blue * 1.4);
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 1.1);
                            rgbValues[i + 2] = (byte)(red * 0.9);
                        }
                        else if (green > red && green > blue) // 植被/绿色区域
                        {
                            // 增强绿色，使植被更加生动
                            rgbValues[i] = (byte)(blue * 0.9);
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 1.3);
                            rgbValues[i + 2] = (byte)(red * 0.95);
                        }
                        else if (red > blue && red > green) // 暖色调区域
                        {
                            // 增强暖色调，使肤色更加健康
                            rgbValues[i] = (byte)(blue * 0.8);
                            rgbValues[i + 1] = (byte)Math.Min(255, green * 1.05);
                            rgbValues[i + 2] = (byte)Math.Min(255, red * 1.2);
                        }
                        
                        // 全局饱和度提升
                        if (summerBrightness > 30 && summerBrightness < 220) // 避开极暗和极亮区域
                        {
                            float maxChannel = Math.Max(Math.Max(rgbValues[i], rgbValues[i+1]), rgbValues[i+2]);
                            float minChannel = Math.Min(Math.Min(rgbValues[i], rgbValues[i+1]), rgbValues[i+2]);
                            float saturation = (maxChannel - minChannel) / maxChannel;
                            
                            if (saturation > 0.1) // 如果原本有一定饱和度
                            {
                                // 适当提高饱和度
                                float factor = 1.2f;
                                float avg = (rgbValues[i] + rgbValues[i+1] + rgbValues[i+2]) / 3.0f;
                                
                                rgbValues[i] = (byte)Math.Min(255, avg + (rgbValues[i] - avg) * factor);
                                rgbValues[i+1] = (byte)Math.Min(255, avg + (rgbValues[i+1] - avg) * factor);
                                rgbValues[i+2] = (byte)Math.Min(255, avg + (rgbValues[i+2] - avg) * factor);
                            }
                        }
                        break;
                        
                    case FilterType.Dystopian:
                        // 反乌托邦电影滤镜 - 冷色调、低饱和度、青蓝色偏移
                        // 降低饱和度
                        float dysAvg = (red + green + blue) / 3.0f;
                        float satRed = red * 0.7f + dysAvg * 0.3f;
                        float satGreen = green * 0.7f + dysAvg * 0.3f;
                        float satBlue = blue * 0.7f + dysAvg * 0.3f;
                        
                        // 加入青蓝色调
                        rgbValues[i] = (byte)Math.Min(255, satBlue * 1.2); // 增强蓝色
                        rgbValues[i + 1] = (byte)Math.Min(255, satGreen * 1.1); // 轻微增强绿色
                        rgbValues[i + 2] = (byte)(satRed * 0.85); // 降低红色
                        
                        // 调整对比度 - 黑暗的世界
                        if (dysAvg < 100)
                        {
                            // 使暗部更暗
                            rgbValues[i] = (byte)(rgbValues[i] * 0.85);
                            rgbValues[i + 1] = (byte)(rgbValues[i + 1] * 0.85);
                            rgbValues[i + 2] = (byte)(rgbValues[i + 2] * 0.85);
                        }
                        break;
                        
                    case FilterType.WesternDesert:
                        // 西部荒漠电影滤镜 - 暖棕色调、高对比度、黄沙特效
                        // 暖色调处理
                        byte westernAvg = (byte)((red + green + blue) / 3);
                        
                        // 增强黄棕色调
                        rgbValues[i] = (byte)Math.Max(0, Math.Min(255, blue * 0.7)); // 降低蓝色
                        rgbValues[i + 1] = (byte)Math.Max(0, Math.Min(255, green * 1.1)); // 增强绿色
                        rgbValues[i + 2] = (byte)Math.Max(0, Math.Min(255, red * 1.3)); // 显著增强红色
                        
                        // 光晕效果和灰尘效果 - 提高亮部黄色，降低整体对比度
                        if (westernAvg > 150) // 亮部区域
                        {
                            // 增加黄色调
                            rgbValues[i] = (byte)Math.Min(255, rgbValues[i] * 0.8); // 降低蓝色
                            rgbValues[i + 1] = (byte)Math.Min(255, rgbValues[i + 1] * 1.1); // 增强绿色
                            rgbValues[i + 2] = (byte)Math.Min(255, rgbValues[i + 2] * 1.05); // 增强红色
                            
                            // 模拟灰尘和阳光效果，轻微降低对比度
                            float dustEffect = 0.15f;
                            rgbValues[i] = (byte)Math.Min(255, rgbValues[i] * (1 - dustEffect) + 220 * dustEffect);
                            rgbValues[i + 1] = (byte)Math.Min(255, rgbValues[i + 1] * (1 - dustEffect) + 210 * dustEffect);
                            rgbValues[i + 2] = (byte)Math.Min(255, rgbValues[i + 2] * (1 - dustEffect) + 180 * dustEffect);
                        }
                        break;
                }
            }
            
            // 将处理后的数据复制回位图
            Marshal.Copy(rgbValues, 0, ptr, bytes);
            
            // 解锁位图
            bitmap.UnlockBits(bmpData);
        }

        // 将滤镜应用到当前打开的图像
        public void ApplyFilterToCurrentImage(FilterType filterType)
        {
            if (_editedBitmap != null)
            {
                try
                {
                    // 应用滤镜
                    ApplyFilter(_editedBitmap, filterType);
                    
                    // 更新界面预览
                    CurrentImage = BitmapToImageSource(_editedBitmap);
                    
                    // 标记为有未保存的更改
                    HasUnsavedChanges = true;
                    
                    StatusMessage = $"已应用滤镜: {filterType}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"应用滤镜失败: {ex.Message}";
                }
            }
        }

        // 实现UI调用的接口方法，供视图绑定
        private void ApplySelectedFilter()
        {
            if (SelectedFilter != null)
            {
                ApplyFilterToCurrentImage(SelectedFilter);
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