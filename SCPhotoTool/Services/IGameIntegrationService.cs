using System;
using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public interface IGameIntegrationService
    {
        /// <summary>
        /// 游戏是否已连接
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// 游戏版本
        /// </summary>
        string GameVersion { get; }
        
        /// <summary>
        /// 连接到游戏
        /// </summary>
        /// <returns>是否连接成功</returns>
        bool Connect();
        
        /// <summary>
        /// 断开游戏连接
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
        /// 获取游戏截图文件夹路径
        /// </summary>
        /// <returns>路径字符串</returns>
        string GetGameScreenshotDirectory();
        
        /// <summary>
        /// 导入游戏自带截图
        /// </summary>
        /// <returns>导入的文件数量</returns>
        Task<int> ImportGameScreenshotsAsync();
        
        /// <summary>
        /// 游戏状态变更事件
        /// </summary>
        event EventHandler<bool> GameConnectionStatusChanged;
    }
} 