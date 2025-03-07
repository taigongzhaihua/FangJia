﻿using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FangJia.UI.Converters
{
    public class ByteToImageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not byte[] { Length: > 0 } imageData) return null!; // Return null for invalid input
            try
            {
                using var stream = new MemoryStream(imageData);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Ensure the image loads completely
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Make the BitmapImage thread-safe
                return bitmapImage;
            }
            catch (Exception)
            {
                // Log error if needed
                return null!; // Return null if the conversion fails
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not BitmapImage { StreamSource: not null } bitmapImage) return null!;
            using var stream = bitmapImage.StreamSource;
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            var imageData = new byte[stream.Length];
            stream.ReadExactly(imageData, 0, imageData.Length);
            return imageData;
        }
    }
}
