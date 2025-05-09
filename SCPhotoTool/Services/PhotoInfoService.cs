using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public interface IPhotoInfoService
    {
        /// <summary>
        /// 给照片添加拍摄信息
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="photoInfo">照片信息</param>
        /// <returns>处理后的照片路径</returns>
        Task<string> AddPhotoInfoAsync(string imagePath, PhotoInfo photoInfo);
        
        /// <summary>
        /// 应用滤镜
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="filterType">滤镜类型</param>
        /// <returns>处理后的照片路径</returns>
        Task<string> ApplyFilterAsync(string imagePath, FilterType filterType);
        
        /// <summary>
        /// 添加构图辅助线
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="compositionType">构图类型</param>
        /// <returns>处理后的照片路径</returns>
        Task<string> AddCompositionGuideAsync(string imagePath, CompositionType compositionType);
        
        /// <summary>
        /// 裁剪图像
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="cropArea">裁剪区域</param>
        /// <returns>处理后的照片路径</returns>
        Task<string> CropImageAsync(string imagePath, CropArea cropArea);
        
        /// <summary>
        /// 添加电影拍摄辅助线
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="aspectRatio">电影宽高比</param>
        /// <returns>处理后的照片路径</returns>
        Task<string> AddFilmGuideAsync(string imagePath, FilmAspectRatio aspectRatio);
    }
    
    public class PhotoInfoService : IPhotoInfoService
    {
        private readonly ISettingsService _settingsService;
        
        public PhotoInfoService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }
        
        public async Task<string> AddPhotoInfoAsync(string imagePath, PhotoInfo photoInfo)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    {
                        // 为图片创建底部空间，用于添加信息
                        int infoHeight = 120; // 信息区域高度
                        int newHeight = originalImage.Height + infoHeight;
                        
                        using (var newImage = new Bitmap(originalImage.Width, newHeight))
                        {
                            using (var g = Graphics.FromImage(newImage))
                            {
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                                // 绘制原始图片
                                g.DrawImage(originalImage, 0, 0, originalImage.Width, originalImage.Height);

                                // 绘制信息背景
                                using (var brush = new SolidBrush(Color.FromArgb(220, 0, 0, 0)))
                                {
                                    g.FillRectangle(brush, 0, originalImage.Height, originalImage.Width, infoHeight);
                                }
                                
                                // 绘制拍摄信息
                                using (var font = new Font("Arial", 12, FontStyle.Regular))
                                using (var fontBold = new Font("Arial", 14, FontStyle.Bold))
                                using (var whiteBrush = new SolidBrush(Color.White))
                                {
                                    int y = originalImage.Height + 15;
                                    
                                    // 绘制标题
                                    if (!string.IsNullOrEmpty(photoInfo.Title))
                                    {
                                        g.DrawString(photoInfo.Title, fontBold, whiteBrush, 20, y);
                                        y += 25;
                                    }
                                    
                                    // 绘制拍摄信息
                                    string infoText = "";
                                    if (!string.IsNullOrEmpty(photoInfo.Location))
                                        infoText += $"地点: {photoInfo.Location} | ";
                                    if (!string.IsNullOrEmpty(photoInfo.CameraSettings))
                                        infoText += $"相机设置: {photoInfo.CameraSettings} | ";
                                    if (!string.IsNullOrEmpty(photoInfo.Date))
                                        infoText += $"日期: {photoInfo.Date} | ";
                                    if (!string.IsNullOrEmpty(photoInfo.Author))
                                        infoText += $"作者: {photoInfo.Author}";
                                    
                                    // 添加程序水印信息（可以在设置中禁用）
                                    if (_settingsService.GetSetting<bool>("AddProgramWatermark", true))
                                    {
                                        infoText += !string.IsNullOrEmpty(infoText) ? " | " : "";
                                        infoText += "由星际公民摄影工具处理";
                                    }
                                    
                                    if (!string.IsNullOrEmpty(infoText))
                                    {
                                        g.DrawString(infoText, font, whiteBrush, 20, y);
                                        y += 25;
                                    }
                                    
                                    // 绘制描述或额外信息
                                    if (!string.IsNullOrEmpty(photoInfo.Description))
                                    {
                                        g.DrawString(photoInfo.Description, font, whiteBrush, 20, y);
                                    }
                                }
                            }

                            // 保存新图片
                            string directory = Path.GetDirectoryName(imagePath);
                            string fileName = Path.GetFileNameWithoutExtension(imagePath);
                            string extension = Path.GetExtension(imagePath);
                            string newFilePath = Path.Combine(directory, $"{fileName}_info{extension}");
                            
                            newImage.Save(newFilePath, originalImage.RawFormat);
                            return newFilePath;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"添加照片信息时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> ApplyFilterAsync(string imagePath, FilterType filterType)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    using (var bitmap = new Bitmap(originalImage))
                    {
                        // 应用滤镜
                        switch (filterType)
                        {
                            case FilterType.BlackAndWhite:
                                ApplyBlackAndWhiteFilter(bitmap);
                                break;
                            case FilterType.Sepia:
                                ApplySepiaFilter(bitmap);
                                break;
                            case FilterType.HighContrast:
                                ApplyHighContrastFilter(bitmap);
                                break;
                            case FilterType.Vintage:
                                ApplyVintageFilter(bitmap);
                                break;
                            case FilterType.ColdBlue:
                                ApplyColdBlueFilter(bitmap);
                                break;
                            case FilterType.WarmOrange:
                                ApplyWarmOrangeFilter(bitmap);
                                break;
                        }
                        
                        // 添加程序水印（如果在设置中启用）
                        if (_settingsService.GetSetting<bool>("AddProgramWatermark", true))
                        {
                            AddWatermark(bitmap, "由星际公民摄影工具处理");
                        }

                        // 保存新图片
                        string directory = Path.GetDirectoryName(imagePath);
                        string fileName = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);
                        string newFilePath = Path.Combine(directory, $"{fileName}_{filterType}{extension}");
                        
                        bitmap.Save(newFilePath, originalImage.RawFormat);
                        return newFilePath;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"应用滤镜时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> AddCompositionGuideAsync(string imagePath, CompositionType compositionType)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    using (var bitmap = new Bitmap(originalImage))
                    {
                        using (var g = Graphics.FromImage(bitmap))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;

                            // 设置线条颜色和粗细
                            using (var pen = new Pen(Color.FromArgb(180, 255, 255, 255), 1))
                            {
                                int width = bitmap.Width;
                                int height = bitmap.Height;

                                switch (compositionType)
                                {
                                    case CompositionType.RuleOfThirds:
                                        // 绘制三分法线
                                        g.DrawLine(pen, width / 3, 0, width / 3, height);
                                        g.DrawLine(pen, 2 * width / 3, 0, 2 * width / 3, height);
                                        g.DrawLine(pen, 0, height / 3, width, height / 3);
                                        g.DrawLine(pen, 0, 2 * height / 3, width, 2 * height / 3);
                                        break;
                                        
                                    case CompositionType.GoldenRatio:
                                        // 绘制黄金分割线
                                        float goldenRatio = 1.618f;
                                        float smallPart = width / (1 + goldenRatio);
                                        float largePart = width - smallPart;
                                        
                                        g.DrawLine(pen, smallPart, 0, smallPart, height);
                                        g.DrawLine(pen, largePart, 0, largePart, height);
                                        
                                        smallPart = height / (1 + goldenRatio);
                                        largePart = height - smallPart;
                                        
                                        g.DrawLine(pen, 0, smallPart, width, smallPart);
                                        g.DrawLine(pen, 0, largePart, width, largePart);
                                        break;
                                        
                                    case CompositionType.Diagonal:
                                        // 绘制对角线
                                        g.DrawLine(pen, 0, 0, width, height);
                                        g.DrawLine(pen, width, 0, 0, height);
                                        break;

                                    case CompositionType.Grid:
                                        // 绘制网格
                                        for (int i = 1; i < 10; i++)
                                        {
                                            g.DrawLine(pen, i * width / 10, 0, i * width / 10, height);
                                            g.DrawLine(pen, 0, i * height / 10, width, i * height / 10);
                                        }
                                        break;
                                }
                            }
                        }
                        
                        // 添加程序水印（如果在设置中启用）
                        if (_settingsService.GetSetting<bool>("AddProgramWatermark", true))
                        {
                            AddWatermark(bitmap, "由星际公民摄影工具处理");
                        }

                        // 保存新图片
                        string directory = Path.GetDirectoryName(imagePath);
                        string fileName = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);
                        string newFilePath = Path.Combine(directory, $"{fileName}_guide{extension}");
                        
                        bitmap.Save(newFilePath, originalImage.RawFormat);
                        return newFilePath;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"添加构图辅助线时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> CropImageAsync(string imagePath, CropArea cropArea)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    {
                        // 确保裁剪区域在图像范围内
                        Rectangle cropRect = new Rectangle(
                            cropArea.X, 
                            cropArea.Y, 
                            Math.Min(cropArea.Width, originalImage.Width - cropArea.X),
                            Math.Min(cropArea.Height, originalImage.Height - cropArea.Y)
                        );
                        
                        using (var bitmap = new Bitmap(cropRect.Width, cropRect.Height))
                        {
                            using (var g = Graphics.FromImage(bitmap))
                            {
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                
                                // 裁剪图像
                                g.DrawImage(originalImage, 
                                    new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                                    cropRect, 
                                    GraphicsUnit.Pixel);
                                
                                // 添加程序水印（如果在设置中启用）
                                if (_settingsService.GetSetting<bool>("AddProgramWatermark", true))
                                {
                                    AddWatermark(bitmap, "由星际公民摄影工具处理");
                                }
                            }
                            
                            // 保存新图片
                            string directory = Path.GetDirectoryName(imagePath);
                            string fileName = Path.GetFileNameWithoutExtension(imagePath);
                            string extension = Path.GetExtension(imagePath);
                            string newFilePath = Path.Combine(directory, $"{fileName}_cropped{extension}");
                            
                            bitmap.Save(newFilePath, originalImage.RawFormat);
                            return newFilePath;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"裁剪图像时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> AddFilmGuideAsync(string imagePath, FilmAspectRatio aspectRatio)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    using (var bitmap = new Bitmap(originalImage))
                    {
                        using (var g = Graphics.FromImage(bitmap))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            
                            // 计算电影宽高比相关参数
                            int width = bitmap.Width;
                            int height = bitmap.Height;
                            int letterboxHeight = 0;
                            
                            float ratio = 0;
                            switch (aspectRatio)
                            {
                                case FilmAspectRatio.Standard: // 4:3
                                    ratio = 4f / 3f;
                                    break;
                                case FilmAspectRatio.Widescreen: // 16:9
                                    ratio = 16f / 9f;
                                    break;
                                case FilmAspectRatio.CinemaScope: // 2.35:1
                                    ratio = 2.35f;
                                    break;
                                case FilmAspectRatio.IMAX: // 1.43:1
                                    ratio = 1.43f;
                                    break;
                                case FilmAspectRatio.Anamorphic: // 2.39:1
                                    ratio = 2.39f;
                                    break;
                                case FilmAspectRatio.Academy: // 1.85:1
                                    ratio = 1.85f;
                                    break;
                            }
                            
                            // 计算黑边高度，模拟电影屏幕
                            float currentRatio = (float)width / height;
                            if (currentRatio < ratio) // 图像太窄
                            {
                                // 需要添加上下黑边
                                int idealHeight = (int)(width / ratio);
                                letterboxHeight = (height - idealHeight) / 2;
                            }
                            else if (currentRatio > ratio) // 图像太宽
                            {
                                // 需要添加左右黑边（实际在这里不处理，只添加电影辅助线）
                                int idealWidth = (int)(height * ratio);
                                int letterboxWidth = (width - idealWidth) / 2;
                                
                                // 绘制左右黑边指示线
                                using (var pen = new Pen(Color.FromArgb(180, 255, 255, 255), 2))
                                {
                                    g.DrawLine(pen, letterboxWidth, 0, letterboxWidth, height);
                                    g.DrawLine(pen, width - letterboxWidth, 0, width - letterboxWidth, height);
                                }
                            }
                            
                            // 绘制上下黑边
                            if (letterboxHeight > 0)
                            {
                                using (var brush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                                {
                                    g.FillRectangle(brush, 0, 0, width, letterboxHeight); // 上黑边
                                    g.FillRectangle(brush, 0, height - letterboxHeight, width, letterboxHeight); // 下黑边
                                }
                                
                                // 绘制上下边界线
                                using (var pen = new Pen(Color.FromArgb(220, 255, 255, 255), 1))
                                {
                                    g.DrawLine(pen, 0, letterboxHeight, width, letterboxHeight); // 上边界线
                                    g.DrawLine(pen, 0, height - letterboxHeight, width, height - letterboxHeight); // 下边界线
                                }
                            }
                            
                            // 绘制三分线（辅助构图）
                            using (var pen = new Pen(Color.FromArgb(120, 255, 255, 255), 1))
                            {
                                int effectiveHeight = height - (letterboxHeight * 2);
                                int startY = letterboxHeight;
                                
                                // 水平三分线
                                g.DrawLine(pen, 0, startY + effectiveHeight / 3, width, startY + effectiveHeight / 3);
                                g.DrawLine(pen, 0, startY + (effectiveHeight * 2) / 3, width, startY + (effectiveHeight * 2) / 3);
                                
                                // 垂直三分线
                                g.DrawLine(pen, width / 3, startY, width / 3, startY + effectiveHeight);
                                g.DrawLine(pen, (width * 2) / 3, startY, (width * 2) / 3, startY + effectiveHeight);
                            }
                            
                            // 添加中心点（辅助对焦）
                            using (var pen = new Pen(Color.FromArgb(200, 255, 255, 255), 1))
                            {
                                int centerX = width / 2;
                                int centerY = height / 2;
                                int size = 10;
                                
                                // 绘制十字线
                                g.DrawLine(pen, centerX - size, centerY, centerX + size, centerY);
                                g.DrawLine(pen, centerX, centerY - size, centerX, centerY + size);
                                
                                // 绘制小圆
                                g.DrawEllipse(pen, centerX - 5, centerY - 5, 10, 10);
                            }
                            
                            // 添加安全区域指示（内边距标记）
                            using (var pen = new Pen(Color.FromArgb(100, 255, 255, 255), 1))
                            {
                                int safeAreaWidth = width - (int)(width * 0.1);
                                int safeAreaHeight = height - (int)(height * 0.1);
                                int offsetX = (width - safeAreaWidth) / 2;
                                int offsetY = (height - safeAreaHeight) / 2;
                                
                                g.DrawRectangle(pen, offsetX, offsetY, safeAreaWidth, safeAreaHeight);
                            }
                            
                            // 添加水印标记
                            g.DrawString($"宽高比: {aspectRatio.ToString()}", 
                                new Font("Arial", 10, FontStyle.Bold), 
                                new SolidBrush(Color.FromArgb(180, 255, 255, 255)), 
                                10, 10);
                        }
                        
                        // 添加程序水印（如果在设置中启用）
                        if (_settingsService.GetSetting<bool>("AddProgramWatermark", true))
                        {
                            AddWatermark(bitmap, "由星际公民摄影工具处理");
                        }
                        
                        // 保存新图片
                        string directory = Path.GetDirectoryName(imagePath);
                        string fileName = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);
                        string newFilePath = Path.Combine(directory, $"{fileName}_film_{aspectRatio}{extension}");
                        
                        bitmap.Save(newFilePath, originalImage.RawFormat);
                        return newFilePath;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"添加电影拍摄辅助线时出错: {ex.Message}", ex);
                }
            });
        }
        
        #region 滤镜应用方法
        
        private void AddWatermark(Bitmap bitmap, string watermarkText)
        {
            try
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    
                    // 设置水印文本格式
                    using (var font = new Font("Arial", 10, FontStyle.Regular))
                    {
                        var size = g.MeasureString(watermarkText, font);
                        
                        // 水印位置：右下角
                        float x = bitmap.Width - size.Width - 10;
                        float y = bitmap.Height - size.Height - 10;
                        
                        // 确保水印位置在图像范围内
                        x = Math.Max(0, Math.Min(x, bitmap.Width - size.Width));
                        y = Math.Max(0, Math.Min(y, bitmap.Height - size.Height));
                        
                        // 绘制半透明背景
                        using (var brush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                        {
                            g.FillRectangle(brush, x - 5, y - 2, size.Width + 10, size.Height + 4);
                        }
                        
                        // 绘制文本
                        using (var brushText = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                        {
                            g.DrawString(watermarkText, font, brushText, x, y);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果水印添加失败，记录错误但不中断整个处理过程
                System.Diagnostics.Debug.WriteLine($"添加水印失败: {ex.Message}");
            }
        }
        
        private void ApplyBlackAndWhiteFilter(Bitmap bitmap)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color oldColor = bitmap.GetPixel(x, y);
                    int grayScale = (int)((oldColor.R * 0.3) + (oldColor.G * 0.59) + (oldColor.B * 0.11));
                    Color newColor = Color.FromArgb(oldColor.A, grayScale, grayScale, grayScale);
                    bitmap.SetPixel(x, y, newColor);
                }
            }
        }
        
        private void ApplySepiaFilter(Bitmap bitmap)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color oldColor = bitmap.GetPixel(x, y);
                    
                    int r = (int)((oldColor.R * 0.393) + (oldColor.G * 0.769) + (oldColor.B * 0.189));
                    int g = (int)((oldColor.R * 0.349) + (oldColor.G * 0.686) + (oldColor.B * 0.168));
                    int b = (int)((oldColor.R * 0.272) + (oldColor.G * 0.534) + (oldColor.B * 0.131));
                    
                    r = Math.Min(r, 255);
                    g = Math.Min(g, 255);
                    b = Math.Min(b, 255);
                    
                    Color newColor = Color.FromArgb(oldColor.A, r, g, b);
                    bitmap.SetPixel(x, y, newColor);
                }
            }
        }
        
        private void ApplyHighContrastFilter(Bitmap bitmap)
        {
            float contrast = 1.5f; // 对比度增加50%
            float brightness = 0f;
            
            float[][] colorMatrixElements = {
                new float[] {contrast, 0, 0, 0, 0},
                new float[] {0, contrast, 0, 0, 0},
                new float[] {0, 0, contrast, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {brightness, brightness, brightness, 0, 1}
            };
            
            var colorMatrix = new ColorMatrix(colorMatrixElements);
            using (var imageAttributes = new ImageAttributes())
            {
                imageAttributes.SetColorMatrix(colorMatrix);
                
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
        }
        
        private void ApplyVintageFilter(Bitmap bitmap)
        {
            float[][] colorMatrixElements = {
                new float[] {0.6f, 0.3f, 0.1f, 0, 0},
                new float[] {0.07f, 0.8f, 0.13f, 0, 0},
                new float[] {0.15f, 0.05f, 0.7f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0.05f, 0, 0.05f, 0, 1}
            };
            
            var colorMatrix = new ColorMatrix(colorMatrixElements);
            using (var imageAttributes = new ImageAttributes())
            {
                imageAttributes.SetColorMatrix(colorMatrix);
                
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
        }
        
        private void ApplyColdBlueFilter(Bitmap bitmap)
        {
            float[][] colorMatrixElements = {
                new float[] {0.8f, 0, 0, 0, 0},
                new float[] {0, 0.9f, 0, 0, 0},
                new float[] {0, 0, 1.2f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0.1f, 0, 1}
            };
            
            var colorMatrix = new ColorMatrix(colorMatrixElements);
            using (var imageAttributes = new ImageAttributes())
            {
                imageAttributes.SetColorMatrix(colorMatrix);
                
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
        }
        
        private void ApplyWarmOrangeFilter(Bitmap bitmap)
        {
            float[][] colorMatrixElements = {
                new float[] {1.2f, 0, 0, 0, 0},
                new float[] {0, 1.0f, 0, 0, 0},
                new float[] {0, 0, 0.8f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0.1f, 0.05f, 0, 0, 1}
            };
            
            var colorMatrix = new ColorMatrix(colorMatrixElements);
            using (var imageAttributes = new ImageAttributes())
            {
                imageAttributes.SetColorMatrix(colorMatrix);
                
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 照片信息类
    /// </summary>
    public class PhotoInfo
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public string Location { get; set; }
        public string CameraSettings { get; set; }
        public string Description { get; set; }
    }
    
    /// <summary>
    /// 裁剪区域类
    /// </summary>
    public class CropArea
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        
        public CropArea(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
    
    /// <summary>
    /// 滤镜类型
    /// </summary>
    public enum FilterType
    {
        BlackAndWhite,
        Sepia,
        HighContrast,
        Vintage,
        ColdBlue,
        WarmOrange
    }
    
    /// <summary>
    /// 构图类型
    /// </summary>
    public enum CompositionType
    {
        RuleOfThirds,
        GoldenRatio,
        Diagonal,
        Grid
    }
    
    /// <summary>
    /// 电影宽高比类型
    /// </summary>
    public enum FilmAspectRatio
    {
        Standard,    // 4:3
        Widescreen,  // 16:9
        CinemaScope, // 2.35:1
        IMAX,        // 1.43:1
        Anamorphic,  // 2.39:1
        Academy      // 1.85:1
    }
} 