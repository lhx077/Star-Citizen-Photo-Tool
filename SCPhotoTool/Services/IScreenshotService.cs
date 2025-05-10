using System;
using System.Drawing;
using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public interface IScreenshotService
    {
        /// <summary>
        /// 当截图热键被触发时发生
        /// </summary>
        event EventHandler HotkeyTriggered;

        /// <summary>
        /// 捕获整个屏幕
        /// </summary>
        /// <returns>屏幕的位图</returns>
        Task<Bitmap> CaptureScreenAsync();

        /// <summary>
        /// 捕获游戏窗口（如果是窗口模式，会去除标题栏）
        /// </summary>
        /// <returns>游戏窗口的位图</returns>
        Task<Bitmap> CaptureGameWindowAsync();
        
        /// <summary>
        /// 捕获用户选择的屏幕区域
        /// </summary>
        /// <returns>选中区域的位图</returns>
        Task<Bitmap> CaptureSelectedAreaAsync();

        /// <summary>
        /// 保存截图到文件
        /// </summary>
        /// <param name="bitmap">要保存的位图</param>
        /// <param name="filePath">文件路径，如果为空，将使用默认路径和文件名</param>
        /// <returns>保存的文件路径</returns>
        Task<string> SaveScreenshotAsync(Bitmap bitmap, string filePath = null);

        /// <summary>
        /// 设置截图热键
        /// </summary>
        /// <param name="hotkey">热键</param>
        void SetHotkey(string hotkey);
        
        /// <summary>
        /// 添加水印到图像
        /// </summary>
        /// <param name="source">源图像</param>
        /// <param name="watermarkText">水印文本</param>
        /// <returns>添加了水印的图像</returns>
        Bitmap AddWatermark(Bitmap source, string watermarkText);
    }
} 