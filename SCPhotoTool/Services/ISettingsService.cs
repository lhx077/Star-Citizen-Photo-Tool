using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public interface ISettingsService
    {
        /// <summary>
        /// 获取设置项值
        /// </summary>
        /// <typeparam name="T">设置值类型</typeparam>
        /// <param name="key">设置项键名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>设置项值</returns>
        T GetSetting<T>(string key, T defaultValue = default);
        
        /// <summary>
        /// 保存设置项
        /// </summary>
        /// <typeparam name="T">设置值类型</typeparam>
        /// <param name="key">设置项键名</param>
        /// <param name="value">设置项值</param>
        /// <returns>是否保存成功</returns>
        Task<bool> SaveSettingAsync<T>(string key, T value);
        
        /// <summary>
        /// 获取默认截图保存路径
        /// </summary>
        /// <returns>路径字符串</returns>
        string GetScreenshotDirectory();
        
        /// <summary>
        /// 设置默认截图保存路径
        /// </summary>
        /// <param name="path">路径字符串</param>
        /// <returns>是否设置成功</returns>
        Task<bool> SetScreenshotDirectoryAsync(string path);
        
        /// <summary>
        /// 获取截图文件名格式
        /// </summary>
        /// <returns>文件名格式</returns>
        string GetScreenshotFileNameFormat();
        
        /// <summary>
        /// 设置截图文件名格式
        /// </summary>
        /// <param name="format">文件名格式</param>
        /// <returns>是否设置成功</returns>
        Task<bool> SetScreenshotFileNameFormatAsync(string format);
        
        /// <summary>
        /// 获取截图质量设置
        /// </summary>
        /// <returns>质量值(0-100)</returns>
        int GetScreenshotQuality();
        
        /// <summary>
        /// 设置截图质量
        /// </summary>
        /// <param name="quality">质量值(0-100)</param>
        /// <returns>是否设置成功</returns>
        Task<bool> SetScreenshotQualityAsync(int quality);
    }
} 