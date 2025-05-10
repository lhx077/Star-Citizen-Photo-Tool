using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace SCPhotoTool.Resources.Converters
{
    /// <summary>
    /// 获取枚举值的Description特性的值
    /// </summary>
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
                
            // 获取枚举值的描述
            Enum enumValue = value as Enum;
            if (enumValue == null)
                return value.ToString();
                
            // 获取Description特性
            string description = GetEnumDescription(enumValue);
            
            // 如果没有Description特性，则返回枚举值的名称
            return !string.IsNullOrEmpty(description) ? description : enumValue.ToString();
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 这里不需要实现转换回来的逻辑
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// 获取枚举值的Description特性
        /// </summary>
        private string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi == null) 
                return value.ToString();
                
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
} 