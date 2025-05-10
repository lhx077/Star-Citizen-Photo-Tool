using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rectangle = System.Drawing.Rectangle;

namespace SCPhotoTool.Services
{
   
    
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
                    // 加载原始图像
                    using (var originalImage = Image.FromFile(imagePath))
                    {
                        Bitmap filteredImage = null;
                        
                        // 应用滤镜
                        switch (filterType)
                        {
                            case FilterType.None:
                                // 不应用滤镜，直接复制
                                filteredImage = new Bitmap(originalImage);
                                break;
                            case FilterType.BlackAndWhite:
                                filteredImage = ApplyBlackAndWhiteFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.Sepia:
                                filteredImage = ApplySepiaFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.HighContrast:
                                filteredImage = ApplyHighContrastFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.Vintage:
                                filteredImage = ApplyVintageFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.ColdBlue:
                                filteredImage = ApplyColdBlueFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.WarmOrange:
                                filteredImage = ApplyWarmOrangeFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.SpaceNebula:
                                filteredImage = ApplySpaceNebulaFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.StarLight:
                                filteredImage = ApplyStarLightFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.CinematicSpace:
                                filteredImage = ApplyCinematicSpaceFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.SciFiTech:
                                filteredImage = ApplySciFiTechFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.VibrantColors:
                                filteredImage = ApplyVibrantColorsFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.CosmicGlow:
                                filteredImage = ApplyCosmicGlowFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.EpicSpace:
                                filteredImage = ApplyEpicSpaceFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.DeepShadows:
                                filteredImage = ApplyDeepShadowsFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.MattePainting:
                                filteredImage = ApplyMattePaintingFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.AlienWorld:
                                filteredImage = ApplyAlienWorldFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.SpaceExploration:
                                filteredImage = ApplySpaceExplorationFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.PlanetSurface:
                                filteredImage = ApplyPlanetSurfaceFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.HollywoodAction:
                                filteredImage = ApplyHollywoodActionFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.FilmNoir:
                                filteredImage = ApplyFilmNoirFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.SummerBlockbuster:
                                filteredImage = ApplySummerBlockbusterFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.Dystopian:
                                filteredImage = ApplyDystopianFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.WesternDesert:
                                filteredImage = ApplyWesternDesertFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.GalacticCore:
                                filteredImage = ApplyGalacticCoreFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.QuantumField:
                                filteredImage = ApplyQuantumFieldFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.DeepSpace:
                                filteredImage = ApplyDeepSpaceFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.EventHorizon:
                                filteredImage = ApplyEventHorizonFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.AuroraSpace:
                                filteredImage = ApplyAuroraSpaceFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.PulsarWave:
                                filteredImage = ApplyPulsarWaveFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.SolarFlare:
                                filteredImage = ApplySolarFlareFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.SupernovaRemnant:
                                filteredImage = ApplySupernovaRemnantFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.CosmicDust:
                                filteredImage = ApplyCosmicDustFilter(new Bitmap(originalImage));
                                break;
                            case FilterType.GasGiant:
                                filteredImage = ApplyGasGiantFilter(new Bitmap(originalImage));
                                break;
                            default:
                                // 其他所有滤镜暂时使用简单的RGB缩放实现
                                float rScale = 1.0f, gScale = 1.0f, bScale = 1.0f;
                                
                                // 根据滤镜类型设置不同的RGB缩放系数
                                switch (filterType)
                                {
                                    case FilterType.PlanetSurface:
                                        rScale = 1.2f;
                                        gScale = 1.1f;
                                        bScale = 0.9f;
                                        break;
                                    case FilterType.HollywoodAction:
                                        rScale = 1.15f;
                                        gScale = 1.05f;
                                        bScale = 0.9f;
                                        break;
                                    case FilterType.SummerBlockbuster:
                                        rScale = 1.1f;
                                        gScale = 1.1f;
                                        bScale = 1.0f;
                                        break;
                                    case FilterType.WesternDesert:
                                        rScale = 1.25f;
                                        gScale = 1.1f;
                                        bScale = 0.7f;
                                        break;
                                    case FilterType.SpaceExploration:
                                        rScale = 0.9f;
                                        gScale = 1.0f;
                                        bScale = 1.2f;
                                        break;
                                    case FilterType.GalacticCore:
                                        rScale = 1.1f;
                                        gScale = 0.9f;
                                        bScale = 1.3f;
                                        break;
                                    case FilterType.QuantumField:
                                        rScale = 0.9f;
                                        gScale = 1.2f;
                                        bScale = 1.3f;
                                        break;
                                    case FilterType.DeepSpace:
                                        rScale = 0.8f;
                                        gScale = 0.8f;
                                        bScale = 1.2f;
                                        break;
                                    case FilterType.EventHorizon:
                                        rScale = 0.7f;
                                        gScale = 0.8f;
                                        bScale = 1.0f;
                                        break;
                                    case FilterType.AuroraSpace:
                                        rScale = 0.8f;
                                        gScale = 1.2f;
                                        bScale = 1.1f;
                                        break;
                                    case FilterType.PulsarWave:
                                        rScale = 1.0f;
                                        gScale = 1.0f;
                                        bScale = 1.4f;
                                        break;
                                    case FilterType.SolarFlare:
                                        rScale = 1.4f;
                                        gScale = 1.1f;
                                        bScale = 0.8f;
                                        break;
                                    case FilterType.SupernovaRemnant:
                                        rScale = 1.3f;
                                        gScale = 1.0f;
                                        bScale = 1.0f;
                                        break;
                                    case FilterType.CosmicDust:
                                        rScale = 1.1f;
                                        gScale = 1.0f;
                                        bScale = 0.9f;
                                        break;
                                    case FilterType.GasGiant:
                                        rScale = 1.0f;
                                        gScale = 1.2f;
                                        bScale = 0.8f;
                                        break;
                                    case FilterType.DeepShadows:
                                        rScale = 0.7f;
                                        gScale = 0.7f;
                                        bScale = 0.8f;
                                        break;
                                    case FilterType.MattePainting:
                                        rScale = 1.1f;
                                        gScale = 1.1f;
                                        bScale = 1.0f;
                                        break;
                                    case FilterType.AlienWorld:
                                        rScale = 0.8f;
                                        gScale = 1.3f;
                                        bScale = 1.0f;
                                        break;
                                    case FilterType.Dystopian:
                                        rScale = 0.9f;
                                        gScale = 0.8f;
                                        bScale = 1.0f;
                                        break;
                                    case FilterType.FilmNoir:
                                        // 使用简易黑白效果
                                        filteredImage = ApplyBlackAndWhiteFilter(new Bitmap(originalImage));
                                        break;
                                }
                                
                                // 如果已经在特殊情况下设置了filteredImage，就不需要再创建
                                if (filteredImage == null)
                                {
                                    // 应用简单的RGB缩放滤镜
                                    filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
                                    for (int x = 0; x < originalImage.Width; x++)
                                    {
                                        for (int y = 0; y < originalImage.Height; y++)
                                        {
                                            Color pixelColor = ((Bitmap)originalImage).GetPixel(x, y);
                                            
                                            int r = Math.Min(255, (int)(pixelColor.R * rScale));
                                            int g = Math.Min(255, (int)(pixelColor.G * gScale));
                                            int b = Math.Min(255, (int)(pixelColor.B * bScale));
                                            
                                            Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                                            filteredImage.SetPixel(x, y, newColor);
                                        }
                                    }
                                }
                                break;
                        }
                        
                        if (filteredImage == null)
                        {
                            // 如果滤镜应用失败，返回原始图像路径
                            return imagePath;
                        }

                        // 生成保存路径
                        string directory = Path.GetDirectoryName(imagePath);
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);
                        string newPath = Path.Combine(directory, $"{fileNameWithoutExt}_filter_{filterType}{extension}");
                        
                        // 保存图像
                        filteredImage.Save(newPath, ImageFormat.Jpeg);
                        
                        // 释放资源
                        originalImage.Dispose();
                        filteredImage.Dispose();
                        
                        return newPath;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"应用滤镜失败: {ex.Message}");
                    return imagePath;
                }
            });
        }
        
        /// <summary>
        /// 应用黑白滤镜
        /// </summary>
        private Bitmap ApplyBlackAndWhiteFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    int gray = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
                    Color newColor = Color.FromArgb(pixelColor.A, gray, gray, gray);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用棕褐色滤镜
        /// </summary>
        private Bitmap ApplySepiaFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    int r = (int)(pixelColor.R * 0.393 + pixelColor.G * 0.769 + pixelColor.B * 0.189);
                    int g = (int)(pixelColor.R * 0.349 + pixelColor.G * 0.686 + pixelColor.B * 0.168);
                    int b = (int)(pixelColor.R * 0.272 + pixelColor.G * 0.534 + pixelColor.B * 0.131);
                    
                    r = Math.Min(r, 255);
                    g = Math.Min(g, 255);
                    b = Math.Min(b, 255);
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用高对比度滤镜
        /// </summary>
        private Bitmap ApplyHighContrastFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    int r = pixelColor.R < 128 ? (int)(pixelColor.R * 0.5) : Math.Min(255, (int)(pixelColor.R * 1.5));
                    int g = pixelColor.G < 128 ? (int)(pixelColor.G * 0.5) : Math.Min(255, (int)(pixelColor.G * 1.5));
                    int b = pixelColor.B < 128 ? (int)(pixelColor.B * 0.5) : Math.Min(255, (int)(pixelColor.B * 1.5));
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用复古滤镜
        /// </summary>
        private Bitmap ApplyVintageFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    int r = Math.Min(255, (int)(pixelColor.R * 1.2));
                    int g = Math.Min(255, (int)(pixelColor.G * 0.9));
                    int b = Math.Min(255, (int)(pixelColor.B * 0.8));
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用冷蓝滤镜
        /// </summary>
        private Bitmap ApplyColdBlueFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    int r = Math.Min(255, (int)(pixelColor.R * 0.8));
                    int g = Math.Min(255, (int)(pixelColor.G * 1.0));
                    int b = Math.Min(255, (int)(pixelColor.B * 1.2));
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用暖橙滤镜
        /// </summary>
        private Bitmap ApplyWarmOrangeFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    int r = Math.Min(255, (int)(pixelColor.R * 1.3));
                    int g = Math.Min(255, (int)(pixelColor.G * 0.9));
                    int b = Math.Min(255, (int)(pixelColor.B * 0.7));
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用星云滤镜
        /// </summary>
        private Bitmap ApplySpaceNebulaFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 增强蓝色和紫色调，创造星云效果
                    int r = Math.Min(255, (int)(pixelColor.R * 1.4));
                    int g = Math.Min(255, (int)(pixelColor.G * 0.6));
                    int b = Math.Min(255, (int)(pixelColor.B * 1.5));
                    
                    // 增加亮度对比度，使星云更加突出
                    int luminance = (int)(0.2126 * r + 0.7152 * g + 0.0722 * b);
                    
                    if (luminance > 180)
                    {
                        // 增亮明亮区域，更好地显示星云亮部
                        r = Math.Min(255, (int)(r * 1.3));
                        g = Math.Min(255, (int)(g * 1.2));
                        b = Math.Min(255, (int)(b * 1.2 + 15)); // 添加额外的蓝色光晕
                    }
                    else if (luminance < 50)
                    {
                        // 深化暗部，增强星空对比度
                        r = (int)(r * 0.7);
                        g = (int)(g * 0.7);
                        b = Math.Min(255, (int)(b * 0.9));
                    }
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用星光滤镜
        /// </summary>
        private Bitmap ApplyStarLightFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    int lightValue = (int)(pixelColor.R * 0.2126 + pixelColor.G * 0.7152 + pixelColor.B * 0.0722);
                    
                    int r, g, b;
                    
                    if (lightValue > 180) // 如果是亮点
                    {
                        // 创造星光膨胀效果，使亮点更明亮
                        r = Math.Min(255, (int)(pixelColor.R * 1.8 + 20));
                        g = Math.Min(255, (int)(pixelColor.G * 1.8 + 20));
                        b = Math.Min(255, (int)(pixelColor.B * 1.8 + 20));
                        
                        // 添加轻微的泛光效果
                        float glowFactor = 0.15f;
                        r = Math.Min(255, (int)(r * (1 - glowFactor) + 255 * glowFactor));
                        g = Math.Min(255, (int)(g * (1 - glowFactor) + 255 * glowFactor));
                        b = Math.Min(255, (int)(b * (1 - glowFactor) + 255 * glowFactor));
                    }
                    else if (lightValue < 50) // 深色部分
                    {
                        // 降低暗部亮度，增强对比度
                        r = (int)(pixelColor.R * 0.7);
                        g = (int)(pixelColor.G * 0.7);
                        b = (int)(pixelColor.B * 0.7);
                    }
                    else // 中间调
                    {
                        // 在中间调添加轻微的蓝色调
                        r = (int)(pixelColor.R * 0.95);
                        g = (int)(pixelColor.G * 0.95);
                        b = Math.Min(255, (int)(pixelColor.B * 1.1));
                    }
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用电影级太空滤镜
        /// </summary>
        private Bitmap ApplyCinematicSpaceFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 获取RGB值并转换为浮点数
                    float redF = pixelColor.R / 255.0f;
                    float greenF = pixelColor.G / 255.0f;
                    float blueF = pixelColor.B / 255.0f;
                    
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
                    int r = (int)(redF * 255);
                    int g = (int)(greenF * 255);
                    int b = (int)(blueF * 255);
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用科幻技术滤镜
        /// </summary>
        private Bitmap ApplySciFiTechFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 强化蓝色和青色调
                    int r = (int)(pixelColor.R * 0.8);
                    int g = Math.Min(255, (int)(pixelColor.G * 1.2));
                    int b = Math.Min(255, (int)(pixelColor.B * 1.3));
                    
                    // 提高对比度
                    int techBrightness = (int)(0.2126 * r + 0.7152 * g + 0.0722 * b);
                    if (techBrightness > 128)
                    {
                        // 增亮高光区域
                        b = Math.Min(255, b + 10);
                        g = Math.Min(255, g + 5);
                    }
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用鲜艳色彩滤镜
        /// </summary>
        private Bitmap ApplyVibrantColorsFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    float maxValue = Math.Max(Math.Max(pixelColor.R, pixelColor.G), pixelColor.B);
                    float minValue = Math.Min(Math.Min(pixelColor.R, pixelColor.G), pixelColor.B);
                    float delta = (maxValue - minValue) / 255.0f;
                    
                    if (delta > 0.05f)  // 只处理有一定饱和度的像素
                    {
                        float lumR = 0.213f, lumG = 0.715f, lumB = 0.072f;
                        float lumaValue = (lumR * pixelColor.R + lumG * pixelColor.G + lumB * pixelColor.B) / 255.0f;
                        float satMult = 1.5f;  // 饱和度增强系数
                        
                        float satR = (1.0f - satMult) * lumaValue + satMult * (pixelColor.R / 255.0f);
                        float satG = (1.0f - satMult) * lumaValue + satMult * (pixelColor.G / 255.0f);
                        float satB = (1.0f - satMult) * lumaValue + satMult * (pixelColor.B / 255.0f);
                        
                        int r = Math.Min(255, (int)(satR * 255));
                        int g = Math.Min(255, (int)(satG * 255));
                        int b = Math.Min(255, (int)(satB * 255));
                        
                        Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                        filteredImage.SetPixel(x, y, newColor);
                    }
                    else
                    {
                        filteredImage.SetPixel(x, y, pixelColor);
                    }
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用宇宙辉光滤镜
        /// </summary>
        private Bitmap ApplyCosmicGlowFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 定义辉光参数
            float glowIntensity = 0.3f;        // 辉光强度
            float colorShiftIntensity = 0.2f;  // 色彩偏移强度
            float contrastBoost = 1.2f;        // 对比度增强
            
            // 星云色彩
            byte[] nebulaColors = new byte[] {
                240, 100, 255,  // 紫色
                50, 100, 255,   // 蓝色
                255, 100, 180,  // 粉色
                50, 200, 255    // 青色
            };
            
            Random random = new Random(42); // 固定随机种子以确保一致效果
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    byte brightness = (byte)((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
                    
                    // 根据亮度分层处理
                    if (brightness > 200) // 高光区域 - 星光和亮部星云
                    {
                        // 增强亮部，创造辉光
                        int b = Math.Min(255, (int)(pixelColor.B * 1.4 + 25));
                        int g = Math.Min(255, (int)(pixelColor.G * 1.3 + 15));
                        int r = Math.Min(255, (int)(pixelColor.R * 1.3 + 15));
                        
                        // 添加彩色辉光效果 - 使用随机的星云色彩
                        int colorIndex = (x * 3 + y) % (nebulaColors.Length / 3) * 3;
                        
                        r = (int)(r * (1 - glowIntensity) + nebulaColors[colorIndex] * glowIntensity);
                        g = (int)(g * (1 - glowIntensity) + nebulaColors[colorIndex + 1] * glowIntensity);
                        b = (int)(b * (1 - glowIntensity) + nebulaColors[colorIndex + 2] * glowIntensity);
                        
                        r = Math.Min(255, r);
                        g = Math.Min(255, g);
                        b = Math.Min(255, b);
                        
                        // 添加微小抖动，使辉光看起来更自然
                        int jitter = 5;
                        r = Math.Min(255, Math.Max(0, r + random.Next(-jitter, jitter)));
                        g = Math.Min(255, Math.Max(0, g + random.Next(-jitter, jitter)));
                        b = Math.Min(255, Math.Max(0, b + random.Next(-jitter, jitter)));
                        
                        filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                    }
                    else if (brightness < 50) // 阴影区域 - 深空
                    {
                        // 增强深色部分的蓝色调，同时保持深黑色
                        float darkFactor = brightness / 50.0f; // 0-1范围
                        
                        int b = (int)(pixelColor.B * (0.8 + darkFactor * 0.4) + 5);
                        int g = (int)(pixelColor.G * 0.85 * darkFactor);
                        int r = (int)(pixelColor.R * 0.85 * darkFactor);
                        
                        // 确保值在有效范围内
                        b = Math.Min(255, Math.Max(0, b));
                        g = Math.Min(255, Math.Max(0, g));
                        r = Math.Min(255, Math.Max(0, r));
                        
                        filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                    }
                    else // 中间调 - 星云区域
                    {
                        // 应用对比度
                        float normalizedR = pixelColor.R / 255.0f;
                        float normalizedG = pixelColor.G / 255.0f;
                        float normalizedB = pixelColor.B / 255.0f;
                        
                        normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * contrastBoost + 0.5f));
                        normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * contrastBoost + 0.5f));
                        normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * contrastBoost + 0.5f));
                        
                        int r = (int)(normalizedR * 255);
                        int g = (int)(normalizedG * 255);
                        int b = (int)(normalizedB * 255);
                        
                        // 中间调添加彩色偏移
                        float blueFactor = Math.Min(1.0f, normalizedB + colorShiftIntensity);
                        float redFactor = normalizedR * (1.0f + colorShiftIntensity * 0.5f);
                        
                        b = Math.Min(255, (int)(b * blueFactor + 8));
                        g = (int)(g * 0.9);
                        r = Math.Min(255, (int)(r * redFactor + 5));
                        
                        filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                    }
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用史诗太空滤镜
        /// </summary>
        private Bitmap ApplyEpicSpaceFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    byte avgBrightness = (byte)((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
                    
                    // 深蓝色调整，增强太空感
                    int r = (int)(pixelColor.R * 0.8);
                    int g = (int)(pixelColor.G * 0.9);
                    int b = Math.Min(255, (int)(pixelColor.B * 1.35));
                    
                    if (avgBrightness > 180) // 亮部增强
                    {
                        // 高光部分添加蓝白辉光，增强星光效果
                        float brightFactor = 0.25f;
                        r = Math.Min(255, (int)(r * (1 - brightFactor) + 230 * brightFactor));
                        g = Math.Min(255, (int)(g * (1 - brightFactor) + 240 * brightFactor));
                        b = Math.Min(255, (int)(b * (1 - brightFactor) + 255 * brightFactor));
                    }
                    else if (avgBrightness < 60) // 暗部增强
                    {
                        // 暗部加深，增强层次感
                        r = (int)(r * 0.7);
                        g = (int)(g * 0.75);
                        b = (int)(b * 0.85);
                    }
                    else if (b > r && b > g) // 蓝色区域强化，如星云
                    {
                        b = Math.Min(255, (int)(b * 1.2));
                        r = Math.Min(255, (int)(r * 1.1)); // 轻微增加红色，形成紫色过渡
                    }
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用外星世界滤镜
        /// </summary>
        private Bitmap ApplyAlienWorldFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 外星世界参数
            float alienColorShift = 0.4f;     // 颜色偏移强度
            float atmosphereEffect = 0.25f;   // 大气效果强度
            float terrainContrast = 1.3f;     // 地形对比度
            
            // 随机种子确保一致的随机效果
            Random random = new Random(13);
            
            // 定义外星色调 - 偏向青绿色系和紫色系
            Color[] alienPalette = new Color[] {
                Color.FromArgb(20, 180, 130),  // 青绿色
                Color.FromArgb(50, 200, 180),  // 青蓝色
                Color.FromArgb(140, 230, 200), // 浅青色
                Color.FromArgb(120, 80, 180),  // 紫色
                Color.FromArgb(180, 100, 200)  // 淡紫色
            };
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    byte brightness = (byte)((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
                    
                    // 基础颜色转换 - 偏向绿色和青色
                    int r = (int)(pixelColor.R * 0.7);
                    int g = Math.Min(255, (int)(pixelColor.G * 1.3));
                    int b = Math.Min(255, (int)(pixelColor.B * 1.1));
                    
                    // 应用对比度增强
                    float normalizedR = r / 255.0f;
                    float normalizedG = g / 255.0f; 
                    float normalizedB = b / 255.0f;
                    
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * terrainContrast + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * terrainContrast + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * terrainContrast + 0.5f));
                    
                    r = (int)(normalizedR * 255);
                    g = (int)(normalizedG * 255);
                    b = (int)(normalizedB * 255);
                    
                    // 根据亮度调整色调
                    if (brightness > 200) // 亮部 - 可能是天空或高光区域
                    {
                        // 添加异域大气层效果
                        Color atmosphereColor = alienPalette[random.Next(alienPalette.Length)];
                        
                        r = (int)(r * (1 - atmosphereEffect) + atmosphereColor.R * atmosphereEffect);
                        g = (int)(g * (1 - atmosphereEffect) + atmosphereColor.G * atmosphereEffect);
                        b = (int)(b * (1 - atmosphereEffect) + atmosphereColor.B * atmosphereEffect);
                        
                        // 添加一点点大气层扩散效果
                        int diffuse = 10;
                        if (random.Next(100) < 30) // 30%的几率添加大气扩散
                        {
                            r = Math.Min(255, r + random.Next(diffuse));
                            g = Math.Min(255, g + random.Next(diffuse));
                            b = Math.Min(255, b + random.Next(diffuse));
                        }
                    }
                    else if (brightness < 60) // 暗部 - 阴影或水域
                    {
                        // 暗部添加蓝绿色调，增强外星感
                        Color shadowColor = alienPalette[0]; // 使用青绿色作为阴影色
                        float shadowIntensity = 0.3f;
                        
                        r = (int)(r * (1 - shadowIntensity) + shadowColor.R * shadowIntensity * 0.5);
                        g = (int)(g * (1 - shadowIntensity) + shadowColor.G * shadowIntensity * 0.5);
                        b = (int)(b * (1 - shadowIntensity) + shadowColor.B * shadowIntensity * 0.5);
                    }
                    
                    // 如果是红色主导的区域，转换为更异域的颜色(可能是植被或地形)
                    if (pixelColor.R > pixelColor.G && pixelColor.R > pixelColor.B)
                    {
                        // 选择一个外星调色板颜色
                        Color alienColor = alienPalette[random.Next(alienPalette.Length)];
                        
                        // 混合原始颜色和外星颜色
                        r = (int)(r * (1 - alienColorShift) + alienColor.R * alienColorShift);
                        g = (int)(g * (1 - alienColorShift) + alienColor.G * alienColorShift);
                        b = (int)(b * (1 - alienColorShift) + alienColor.B * alienColorShift);
                    }
                    
                    // 确保值在有效范围内
                    r = Math.Min(255, Math.Max(0, r));
                    g = Math.Min(255, Math.Max(0, g));
                    b = Math.Min(255, Math.Max(0, b));
                    
                    // 最终的色调调整 - 确保整体偏向青绿色系
                    float finalAdjustment = 0.05f;
                    r = (int)(r * (1 - finalAdjustment));
                    g = Math.Min(255, (int)(g * (1 + finalAdjustment)));
                    b = (int)(b * (1 + finalAdjustment * 0.5f));
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 应用太空探索滤镜
        /// </summary>
        private Bitmap ApplySpaceExplorationFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 太空探索参数
            float metalFactor = 0.8f;          // 金属感强度
            float techGlowIntensity = 0.25f;   // 科技光晕强度
            float structureEnhance = 1.15f;    // 结构增强因子
            float spaceContrast = 1.2f;        // 太空对比度
            
            // 科技HUD/全息颜色
            Color[] techColors = new Color[] {
                Color.FromArgb(80, 180, 255),   // 蓝色
                Color.FromArgb(0, 230, 255),    // 青色
                Color.FromArgb(0, 180, 210),    // 深青色
                Color.FromArgb(160, 230, 255)   // 浅蓝色
            };
            
            Random random = new Random(27);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 获取亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0f;
                    
                    // 应用对比度增强
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * spaceContrast + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * spaceContrast + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * spaceContrast + 0.5f));
                    
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    // 调整色调，强化金属感
                    float blueBoost = 1.2f;
                    
                    // 增强亮部对比度，让金属表面更加突出
                    if (brightness > 150) // 亮部 - 可能是金属表面反光
                    {
                        // 选择一个科技颜色进行混合
                        Color techColor = techColors[random.Next(techColors.Length)];
                        float brightnessFactor = Math.Min(1.0f, brightness / 255.0f);
                        
                        // 科技光晕效果 - 强度随亮度增加而增加
                        float glowFactor = techGlowIntensity * brightnessFactor;
                        
                        b = (int)(b * (1 - glowFactor) + techColor.B * glowFactor);
                        g = (int)(g * (1 - glowFactor) + techColor.G * glowFactor);
                        r = (int)(r * (1 - glowFactor) + techColor.R * glowFactor);
                        
                        // 增强金属高光
                        float reflectionFactor = Math.Min(1.0f, brightnessFactor * 1.5f);
                        
                        if (reflectionFactor > 0.7f) // 主要是高光区域
                        {
                            // 增加金属质感的反光效果
                            float metalHighlight = (reflectionFactor - 0.7f) / 0.3f * 0.2f;
                            b = Math.Min(255, b + (int)(metalHighlight * 50));
                            g = Math.Min(255, g + (int)(metalHighlight * 50));
                            r = Math.Min(255, r + (int)(metalHighlight * 50));
                        }
                    }
                    else if (brightness < 60) // 暗部 - 阴影或太空背景
                    {
                        // 阴影区域减弱红色，增强蓝色
                        float darkFactor = Math.Min(1.0f, brightness / 60.0f);
                        
                        b = (int)(b * blueBoost * (0.8f + darkFactor * 0.4f));
                        g = (int)(g * (0.8f + darkFactor * 0.2f));
                        r = (int)(r * (0.7f + darkFactor * 0.2f));
                        
                        // 确保暗部不会太暗
                        b = Math.Max(5, Math.Min(255, b));
                        g = Math.Max(2, Math.Min(255, g));
                        r = Math.Max(2, Math.Min(255, r));
                    }
                    else // 中间调 - 飞船结构和设备
                    {
                        // 中间调增加科技感蓝色
                        b = Math.Min(255, (int)(b * blueBoost * 1.05));
                        g = (int)(g * 0.95);
                        r = (int)(r * metalFactor);
                        
                        // 结构增强 - 增加细节
                        float midtoneFactor = 1.0f - Math.Abs(brightness / 255.0f - 0.5f) * 2.0f; // 接近中间调时为1，接近两端时为0
                        float structureFactor = midtoneFactor * structureEnhance;
                        
                        // 只对绿色和蓝色应用结构增强，保留红色的金属质感
                        g = (int)(g * structureFactor);
                        b = (int)(b * structureFactor);
                        
                        // 确保值在有效范围内
                        b = Math.Min(255, Math.Max(0, b));
                        g = Math.Min(255, Math.Max(0, g));
                        r = Math.Min(255, Math.Max(0, r));
                    }
                    
                    // 增加科技线条效果 - 在随机位置添加淡蓝色线条
                    if ((x % 50 == 0 || y % 50 == 0) && random.Next(100) < 15) // 15%的几率
                    {
                        Color lineColor = techColors[random.Next(techColors.Length)];
                        float lineIntensity = 0.3f * (brightness / 255.0f);
                        
                        b = (int)(b * (1 - lineIntensity) + lineColor.B * lineIntensity);
                        g = (int)(g * (1 - lineIntensity) + lineColor.G * lineIntensity);
                        r = (int)(r * (1 - lineIntensity) + lineColor.R * lineIntensity);
                    }
                    
                    // 最终的蓝色科技感增强
                    float finalBlueShift = 0.05f;
                    b = Math.Min(255, (int)(b * (1 + finalBlueShift)));
                    r = (int)(r * (1 - finalBlueShift));
                    
                    // 确保值在有效范围内
                    b = Math.Min(255, Math.Max(0, b));
                    g = Math.Min(255, Math.Max(0, g));
                    r = Math.Min(255, Math.Max(0, r));
                    
                    Color newColor = Color.FromArgb(pixelColor.A, r, g, b);
                    filteredImage.SetPixel(x, y, newColor);
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 星体星云滤镜 - 更丰富的星云效果
        /// </summary>
        private Bitmap ApplyAstralNebulaFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 星云颜色定义 - 更丰富的色彩
            Color[] nebulaColors = new Color[] {
                Color.FromArgb(150, 50, 220),  // 深紫色
                Color.FromArgb(90, 30, 200),   // 暗紫色
                Color.FromArgb(40, 120, 240),  // 亮蓝色
                Color.FromArgb(220, 70, 180),  // 粉紫色
                Color.FromArgb(30, 180, 240)   // 天蓝色
            };
            
            Random random = new Random(38);
            
            // 定义星云参数
            float nebulaIntensity = 0.35f;     // 星云强度
            float starBrightness = 1.5f;       // 星星亮度增强
            float gasCloudEffect = 0.25f;      // 气体云效果强度
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0f;
                    
                    // 根据亮度分层处理
                    if (brightness > 200) // 高亮区 - 星星
                    {
                        // 增强星星亮度和色彩
                        int r = Math.Min(255, (int)(pixelColor.R * starBrightness));
                        int g = Math.Min(255, (int)(pixelColor.G * starBrightness));
                        int b = Math.Min(255, (int)(pixelColor.B * starBrightness + 20));
                        
                        // 添加星星闪烁效果
                        if (random.Next(100) < 30) // 30% 几率添加闪烁
                        {
                            int flicker = random.Next(20, 40);
                            r = Math.Min(255, r + flicker);
                            g = Math.Min(255, g + flicker);
                            b = Math.Min(255, b + flicker);
                        }
                        
                        filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                    }
                    else if (brightness < 60) // 暗区 - 深空
                    {
                        // 选择一个星云颜色
                        Color nebulaColor = nebulaColors[random.Next(nebulaColors.Length)];
                        
                        // 计算混合系数 - 越暗的区域越多原始色彩
                        float darkFactor = brightness / 60.0f;
                        float mixFactor = darkFactor * gasCloudEffect;
                        
                        // 混合原始颜色和星云颜色
                        int r = (int)(pixelColor.R * (1 - mixFactor) + nebulaColor.R * mixFactor);
                        int g = (int)(pixelColor.G * (1 - mixFactor) + nebulaColor.G * mixFactor);
                        int b = (int)(pixelColor.B * (1 - mixFactor) + nebulaColor.B * mixFactor);
                        
                        // 增强蓝色和紫色调，提升星云质感
                        b = Math.Min(255, (int)(b * 1.1));
                        
                        // 确保值在有效范围内
                        r = Math.Min(255, Math.Max(0, r));
                        g = Math.Min(255, Math.Max(0, g));
                        b = Math.Min(255, Math.Max(0, b));
                        
                        filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                    }
                    else // 中间调 - 主要星云区域
                    {
                        // 选择星云颜色 - 使颜色分布更随机但有规律
                        Color nebulaColor = nebulaColors[(x / 30 + y / 30) % nebulaColors.Length];
                        
                        // 中间调增强
                        float enhanceFactor = 0.8f + (brightness / 255.0f) * 0.4f;
                        
                        // 计算中间调混合系数
                        float colorMix = nebulaIntensity * (1.0f - Math.Abs((brightness - 128) / 128.0f));
                        
                        // 混合原始颜色和星云颜色
                        int r = (int)(pixelColor.R * (1 - colorMix) + nebulaColor.R * colorMix);
                        int g = (int)(pixelColor.G * (1 - colorMix) + nebulaColor.G * colorMix);
                        int b = (int)(pixelColor.B * (1 - colorMix) + nebulaColor.B * colorMix);
                        
                        // 根据位置调整颜色，增加云气波动效果
                        float positionEffect = (float)Math.Sin((x + y) / 50.0) * 0.1f + 0.9f;
                        
                        r = (int)(r * positionEffect);
                        g = (int)(g * positionEffect);
                        b = (int)((b * positionEffect) * 1.1f); // 稍微增强蓝色
                        
                        // 确保值在有效范围内
                        r = Math.Min(255, Math.Max(0, r));
                        g = Math.Min(255, Math.Max(0, g));
                        b = Math.Min(255, Math.Max(0, b));
                        
                        filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                    }
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 超新星遗迹滤镜
        /// </summary>
        private Bitmap ApplySupernovaRemnantFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 超新星遗迹参数
            float shockwavePower = 0.4f;     // 冲击波强度
            float nebulaIntensity = 0.6f;    // 星云强度
            float colorIntensity = 1.4f;     // 色彩强度
            float dustFactor = 0.3f;         // 尘埃因子
            
            // 超新星遗迹的特征颜色
            Color[] remnantColors = new Color[] {
                Color.FromArgb(255, 100, 50),   // 橙红色
                Color.FromArgb(200, 70, 255),   // 紫色
                Color.FromArgb(50, 150, 255),   // 蓝色
                Color.FromArgb(255, 200, 100)   // 黄色
            };
            
            // 计算图像中心
            int centerX = originalImage.Width / 2;
            int centerY = originalImage.Height / 2;
            float maxRadius = (float)Math.Sqrt(centerX * centerX + centerY * centerY);
            
            Random random = new Random(27);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算到中心的距离
                    int deltaX = x - centerX;
                    int deltaY = y - centerY;
                    float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                    float normalizedDistance = distance / maxRadius;
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 冲击波效果 - 在特定距离范围内更强烈
                    float shockwaveRange = 0.2f; // 冲击波范围
                    float shockwaveCenter = 0.5f; // 冲击波中心位置
                    float shockwaveFactor = 0;
                    
                    if (Math.Abs(normalizedDistance - shockwaveCenter) < shockwaveRange)
                    {
                        // 计算冲击波强度 - 在中心位置最强，边缘较弱
                        shockwaveFactor = (shockwaveRange - Math.Abs(normalizedDistance - shockwaveCenter)) / shockwaveRange;
                        shockwaveFactor *= shockwavePower;
                    }
                    
                    // 计算角度用于色彩变化
                    float angle = (float)Math.Atan2(deltaY, deltaX);
                    float normalizedAngle = (angle + (float)Math.PI) / ((float)Math.PI * 2); // 0到1
                    
                    // 选择星云颜色
                    int colorIndex = (int)(normalizedAngle * remnantColors.Length);
                    colorIndex = Math.Min(remnantColors.Length - 1, Math.Max(0, colorIndex));
                    Color remnantColor = remnantColors[colorIndex];
                    
                    // 基础色彩处理
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    // 应用色彩强度
                    float colorFactor = brightness * colorIntensity;
                    normalizedR *= colorFactor;
                    normalizedG *= colorFactor;
                    normalizedB *= colorFactor;
                    
                    // 混合星云颜色
                    float nebulaFactor = nebulaIntensity * (1 - normalizedDistance * 0.7f);
                    normalizedR = normalizedR * (1 - nebulaFactor) + (remnantColor.R / 255.0f) * nebulaFactor;
                    normalizedG = normalizedG * (1 - nebulaFactor) + (remnantColor.G / 255.0f) * nebulaFactor;
                    normalizedB = normalizedB * (1 - nebulaFactor) + (remnantColor.B / 255.0f) * nebulaFactor;
                    
                    // 应用冲击波效果
                    if (shockwaveFactor > 0)
                    {
                        // 冲击波区域增亮
                        normalizedR += shockwaveFactor;
                        normalizedG += shockwaveFactor;
                        normalizedB += shockwaveFactor;
                    }
                    
                    // 添加尘埃效果
                    if (random.Next(100) < dustFactor * 100)
                    {
                        float dustAmount = (float)random.NextDouble() * 0.2f;
                        normalizedR = Math.Max(0, normalizedR - dustAmount);
                        normalizedG = Math.Max(0, normalizedG - dustAmount);
                        normalizedB = Math.Max(0, normalizedB - dustAmount);
                    }
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 宇宙尘埃滤镜
        /// </summary>
        private Bitmap ApplyCosmicDustFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 宇宙尘埃参数
            float dustDensity = 0.4f;        // 尘埃密度
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    byte brightness = (byte)((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
                    
                    // 降低饱和度，增加蓝色调和降低红色
                    int r = (int)(pixelColor.R * 0.7);
                    int g = (int)(pixelColor.G * 0.8);
                    int b = (int)(pixelColor.B * 0.9);
                    
                    // 确保暗部更暗
                    if (brightness < 128)
                    {
                        r = (int)(r * 0.8);
                        g = (int)(g * 0.85);
                    }
                    
                    // 提高对比度
                    r = (int)((r / 255.0f - 0.5f) * 1.2f * 255 + 127.5f);
                    g = (int)((g / 255.0f - 0.5f) * 1.2f * 255 + 127.5f);
                    b = (int)((b / 255.0f - 0.5f) * 1.2f * 255 + 127.5f);
                    
                    // 确保值在有效范围内
                    r = Math.Min(255, Math.Max(0, r));
                    g = Math.Min(255, Math.Max(0, g));
                    b = Math.Min(255, Math.Max(0, b));
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
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
        
        #region 辅助方法
        
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
        
        #endregion
        
        #region IPhotoInfoService未实现的接口方法
        
        public async Task<PhotoInfo> GetPhotoInfoAsync(string imagePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var photoInfo = new PhotoInfo();
                    
                    // 设置默认值
                    photoInfo.Title = Path.GetFileNameWithoutExtension(imagePath);
                    photoInfo.DateTaken = File.GetCreationTime(imagePath);
                    photoInfo.GameVersion = _settingsService.GetSetting<string>("CurrentGameVersion", "3.22.0");
                    photoInfo.Author = _settingsService.GetSetting<string>("DefaultAuthor", "星际公民玩家");
                    photoInfo.Copyright = $"© {DateTime.Now.Year} {photoInfo.Author}";
                    
                    // 如果文件存在，尝试读取EXIF信息
                    if (File.Exists(imagePath))
                    {
                        using (var image = Image.FromFile(imagePath))
                        {
                            // 读取EXIF数据
                            if (Array.IndexOf(image.PropertyIdList, 0x9003) >= 0) // DateTaken
                            {
                                var property = image.GetPropertyItem(0x9003);
                                string dateTaken = System.Text.Encoding.ASCII.GetString(property.Value).TrimEnd('\0');
                                if (DateTime.TryParseExact(dateTaken, "yyyy:MM:dd HH:mm:ss", null, 
                                    System.Globalization.DateTimeStyles.None, out DateTime dateTime))
                                {
                                    photoInfo.DateTaken = dateTime;
                                }
                            }
                            
                            // 读取相机型号
                            if (Array.IndexOf(image.PropertyIdList, 0x0110) >= 0) // CameraModel
                            {
                                var property = image.GetPropertyItem(0x0110);
                                string cameraModel = System.Text.Encoding.ASCII.GetString(property.Value).TrimEnd('\0');
                                photoInfo.CameraSettings = $"相机: {cameraModel}";
                            }
                            
                            // 读取光圈值
                            if (Array.IndexOf(image.PropertyIdList, 0x829D) >= 0) // FNumber
                            {
                                var property = image.GetPropertyItem(0x829D);
                                if (property.Type == 5 && property.Value.Length >= 8) // 5 = Rational
                                {
                                    uint numerator = BitConverter.ToUInt32(property.Value, 0);
                                    uint denominator = BitConverter.ToUInt32(property.Value, 4);
                                    if (denominator != 0)
                                    {
                                        float fNumber = numerator / (float)denominator;
                                        photoInfo.CameraSettings += $", 光圈: f/{fNumber:F1}";
                                    }
                                }
                            }
                            
                            // 读取曝光时间
                            if (Array.IndexOf(image.PropertyIdList, 0x829A) >= 0) // ExposureTime
                            {
                                var property = image.GetPropertyItem(0x829A);
                                if (property.Type == 5 && property.Value.Length >= 8) // 5 = Rational
                                {
                                    uint numerator = BitConverter.ToUInt32(property.Value, 0);
                                    uint denominator = BitConverter.ToUInt32(property.Value, 4);
                                    if (denominator != 0)
                                    {
                                        float exposureTime = numerator / (float)denominator;
                                        photoInfo.CameraSettings += $", 曝光时间: {exposureTime:F3}秒";
                                    }
                                }
                            }
                            
                            // 读取ISO值
                            if (Array.IndexOf(image.PropertyIdList, 0x8827) >= 0) // ISO
                            {
                                var property = image.GetPropertyItem(0x8827);
                                if (property.Type == 3) // 3 = Short
                                {
                                    ushort iso = BitConverter.ToUInt16(property.Value, 0);
                                    photoInfo.CameraSettings += $", ISO: {iso}";
                                }
                            }
                            
                            // 读取GPS信息(如果有)
                            if (TryGetGPSCoordinates(image, out string location))
                            {
                                photoInfo.Location = location;
                            }
                        }
                    }
                    
                    return photoInfo;
                }
                catch (Exception ex)
                {
                    // 出错时返回基本信息
                    var photoInfo = new PhotoInfo
                    {
                        Title = Path.GetFileNameWithoutExtension(imagePath),
                        DateTaken = File.GetCreationTime(imagePath),
                        GameVersion = _settingsService.GetSetting<string>("CurrentGameVersion", "3.22.0"),
                        Description = $"读取信息时出错: {ex.Message}"
                    };
                    return photoInfo;
                }
            });
        }
        
        /// <summary>
        /// 尝试获取GPS坐标信息
        /// </summary>
        private bool TryGetGPSCoordinates(Image image, out string location)
        {
            location = string.Empty;
            try
            {
                // 检查是否有GPS信息
                if (Array.IndexOf(image.PropertyIdList, 0x0001) < 0 || // GPSLatitudeRef
                    Array.IndexOf(image.PropertyIdList, 0x0002) < 0 || // GPSLatitude
                    Array.IndexOf(image.PropertyIdList, 0x0003) < 0 || // GPSLongitudeRef
                    Array.IndexOf(image.PropertyIdList, 0x0004) < 0)   // GPSLongitude
                {
                    return false;
                }
                
                // 获取纬度参考 (N 或 S)
                var latRef = image.GetPropertyItem(0x0001);
                string latitudeRef = System.Text.Encoding.ASCII.GetString(latRef.Value, 0, 1);
                
                // 获取纬度值
                var latValue = image.GetPropertyItem(0x0002);
                double[] latitude = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    uint numerator = BitConverter.ToUInt32(latValue.Value, i * 8);
                    uint denominator = BitConverter.ToUInt32(latValue.Value, i * 8 + 4);
                    latitude[i] = denominator != 0 ? numerator / (double)denominator : 0;
                }
                
                // 获取经度参考 (E 或 W)
                var lngRef = image.GetPropertyItem(0x0003);
                string longitudeRef = System.Text.Encoding.ASCII.GetString(lngRef.Value, 0, 1);
                
                // 获取经度值
                var lngValue = image.GetPropertyItem(0x0004);
                double[] longitude = new double[3];
                for (int i = 0; i < 3; i++)
                {
                    uint numerator = BitConverter.ToUInt32(lngValue.Value, i * 8);
                    uint denominator = BitConverter.ToUInt32(lngValue.Value, i * 8 + 4);
                    longitude[i] = denominator != 0 ? numerator / (double)denominator : 0;
                }
                
                // 转换为十进制格式
                double latitudeDecimal = latitude[0] + latitude[1] / 60 + latitude[2] / 3600;
                if (latitudeRef == "S") latitudeDecimal = -latitudeDecimal;
                
                double longitudeDecimal = longitude[0] + longitude[1] / 60 + longitude[2] / 3600;
                if (longitudeRef == "W") longitudeDecimal = -longitudeDecimal;
                
                // 格式化为人类可读形式
                location = $"{Math.Abs(latitudeDecimal):F6}°{latitudeRef}, {Math.Abs(longitudeDecimal):F6}°{longitudeRef}";
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<PhotoInfo> GetBasicInfoAsync(string imagePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var photoInfo = new PhotoInfo();
                    
                    // 设置基本文件信息
                    photoInfo.Title = Path.GetFileNameWithoutExtension(imagePath);
                    
                    // 获取文件创建时间和修改时间
                    var fileInfo = new FileInfo(imagePath);
                    photoInfo.DateTaken = fileInfo.CreationTime;
                    
                    // 尝试从EXIF数据获取日期
                    try
                    {
                        using (var image = Image.FromFile(imagePath))
                        {
                            // 读取EXIF日期数据
                            if (Array.IndexOf(image.PropertyIdList, 0x9003) >= 0) // DateTaken
                            {
                                var property = image.GetPropertyItem(0x9003);
                                string dateTaken = System.Text.Encoding.ASCII.GetString(property.Value).TrimEnd('\0');
                                if (DateTime.TryParseExact(dateTaken, "yyyy:MM:dd HH:mm:ss", null, 
                                    System.Globalization.DateTimeStyles.None, out DateTime dateTime))
                                {
                                    photoInfo.DateTaken = dateTime;
                                }
                            }
                            
                            // 根据文件类型尝试获取尺寸信息
                            photoInfo.Description = $"分辨率: {image.Width}x{image.Height}像素";
                            
                            // 检查文件大小
                            double fileSizeKB = fileInfo.Length / 1024.0;
                            if (fileSizeKB > 1024)
                            {
                                photoInfo.Description += $", 大小: {fileSizeKB / 1024:F2}MB";
                            }
                            else
                            {
                                photoInfo.Description += $", 大小: {fileSizeKB:F2}KB";
                            }
                            
                            // 获取图像格式
                            switch (image.RawFormat.Guid.ToString())
                            {
                                case "b96b3cab-0728-11d3-9d7b-0000f81ef32e":
                                    photoInfo.Description += ", 格式: JPEG";
                                    break;
                                case "b96b3caf-0728-11d3-9d7b-0000f81ef32e":
                                    photoInfo.Description += ", 格式: PNG";
                                    break;
                                case "b96b3cb0-0728-11d3-9d7b-0000f81ef32e":
                                    photoInfo.Description += ", 格式: GIF";
                                    break;
                                case "b96b3cb1-0728-11d3-9d7b-0000f81ef32e":
                                    photoInfo.Description += ", 格式: TIFF";
                                    break;
                                case "b96b3cb5-0728-11d3-9d7b-0000f81ef32e":
                                    photoInfo.Description += ", 格式: BMP";
                                    break;
                                default:
                                    photoInfo.Description += ", 格式: 其他";
                                    break;
                            }
                        }
                    }
                    catch
                    {
                        // 如果无法获取图像信息，仅添加文件大小
                        double fileSizeKB = fileInfo.Length / 1024.0;
                        if (fileSizeKB > 1024)
                        {
                            photoInfo.Description = $"大小: {fileSizeKB / 1024:F2}MB";
                        }
                        else
                        {
                            photoInfo.Description = $"大小: {fileSizeKB:F2}KB";
                        }
                    }
                    
                    return photoInfo;
                }
                catch (Exception ex)
                {
                    // 发生异常时返回最基本信息
                    return new PhotoInfo
                    {
                        Title = Path.GetFileNameWithoutExtension(imagePath),
                        DateTaken = DateTime.Now,
                        Description = $"读取信息失败: {ex.Message}"
                    };
                }
            });
        }
        
        public async Task<string> AdjustBrightnessAsync(string imagePath, int brightness)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    using (var bitmap = new Bitmap(originalImage))
                    {
                        // 亮度调整实现
                        // 将亮度范围(-100到100)转换为系数(0.0到2.0)
                        float brightnessScale = (brightness + 100) / 100.0f;
                        
                        // 创建颜色矩阵用于亮度调整
                        ColorMatrix colorMatrix = new ColorMatrix(new float[][] 
                        {
                            new float[] {1, 0, 0, 0, 0},
                            new float[] {0, 1, 0, 0, 0},
                            new float[] {0, 0, 1, 0, 0},
                            new float[] {0, 0, 0, 1, 0},
                            new float[] {brightnessScale - 1, brightnessScale - 1, brightnessScale - 1, 0, 1}
                        });
                        
                        // 创建图像属性对象
                        using (ImageAttributes imageAttributes = new ImageAttributes())
                        {
                            // 设置颜色矩阵
                            imageAttributes.SetColorMatrix(colorMatrix);
                            
                            // 创建图形对象
                            using (Graphics graphics = Graphics.FromImage(bitmap))
                            {
                                // 设置图形品质
                                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                
                                // 绘制调整后的图像
                                graphics.DrawImage(
                                    originalImage,
                                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                    0,
                                    0,
                                    originalImage.Width,
                                    originalImage.Height,
                                    GraphicsUnit.Pixel,
                                    imageAttributes);
                            }
                        }
                        
                        // 保存新图片
                        string directory = Path.GetDirectoryName(imagePath);
                        string fileName = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);
                        string newFilePath = Path.Combine(directory, $"{fileName}_brightness{extension}");
                        
                        bitmap.Save(newFilePath, originalImage.RawFormat);
                        return newFilePath;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"调整亮度时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> AdjustContrastAsync(string imagePath, int contrast)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    using (var bitmap = new Bitmap(originalImage))
                    {
                        // 对比度调整实现
                        // 将对比度范围(-100到100)转换为系数(0.0到2.0)
                        float contrastScale = (contrast + 100) / 100.0f;
                        
                        // 创建颜色矩阵用于对比度调整
                        ColorMatrix colorMatrix = new ColorMatrix(new float[][] 
                        {
                            new float[] {contrastScale, 0, 0, 0, 0},
                            new float[] {0, contrastScale, 0, 0, 0},
                            new float[] {0, 0, contrastScale, 0, 0},
                            new float[] {0, 0, 0, 1, 0},
                            new float[] {-0.5f * (contrastScale - 1), -0.5f * (contrastScale - 1), -0.5f * (contrastScale - 1), 0, 1}
                        });
                        
                        // 创建图像属性对象
                        using (ImageAttributes imageAttributes = new ImageAttributes())
                        {
                            // 设置颜色矩阵
                            imageAttributes.SetColorMatrix(colorMatrix);
                            
                            // 创建图形对象
                            using (Graphics graphics = Graphics.FromImage(bitmap))
                            {
                                // 设置图形品质
                                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                
                                // 绘制调整后的图像
                                graphics.DrawImage(
                                    originalImage,
                                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                    0,
                                    0,
                                    originalImage.Width,
                                    originalImage.Height,
                                    GraphicsUnit.Pixel,
                                    imageAttributes);
                            }
                        }
                        
                        // 保存新图片
                        string directory = Path.GetDirectoryName(imagePath);
                        string fileName = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);
                        string newFilePath = Path.Combine(directory, $"{fileName}_contrast{extension}");
                        
                        bitmap.Save(newFilePath, originalImage.RawFormat);
                        return newFilePath;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"调整对比度时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> AdjustSaturationAsync(string imagePath, int saturation)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    using (var bitmap = new Bitmap(originalImage))
                    {
                        // 饱和度调整实现
                        // 将饱和度范围(-100到100)转换为系数(0.0到2.0)
                        float saturationScale = (saturation + 100) / 100.0f;
                        
                        // 创建饱和度调整的颜色矩阵
                        // 使用标准的饱和度矩阵计算公式
                        float satOne = 1.0f - saturationScale;
                        float satLumR = 0.3086f * satOne;
                        float satLumG = 0.6094f * satOne;
                        float satLumB = 0.0820f * satOne;
                        
                        ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                        {
                            new float[] {satLumR + saturationScale, satLumG, satLumB, 0, 0},
                            new float[] {satLumR, satLumG + saturationScale, satLumB, 0, 0},
                            new float[] {satLumR, satLumG, satLumB + saturationScale, 0, 0},
                            new float[] {0, 0, 0, 1, 0},
                            new float[] {0, 0, 0, 0, 1}
                        });
                        
                        // 创建图像属性对象
                        using (ImageAttributes imageAttributes = new ImageAttributes())
                        {
                            // 设置颜色矩阵
                            imageAttributes.SetColorMatrix(colorMatrix);
                            
                            // 创建图形对象
                            using (Graphics graphics = Graphics.FromImage(bitmap))
                            {
                                // 设置图形品质
                                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                
                                // 绘制调整后的图像
                                graphics.DrawImage(
                                    originalImage,
                                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                    0,
                                    0,
                                    originalImage.Width,
                                    originalImage.Height,
                                    GraphicsUnit.Pixel,
                                    imageAttributes);
                            }
                        }
                        
                        // 保存新图片
                        string directory = Path.GetDirectoryName(imagePath);
                        string fileName = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);
                        string newFilePath = Path.Combine(directory, $"{fileName}_saturation{extension}");
                        
                        bitmap.Save(newFilePath, originalImage.RawFormat);
                        return newFilePath;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"调整饱和度时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> CropImageAsync(string imagePath, Rectangle rect)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    {
                        // 确保裁剪区域在图像范围内
                        Rectangle cropRect = new Rectangle(
                            rect.X,
                            rect.Y,
                            Math.Min(rect.Width, originalImage.Width - rect.X),
                            Math.Min(rect.Height, originalImage.Height - rect.Y)
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
        
        public async Task<string> CropToAspectRatioAsync(string imagePath, FilmAspectRatio aspectRatio)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    using (var bitmap = new Bitmap(originalImage))
                    {
                        // 计算宽高比
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
                        
                        // 根据宽高比计算裁剪区域
                        int width = bitmap.Width;
                        int height = bitmap.Height;
                        int newWidth, newHeight;
                        
                        float currentRatio = (float)width / height;
                        
                        if (currentRatio > ratio) // 图像太宽
                        {
                            newHeight = height;
                            newWidth = (int)(height * ratio);
                        }
                        else // 图像太高
                        {
                            newWidth = width;
                            newHeight = (int)(width / ratio);
                        }
                        
                        // 计算裁剪区域
                        int x = (width - newWidth) / 2;
                        int y = (height - newHeight) / 2;
                        
                        // 创建裁剪后的图像
                        using (var croppedBitmap = new Bitmap(newWidth, newHeight))
                        {
                            using (var g = Graphics.FromImage(croppedBitmap))
                            {
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                
                                g.DrawImage(bitmap,
                                    new Rectangle(0, 0, newWidth, newHeight),
                                    new Rectangle(x, y, newWidth, newHeight),
                                    GraphicsUnit.Pixel);
                            }
                            
                            // 保存新图片
                            string directory = Path.GetDirectoryName(imagePath);
                            string fileName = Path.GetFileNameWithoutExtension(imagePath);
                            string extension = Path.GetExtension(imagePath);
                            string newFilePath = Path.Combine(directory, $"{fileName}_aspect_{aspectRatio}{extension}");
                            
                            croppedBitmap.Save(newFilePath, originalImage.RawFormat);
                            return newFilePath;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"按宽高比裁剪图像时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> RotateImageAsync(string imagePath, int angle)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    using (var bitmap = new Bitmap(originalImage))
                    {
                        // 根据角度选择旋转方向
                        RotateFlipType rotateFlipType;
                        switch (angle)
                        {
                            case 90:
                                rotateFlipType = RotateFlipType.Rotate90FlipNone;
                                break;
                            case 180:
                                rotateFlipType = RotateFlipType.Rotate180FlipNone;
                                break;
                            case 270:
                                rotateFlipType = RotateFlipType.Rotate270FlipNone;
                                break;
                            default:
                                throw new ArgumentException($"不支持的旋转角度: {angle}，仅支持90、180、270度");
                        }
                        
                        // 旋转图像
                        bitmap.RotateFlip(rotateFlipType);
                        
                        // 保存新图片
                        string directory = Path.GetDirectoryName(imagePath);
                        string fileName = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);
                        string newFilePath = Path.Combine(directory, $"{fileName}_rotate{angle}{extension}");
                        
                        bitmap.Save(newFilePath, originalImage.RawFormat);
                        return newFilePath;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"旋转图像时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> FlipImageAsync(string imagePath, bool horizontal)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    using (var bitmap = new Bitmap(originalImage))
                    {
                        // 根据方向选择翻转类型
                        RotateFlipType rotateFlipType = horizontal
                            ? RotateFlipType.RotateNoneFlipX
                            : RotateFlipType.RotateNoneFlipY;
                        
                        // 翻转图像
                        bitmap.RotateFlip(rotateFlipType);
                        
                        // 保存新图片
                        string directory = Path.GetDirectoryName(imagePath);
                        string fileName = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);
                        string flipType = horizontal ? "horizontal" : "vertical";
                        string newFilePath = Path.Combine(directory, $"{fileName}_flip_{flipType}{extension}");
                        
                        bitmap.Save(newFilePath, originalImage.RawFormat);
                        return newFilePath;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"翻转图像时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> ResizeImageAsync(string imagePath, int width, int height)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var originalImage = Image.FromFile(imagePath))
                    {
                        using (var resizedBitmap = new Bitmap(width, height))
                        {
                            using (var g = Graphics.FromImage(resizedBitmap))
                            {
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                
                                g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, width, height));
                            }
                            
                            // 保存新图片
                            string directory = Path.GetDirectoryName(imagePath);
                            string fileName = Path.GetFileNameWithoutExtension(imagePath);
                            string extension = Path.GetExtension(imagePath);
                            string newFilePath = Path.Combine(directory, $"{fileName}_resized_{width}x{height}{extension}");
                            
                            resizedBitmap.Save(newFilePath, originalImage.RawFormat);
                            return newFilePath;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"调整图像大小时出错: {ex.Message}", ex);
                }
            });
        }
        
        public async Task<string> ShowCompositionGuideAsync(string imagePath, CompositionType compositionType)
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
                            
                            int width = bitmap.Width;
                            int height = bitmap.Height;
                            
                            // 根据不同的构图类型绘制辅助线
                            switch (compositionType)
                            {
                                case CompositionType.RuleOfThirds:
                                    DrawRuleOfThirds(g, width, height);
                                    break;
                                case CompositionType.GoldenRatio:
                                    DrawGoldenRatio(g, width, height);
                                    break;
                                case CompositionType.Diagonal:
                                    DrawDiagonal(g, width, height);
                                    break;
                                case CompositionType.Grid:
                                    DrawGrid(g, width, height);
                                    break;
                            }
                        }
                        
                        // 保存新图片
                        string directory = Path.GetDirectoryName(imagePath);
                        string fileName = Path.GetFileNameWithoutExtension(imagePath);
                        string extension = Path.GetExtension(imagePath);
                        string newFilePath = Path.Combine(directory, $"{fileName}_comp_{compositionType}{extension}");
                        
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
        
        private void DrawRuleOfThirds(Graphics g, int width, int height)
        {
            using (var pen = new Pen(Color.FromArgb(150, 255, 255, 255), 1))
            {
                // 绘制水平三分线
                g.DrawLine(pen, 0, height / 3, width, height / 3);
                g.DrawLine(pen, 0, height * 2 / 3, width, height * 2 / 3);
                
                // 绘制垂直三分线
                g.DrawLine(pen, width / 3, 0, width / 3, height);
                g.DrawLine(pen, width * 2 / 3, 0, width * 2 / 3, height);
            }
        }
        
        private void DrawGoldenRatio(Graphics g, int width, int height)
        {
            using (var pen = new Pen(Color.FromArgb(150, 255, 255, 255), 1))
            {
                float goldenRatio = 0.618f;
                
                // 水平黄金分割线
                int y1 = (int)(height * goldenRatio);
                int y2 = (int)(height * (1 - goldenRatio));
                g.DrawLine(pen, 0, y1, width, y1);
                g.DrawLine(pen, 0, y2, width, y2);
                
                // 垂直黄金分割线
                int x1 = (int)(width * goldenRatio);
                int x2 = (int)(width * (1 - goldenRatio));
                g.DrawLine(pen, x1, 0, x1, height);
                g.DrawLine(pen, x2, 0, x2, height);
            }
        }
        
        private void DrawDiagonal(Graphics g, int width, int height)
        {
            using (var pen = new Pen(Color.FromArgb(150, 255, 255, 255), 1))
            {
                // 绘制对角线
                g.DrawLine(pen, 0, 0, width, height); // 左上到右下
                g.DrawLine(pen, width, 0, 0, height); // 右上到左下
            }
        }
        
        private void DrawGrid(Graphics g, int width, int height)
        {
            using (var pen = new Pen(Color.FromArgb(120, 255, 255, 255), 1))
            {
                // 绘制网格线，每个单元格为总宽/高的1/10
                int gridSize = Math.Min(width, height) / 10;
                
                // 绘制水平线
                for (int y = gridSize; y < height; y += gridSize)
                {
                    g.DrawLine(pen, 0, y, width, y);
                }
                
                // 绘制垂直线
                for (int x = gridSize; x < width; x += gridSize)
                {
                    g.DrawLine(pen, x, 0, x, height);
                }
            }
        }
        
        #endregion
        
        /// <summary>
        /// 行星表面滤镜
        /// </summary>
        private Bitmap ApplyPlanetSurfaceFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 行星表面参数
            float redBoost = 1.2f;          // 红色增强
            float greenBoost = 1.1f;        // 绿色增强
            float blueReduction = 0.9f;     // 蓝色减弱
            float contrastFactor = 1.15f;   // 对比度增强
            float terrainDetail = 0.1f;     // 地形细节增强
            
            // 使用固定随机种子以确保一致效果
            Random random = new Random(57);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用对比度
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * contrastFactor + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * contrastFactor + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * contrastFactor + 0.5f));
                    
                    // 调整RGB色彩平衡以模拟行星表面
                    normalizedR *= redBoost;
                    normalizedG *= greenBoost;
                    normalizedB *= blueReduction;
                    
                    // 添加地形细节 - 在较暗区域增加随机变化
                    if (brightness < 0.5f)
                    {
                        float detailVariation = terrainDetail * (0.5f - brightness) * 2.0f;
                        float randValue = (float)random.NextDouble() * 2.0f - 1.0f; // -1到1的随机值
                        
                        normalizedR += randValue * detailVariation;
                        normalizedG += randValue * detailVariation;
                        normalizedB += randValue * detailVariation * 0.5f; // 蓝色变化更小
                    }
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 好莱坞动作片滤镜
        /// </summary>
        private Bitmap ApplyHollywoodActionFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 好莱坞动作片参数
            float contrastBoost = 1.3f;     // 对比度增强
            float warmthFactor = 1.15f;     // 暖色调强度
            float saturation = 1.1f;        // 饱和度增强
            float shadowDarken = 0.8f;      // 阴影加深系数
            float highlightBoost = 1.2f;    // 高光增强系数
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用对比度
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * contrastBoost + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * contrastBoost + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * contrastBoost + 0.5f));
                    
                    // 应用饱和度
                    float avgColor = (normalizedR + normalizedG + normalizedB) / 3.0f;
                    normalizedR = avgColor + (normalizedR - avgColor) * saturation;
                    normalizedG = avgColor + (normalizedG - avgColor) * saturation;
                    normalizedB = avgColor + (normalizedB - avgColor) * saturation;
                    
                    // 应用暖色调 - 增强红色和黄色，减弱蓝色
                    normalizedR *= warmthFactor;
                    normalizedG *= 1.05f;
                    normalizedB *= 0.9f;
                    
                    // 根据亮度区分阴影和高光处理
                    if (brightness < 0.3f) // 阴影区域
                    {
                        normalizedR *= shadowDarken;
                        normalizedG *= shadowDarken;
                        normalizedB *= shadowDarken;
                    }
                    else if (brightness > 0.7f) // 高光区域
                    {
                        normalizedR = Math.Min(1.0f, normalizedR * highlightBoost);
                        normalizedG = Math.Min(1.0f, normalizedG * highlightBoost);
                        normalizedB = Math.Min(1.0f, normalizedB * highlightBoost);
                    }
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 电影黑白滤镜
        /// </summary>
        private Bitmap ApplyFilmNoirFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 电影黑白参数
            float contrastFactor = 1.5f;    // 高对比度
            float grainAmount = 0.03f;      // 胶片颗粒度
            float vignetteStrength = 0.3f;  // 暗角强度
            
            Random random = new Random(39);
            
            // 计算图像中心用于暗角效果
            int centerX = originalImage.Width / 2;
            int centerY = originalImage.Height / 2;
            float maxDistance = (float)Math.Sqrt(centerX * centerX + centerY * centerY);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 转换为黑白 - 使用加权平均法以保留更多细节
                    int gray = (int)(pixelColor.R * 0.299 + pixelColor.G * 0.587 + pixelColor.B * 0.114);
                    
                    // 应用对比度
                    float normalizedGray = gray / 255.0f;
                    normalizedGray = Math.Min(1.0f, Math.Max(0.0f, (normalizedGray - 0.5f) * contrastFactor + 0.5f));
                    
                    // 添加胶片颗粒
                    float noise = (float)random.NextDouble() * 2 - 1; // -1到1之间的随机值
                    normalizedGray += noise * grainAmount;
                    
                    // 应用暗角效果 - 边缘变暗
                    float distanceFromCenter = (float)Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    float vignetteEffect = 1.0f - (distanceFromCenter / maxDistance) * vignetteStrength;
                    normalizedGray *= vignetteEffect;
                    
                    // 确保值在有效范围内
                    normalizedGray = Math.Min(1.0f, Math.Max(0.0f, normalizedGray));
                    
                    // 转换回字节
                    int grayByte = (int)(normalizedGray * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, grayByte, grayByte, grayByte));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 夏季大片滤镜
        /// </summary>
        private Bitmap ApplySummerBlockbusterFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 夏季大片参数
            float blueOrangeFactor = 0.3f;  // 蓝橙对比强度
            float saturation = 1.2f;        // 高饱和度
            float brightnessBoost = 1.1f;   // 亮度提升
            float sharpnessSimulation = 1.3f; // 锐度模拟(对比度增强)
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用锐度模拟(本质是局部对比度)
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - brightness) * sharpnessSimulation + brightness));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - brightness) * sharpnessSimulation + brightness));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - brightness) * sharpnessSimulation + brightness));
                    
                    // 应用饱和度
                    float avgColor = (normalizedR + normalizedG + normalizedB) / 3.0f;
                    normalizedR = avgColor + (normalizedR - avgColor) * saturation;
                    normalizedG = avgColor + (normalizedG - avgColor) * saturation;
                    normalizedB = avgColor + (normalizedB - avgColor) * saturation;
                    
                    // 应用蓝橙色调 - 夏季大片的典型色彩
                    // 阴影偏蓝，高光偏橙
                    if (brightness < 0.5f) // 阴影区域
                    {
                        // 添加蓝色调
                        normalizedR *= (1.0f - blueOrangeFactor * (0.5f - brightness) * 2.0f);
                        normalizedB += blueOrangeFactor * (0.5f - brightness) * 2.0f;
                    }
                    else // 高光区域
                    {
                        // 添加橙色调
                        normalizedR += blueOrangeFactor * (brightness - 0.5f) * 2.0f;
                        normalizedB *= (1.0f - blueOrangeFactor * (brightness - 0.5f));
                    }
                    
                    // 整体亮度提升
                    normalizedR *= brightnessBoost;
                    normalizedG *= brightnessBoost;
                    normalizedB *= brightnessBoost;
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 西部荒漠滤镜
        /// </summary>
        private Bitmap ApplyWesternDesertFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 西部荒漠参数
            float yellowBoost = 1.25f;      // 黄色增强
            float redBoost = 1.15f;         // 红色增强
            float blueReduction = 0.7f;     // 蓝色减弱
            float contrastBoost = 1.1f;     // 对比度增强
            float dustEffect = 0.1f;        // 灰尘效果
            
            Random random = new Random(71);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用对比度
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * contrastBoost + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * contrastBoost + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * contrastBoost + 0.5f));
                    
                    // 应用西部荒漠色彩 - 偏黄偏红，减弱蓝色
                    normalizedR *= redBoost;
                    normalizedG *= yellowBoost * 0.9f; // 黄色的绿色分量稍低
                    normalizedB *= blueReduction;
                    
                    // 添加灰尘效果 - 随机降低亮度并稍微提高红色
                    if (random.NextDouble() < dustEffect)
                    {
                        float dustAmount = (float)random.NextDouble() * 0.1f;
                        normalizedR = Math.Min(1.0f, normalizedR + dustAmount * 0.5f);
                        normalizedG = Math.Max(0.0f, normalizedG - dustAmount);
                        normalizedB = Math.Max(0.0f, normalizedB - dustAmount);
                    }
                    
                    // 天空处理 - 如果是偏蓝色且亮度高，保持一定的蓝色
                    if (normalizedB > normalizedR && normalizedB > normalizedG && brightness > 0.6f)
                    {
                        // 天空区域 - 使其偏淡蓝色而不是完全黄
                        normalizedB = Math.Max(normalizedB * 0.9f, 0.6f);
                        normalizedR *= 0.9f;
                    }
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 银河核心滤镜
        /// </summary>
        private Bitmap ApplyGalacticCoreFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 银河核心参数
            float purpleBlueBoost = 1.3f;   // 紫蓝色增强
            float redPinkBoost = 1.1f;      // 红粉色增强
            float greenReduction = 0.8f;    // 绿色减弱
            float contrastBoost = 1.2f;     // 对比度增强
            float starDensity = 0.01f;      // 星星密度
            
            Random random = new Random(42); // 固定随机种子确保一致效果
            
            // 计算图像中心用于辐射效果
            int centerX = originalImage.Width / 2;
            int centerY = originalImage.Height / 2;
            float maxDistance = (float)Math.Sqrt(centerX * centerX + centerY * centerY);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算到中心的距离比例（0-1）
                    float distanceFromCenter = (float)Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY)) / maxDistance;
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用对比度
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * contrastBoost + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * contrastBoost + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * contrastBoost + 0.5f));
                    
                    // 应用银河核心特有的颜色调整
                    // 增强蓝紫色，适度增加红粉色，减弱绿色
                    normalizedB *= purpleBlueBoost;
                    normalizedR = normalizedR * 0.7f + normalizedB * 0.3f * redPinkBoost; // 混合一些蓝色到红色通道以产生紫色
                    normalizedG *= greenReduction;
                    
                    // 根据与中心的距离添加辐射效果 - 中心更明亮
                    float centerGlow = Math.Max(0, 1.0f - distanceFromCenter * 1.2f);
                    normalizedR += centerGlow * 0.3f;
                    normalizedB += centerGlow * 0.5f;
                    
                    // 随机添加星星（高亮点）
                    if (random.NextDouble() < starDensity * (1.0f - 0.7f * distanceFromCenter)) // 中心区域星星密度更高
                    {
                        normalizedR = normalizedG = normalizedB = 1.0f;
                    }
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 量子场滤镜
        /// </summary>
        private Bitmap ApplyQuantumFieldFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 量子场参数
            float greenTealBoost = 1.2f;     // 绿青色增强
            float blueBoost = 1.3f;          // 蓝色增强
            float redReduction = 0.7f;       // 红色减弱
            float wavyEffect = 0.05f;        // 波动效果强度
            float particleEffect = 0.03f;    // 量子粒子效果
            
            Random random = new Random(123);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用波动效果 - 使用正弦波模拟量子波动
                    float waveX = (float)Math.Sin(x * 0.1f) * wavyEffect;
                    float waveY = (float)Math.Cos(y * 0.1f) * wavyEffect;
                    
                    // 使用数学函数模拟一个量子场的分布
                    float fieldIntensity = (float)(0.5f + 0.5f * Math.Sin(x * 0.05f) * Math.Cos(y * 0.05f));
                    
                    // 应用颜色调整
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    // 应用波动效果扭曲颜色通道
                    normalizedR = (normalizedR + waveX) * redReduction;
                    normalizedG = (normalizedG + waveY) * greenTealBoost;
                    normalizedB = normalizedB * blueBoost + fieldIntensity * 0.2f;
                    
                    // 增强绿色和蓝色之间的混合以产生量子效果
                    float blueGreenMix = Math.Min(normalizedG, normalizedB) * 0.5f;
                    normalizedG += blueGreenMix * 0.3f;
                    normalizedB += blueGreenMix * 0.3f;
                    
                    // 添加随机量子粒子
                    if (random.NextDouble() < particleEffect)
                    {
                        float particleIntensity = (float)random.NextDouble();
                        normalizedG = Math.Min(1.0f, normalizedG + particleIntensity * 0.5f);
                        normalizedB = Math.Min(1.0f, normalizedB + particleIntensity * 0.5f);
                    }
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 深空区域滤镜
        /// </summary>
        private Bitmap ApplyDeepSpaceFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 深空区域参数
            float darknessFactor = 0.7f;      // 整体黑暗度
            float blueIndigo = 1.2f;          // 靛蓝色增强
            float purpleTint = 1.1f;          // 紫色调增强
            float starBrightness = 1.5f;      // 星星亮度
            float starDensity = 0.005f;       // 星星密度
            
            Random random = new Random(31);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用颜色调整 - 深空以暗蓝色、靛蓝色和紫色为主
                    float normalizedR = pixelColor.R / 255.0f * darknessFactor;
                    float normalizedG = pixelColor.G / 255.0f * darknessFactor * 0.8f;
                    float normalizedB = pixelColor.B / 255.0f * blueIndigo;
                    
                    // 增加一些紫色调
                    normalizedR = normalizedR * 0.7f + normalizedB * 0.3f * purpleTint;
                    
                    // 如果原始像素足够亮，保留为星星或远处星系
                    if (brightness > 0.7f)
                    {
                        // 增强高亮区域作为星星
                        normalizedR = Math.Min(1.0f, normalizedR * starBrightness);
                        normalizedG = Math.Min(1.0f, normalizedG * starBrightness);
                        normalizedB = Math.Min(1.0f, normalizedB * starBrightness);
                    }
                    
                    // 随机添加额外的星星
                    if (random.NextDouble() < starDensity)
                    {
                        float starSize = (float)random.NextDouble() * 0.5f + 0.5f;
                        normalizedR = normalizedG = normalizedB = starSize;
                    }
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 事件视界滤镜
        /// </summary>
        private Bitmap ApplyEventHorizonFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 事件视界参数
            float darkFactor = 0.7f;        // 暗化系数
            float redShift = 0.2f;          // 红移效果
            float distortionFactor = 0.15f; // 扭曲效果强度
            float glowIntensity = 0.4f;     // 外缘发光强度
            
            // 计算图像中心用于黑洞效果
            int centerX = originalImage.Width / 2;
            int centerY = originalImage.Height / 2;
            float maxDistance = (float)Math.Sqrt(centerX * centerX + centerY * centerY);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    // 计算到图像中心的距离和角度
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    float normalizedDistance = distance / maxDistance;
                    float angle = (float)Math.Atan2(dy, dx);
                    
                    // 事件视界效果 - 根据到中心的距离应用扭曲
                    // 越靠近中心扭曲越强
                    float distortionAmount = distortionFactor * (1.0f - normalizedDistance);
                    float distortedAngle = angle + distortionAmount * 10.0f;
                    
                    // 使用扭曲后的坐标获取原始图像颜色
                    int srcX = (int)(centerX + Math.Cos(distortedAngle) * distance);
                    int srcY = (int)(centerY + Math.Sin(distortedAngle) * distance);
                    
                    // 确保坐标在图像范围内
                    srcX = Math.Min(Math.Max(srcX, 0), originalImage.Width - 1);
                    srcY = Math.Min(Math.Max(srcY, 0), originalImage.Height - 1);
                    
                    Color pixelColor = originalImage.GetPixel(srcX, srcY);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用颜色调整 - 中心偏暗，外缘偏红（红移效果）
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    // 应用暗化效果 - 中心非常暗
                    float darkenMultiplier = normalizedDistance * darkFactor + (1.0f - darkFactor);
                    normalizedR *= darkenMultiplier;
                    normalizedG *= darkenMultiplier;
                    normalizedB *= darkenMultiplier;
                    
                    // 应用红移效果 - 距离越远越红
                    normalizedR += normalizedDistance * redShift;
                    normalizedG -= normalizedDistance * redShift * 0.5f;
                    normalizedB -= normalizedDistance * redShift * 0.5f;
                    
                    // 外缘发光效果 - 在事件视界边缘产生光环
                    float edgeGlow = Math.Max(0, 1.0f - Math.Abs(normalizedDistance - 0.5f) * 5.0f);
                    normalizedR += edgeGlow * glowIntensity;
                    normalizedG += edgeGlow * glowIntensity * 0.7f;
                    normalizedB += edgeGlow * glowIntensity * 0.5f;
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 太空极光滤镜
        /// </summary>
        private Bitmap ApplyAuroraSpaceFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 太空极光参数
            float greenBoost = 1.3f;       // 绿色增强
            float blueBoost = 1.1f;        // 蓝色增强
            float wavyEffectX = 0.05f;     // 水平波纹效果
            float wavyEffectY = 0.03f;     // 垂直波纹效果
            float auroraIntensity = 0.4f;  // 极光强度
            
            Random random = new Random(85);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    // 使用正弦波模拟极光的波动效果
                    float waveX = (float)Math.Sin(y * 0.05f + x * 0.02f) * wavyEffectX;
                    float waveY = (float)Math.Cos(x * 0.05f + y * 0.03f) * wavyEffectY;
                    
                    // 计算偏移后的采样坐标
                    int sampleX = Math.Min(Math.Max((int)(x + waveX * 10), 0), originalImage.Width - 1);
                    int sampleY = Math.Min(Math.Max((int)(y + waveY * 10), 0), originalImage.Height - 1);
                    
                    Color pixelColor = originalImage.GetPixel(sampleX, sampleY);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用颜色调整 - 增强绿色和蓝色以模拟极光
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    // 根据位置计算极光颜色因子
                    float auroraFactor = Math.Max(0, (float)Math.Sin(y * 0.01f + x * 0.005f));
                    
                    // 应用极光效果 - 主要增强绿色和蓝色
                    normalizedG = Math.Min(1.0f, normalizedG * greenBoost + auroraFactor * auroraIntensity);
                    normalizedB = Math.Min(1.0f, normalizedB * blueBoost + auroraFactor * auroraIntensity * 0.5f);
                    
                    // 亮度较高的区域添加更多蓝色
                    if (brightness > 0.6f)
                    {
                        normalizedB = Math.Min(1.0f, normalizedB + 0.2f);
                    }
                    
                    // 随机添加极光闪烁效果
                    if (random.NextDouble() < 0.01f)
                    {
                        normalizedG = Math.Min(1.0f, normalizedG + 0.3f);
                        normalizedB = Math.Min(1.0f, normalizedB + 0.2f);
                    }
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
        
        /// <summary>
        /// 脉冲星波滤镜
        /// </summary>
        private Bitmap ApplyPulsarWaveFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 脉冲星波参数
            float blueBoost = 1.4f;        // 蓝色增强
            float contrast = 1.2f;         // 对比度增强
            float pulseFactor = 0.15f;     // 脉冲效果强度
            float waveSpeed = 3.0f;        // 波动速度
            
            // 计算图像中心
            int centerX = originalImage.Width / 2;
            int centerY = originalImage.Height / 2;
            float maxDistance = (float)Math.Sqrt(centerX * centerX + centerY * centerY);
            
            Random random = new Random(92);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算到中心的距离
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    float normalizedDistance = distance / maxDistance;
                    
                    // 计算脉冲波效果
                    float pulse = (float)Math.Sin(normalizedDistance * waveSpeed * Math.PI) * pulseFactor;
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用对比度
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * contrast + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * contrast + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * contrast + 0.5f));
                    
                    // 应用脉冲波效果 - 主要增强蓝色
                    normalizedB = Math.Min(1.0f, normalizedB * blueBoost + pulse);
                    normalizedR = Math.Min(1.0f, normalizedR + pulse * 0.4f);
                    
                    // 随机添加星点闪烁
                    if (random.NextDouble() < 0.005f)
                    {
                        float flicker = (float)random.NextDouble() * 0.5f + 0.5f;
                        normalizedB = flicker;
                        normalizedR = flicker * 0.5f;
                        normalizedG = flicker * 0.7f;
                    }
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }

        /// <summary>
        /// 太阳耀斑滤镜
        /// </summary>
        private Bitmap ApplySolarFlareFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 太阳耀斑参数
            float redBoost = 1.4f;        // 红色增强
            float yellowBoost = 1.3f;     // 黄色增强
            float blueReduction = 0.8f;   // 蓝色减弱
            float flareIntensity = 0.3f;  // 耀斑强度
            float flareDensity = 0.02f;   // 耀斑密度
            
            Random random = new Random(107);
            
            // 计算图像中心用于模拟太阳位置
            int centerX = originalImage.Width / 2;
            int centerY = originalImage.Height / 2;
            float maxDistance = (float)Math.Sqrt(centerX * centerX + centerY * centerY);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 计算到中心的距离比例
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    float normalizedDistance = distance / maxDistance;
                    
                    // 应用颜色调整 - 太阳耀斑以红橙黄为主
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    // 基础颜色调整
                    normalizedR *= redBoost;
                    normalizedG = normalizedG * 0.9f + normalizedR * 0.1f * yellowBoost; // 黄色 = 红色+绿色
                    normalizedB *= blueReduction;
                    
                    // 耀斑强度随距离减弱
                    float flareEffect = Math.Max(0, (1.0f - normalizedDistance * 1.2f)) * flareIntensity;
                    
                    // 应用耀斑效果 - 中心区域更亮
                    normalizedR += flareEffect;
                    normalizedG += flareEffect * 0.7f;
                    
                    // 随机添加亮点模拟小耀斑
                    if (random.NextDouble() < flareDensity * (1.0f - normalizedDistance))
                    {
                        float flareSpot = (float)random.NextDouble() * 0.5f + 0.5f;
                        normalizedR = Math.Min(1.0f, normalizedR + flareSpot * 0.7f);
                        normalizedG = Math.Min(1.0f, normalizedG + flareSpot * 0.5f);
                    }
                    
                    // 高亮区域转为红黄色调
                    if (brightness > 0.7f)
                    {
                        normalizedR = Math.Min(1.0f, normalizedR * 1.3f);
                        normalizedG = Math.Min(1.0f, normalizedG * 1.2f);
                        normalizedB = Math.Min(0.7f, normalizedB);
                    }
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }

        /// <summary>
        /// 数字绘景滤镜
        /// </summary>
        private Bitmap ApplyMattePaintingFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 数字绘景参数
            float saturationBoost = 1.1f;   // 饱和度增强
            float contrastBoost = 1.1f;     // 对比度增强
            float colorBlending = 0.15f;    // 色彩混合强度
            float detailReduction = 0.9f;   // 细节减弱（模拟画笔效果）
            float atmosphereEffect = 0.1f;  // 大气透视效果
            
            // 使用原始图像的平均颜色作为基调
            int totalR = 0, totalG = 0, totalB = 0;
            int sampleCount = Math.Min(originalImage.Width * originalImage.Height, 1000); // 限制采样数量
            
            Random random = new Random(63);
            for (int i = 0; i < sampleCount; i++)
            {
                int x = random.Next(originalImage.Width);
                int y = random.Next(originalImage.Height);
                Color sampleColor = originalImage.GetPixel(x, y);
                totalR += sampleColor.R;
                totalG += sampleColor.G;
                totalB += sampleColor.B;
            }
            
            float avgR = totalR / (float)sampleCount / 255.0f;
            float avgG = totalG / (float)sampleCount / 255.0f;
            float avgB = totalB / (float)sampleCount / 255.0f;
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用颜色调整
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    // 应用对比度
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * contrastBoost + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * contrastBoost + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * contrastBoost + 0.5f));
                    
                    // 应用饱和度增强
                    float avgColorPixel = (normalizedR + normalizedG + normalizedB) / 3.0f;
                    normalizedR = avgColorPixel + (normalizedR - avgColorPixel) * saturationBoost;
                    normalizedG = avgColorPixel + (normalizedG - avgColorPixel) * saturationBoost;
                    normalizedB = avgColorPixel + (normalizedB - avgColorPixel) * saturationBoost;
                    
                    // 与平均颜色混合以模拟艺术风格的色彩和谐
                    normalizedR = normalizedR * (1 - colorBlending) + avgR * colorBlending;
                    normalizedG = normalizedG * (1 - colorBlending) + avgG * colorBlending;
                    normalizedB = normalizedB * (1 - colorBlending) + avgB * colorBlending;
                    
                    // 模拟大气透视效果 - 距离越远越蓝
                    float yFactor = y / (float)originalImage.Height; // 假设y坐标越大表示越远
                    normalizedB = normalizedB * (1.0f - yFactor * atmosphereEffect) + yFactor * atmosphereEffect;
                    
                    // 应用画笔效果 - 减少局部细节
                    float noiseReduction = (float)random.NextDouble() * detailReduction;
                    normalizedR = normalizedR * (1.0f - noiseReduction) + avgR * noiseReduction;
                    normalizedG = normalizedG * (1.0f - noiseReduction) + avgG * noiseReduction;
                    normalizedB = normalizedB * (1.0f - noiseReduction) + avgB * noiseReduction;
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }

        /// <summary>
        /// 反乌托邦滤镜
        /// </summary>
        private Bitmap ApplyDystopianFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 反乌托邦参数
            float contrastBoost = 1.2f;     // 对比度增强
            float blueTint = 0.8f;          // 蓝色调强度
            float greenTint = 0.9f;         // 绿色调强度
            float redReduction = 0.9f;      // 红色减弱
            float shadowsDarken = 0.7f;     // 阴影加深
            float grainAmount = 0.03f;      // 颗粒感强度
            
            Random random = new Random(47);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用基本颜色调整
                    float normalizedR = pixelColor.R / 255.0f * redReduction;
                    float normalizedG = pixelColor.G / 255.0f * greenTint;
                    float normalizedB = pixelColor.B / 255.0f * blueTint;
                    
                    // 应用对比度
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * contrastBoost + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * contrastBoost + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * contrastBoost + 0.5f));
                    
                    // 暗化阴影区域
                    if (brightness < 0.5f)
                    {
                        float shadowFactor = 1.0f - (0.5f - brightness) * 2.0f * (1.0f - shadowsDarken);
                        normalizedR *= shadowFactor;
                        normalizedG *= shadowFactor;
                        normalizedB *= shadowFactor;
                    }
                    
                    // 应用青蓝色调 - 反乌托邦常见色调
                    normalizedB = Math.Max(normalizedB, Math.Min(1.0f, normalizedB + 0.1f));
                    
                    // 添加颗粒感
                    float grain = ((float)random.NextDouble() * 2.0f - 1.0f) * grainAmount;
                    normalizedR += grain;
                    normalizedG += grain;
                    normalizedB += grain;
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }

        /// <summary>
        /// 气态巨行星滤镜
        /// </summary>
        private Bitmap ApplyGasGiantFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 气态巨行星参数
            float contrastBoost = 1.1f;      // 对比度增强
            float colorVariation = 0.2f;     // 颜色变化强度
            float swirl = 0.15f;             // 旋涡效果强度
            float bandEffect = 0.2f;         // 条带效果强度
            float brightness = 1.1f;         // 整体亮度
            
            Random random = new Random(54);
            
            // 气态巨行星的基色 - 可以是橙棕色(木星)、蓝绿色(天王星)、黄色(土星)等
            float[] baseColor = new float[3];
            // 随机选择行星类型
            int planetType = random.Next(4);
            switch (planetType)
            {
                case 0: // 木星类型 - 橙棕色
                    baseColor[0] = 1.0f;  // 红色
                    baseColor[1] = 0.7f;  // 绿色
                    baseColor[2] = 0.4f;  // 蓝色
                    break;
                case 1: // 土星类型 - 黄棕色
                    baseColor[0] = 0.9f;  // 红色
                    baseColor[1] = 0.85f; // 绿色
                    baseColor[2] = 0.5f;  // 蓝色
                    break;
                case 2: // 天王星类型 - 青蓝色
                    baseColor[0] = 0.5f;  // 红色
                    baseColor[1] = 0.8f;  // 绿色
                    baseColor[2] = 0.9f;  // 蓝色
                    break;
                case 3: // 海王星类型 - 深蓝色
                    baseColor[0] = 0.3f;  // 红色
                    baseColor[1] = 0.4f;  // 绿色
                    baseColor[2] = 0.9f;  // 蓝色
                    break;
            }
            
            // 计算图像中心用于旋涡效果
            int centerX = originalImage.Width / 2;
            int centerY = originalImage.Height / 2;
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    // 计算相对于中心的坐标
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    float angle = (float)Math.Atan2(dy, dx);
                    
                    // 应用旋涡扭曲效果
                    float distortionFactor = swirl * (1.0f - Math.Min(1.0f, distance / (originalImage.Width / 2)));
                    float distortedAngle = angle + distortionFactor * 10.0f;
                    
                    // 计算扭曲后的坐标
                    int sourceX = (int)(centerX + Math.Cos(distortedAngle) * distance);
                    int sourceY = (int)(centerY + Math.Sin(distortedAngle) * distance);
                    
                    // 确保坐标在图像范围内
                    sourceX = Math.Min(Math.Max(sourceX, 0), originalImage.Width - 1);
                    sourceY = Math.Min(Math.Max(sourceY, 0), originalImage.Height - 1);
                    
                    Color pixelColor = originalImage.GetPixel(sourceX, sourceY);
                    
                    // 应用颜色调整
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    // 添加基于y坐标的条带效果
                    float bandFactor = (float)Math.Sin(y * 0.05f) * bandEffect;
                    
                    // 与基色混合
                    normalizedR = (normalizedR + baseColor[0] + bandFactor) / 3.0f * brightness;
                    normalizedG = (normalizedG + baseColor[1] + bandFactor) / 3.0f * brightness;
                    normalizedB = (normalizedB + baseColor[2] + bandFactor) / 3.0f * brightness;
                    
                    // 应用对比度
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * contrastBoost + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * contrastBoost + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * contrastBoost + 0.5f));
                    
                    // 添加随机颜色变化以模拟气体流动
                    float colorRandom = (float)random.NextDouble() * colorVariation - colorVariation / 2;
                    normalizedR += colorRandom;
                    normalizedG += colorRandom;
                    normalizedB += colorRandom * 0.5f; // 蓝色通道变化较小
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }

        /// <summary>
        /// 深沉阴影滤镜
        /// </summary>
        private Bitmap ApplyDeepShadowsFilter(Bitmap originalImage)
        {
            Bitmap filteredImage = new Bitmap(originalImage.Width, originalImage.Height);
            
            // 深沉阴影参数
            float shadowDarkness = 0.7f;    // 阴影暗化程度
            float contrast = 1.3f;          // 对比度增强
            float blueShift = 0.1f;         // 蓝色偏移
            float vignette = 0.25f;         // 暗角效果
            
            // 计算图像中心用于暗角效果
            int centerX = originalImage.Width / 2;
            int centerY = originalImage.Height / 2;
            float maxDistance = (float)Math.Sqrt(centerX * centerX + centerY * centerY);
            
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    
                    // 计算亮度
                    float brightness = (pixelColor.R + pixelColor.G + pixelColor.B) / (3.0f * 255.0f);
                    
                    // 应用颜色调整
                    float normalizedR = pixelColor.R / 255.0f;
                    float normalizedG = pixelColor.G / 255.0f;
                    float normalizedB = pixelColor.B / 255.0f;
                    
                    // 应用对比度
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, (normalizedR - 0.5f) * contrast + 0.5f));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, (normalizedG - 0.5f) * contrast + 0.5f));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, (normalizedB - 0.5f) * contrast + 0.5f));
                    
                    // 暗化深色区域，保留高光
                    if (brightness < 0.5f)
                    {
                        float shadowFactor = shadowDarkness + (0.5f - brightness) / 0.5f * (1.0f - shadowDarkness);
                        normalizedR *= shadowFactor;
                        normalizedG *= shadowFactor;
                        normalizedB *= shadowFactor * (1.0f + blueShift); // 阴影区域增加蓝色
                    }
                    
                    // 应用暗角效果
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    float distanceFactor = distance / maxDistance;
                    float vignetteEffect = 1.0f - distanceFactor * vignette;
                    
                    normalizedR *= vignetteEffect;
                    normalizedG *= vignetteEffect;
                    normalizedB *= vignetteEffect;
                    
                    // 确保值在有效范围内
                    normalizedR = Math.Min(1.0f, Math.Max(0.0f, normalizedR));
                    normalizedG = Math.Min(1.0f, Math.Max(0.0f, normalizedG));
                    normalizedB = Math.Min(1.0f, Math.Max(0.0f, normalizedB));
                    
                    // 转换回字节
                    int r = (int)(normalizedR * 255);
                    int g = (int)(normalizedG * 255);
                    int b = (int)(normalizedB * 255);
                    
                    filteredImage.SetPixel(x, y, Color.FromArgb(pixelColor.A, r, g, b));
                }
            }
            
            return filteredImage;
        }
    }
    
    /// <summary>
    /// 照片信息类
    /// </summary>
   public class PhotoInfo
    {
        /// <summary>
        /// 照片标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 照片描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 拍摄地点
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }
        
        /// <summary>
        /// 拍摄日期
        /// </summary>
        public DateTime? DateTaken { get; set; }
        
        /// <summary>
        /// 格式化后的日期字符串
        /// </summary>
        public string Date => DateTaken?.ToString("yyyy-MM-dd HH:mm");
        
        /// <summary>
        /// 相机设置
        /// </summary>
        public string CameraSettings { get; set; }
        
        /// <summary>
        /// 版权信息
        /// </summary>
        public string Copyright { get; set; }
        
        /// <summary>
        /// 游戏版本
        /// </summary>
        public string GameVersion { get; set; }
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
        None,
        BlackAndWhite,
        Sepia,
        HighContrast,
        Vintage,
        ColdBlue,
        WarmOrange,
        SpaceNebula,
        StarLight,
        CinematicSpace,
        SciFiTech,
        VibrantColors,
        CosmicGlow,
        EpicSpace,
        DeepShadows,
        MattePainting,
        AlienWorld,
        SpaceExploration,    // 太空探索滤镜
        PlanetSurface,       // 行星表面滤镜
        HollywoodAction,     // 好莱坞动作片滤镜
        FilmNoir,            // 黑色电影滤镜
        SummerBlockbuster,   // 夏季大片滤镜
        Dystopian,           // 反乌托邦电影滤镜
        WesternDesert,       // 西部荒漠电影滤镜
        GalacticCore,        // 银河核心滤镜
        QuantumField,        // 量子场滤镜
        DeepSpace,           // 深空区域滤镜
        EventHorizon,        // 事件视界滤镜
        AuroraSpace,         // 太空极光滤镜
        PulsarWave,          // 脉冲星波滤镜
        SolarFlare,          // 太阳耀斑滤镜
        SupernovaRemnant,    // 超新星遗迹滤镜
        CosmicDust,          // 宇宙尘埃滤镜
        GasGiant             // 气态巨行星滤镜
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