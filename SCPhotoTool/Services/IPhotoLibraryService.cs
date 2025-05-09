using SCPhotoTool.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace SCPhotoTool.Services
{
    public interface IPhotoLibraryService
    {
        /// <summary>
        /// 获取照片库中的所有照片
        /// </summary>
        /// <returns>照片列表</returns>
        Task<IEnumerable<Photo>> GetAllPhotosAsync();
        
        /// <summary>
        /// 根据标签过滤照片
        /// </summary>
        /// <param name="tags">要筛选的标签</param>
        /// <returns>符合标签条件的照片列表</returns>
        Task<IEnumerable<Photo>> GetPhotosByTagsAsync(IEnumerable<string> tags);
        
        /// <summary>
        /// 添加照片到库中
        /// </summary>
        /// <param name="photoPath">照片文件路径</param>
        /// <param name="tags">照片标签</param>
        /// <returns>新增的照片对象</returns>
        Task<Photo> AddPhotoAsync(string photoPath, IEnumerable<string> tags = null);
        
        /// <summary>
        /// 更新照片信息
        /// </summary>
        /// <param name="photoId">照片ID</param>
        /// <param name="tags">新的标签</param>
        /// <param name="description">新的描述</param>
        /// <returns>更新后的照片对象</returns>
        Task<Photo> UpdatePhotoAsync(string photoId, IEnumerable<string> tags = null, string description = null);
        
        /// <summary>
        /// 删除照片
        /// </summary>
        /// <param name="photoId">要删除的照片ID</param>
        /// <returns>删除是否成功</returns>
        Task<bool> DeletePhotoAsync(string photoId);
        
        /// <summary>
        /// 获取所有标签
        /// </summary>
        /// <returns>标签列表</returns>
        Task<IEnumerable<string>> GetAllTagsAsync();
        
        /// <summary>
        /// 获取照片缩略图
        /// </summary>
        /// <param name="photoId">照片ID</param>
        /// <returns>缩略图位图</returns>
        Task<Bitmap> GetThumbnailAsync(string photoId);

        Task<IEnumerable<string>> GetPhotosAsync();
        Task<string> GetPhotoPathAsync(string photoId);
    }
} 