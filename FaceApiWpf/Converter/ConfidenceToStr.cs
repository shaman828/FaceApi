/**************************************************************************************************
 * Author:      bachelor828@live.com
 * FileName:    ConfidenceToStr
 * FrameWork:   4.5.2
 * CreateDate:  2016/9/7 16:19:57
 * Description:  
 * 
 * ************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FaceApiWpf.Converter
{
    public class ConfidenceToStr : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var data = (double)value;
            if (data < 0)
            {
                return "完全不匹配";
            }
            else if (data > 0 && data <= 0.5)
            {
                return "验证不通过";
            }
            else if (data > 0.5)
            {
                return "通过";
            }
            return "";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
