using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public interface IPhotoInfoService
    {
        /// <summary>
        /// 给照片添加拍摄信息
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="info">拍摄信息</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> AddPhotoInfoAsync(string imagePath, PhotoInfo info);
        
        /// <summary>
        /// 从照片中获取拍摄信息
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <returns>拍摄信息</returns>
        Task<PhotoInfo> GetPhotoInfoAsync(string imagePath);
        
        /// <summary>
        /// 获取照片的基本信息
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <returns>基本信息</returns>
        Task<PhotoInfo> GetBasicInfoAsync(string imagePath);
        
        /// <summary>
        /// 调整照片亮度
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="brightness">亮度值(-100到100)</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> AdjustBrightnessAsync(string imagePath, int brightness);
        
        /// <summary>
        /// 调整照片对比度
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="contrast">对比度值(-100到100)</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> AdjustContrastAsync(string imagePath, int contrast);
        
        /// <summary>
        /// 调整照片饱和度
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="saturation">饱和度值(-100到100)</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> AdjustSaturationAsync(string imagePath, int saturation);
        
        /// <summary>
        /// 应用滤镜
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="filterType">滤镜类型</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> ApplyFilterAsync(string imagePath, FilterType filterType);
        
        /// <summary>
        /// 裁剪照片
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="cropArea">裁剪区域</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> CropImageAsync(string imagePath, CropArea cropArea);
        
        /// <summary>
        /// 按指定宽高比裁剪照片
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="aspectRatio">宽高比类型</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> CropToAspectRatioAsync(string imagePath, FilmAspectRatio aspectRatio);
        
        /// <summary>
        /// 旋转照片
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="angle">旋转角度(90、180、270)</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> RotateImageAsync(string imagePath, int angle);
        
        /// <summary>
        /// 翻转照片
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="horizontal">是否水平翻转</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> FlipImageAsync(string imagePath, bool horizontal);
        
        /// <summary>
        /// 调整照片大小
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="width">新宽度</param>
        /// <param name="height">新高度</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> ResizeImageAsync(string imagePath, int width, int height);
        
        /// <summary>
        /// 显示构图辅助线
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="compositionType">构图类型</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> ShowCompositionGuideAsync(string imagePath, CompositionType compositionType);
        
        /// <summary>
        /// 添加构图辅助线
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="compositionType">构图类型</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> AddCompositionGuideAsync(string imagePath, CompositionType compositionType);
        
        /// <summary>
        /// 添加电影拍摄辅助线
        /// </summary>
        /// <param name="imagePath">照片路径</param>
        /// <param name="aspectRatio">宽高比类型</param>
        /// <returns>保存后的照片路径</returns>
        Task<string> AddFilmGuideAsync(string imagePath, FilmAspectRatio aspectRatio);
    }
    
    
} 