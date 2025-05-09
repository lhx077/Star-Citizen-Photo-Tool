using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace SCPhotoTool.Models
{
    public class Photo
    {
        /// <summary>
        /// 照片唯一标识
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 照片文件路径
        /// </summary>
        public string FilePath { get; set; }
        
        /// <summary>
        /// 缩略图路径
        /// </summary>
        public string ThumbnailPath { get; set; }
        
        /// <summary>
        /// 照片标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 照片描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 拍摄地点
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }
        
        /// <summary>
        /// 拍摄日期
        /// </summary>
        public DateTime? DateTaken { get; set; }
        
        /// <summary>
        /// 相机设置
        /// </summary>
        public string CameraSettings { get; set; }
        
        /// <summary>
        /// 照片标签
        /// </summary>
        public List<string> Tags { get; set; }
        
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// 最后修改日期
        /// </summary>
        public DateTime ModifiedDate { get; set; }
        
        /// <summary>
        /// 缩略图图像(UI展示用)
        /// </summary>
        public BitmapImage ThumbnailImage { get; set; }
        
        /// <summary>
        /// 照片宽度
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// 照片高度
        /// </summary>
        public int Height { get; set; }
        
        /// <summary>
        /// 文件大小(字节)
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// 照片类型(jpeg, png等)
        /// </summary>
        public string FileType { get; set; }
        
        /// <summary>
        /// 是否为收藏
        /// </summary>
        public bool IsFavorite { get; set; }
        
        /// <summary>
        /// 评分(1-5)
        /// </summary>
        public int Rating { get; set; }
        
        /// <summary>
        /// 游戏位置信息
        /// </summary>
        public GameLocationInfo GameLocation { get; set; }
        
        /// <summary>
        /// 元数据信息
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public Photo()
        {
            Id = Guid.NewGuid().ToString();
            Tags = new List<string>();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }

    public class GameLocationInfo
    {
        /// <summary>
        /// 游戏内星系
        /// </summary>
        public string System { get; set; }
        
        /// <summary>
        /// 游戏内行星
        /// </summary>
        public string Planet { get; set; }
        
        /// <summary>
        /// 游戏内位置
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// 坐标信息
        /// </summary>
        public string Coordinates { get; set; }
    }
} 