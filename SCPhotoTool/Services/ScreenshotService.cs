using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Size = System.Drawing.Size;
using DrawFont = System.Drawing.Font;
using DrawFontStyle = System.Drawing.FontStyle;
using DrawRectangle = System.Drawing.Rectangle;
using DrawColor = System.Drawing.Color;
using DrawBrush = System.Drawing.SolidBrush;

namespace SCPhotoTool.Services
{
    public class ScreenshotService : IScreenshotService, IDisposable
    {
        private readonly ISettingsService _settingsService;
        private readonly IGameIntegrationService _gameIntegrationService;
        private string _currentHotkey;

        public event EventHandler HotkeyTriggered;

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // 热键修饰符常量
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;

        // 热键ID
        private const int HOTKEY_ID = 9000;

        // 窗口句柄
        private IntPtr _windowHandle;

        // 消息钩子
        private HwndSource _source;

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
            
            // 在应用程序加载完成后注册热键
            if (Application.Current != null)
            {
                if (Application.Current.MainWindow != null)
                {
                    // 主窗口已加载，直接注册热键
                    SetupHotkeys();
                }
                else
                {
                    // 等待主窗口加载完成
                    Application.Current.Startup += (s, e) => {
                        Application.Current.Dispatcher.InvokeAsync(() => {
            SetupHotkeys();
                        }, System.Windows.Threading.DispatcherPriority.Loaded);
                    };
                }
            }
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
                    // 捕获整个屏幕作为背景
                    Bitmap fullScreenBitmap = CaptureScreen();
                    
                    // 使用UI线程创建并显示区域选择窗口
                    AreaSelectedInfo selectedArea = null;
                    
                    Application.Current.Dispatcher.Invoke(() => {
                        var areaSelectWindow = new Views.AreaSelectWindow(fullScreenBitmap);
                        if (areaSelectWindow.ShowDialog() == true)
                        {
                            selectedArea = areaSelectWindow.SelectedArea;
                        }
                    });
                    
                    // 如果用户取消了选择，返回null
                    if (selectedArea == null)
                    {
                        return null;
                    }
                    
                    // 获取选中区域的截图
                    Bitmap screenshot = new Bitmap(selectedArea.Width, selectedArea.Height);

                    using (Graphics g = Graphics.FromImage(screenshot))
                    {
                        g.CopyFromScreen(selectedArea.X, selectedArea.Y, 0, 0, 
                            new Size(selectedArea.Width, selectedArea.Height));
                    }
                    
                    // 释放全屏位图
                    fullScreenBitmap.Dispose();

                    return screenshot;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"选区截图异常: {ex.Message}");
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
            // 如果应用程序未加载完成或已关闭，则跳过
            if (Application.Current == null || Application.Current.MainWindow == null)
                return;
                
            // 注销之前的热键
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
            }
            
            if (_source != null)
            {
                _source.RemoveHook(HwndHook);
            }
            
            try
            {
                // 获取主窗口句柄
                _windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                _source = HwndSource.FromHwnd(_windowHandle);
                _source?.AddHook(HwndHook);
                
                // 解析热键字符串
                uint modifiers = 0;
                uint key = 0;
                
                // 解析热键
                ParseHotkey(_currentHotkey, out modifiers, out key);
                
                // 注册热键
                if (key != 0)
                {
                    // 添加MOD_NOREPEAT标志，避免热键被持续触发
                    modifiers |= MOD_NOREPEAT;
                    
                    // 注册热键
                    bool registered = RegisterHotKey(_windowHandle, HOTKEY_ID, modifiers, key);
                    
                    if (!registered)
                    {
                        System.Diagnostics.Debug.WriteLine($"热键注册失败: {_currentHotkey}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"热键注册成功: {_currentHotkey}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置热键时出错: {ex.Message}");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            
            // 检查是否是热键消息
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                // 热键被触发
                OnHotkeyPressed();
                handled = true;
            }
            
            return IntPtr.Zero;
        }

        private void ParseHotkey(string hotkeyString, out uint modifiers, out uint key)
        {
            modifiers = 0;
            key = 0;
            
            if (string.IsNullOrEmpty(hotkeyString))
                return;
            
            // 分割热键字符串，例如 "Ctrl+Shift+S"
            string[] parts = hotkeyString.Split('+');
            
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                
                if (trimmedPart.Equals("Ctrl", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= MOD_CONTROL;
                }
                else if (trimmedPart.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= MOD_ALT;
                }
                else if (trimmedPart.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= MOD_SHIFT;
                }
                else if (trimmedPart.Equals("Win", StringComparison.OrdinalIgnoreCase))
                {
                    modifiers |= MOD_WIN;
                }
                else
                {
                    // 解析键值
                    key = ConvertKeyStringToVirtualKey(trimmedPart);
                }
            }
            
            // 处理单键热键，例如仅 "PrintScreen"
            if (parts.Length == 1 && modifiers == 0)
            {
                key = ConvertKeyStringToVirtualKey(parts[0].Trim());
            }
        }

        private uint ConvertKeyStringToVirtualKey(string keyString)
        {
            // 常用键的虚拟键码映射
            if (keyString.Equals("PrintScreen", StringComparison.OrdinalIgnoreCase))
                return 0x2C; // VK_SNAPSHOT
            
            if (keyString.Equals("F1", StringComparison.OrdinalIgnoreCase))
                return 0x70; // VK_F1
            
            if (keyString.Equals("F2", StringComparison.OrdinalIgnoreCase))
                return 0x71; // VK_F2
            
            if (keyString.Equals("F3", StringComparison.OrdinalIgnoreCase))
                return 0x72; // VK_F3
            
            if (keyString.Equals("F4", StringComparison.OrdinalIgnoreCase))
                return 0x73; // VK_F4
            
            if (keyString.Equals("F5", StringComparison.OrdinalIgnoreCase))
                return 0x74; // VK_F5
            
            if (keyString.Equals("F6", StringComparison.OrdinalIgnoreCase))
                return 0x75; // VK_F6
            
            if (keyString.Equals("F7", StringComparison.OrdinalIgnoreCase))
                return 0x76; // VK_F7
            
            if (keyString.Equals("F8", StringComparison.OrdinalIgnoreCase))
                return 0x77; // VK_F8
            
            if (keyString.Equals("F9", StringComparison.OrdinalIgnoreCase))
                return 0x78; // VK_F9
            
            if (keyString.Equals("F10", StringComparison.OrdinalIgnoreCase))
                return 0x79; // VK_F10
            
            if (keyString.Equals("F11", StringComparison.OrdinalIgnoreCase))
                return 0x7A; // VK_F11
            
            if (keyString.Equals("F12", StringComparison.OrdinalIgnoreCase))
                return 0x7B; // VK_F12
            
            if (keyString.Equals("Home", StringComparison.OrdinalIgnoreCase))
                return 0x24; // VK_HOME
            
            if (keyString.Equals("End", StringComparison.OrdinalIgnoreCase))
                return 0x23; // VK_END
            
            if (keyString.Equals("Insert", StringComparison.OrdinalIgnoreCase))
                return 0x2D; // VK_INSERT
            
            if (keyString.Equals("Delete", StringComparison.OrdinalIgnoreCase))
                return 0x2E; // VK_DELETE
            
            if (keyString.Equals("PageUp", StringComparison.OrdinalIgnoreCase))
                return 0x21; // VK_PRIOR
            
            if (keyString.Equals("PageDown", StringComparison.OrdinalIgnoreCase))
                return 0x22; // VK_NEXT
            
            if (keyString.Length == 1)
            {
                // 单个字符的按键 (A-Z, 0-9 等)
                char character = keyString.ToUpper()[0];
                return (uint)character;
            }
            
            return 0; // 未知键
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

        // 热键触发处理方法
        private void OnHotkeyPressed()
        {
            // 触发热键事件
            HotkeyTriggered?.Invoke(this, EventArgs.Empty);
        }

        // 清理资源
        public void Dispose()
        {
            // 注销热键
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
            }
            
            if (_source != null)
            {
                _source.RemoveHook(HwndHook);
                _source = null;
            }
        }
    }
} 