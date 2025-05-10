using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SCPhotoTool.Resources.Converters
{
    /// <summary>
    /// 将文件路径转换为图像
    /// </summary>
    public class FilePathToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
                
            string path = value.ToString();
            
            // 检查文件是否存在
            if (!File.Exists(path))
                return null;
                
            try
            {
                // 创建BitmapImage
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(path);
                image.EndInit();
                image.Freeze(); // 使图像可以跨线程访问
                
                return image;
            }
            catch
            {
                // 如果加载失败，返回null
                return null;
            }
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 这里不需要实现转换回来的逻辑
            throw new NotImplementedException();
        }
    }
} 