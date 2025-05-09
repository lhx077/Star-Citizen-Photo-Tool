using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SCPhotoTool.Resources
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLower())
                {
                    case "已连接":
                    case "connected":
                        return new SolidColorBrush(Colors.LimeGreen);
                    case "断开连接":
                    case "disconnected":
                        return new SolidColorBrush(Colors.Red);
                    case "正在连接":
                    case "connecting":
                        return new SolidColorBrush(Colors.Orange);
                    default:
                        return new SolidColorBrush(Colors.Gray);
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 