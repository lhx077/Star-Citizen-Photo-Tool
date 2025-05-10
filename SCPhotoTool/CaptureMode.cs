using System;
using System.ComponentModel;

namespace SCPhotoTool
{
    /// <summary>
    /// 截图模式枚举
    /// </summary>
    public enum CaptureMode
    {
        /// <summary>
        /// 游戏窗口
        /// </summary>
        [Description("游戏窗口")]
        GameWindow,
        
        /// <summary>
        /// 选择区域
        /// </summary>
        [Description("选择区域")]
        SelectedArea,
        
        /// <summary>
        /// 全屏
        /// </summary>
        [Description("全屏")]
        FullScreen
    }
} 