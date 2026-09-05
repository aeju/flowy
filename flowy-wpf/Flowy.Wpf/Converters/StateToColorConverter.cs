using System.Globalization;    // CultureInfo(변환 메서드 파라미터)를 쓰기 위함
using System.Windows.Data;     // IValueConverter를 쓰기 위함
using System.Windows.Media;    // Brushes(SeaGreen 등 색)를 쓰기 위함

namespace Flowy.Wpf.Converters
{
    // 공정 상태(enum) -> 표시 색으로 변환
    internal class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = value?.ToString();
            return state switch // Brushes: WPF의 칠하는 도구 모음
            {
                "가동" => Brushes.SeaGreen,    // Running
                "대기" => Brushes.Goldenrod,   // Idle
                "이상" => Brushes.Firebrick,   // Error
                "정지" => Brushes.Gray,        // Stopped
                _ => Brushes.Black
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException(); // 역변환 불필요
    }
}
