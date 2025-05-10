using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SCPhotoTool.Services
{
    public class GameIntegrationService : IGameIntegrationService
    {
        private readonly ISettingsService _settingsService;
        private readonly IPhotoLibraryService _photoLibraryService;
        private Process _gameProcess;
        private System.Threading.Timer _gameDetectionTimer;
        private const string SC_ALPHA_VERSION = "4.1";
        private const string SC_BUILD_VERSION = "8797428"; // Alpha 4.1最新构建版本号

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);
        
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        
        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        
        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_BORDER = 0x00800000;

        public bool IsConnected => _gameProcess != null && !_gameProcess.HasExited;
        public string GameVersion { get; private set; } = "未知";
        public bool IsWindowedMode { get; private set; } = false;
        public bool IsWindowedFullscreen { get; private set; } = false;
        public int TitleBarHeight { get; private set; } = 32; // 默认标题栏高度

        public event EventHandler<bool> GameConnectionStatusChanged;

        public GameIntegrationService(ISettingsService settingsService, IPhotoLibraryService photoLibraryService)
        {
            _settingsService = settingsService;
            _photoLibraryService = photoLibraryService;
            
            // 启动定时检测游戏进程是否存在
            _gameDetectionTimer = new System.Threading.Timer(DetectGameProcess, null, 
                TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(15));
        }

        private void DetectGameProcess(object state)
        {
            if (!IsConnected)
            {
                // 如果没有连接，尝试连接
                if (Connect())
                {
                    // 连接成功，通知UI更新
                }
            }
            else
            {
                // 已连接，检查进程是否还存在
                if (_gameProcess.HasExited)
                {
                    // 进程已退出，断开连接
                    Disconnect();
                }
                else
                {
                    // 进程仍然存在，更新窗口模式检测
                    DetectWindowMode();
                }
            }
        }

        public bool Connect()
        {
            try
            {
                // 尝试寻找运行中的Star Citizen进程
                // 首先尝试StarCitizen.exe (RSI启动器)
                Process[] processes = Process.GetProcessesByName("StarCitizen");
                if (processes.Length > 0)
                {
                    _gameProcess = processes[0];
                    GameVersion = DetectGameVersion();
                    DetectWindowMode();
                    
                    GameConnectionStatusChanged?.Invoke(this, true);
                    return true;
                }

                // 尝试查找starcitizen.exe (WINE/Linux环境)
                processes = Process.GetProcessesByName("starcitizen");
                if (processes.Length > 0)
                {
                    _gameProcess = processes[0];
                    GameVersion = DetectGameVersion();
                    DetectWindowMode();
                    
                    GameConnectionStatusChanged?.Invoke(this, true);
                    return true;
                }

                // 尝试RSI启动器
                processes = Process.GetProcessesByName("RSI Launcher");
                if (processes.Length > 0)
                {
                    // 启动器已运行，尝试查找游戏进程
                    Process[] studioProcesses = Process.GetProcessesByName("starcitizen");
                    if (studioProcesses.Length > 0)
                    {
                        _gameProcess = studioProcesses[0];
                        GameVersion = DetectGameVersion();
                        DetectWindowMode();
                        
                        GameConnectionStatusChanged?.Invoke(this, true);
                        return true;
                    }
                }

                // 尝试通过窗口标题查找
                IntPtr windowHandle = FindStarCitizenWindow();
                if (windowHandle != IntPtr.Zero)
                {
                    uint processId;
                    GetWindowThreadProcessId(windowHandle, out processId);
                    _gameProcess = Process.GetProcessById((int)processId);
                    GameVersion = DetectGameVersion();
                    DetectWindowMode();
                    
                    GameConnectionStatusChanged?.Invoke(this, true);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private IntPtr FindStarCitizenWindow()
        {
            // 搜索包含"Star Citizen"的窗口
            IntPtr foundWindow = IntPtr.Zero;
            
            EnumWindows((hWnd, lParam) => {
                if (IsWindowVisible(hWnd))
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder(length + 1);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        string title = sb.ToString();
                        
                        if (title.Contains("Star Citizen") || 
                            title.ToLower().Contains("starcitizen") ||
                            title.Contains("SC Alpha") ||
                            title.Contains($"SC {SC_ALPHA_VERSION}"))
                        {
                            foundWindow = hWnd;
                            return false; // 停止枚举
                        }
                    }
                }
                return true; // 继续枚举
            }, IntPtr.Zero);
            
            return foundWindow;
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        public void Disconnect()
        {
            _gameProcess = null;
            GameVersion = "未知";
            IsWindowedMode = false;
            IsWindowedFullscreen = false;
            GameConnectionStatusChanged?.Invoke(this, false);
        }

        public IntPtr GetGameWindowHandle()
        {
            if (!IsConnected)
                return IntPtr.Zero;

            try
            {
                return _gameProcess.MainWindowHandle;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        public string GetGameWindowTitle()
        {
            IntPtr handle = GetGameWindowHandle();
            if (handle == IntPtr.Zero)
                return string.Empty;

            const int nChars = 256;
            System.Text.StringBuilder buff = new System.Text.StringBuilder(nChars);
            
            if (GetWindowText(handle, buff, nChars) > 0)
            {
                return buff.ToString();
            }
            return string.Empty;
        }

        private void DetectWindowMode()
        {
            IntPtr hwnd = GetGameWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                // 获取窗口样式
                IntPtr style = GetWindowLong(hwnd, GWL_STYLE);
                
                // 判断是否有标题栏和可调整边框
                bool hasCaption = (style.ToInt64() & WS_CAPTION) != 0;
                bool hasThickFrame = (style.ToInt64() & WS_THICKFRAME) != 0;
                bool hasBorder = (style.ToInt64() & WS_BORDER) != 0;
                
                IsWindowedMode = hasCaption || hasThickFrame || hasBorder;
                
                // 判断是否为无边框全屏窗口模式
                if (!IsWindowedMode)
                {
                    // 获取窗口尺寸
                    GetWindowRect(hwnd, out RECT windowRect);
                    int windowWidth = windowRect.Right - windowRect.Left;
                    int windowHeight = windowRect.Bottom - windowRect.Top;
                    
                    // 获取屏幕尺寸
                    int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                    int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                    
                    // 如果窗口尺寸与屏幕尺寸相近，且没有边框，可能是无边框全屏模式
                    if (Math.Abs(windowWidth - screenWidth) < 10 && Math.Abs(windowHeight - screenHeight) < 10)
                    {
                        IsWindowedFullscreen = true;
                    }
                }
                else
                {
                    IsWindowedFullscreen = false;
                }
                
                // 计算标题栏高度
                if (IsWindowedMode && !IsWindowedFullscreen)
                {
                    // 如果是窗口模式，获取标题栏高度
                    GetWindowRect(hwnd, out RECT windowRect);
                    GetClientRect(hwnd, out RECT clientRect);
                    
                    POINT clientPoint = new POINT { X = 0, Y = 0 };
                    ClientToScreen(hwnd, ref clientPoint);
                    
                    // 计算标题栏高度
                    TitleBarHeight = clientPoint.Y - windowRect.Top;
                    
                    // 防止出现负值
                    if (TitleBarHeight < 0) TitleBarHeight = hasCaption ? 32 : 0;
                }
                else
                {
                    TitleBarHeight = 0;
                }
            }
        }

        public string GetGameScreenshotDirectory()
        {
            // 尝试查找游戏截图目录
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string defaultPath = Path.Combine(userProfile, "Pictures", "Star Citizen");
            
            // 从设置中读取自定义路径
            string customPath = _settingsService.GetSetting<string>("GameScreenshotDir", null);
            if (!string.IsNullOrEmpty(customPath) && Directory.Exists(customPath))
            {
                return customPath;
            }

            // Alpha 4.1的可能路径
            string alphaPath = Path.Combine(userProfile, "Roberts Space Industries", "StarCitizen", "LIVE", "screenshots");
            if (Directory.Exists(alphaPath))
            {
                return alphaPath;
            }
            
            // 尝试不同大小写的目录名
            string alphaPathAlt = Path.Combine(userProfile, "Roberts Space Industries", "StarCitizen", "LIVE", "ScreenShots");
            if (Directory.Exists(alphaPathAlt))
            {
                return alphaPathAlt;
            }
            
            // PTU环境下的可能路径
            string ptuPath = Path.Combine(userProfile, "Roberts Space Industries", "StarCitizen", "PTU", "screenshots");
            if (Directory.Exists(ptuPath))
            {
                return ptuPath;
            }
            
            // PTU大写目录
            string ptuPathAlt = Path.Combine(userProfile, "Roberts Space Industries", "StarCitizen", "PTU", "ScreenShots");
            if (Directory.Exists(ptuPathAlt))
            {
                return ptuPathAlt;
            }

            // 创建默认目录
            if (!Directory.Exists(defaultPath))
            {
                try
                {
                    Directory.CreateDirectory(defaultPath);
                }
                catch
                {
                    // 忽略创建目录失败的情况
                }
            }

            return defaultPath;
        }

        public async Task<int> ImportGameScreenshotsAsync()
        {
            string directory = GetGameScreenshotDirectory();
            if (!Directory.Exists(directory))
                return 0;

            // 获取支持的图片文件格式
            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
            List<string> importedFiles = new List<string>();

            try
            {
                // 查找所有图片文件
                foreach (string ext in supportedExtensions)
                {
                    string[] files = Directory.GetFiles(directory, $"*{ext}", SearchOption.AllDirectories);
                    importedFiles.AddRange(files);
                }

                // 导入到照片库
                int importCount = 0;
                foreach (string file in importedFiles)
                {
                    // 检查文件是否已导入
                    if (await _photoLibraryService.IsPhotoImportedAsync(file))
                        continue;
                    
                    // 从文件名中尝试提取版本信息
                    string fileName = Path.GetFileName(file);
                    string versionTag = $"Alpha {SC_ALPHA_VERSION}";
                    
                    // 检查文件名是否包含版本信息
                    if (fileName.Contains("4.1") || fileName.Contains("41"))
                    {
                        versionTag = "Alpha 4.1";
                    }
                    
                    // 给每张导入的照片添加游戏导入标签和版本标签
                    await _photoLibraryService.AddPhotoAsync(file, new[] { "游戏导入", "Star Citizen", versionTag });
                    importCount++;
                }

                return importCount;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private string DetectGameVersion()
        {
            try
            {
                // 尝试从进程信息或窗口标题获取版本信息
                string windowTitle = GetGameWindowTitle();
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    // 从窗口标题中提取版本信息 (如果有)
                    if (windowTitle.Contains("LIVE") || windowTitle.Contains("PTU"))
                    {
                        // 使用正则表达式尝试提取版本号
                        var match = Regex.Match(windowTitle, @"(\d+\.\d+)");
                        if (match.Success)
                        {
                            return $"Alpha {match.Value}";
                        }
                        
                        if (windowTitle.Contains("LIVE"))
                        {
                            return $"Alpha {SC_ALPHA_VERSION} LIVE版本";
                        }
                        else if (windowTitle.Contains("PTU"))
                        {
                            return $"Alpha {SC_ALPHA_VERSION} PTU测试版";
                        }
                        
                        return windowTitle;
                    }
                }

                // 如果无法从窗口标题获取，尝试从文件信息获取
                if (_gameProcess != null && !string.IsNullOrEmpty(_gameProcess.MainModule?.FileName))
                {
                    string exePath = _gameProcess.MainModule.FileName;
                    string directory = Path.GetDirectoryName(exePath);
                    
                    // 检查version.db文件
                    string[] versionFiles = {
                        Path.Combine(directory, "version.txt"),
                        Path.Combine(directory, "version.db"),
                        Path.Combine(directory, "..\\build_manifest.id"),
                        Path.Combine(directory, "..\\Game.log")
                    };
                    
                    foreach (string versionFile in versionFiles)
                    {
                        if (File.Exists(versionFile))
                        {
                            string versionContent = File.ReadAllText(versionFile);
                            // 解析版本文件内容
                            var match = Regex.Match(versionContent, @"(\d+\.\d+)");
                            if (match.Success)
                            {
                                return $"Alpha {match.Value}";
                            }
                            
                            // 检查是否包含构建版本号
                            if (versionContent.Contains(SC_BUILD_VERSION))
                            {
                                return $"Alpha {SC_ALPHA_VERSION} (构建版本 {SC_BUILD_VERSION})";
                            }
                        }
                    }
                    
                    // 如果是LIVE或PTU目录
                    if (directory.Contains("LIVE"))
                    {
                        return $"Alpha {SC_ALPHA_VERSION} LIVE版本";
                    }
                    else if (directory.Contains("PTU"))
                    {
                        return $"Alpha {SC_ALPHA_VERSION} PTU测试版";
                    }
                }

                return $"Alpha {SC_ALPHA_VERSION}";
            }
            catch
            {
                return $"Alpha {SC_ALPHA_VERSION}";
            }
        }
        
        public Rectangle GetGameWindowRect()
        {
            IntPtr hwnd = GetGameWindowHandle();
            if (hwnd != IntPtr.Zero)
            {
                GetWindowRect(hwnd, out RECT rect);
                return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
            return new Rectangle(0, 0, 0, 0);
        }
        
        public Rectangle GetGameClientRect()
        {
            IntPtr hwnd = GetGameWindowHandle();
            Rectangle windowRect = GetGameWindowRect();
            
            if (hwnd != IntPtr.Zero && !windowRect.IsEmpty)
            {
                if (IsWindowedMode && !IsWindowedFullscreen)
                {
                    // 返回不包括标题栏的客户区域
                    return new Rectangle(
                        windowRect.X, 
                        windowRect.Y + TitleBarHeight,
                        windowRect.Width, 
                        windowRect.Height - TitleBarHeight);
                }
                else
                {
                    // 全屏或无边框全屏模式
                    return windowRect;
                }
            }
            
            return windowRect;
        }

        public (int X, int Y, int Width, int Height) GetGameWindowCoords()
        {
            Rectangle rect = GetGameWindowRect();
            return (rect.X, rect.Y, rect.Width, rect.Height);
        }
        
        public (int X, int Y, int Width, int Height) GetGameClientCoords()
        {
            Rectangle rect = GetGameClientRect();
            return (rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }
    
    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        public bool IsEmpty => Width == 0 || Height == 0;
    }
} 