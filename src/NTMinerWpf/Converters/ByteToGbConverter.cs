﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace NTMiner.Converters {
    [ValueConversion(typeof(long), typeof(string))]
    public class ByteToGbConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double l = System.Convert.ToDouble(value);
            const long g = 1024 * 1024 * 1024;
            return (l / g).ToString("f1") + " GB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}