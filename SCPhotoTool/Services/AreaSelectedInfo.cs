using System;

namespace SCPhotoTool.Services
{
    /// <summary>
    /// 存储用户选择的屏幕区域信息
    /// </summary>
    public class AreaSelectedInfo
    {
        /// <summary>
        /// 选择区域的X坐标
        /// </summary>
        public int X { get; set; }
        
        /// <summary>
        /// 选择区域的Y坐标
        /// </summary>
        public int Y { get; set; }
        
        /// <summary>
        /// 选择区域的宽度
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// 选择区域的高度
        /// </summary>
        public int Height { get; set; }
        
        /// <summary>
        /// 创建一个新的区域选择信息实例
        /// </summary>
        public AreaSelectedInfo(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        /// <summary>
        /// 创建一个空的区域选择信息实例
        /// </summary>
        public AreaSelectedInfo()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }
    }
} 