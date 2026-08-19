using System.Collections.ObjectModel;   // ObservableCollection을 쓰기 위함

namespace Flowy.Wpf.ViewModels
{
    public class MainViewModel  // public이어야 XAML에서 접근 가능 (internal이면 바인딩 안 됨)
    {
        // ObservableCollection = "변경 시 화면에 자동 알림"이 내장된 목록 (= 이벤트 버스의 WPF 기본형)
        // 일반 List<string>을 쓰면 항목 추가/삭제가 화면에 자동 반영이 안 됨
        public ObservableCollection<string> Processes { get; }  // { get; } = 외부에서 읽기만 가능

        public MainViewModel()
        {
            Processes = new ObservableCollection<string>
            {
                "W1 - 가동",  // 지금은 하드코딩.
                "W2 - 대기",
                "W3 - 가동",
                "W4 - 이상"
            };
        }
    }
}
