using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SCPhotoTool.Resources.Converters
{
    /// <summary>
    /// 将枚举值转换为ComboBox的ItemsSource
    /// </summary>
    public class EnumToItemsSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 检查是否提供了枚举类型作为参数
            Type enumType = parameter as Type;
            
            // 如果提供了枚举类型参数，使用参数中的枚举类型
            if (enumType != null && enumType.IsEnum)
            {
                return Enum.GetValues(enumType).Cast<object>().ToList();
            }
            
            // 如果输入是枚举值
            if (value is Enum)
            {
                // 获取枚举类型
                enumType = value.GetType();
                
                // 返回枚举中的所有值
                return Enum.GetValues(enumType).Cast<object>().ToList();
            }
            
            // 如果输入是Type类型，且是枚举类型
            if (value is Type typeValue && typeValue.IsEnum)
            {
                return Enum.GetValues(typeValue).Cast<object>().ToList();
            }
            
            return null;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 这里不需要实现转换回来的逻辑
            throw new NotImplementedException();
        }
    }
} 