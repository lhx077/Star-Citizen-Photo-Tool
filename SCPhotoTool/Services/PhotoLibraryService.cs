using SCPhotoTool.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public class PhotoLibraryService : IPhotoLibraryService
    {
        private readonly ISettingsService _settingsService;
        private readonly string _libraryDirectory;
        private readonly string _libraryFilePath;
        private readonly string _thumbnailsDirectory;
        private readonly JsonSerializerOptions _jsonOptions;
        private List<Photo> _photos;

        public PhotoLibraryService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            
            // 库文件存储在用户本地应用数据目录
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SCPhotoTool");
            
            _libraryDirectory = appDataPath;
            _libraryFilePath = Path.Combine(appDataPath, "library.json");
            _thumbnailsDirectory = Path.Combine(appDataPath, "Thumbnails");
            
            // 确保目录存在
            Directory.CreateDirectory(_libraryDirectory);
            Directory.CreateDirectory(_thumbnailsDirectory);
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            // 加载照片库
            LoadLibrary();
        }

        public async Task<IEnumerable<Photo>> GetAllPhotosAsync()
        {
            return await Task.FromResult(_photos);
        }

        public async Task<IEnumerable<Photo>> GetPhotosByTagsAsync(IEnumerable<string> tags)
        {
            if (tags == null || !tags.Any())
                return await GetAllPhotosAsync();
                
            return await Task.FromResult(
                _photos.Where(p => tags.All(tag => p.Tags.Contains(tag))).ToList());
        }

        public async Task<Photo> AddPhotoAsync(string photoPath, IEnumerable<string> tags = null)
        {
            if (!File.Exists(photoPath))
                throw new FileNotFoundException("照片文件不存在", photoPath);
                
            // 创建照片对象
            var photo = new Photo
            {
                FilePath = photoPath,
                Title = Path.GetFileNameWithoutExtension(photoPath),
                CreatedDate = File.GetCreationTime(photoPath),
                ModifiedDate = DateTime.Now
            };
            
            // 添加标签
            if (tags != null)
            {
                photo.Tags.AddRange(tags);
            }
            
            // 获取文件信息
            var fileInfo = new FileInfo(photoPath);
            photo.FileSize = fileInfo.Length;
            photo.FileType = Path.GetExtension(photoPath).TrimStart('.').ToLower();
            
            // 获取图片尺寸
            using (var image = Image.FromFile(photoPath))
            {
                photo.Width = image.Width;
                photo.Height = image.Height;
                
                // 生成缩略图
                string thumbnailFileName = $"{photo.Id}.jpg";
                string thumbnailPath = Path.Combine(_thumbnailsDirectory, thumbnailFileName);
                
                await Task.Run(() => GenerateThumbnail(image, thumbnailPath, 250));
                photo.ThumbnailPath = thumbnailPath;
            }
            
            // 添加到集合并保存
            _photos.Add(photo);
            await SaveLibraryAsync();
            
            return photo;
        }

        public async Task<Photo> UpdatePhotoAsync(string photoId, IEnumerable<string> tags = null, string description = null)
        {
            var photo = _photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null)
                throw new KeyNotFoundException("找不到指定的照片");
                
            // 更新标签
            if (tags != null)
            {
                photo.Tags = tags.ToList();
            }
            
            // 更新描述
            if (description != null)
            {
                photo.Description = description;
            }
            
            photo.ModifiedDate = DateTime.Now;
            
            await SaveLibraryAsync();
            return photo;
        }

        public async Task<bool> DeletePhotoAsync(string photoId)
        {
            var photo = _photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null)
                return false;
                
            try
            {
                // 删除缩略图
                if (!string.IsNullOrEmpty(photo.ThumbnailPath) && File.Exists(photo.ThumbnailPath))
                {
                    File.Delete(photo.ThumbnailPath);
                }
                
                // 从集合中移除并保存
                _photos.Remove(photo);
                await SaveLibraryAsync();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetAllTagsAsync()
        {
            // 返回所有照片中使用的唯一标签
            var allTags = new HashSet<string>();
            
            foreach (var photo in _photos)
            {
                foreach (var tag in photo.Tags)
                {
                    allTags.Add(tag);
                }
            }
            
            return await Task.FromResult(allTags.ToList());
        }

        public async Task<Bitmap> GetThumbnailAsync(string photoId)
        {
            var photo = _photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null)
                throw new KeyNotFoundException("找不到指定的照片");
                
            if (string.IsNullOrEmpty(photo.ThumbnailPath) || !File.Exists(photo.ThumbnailPath))
            {
                // 如果缩略图不存在，尝试重新生成
                if (File.Exists(photo.FilePath))
                {
                    using (var image = Image.FromFile(photo.FilePath))
                    {
                        string thumbnailFileName = $"{photo.Id}.jpg";
                        string thumbnailPath = Path.Combine(_thumbnailsDirectory, thumbnailFileName);
                        
                        await Task.Run(() => GenerateThumbnail(image, thumbnailPath, 250));
                        photo.ThumbnailPath = thumbnailPath;
                        await SaveLibraryAsync();
                    }
                }
                else
                {
                    throw new FileNotFoundException("照片文件不存在", photo.FilePath);
                }
            }
            
            return await Task.Run(() => new Bitmap(photo.ThumbnailPath));
        }
        
        public async Task<IEnumerable<string>> GetPhotosAsync()
        {
            return _photos.Select(p => p.FilePath);
        }
        
        public async Task<string> GetPhotoPathAsync(string photoId)
        {
            var photo = _photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null)
                throw new KeyNotFoundException("找不到指定的照片");
            
            return photo.FilePath;
        }
        
        private void LoadLibrary()
        {
            try
            {
                if (File.Exists(_libraryFilePath))
                {
                    string json = File.ReadAllText(_libraryFilePath);
                    _photos = JsonSerializer.Deserialize<List<Photo>>(json, _jsonOptions);
                }
            }
            catch
            {
                // 加载失败时初始化为空集合
                _photos = new List<Photo>();
            }
            
            // 确保集合不为null
            if (_photos == null)
            {
                _photos = new List<Photo>();
            }
            
            // 验证照片文件的有效性
            for (int i = _photos.Count - 1; i >= 0; i--)
            {
                if (!File.Exists(_photos[i].FilePath))
                {
                    _photos.RemoveAt(i);
                }
            }
        }
        
        private async Task SaveLibraryAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_photos, _jsonOptions);
                await File.WriteAllTextAsync(_libraryFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存照片库失败: {ex.Message}");
                // 在实际应用中，这里应当记录日志或通知用户
            }
        }
        
        private void GenerateThumbnail(Image original, string outputPath, int maxSize)
        {
            int width, height;
            
            // 计算缩略图尺寸，保持原始比例
            if (original.Width > original.Height)
            {
                width = maxSize;
                height = (int)(original.Height * ((float)maxSize / original.Width));
            }
            else
            {
                height = maxSize;
                width = (int)(original.Width * ((float)maxSize / original.Height));
            }
            
            using (var thumbnail = new Bitmap(width, height))
            {
                using (var graphics = Graphics.FromImage(thumbnail))
                {
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.DrawImage(original, 0, 0, width, height);
                }
                
                // 保存为JPEG格式
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 85L);
                
                thumbnail.Save(outputPath, GetEncoder(ImageFormat.Jpeg), encoderParams);
            }
        }
        
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
} 