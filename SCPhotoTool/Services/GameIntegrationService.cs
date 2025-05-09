using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public class GameIntegrationService : IGameIntegrationService
    {
        private readonly ISettingsService _settingsService;
        private readonly IPhotoLibraryService _photoLibraryService;
        private Process _gameProcess;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        public bool IsConnected => _gameProcess != null && !_gameProcess.HasExited;
        public string GameVersion { get; private set; } = "未知";

        public event EventHandler<bool> GameConnectionStatusChanged;

        public GameIntegrationService(ISettingsService settingsService, IPhotoLibraryService photoLibraryService)
        {
            _settingsService = settingsService;
            _photoLibraryService = photoLibraryService;
        }

        public bool Connect()
        {
            try
            {
                // 尝试寻找运行中的Star Citizen进程
                Process[] processes = Process.GetProcessesByName("StarCitizen");
                if (processes.Length > 0)
                {
                    _gameProcess = processes[0];
                    GameVersion = DetectGameVersion();
                    
                    GameConnectionStatusChanged?.Invoke(this, true);
                    return true;
                }

                // 尝试通过窗口标题查找
                IntPtr windowHandle = FindWindow(null, "Star Citizen");
                if (windowHandle != IntPtr.Zero)
                {
                    uint processId;
                    GetWindowThreadProcessId(windowHandle, out processId);
                    _gameProcess = Process.GetProcessById((int)processId);
                    GameVersion = DetectGameVersion();
                    
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

        public void Disconnect()
        {
            _gameProcess = null;
            GameVersion = "未知";
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

            // 尝试自动检测RSI启动器默认路径
            string rsiPath = Path.Combine(userProfile, "Roberts Space Industries", "StarCitizen", "LIVE", "ScreenShots");
            if (Directory.Exists(rsiPath))
            {
                return rsiPath;
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
                    string[] files = Directory.GetFiles(directory, $"*{ext}");
                    importedFiles.AddRange(files);
                }

                // 导入到照片库
                int importCount = 0;
                foreach (string file in importedFiles)
                {
                    // 给每张导入的照片添加游戏导入标签
                    await _photoLibraryService.AddPhotoAsync(file, new[] { "游戏导入", "Star Citizen" });
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
                        return windowTitle;
                    }
                }

                // 如果无法从窗口标题获取，尝试从文件信息获取
                if (_gameProcess != null && !string.IsNullOrEmpty(_gameProcess.MainModule?.FileName))
                {
                    string exePath = _gameProcess.MainModule.FileName;
                    string directory = Path.GetDirectoryName(exePath);
                    
                    // 如果是LIVE或PTU目录
                    if (directory.Contains("LIVE"))
                    {
                        return "LIVE版本";
                    }
                    else if (directory.Contains("PTU"))
                    {
                        return "PTU测试版";
                    }
                }

                return "已连接";
            }
            catch
            {
                return "已连接";
            }
        }
    }
} 