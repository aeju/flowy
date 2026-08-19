using Flowy.Wpf.ViewModels;    // MainViewModel을 쓰기 위함
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flowy.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();    // XAML을 로드해 화면 구성 (WPF 기본)
            DataContext = new MainViewModel(); // 이 창이 바라볼 ViewModel 지정
                                               // DataContext = "이 화면의 데이터 공급원은 이거다"라는 선언 (이게 있어야 XAML의 {Binding}이 ViewModel을 찾아감)
        }
    }
}