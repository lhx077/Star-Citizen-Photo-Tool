namespace SCPhotoTool.Services
{
    /// <summary>
    /// 构图类型枚举
    /// </summary>
    public enum CompositionType
    {
        /// <summary>
        /// 无构图辅助
        /// </summary>
        None,
        
        /// <summary>
        /// 三分法构图
        /// </summary>
        RuleOfThirds,
        
        /// <summary>
        /// 黄金比例构图
        /// </summary>
        GoldenRatio,
        
        /// <summary>
        /// 对角线构图
        /// </summary>
        Diagonal,
        
        /// <summary>
        /// 网格构图
        /// </summary>
        Grid,
        
        /// <summary>
        /// 中心构图
        /// </summary>
        Center,
        
        /// <summary>
        /// 对称构图
        /// </summary>
        Symmetry,
        
        /// <summary>
        /// 引导线构图
        /// </summary>
        LeadingLines,
        
        /// <summary>
        /// 框架构图
        /// </summary>
        Framing
    }
} 