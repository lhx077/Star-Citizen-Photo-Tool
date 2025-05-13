using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SCPhotoTool.Resources.Converters
{
    /// <summary>
    /// 将布尔值转换为表示连接状态的画刷
    /// </summary>
    public class BoolToConnectionBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected 
                    ? new SolidColorBrush(Color.FromRgb(0, 255, 136)) // 连接状态 - 绿色
                    : new SolidColorBrush(Color.FromRgb(255, 81, 81)); // 断开状态 - 红色
            }
            
            return new SolidColorBrush(Color.FromRgb(170, 170, 170)); // 未知状态 - 灰色
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 