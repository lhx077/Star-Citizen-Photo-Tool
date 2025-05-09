using System;
using System.Drawing;
using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public interface IScreenshotService
    {
        /// <summary>
        /// 捕获全屏截图
        /// </summary>
        /// <returns>截图的位图</returns>
        Task<Bitmap> CaptureScreenAsync();
        
        /// <summary>
        /// 捕获游戏窗口截图
        /// </summary>
        /// <returns>截图的位图</returns>
        Task<Bitmap> CaptureGameWindowAsync();
        
        /// <summary>
        /// 保存截图
        /// </summary>
        /// <param name="bitmap">要保存的位图</param>
        /// <param name="filePath">保存路径，如为null则使用默认路径</param>
        /// <returns>保存后的文件路径</returns>
        Task<string> SaveScreenshotAsync(Bitmap bitmap, string filePath = null);
        
        /// <summary>
        /// 截图热键功能
        /// </summary>
        event EventHandler HotkeyTriggered;
        
        /// <summary>
        /// 设置截图热键
        /// </summary>
        /// <param name="hotkey">热键组合</param>
        void SetHotkey(string hotkey);
    }
} 