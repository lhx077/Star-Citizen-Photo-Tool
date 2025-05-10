namespace SCPhotoTool.Services
{
    /// <summary>
    /// 电影宽高比枚举
    /// </summary>
    public enum FilmAspectRatio
    {
        /// <summary>
        /// 标准宽高比 (4:3)
        /// </summary>
        Standard,
        
        /// <summary>
        /// 宽屏宽高比 (16:9)
        /// </summary>
        Widescreen,
        
        /// <summary>
        /// 电影范围宽高比 (2.35:1)
        /// </summary>
        CinemaScope,
        
        /// <summary>
        /// IMAX宽高比 (1.43:1)
        /// </summary>
        IMAX,
        
        /// <summary>
        /// 变形宽高比 (2.39:1)
        /// </summary>
        Anamorphic,
        
        /// <summary>
        /// 学院宽高比 (1.85:1)
        /// </summary>
        Academy
    }
} 