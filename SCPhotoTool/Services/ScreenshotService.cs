using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Size = System.Drawing.Size;
using DrawFont = System.Drawing.Font;
using DrawFontStyle = System.Drawing.FontStyle;
using DrawRectangle = System.Drawing.Rectangle;
using DrawColor = System.Drawing.Color;
using DrawBrush = System.Drawing.SolidBrush;

namespace SCPhotoTool.Services
{
    public class ScreenshotService : IScreenshotService
    {
        private readonly ISettingsService _settingsService;
        private readonly IGameIntegrationService _gameIntegrationService;
        private string _currentHotkey;

        public event EventHandler HotkeyTriggered;

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public ScreenshotService(ISettingsService settingsService, IGameIntegrationService gameIntegrationService)
        {
            _settingsService = settingsService;
            _gameIntegrationService = gameIntegrationService;
            
            // 初始化热键
            _currentHotkey = _settingsService.GetSetting<string>("ScreenshotHotkey", "PrintScreen");
            SetupHotkeys();
        }

        public async Task<Bitmap> CaptureScreenAsync()
        {
            return await Task.Run(() =>
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                Bitmap screenshot = new Bitmap((int)screenWidth, (int)screenHeight);

                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(0, 0, 0, 0, new Size((int)screenWidth, (int)screenHeight));
                }

                return screenshot;
            });
        }

        public async Task<Bitmap> CaptureGameWindowAsync()
        {
            return await Task.Run(() =>
            {
                // 获取游戏窗口
                if (!_gameIntegrationService.IsConnected)
                {
                    // 如果找不到游戏窗口，退回到全屏截图
                    return CaptureScreen();
                }

                // 检查窗口模式
                if (_gameIntegrationService.IsWindowedMode)
                {
                    // 使用客户区域截图（去除标题栏）
                    // 直接获取客户区坐标和尺寸，避免使用Rectangle结构体
                    var (x, y, width, height) = _gameIntegrationService.GetGameClientCoords();
                    
                    if (width > 0 && height > 0)
                    {
                        try
                        {
                            Bitmap screenshot = new Bitmap(width, height);
                            
                            using (Graphics g = Graphics.FromImage(screenshot))
                            {
                                g.CopyFromScreen(x, y, 0, 0, 
                                    new Size(width, height));
                            }
                            
                            return screenshot;
                        }
                        catch (Exception)
                        {
                            // 如果截图失败，回退到全屏截图
                            return CaptureScreen();
                        }
                    }
                }
                else
                {
                    // 全屏模式，获取整个窗口
                    // 直接获取窗口坐标和尺寸，避免使用Rectangle结构体
                    var (x, y, width, height) = _gameIntegrationService.GetGameWindowCoords();
                    
                    if (width > 0 && height > 0)
                    {
                        try
                        {
                            Bitmap screenshot = new Bitmap(width, height);
                            
                            using (Graphics g = Graphics.FromImage(screenshot))
                            {
                                // 使用高质量设置获取更清晰的截图
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                
                                g.CopyFromScreen(x, y, 0, 0, 
                                    new Size(width, height));
                            }
                            
                            return screenshot;
                        }
                        catch (Exception)
                        {
                            // 如果截图失败，回退到全屏截图
                            return CaptureScreen();
                        }
                    }
                }

                // 如果无法获取窗口大小，退回到全屏截图
                return CaptureScreen();
            });
        }
        
        public async Task<Bitmap> CaptureSelectedAreaAsync()
        {
            // 创建一个截图选择工具
            return await Task.Run(() =>
            {
                try
                {
                    // 在实际实现中，这里应该显示一个区域选择UI
                    // 让用户可以拖动鼠标选择要截图的区域
                    
                    // 模拟用户选择的区域（实际应用中这部分需要交互）
                    int x = 100;
                    int y = 100;
                    int width = 800;
                    int height = 600;
                    
                    // 获取选中区域的截图
                    Bitmap screenshot = new Bitmap(width, height);
                    
                    using (Graphics g = Graphics.FromImage(screenshot))
                    {
                        g.CopyFromScreen(x, y, 0, 0, new Size(width, height));
                    }
                    
                    return screenshot;
                }
                catch (Exception)
                {
                    // 如果选区截图失败，返回全屏截图
                    return CaptureScreen();
                }
            });
        }

        public async Task<string> SaveScreenshotAsync(Bitmap bitmap, string filePath = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (string.IsNullOrEmpty(filePath))
            {
                string directory = _settingsService.GetScreenshotDirectory();
                string fileNameFormat = _settingsService.GetScreenshotFileNameFormat();
                string fileName = string.Format(fileNameFormat, DateTime.Now);
                filePath = Path.Combine(directory, $"{fileName}.png");
                
                // 确保目录存在
                Directory.CreateDirectory(directory);
            }

            return await Task.Run(() =>
            {
                // 获取质量设置
                int quality = _settingsService.GetScreenshotQuality();
                
                // 创建质量参数
                ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
                EncoderParameters parameters = new EncoderParameters(1);
                parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                // 保存为JPEG格式带质量设置
                if (Path.GetExtension(filePath).ToLower() == ".jpg" || Path.GetExtension(filePath).ToLower() == ".jpeg")
                {
                    bitmap.Save(filePath, jpegCodec, parameters);
                }
                else // 保存为PNG
                {
                    bitmap.Save(filePath, ImageFormat.Png);
                }

                return filePath;
            });
        }
        
        // 添加水印到图像
        public Bitmap AddWatermark(Bitmap source, string watermarkText)
        {
            if (source == null)
                return source;
                
            Bitmap result = new Bitmap(source.Width, source.Height);
            
            using (Graphics g = Graphics.FromImage(result))
            {
                // 绘制原图像
                g.DrawImage(source, 0, 0, source.Width, source.Height);
                
                if (!string.IsNullOrEmpty(watermarkText))
                {
                    // 获取游戏版本信息
                    string gameVersion = _gameIntegrationService.IsConnected 
                        ? _gameIntegrationService.GameVersion 
                        : "SC Alpha 4.1";
                
                    // 组合水印文本，如果用户已提供文本，附加版本信息
                    string fullWatermarkText = watermarkText;
                    if (!watermarkText.Contains(gameVersion))
                    {
                        fullWatermarkText += $" | {gameVersion}";
                    }
                    
                    // 创建半透明文字
                    using (DrawFont font = new DrawFont("Arial", 20, DrawFontStyle.Bold))
                    using (DrawBrush textBrush = new DrawBrush(DrawColor.FromArgb(180, 255, 255, 255)))
                    using (DrawBrush shadowBrush = new DrawBrush(DrawColor.FromArgb(120, 0, 0, 0)))
                    using (StringFormat format = new StringFormat())
                    {
                        format.Alignment = StringAlignment.Far;
                        format.LineAlignment = StringAlignment.Far;
                        
                        // 添加阴影效果
                        System.Drawing.Rectangle textRect = new System.Drawing.Rectangle(0, 0, source.Width, source.Height);
                        g.DrawString(fullWatermarkText, font, shadowBrush, 
                            new System.Drawing.Rectangle(textRect.X + 2, textRect.Y + 2, textRect.Width, textRect.Height), 
                            format);
                        
                        // 绘制主水印文本
                        g.DrawString(fullWatermarkText, font, textBrush, textRect, format);
                    }
                }
            }
            
            return result;
        }

        public void SetHotkey(string hotkey)
        {
            _currentHotkey = hotkey;
            _settingsService.SaveSettingAsync("ScreenshotHotkey", hotkey);
            SetupHotkeys();
        }

        private void SetupHotkeys()
        {
            // 在实际应用中，这里需要实现全局热键注册
            // 由于需要调用底层API进行热键注册，这部分代码较为复杂
            // 实际实现可能需要使用P/Invoke注册系统热键或使用第三方库
        }

        private Bitmap CaptureScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Bitmap screenshot = new Bitmap((int)screenWidth, (int)screenHeight);

            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(0, 0, 0, 0, new Size((int)screenWidth, (int)screenHeight));
            }

            return screenshot;
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

        // 在实际应用中，需要添加热键触发的处理方法
        private void OnHotkeyPressed()
        {
            HotkeyTriggered?.Invoke(this, EventArgs.Empty);
        }
    }
} 