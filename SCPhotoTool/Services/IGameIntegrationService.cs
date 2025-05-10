using System;
using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public interface IGameIntegrationService
    {
        /// <summary>
        /// 指示是否已连接到游戏
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// 获取游戏版本信息
        /// </summary>
        string GameVersion { get; }
        
        /// <summary>
        /// 指示游戏是否以窗口模式运行
        /// </summary>
        bool IsWindowedMode { get; }
        
        /// <summary>
        /// 获取游戏窗口标题栏高度（如果是窗口模式）
        /// </summary>
        int TitleBarHeight { get; }

        /// <summary>
        /// 当游戏连接状态改变时触发
        /// </summary>
        event EventHandler<bool> GameConnectionStatusChanged;

        /// <summary>
        /// 尝试连接到游戏
        /// </summary>
        /// <returns>是否成功连接</returns>
        bool Connect();

        /// <summary>
        /// 断开与游戏的连接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 获取游戏窗口句柄
        /// </summary>
        /// <returns>窗口句柄</returns>
        IntPtr GetGameWindowHandle();

        /// <summary>
        /// 获取游戏窗口标题
        /// </summary>
        /// <returns>窗口标题</returns>
        string GetGameWindowTitle();

        /// <summary>
        /// 获取游戏截图目录
        /// </summary>
        /// <returns>截图目录路径</returns>
        string GetGameScreenshotDirectory();

        /// <summary>
        /// 导入游戏截图到照片库
        /// </summary>
        /// <returns>导入的截图数量</returns>
        Task<int> ImportGameScreenshotsAsync();
        
        /// <summary>
        /// 获取游戏窗口的矩形区域（包括标题栏）
        /// </summary>
        /// <returns>窗口矩形</returns>
        Rectangle GetGameWindowRect();
        
        /// <summary>
        /// 获取游戏客户区域的矩形（不包括标题栏）
        /// </summary>
        /// <returns>客户区矩形</returns>
        Rectangle GetGameClientRect();

        // 添加以下新方法，返回矩形的坐标和尺寸
        (int X, int Y, int Width, int Height) GetGameWindowCoords();
        (int X, int Y, int Width, int Height) GetGameClientCoords();
    }
    
  
} 