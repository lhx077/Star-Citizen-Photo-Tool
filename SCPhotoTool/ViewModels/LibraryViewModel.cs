using SCPhotoTool.Models;
using SCPhotoTool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;
using System.Drawing;
using System.Windows.Threading;

namespace SCPhotoTool.ViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        private readonly IPhotoLibraryService _photoLibraryService;
        private readonly IGameIntegrationService _gameIntegrationService;
        private readonly EditorViewModel _editorViewModel;
        private readonly IPhotoInfoService _photoInfoService;

        private ObservableCollection<Photo> _photos;
        private ObservableCollection<Photo> _filteredPhotos;
        private ObservableCollection<TagItem> _tags;
        private ObservableCollection<string> _selectedTags;
        private bool _isBusy;
        private string _statusMessage = "就绪";
        private string _searchText;
        private DispatcherTimer _searchDebounceTimer;

        public ObservableCollection<Photo> Photos
        {
            get => _photos;
            set => SetProperty(ref _photos, value);
        }

        public ObservableCollection<Photo> FilteredPhotos
        {
            get => _filteredPhotos;
            set => SetProperty(ref _filteredPhotos, value);
        }

        public ObservableCollection<TagItem> Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        public ObservableCollection<string> SelectedTags
        {
            get => _selectedTags;
            set => SetProperty(ref _selectedTags, value);
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

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterPhotos();
                }
            }
        }

        public ICommand RefreshPhotosCommand { get; }
        public ICommand RefreshTagsCommand { get; }
        public ICommand ClearTagsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ImportPhotosCommand { get; }
        public ICommand EditPhotoCommand { get; }
        public ICommand ViewPhotoCommand { get; }
        public ICommand DeletePhotoCommand { get; }
        public ICommand EditTagsCommand { get; }

        public LibraryViewModel(
            IPhotoLibraryService photoLibraryService,
            IGameIntegrationService gameIntegrationService,
            EditorViewModel editorViewModel,
            IPhotoInfoService photoInfoService)
        {
            _photoLibraryService = photoLibraryService;
            _gameIntegrationService = gameIntegrationService;
            _editorViewModel = editorViewModel;
            _photoInfoService = photoInfoService;

            Photos = new ObservableCollection<Photo>();
            FilteredPhotos = new ObservableCollection<Photo>();
            Tags = new ObservableCollection<TagItem>();
            SelectedTags = new ObservableCollection<string>();

            // 初始化命令
            RefreshPhotosCommand = new RelayCommand(_ => LoadPhotosAsync());
            RefreshTagsCommand = new RelayCommand(_ => LoadTagsAsync());
            ClearTagsCommand = new RelayCommand(_ => ClearTags());
            SearchCommand = new RelayCommand(_ => FilterPhotos());
            ImportPhotosCommand = new RelayCommand(_ => ImportPhotosAsync());
            EditPhotoCommand = new RelayCommand(p => EditPhoto(p as Photo));
            ViewPhotoCommand = new RelayCommand(p => ViewPhoto(p as Photo));
            DeletePhotoCommand = new RelayCommand(p => DeletePhotoAsync(p as Photo));
            EditTagsCommand = new RelayCommand(p => EditPhotoTags(p as Photo));

            // 初始化搜索防抖动定时器
            _searchDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

            // 加载照片和标签
            Task.Run(async () =>
            {
                await LoadTagsAsync();
                await LoadPhotosAsync();
            });
        }

        public void AddSelectedTag(string tag)
        {
            if (!SelectedTags.Contains(tag))
            {
                SelectedTags.Add(tag);
                FilterPhotos();
            }
        }

        public void RemoveSelectedTag(string tag)
        {
            if (SelectedTags.Contains(tag))
            {
                SelectedTags.Remove(tag);
                FilterPhotos();
            }
        }

        private void ClearTags()
        {
            SelectedTags.Clear();
            
            // 更新所有标签项的选择状态
            foreach (var tagItem in Tags)
            {
                tagItem.IsSelected = false;
            }
            
            FilterPhotos();
        }

        private async Task LoadPhotosAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "正在加载照片...";

                // 获取所有照片
                var photos = await _photoLibraryService.GetAllPhotosAsync();
                
                // 更新UI
                Application.Current.Dispatcher.Invoke(() => {
                    // 更新集合
                    Photos.Clear();
                    
                    foreach (var photo in photos)
                    {
                        Photos.Add(photo);
                    }
                    
                    FilterPhotos();
                });
                
                // 异步加载缩略图
                await LoadThumbnailsAsync(photos);
                
                StatusMessage = $"已加载 {Photos.Count} 张照片";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载照片失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadThumbnailsAsync(IEnumerable<Photo> photos)
        {
            foreach (var photo in photos)
            {
                try
                {
                    if (photo.ThumbnailImage != null)
                        continue;
                        
                    // 检查文件是否存在
                    if (!File.Exists(photo.FilePath))
                    {
                        continue;
                    }
                    
                    // 加载缩略图
                    var thumbnail = await _photoLibraryService.GetThumbnailAsync(photo.Id);
                    
                    // 如果无法从缓存获取缩略图，则从原始文件创建
                    if (thumbnail == null)
                    {
                        try
                        {
                            // 从文件创建缩略图
                            using (var originalImage = new Bitmap(photo.FilePath))
                            {
                                // 创建缩略图（宽度为200，高度按比例缩放）
                                int width = 200;
                                int height = (int)(originalImage.Height * (width / (float)originalImage.Width));
                                
                                thumbnail = new Bitmap(originalImage, width, height);
                                
                                // 保存缩略图到缓存
                                await _photoLibraryService.SaveThumbnailAsync(photo.Id, thumbnail);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"创建缩略图失败: {ex.Message}");
                            continue;
                        }
                    }
                    
                    // 转换为BitmapImage并更新UI
                    var thumbnailImage = BitmapToImageSource(thumbnail);
                    
                    // 在UI线程更新照片的缩略图
                    Application.Current.Dispatcher.Invoke(() => {
                        photo.ThumbnailImage = thumbnailImage;
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"加载缩略图失败: {ex.Message}");
                    // 如果缩略图加载失败，跳过
                    continue;
                }
            }
        }

        private BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                return null;
                
            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // 使图像可以跨线程访问
                
                return bitmapImage;
            }
        }

        private async Task LoadTagsAsync()
        {
            try
            {
                IsBusy = true;
                
                // 获取所有标签
                var tags = await _photoLibraryService.GetAllTagsAsync();
                
                // 更新集合
                Tags.Clear();
                foreach (var tag in tags)
                {
                    Tags.Add(new TagItem
                    {
                        Name = tag,
                        IsSelected = SelectedTags.Contains(tag)
                    });
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载标签失败: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterPhotos()
        {
            // 清空当前筛选结果
            FilteredPhotos.Clear();
            
            // 应用筛选条件
            IEnumerable<Photo> filteredPhotos = Photos;
            
            // 按标签筛选
            if (SelectedTags.Count > 0)
            {
                filteredPhotos = filteredPhotos.Where(p => 
                    p.Tags != null && 
                    SelectedTags.All(t => p.Tags.Contains(t)));
            }
            
            // 按搜索文本筛选
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchText = SearchText.ToLower();
                filteredPhotos = filteredPhotos.Where(p => 
                    (p.Title != null && p.Title.ToLower().Contains(searchText)) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchText)) ||
                    (p.Author != null && p.Author.ToLower().Contains(searchText)) ||
                    (p.Location != null && p.Location.ToLower().Contains(searchText)) ||
                    (p.Tags != null && p.Tags.Any(t => t.ToLower().Contains(searchText))));
            }
            
            // 更新UI
            foreach (var photo in filteredPhotos)
            {
                FilteredPhotos.Add(photo);
            }
        }

        private async Task ImportPhotosAsync()
        {
            try
            {
                // 打开文件选择对话框
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp|所有文件|*.*",
                    Multiselect = true,
                    Title = "选择要导入的照片"
                };
                
                bool? result = dialog.ShowDialog();
                
                if (result == true)
                {
                    IsBusy = true;
                    StatusMessage = "正在导入照片...";
                    
                    int importedCount = 0;
                    
                    foreach (string file in dialog.FileNames)
                    {
                        try
                        {
                            // 添加照片到库
                            var photo = await _photoLibraryService.AddPhotoAsync(file, new[] { "导入" });
                            
                            // 加载缩略图
                            var thumbnail = await _photoLibraryService.GetThumbnailAsync(photo.Id);
                            photo.ThumbnailImage = BitmapToImageSource(thumbnail);
                            
                            // 添加到集合
                            Photos.Add(photo);
                            importedCount++;
                        }
                        catch
                        {
                            // 跳过导入失败的照片
                            continue;
                        }
                    }
                    
                    // 更新标签
                    await LoadTagsAsync();
                    
                    // 应用筛选
                    FilterPhotos();
                    
                    StatusMessage = $"已导入 {importedCount} 张照片";
                }
                }
                catch (Exception ex)
                {
                StatusMessage = $"导入照片失败: {ex.Message}";
                }
                finally
                {
                    IsBusy = false;
            }
        }

        private void EditPhoto(Photo photo)
        {
            if (photo != null)
            {
                try
                {
                    // 确保文件存在
                    if (!File.Exists(photo.FilePath))
                    {
                        Views.MessageWindow.ShowMessage($"找不到照片文件: {photo.FilePath}", "错误", Views.MessageWindow.MessageType.Error);
                        return;
                    }
                    
                    // 将照片传递给编辑器视图模型
                    _editorViewModel.OpenImage(photo.FilePath);
                    
                    // 切换到编辑器视图
                    RaiseNavigationRequested("Editor");
                    
                    StatusMessage = $"正在编辑照片: {Path.GetFileName(photo.FilePath)}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"无法编辑照片: {ex.Message}";
                    Views.MessageWindow.ShowMessage($"打开编辑器失败: {ex.Message}", "错误", Views.MessageWindow.MessageType.Error);
                }
            }
        }

        private void ViewPhoto(Photo photo)
        {
            if (photo != null)
            {
                try
                {
                    // 确保文件存在
                    if (!File.Exists(photo.FilePath))
                    {
                        Views.MessageWindow.ShowMessage($"找不到照片文件: {photo.FilePath}", "错误", Views.MessageWindow.MessageType.Error);
                        return;
                    }
                    
                    // 创建并显示内置的照片查看器窗口
                    Application.Current.Dispatcher.Invoke(() => {
                        try
                        {
                            var viewerWindow = new Views.PhotoViewerWindow(photo.FilePath);
                            viewerWindow.Owner = Application.Current.MainWindow;
                            viewerWindow.ShowDialog();
                            
                            StatusMessage = $"已查看照片: {Path.GetFileName(photo.FilePath)}";
                        }
                        catch (Exception ex)
                        {
                            StatusMessage = $"无法打开照片查看器: {ex.Message}";
                            Views.MessageWindow.ShowMessage($"打开照片查看器失败: {ex.Message}", "错误", Views.MessageWindow.MessageType.Error);
                        }
                    });
                }
                catch (Exception ex)
                {
                    StatusMessage = $"无法打开照片: {ex.Message}";
                    Views.MessageWindow.ShowMessage($"无法打开照片: {ex.Message}", "错误", Views.MessageWindow.MessageType.Error);
                }
            }
        }

        private async Task DeletePhotoAsync(Photo photo)
        {
            if (photo != null)
            {
                try
                {
                    // 使用自制对话框询问用户确认
                    bool? result = Views.MessageWindow.ShowMessage(
                        $"确定要删除照片 '{photo.Title ?? System.IO.Path.GetFileName(photo.FilePath)}' 吗？此操作无法撤销。",
                        "确认删除",
                        Views.MessageWindow.MessageType.Question);
                        
                    if (result == true)
                    {
                        IsBusy = true;
                        StatusMessage = "正在删除照片...";
                        
                        // 从库中删除
                        await _photoLibraryService.DeletePhotoAsync(photo.Id);
                        
                        // 从集合中移除
                        Photos.Remove(photo);
                        FilteredPhotos.Remove(photo);
                        
                        StatusMessage = "照片已删除";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"删除照片失败: {ex.Message}";
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private void RaiseNavigationRequested(string viewName)
        {
            NavigationRequested?.Invoke(this, new NavigationEventArgs(viewName));
        }
        
        public event EventHandler<NavigationEventArgs> NavigationRequested;

        private void SearchDebounceTimer_Tick(object sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            FilterPhotos();
        }

        private async void EditPhotoTags(Photo photo)
        {
            if (photo == null)
                return;
                
            try
            {
                // 确保文件存在
                if (!File.Exists(photo.FilePath))
                {
                    Views.MessageWindow.ShowMessage($"找不到照片文件: {photo.FilePath}", "错误", Views.MessageWindow.MessageType.Error);
                    return;
                }
                
                // 获取所有可用标签
                var allTags = Tags.Select(t => t.Name).ToList();
                
                // 打开标签管理窗口
                Application.Current.Dispatcher.Invoke(() => {
                    var tagWindow = new Views.TagManagementWindow(photo.Tags, allTags);
                    tagWindow.Owner = Application.Current.MainWindow;
                    
                    if (tagWindow.ShowDialog() == true && tagWindow.SelectedTags != null)
                    {
                        // 更新照片标签
                        UpdatePhotoTagsAsync(photo, tagWindow.SelectedTags);
                    }
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"编辑标签失败: {ex.Message}";
                Views.MessageWindow.ShowMessage($"编辑标签失败: {ex.Message}", "错误", Views.MessageWindow.MessageType.Error);
            }
        }
        
        private async void UpdatePhotoTagsAsync(Photo photo, IEnumerable<string> newTags)
        {
            try
            {
                IsBusy = true;
                StatusMessage = "正在更新标签...";
                
                // 更新照片标签
                await _photoLibraryService.UpdatePhotoAsync(photo.Id, newTags, photo.Description);
                
                // 更新本地模型
                photo.Tags.Clear();
                foreach (var tag in newTags)
                {
                    photo.Tags.Add(tag);
                }
                
                // 刷新标签列表
                await LoadTagsAsync();
                
                // 应用筛选
                FilterPhotos();
                
                StatusMessage = "标签已更新";
            }
            catch (Exception ex)
            {
                StatusMessage = $"更新标签失败: {ex.Message}";
                Views.MessageWindow.ShowMessage($"更新标签失败: {ex.Message}", "错误", Views.MessageWindow.MessageType.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public class TagItem : ViewModelBase
    {
        private string _name;
        private bool _isSelected;
        
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        
        public override string ToString()
        {
            return Name;
        }
    }
    
    public class NavigationEventArgs : EventArgs
    {
        public string ViewName { get; }
        
        public NavigationEventArgs(string viewName)
        {
            ViewName = viewName;
        }
    }
} 