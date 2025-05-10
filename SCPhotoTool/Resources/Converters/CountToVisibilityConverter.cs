using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SCPhotoTool.Resources.Converters
{
    /// <summary>
    /// 根据集合数量决定元素可见性
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
                
            // 如果是数字类型
            if (value is int count)
            {
                // 如果集合不为空，则显示
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 这里不需要实现转换回来的逻辑
            throw new NotImplementedException();
        }
    }
} 