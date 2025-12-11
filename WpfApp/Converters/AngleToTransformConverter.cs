using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace WpfApp
{
    public class AngleToTransformConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double angle && parameter is string axis)
            {
                var angleRad = angle * Math.PI / 180.0;
                var rotation = axis switch
                {
                    "X" => new AxisAngleRotation3D(new Vector3D(1, 0, 0), angle),
                    "Y" => new AxisAngleRotation3D(new Vector3D(0, 1, 0), angle),
                    "Z" => new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle),
                    _ => new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle)
                };
                return new RotateTransform3D(rotation);
            }
            return Transform3D.Identity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

