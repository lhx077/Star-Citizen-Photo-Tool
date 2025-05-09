using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Size = System.Drawing.Size;

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
                IntPtr gameWindow = _gameIntegrationService.GetGameWindowHandle();
                if (gameWindow == IntPtr.Zero)
                {
                    // 如果找不到游戏窗口，退回到全屏截图
                    return CaptureScreen();
                }

                if (GetWindowRect(gameWindow, out RECT rect))
                {
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    Bitmap screenshot = new Bitmap(width, height);

                    using (Graphics g = Graphics.FromImage(screenshot))
                    {
                        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
                    }

                    return screenshot;
                }
                else
                {
                    // 如果无法获取窗口大小，退回到全屏截图
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