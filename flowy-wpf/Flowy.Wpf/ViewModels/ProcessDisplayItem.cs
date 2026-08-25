using Flowy.Core.StateMachine;
using System.ComponentModel;    // INotifyPropertyChanged를 쓰기 위함

namespace Flowy.Wpf.ViewModels
{
    // WorkProcess 화면 표시용으로 가공하는 래퍼 (View에 넘기는 역할)
    // INotifyPropertyChanged: 프로퍼티 값이 바뀌면 PropertyChanged 이벤트를 발행하는 표준 인터페이스
    // WPF 바인딩이 이 이벤트를 구독하고 있다가, 발행되면 화면을 자동 갱신
    public class ProcessDisplayItem : INotifyPropertyChanged 
    {
        private readonly WorkProcess _process;

        public ProcessDisplayItem(WorkProcess process)
        {
            _process = process;
        }

        // get 전용 프로퍼티 (상태를 저장하지 않고 매번 원본에서 계산해, tick으로 바뀐 최신 상태를 반영)
        public string ProcessName => _process.ProcessName;

        public string StatusText => _process.StateMachine.CurrentStateType switch
        {
            ProcessStateType.Running => "가동",
            ProcessStateType.Idle => "대기",
            ProcessStateType.Error => "이상",
            ProcessStateType.Stopped => "정지",
            _ => "알수없음"
        };

        public string CycleTimeText => $"({_process.CycleTimeSeconds})";

        // 앞 대기열 수
        // 0이면 빈 문자열 (안 보임), 쌓이면 "대기 N"으로 병목 표시
        public string QueueText => _process.QueueLength > 0 ? $"대기 {_process.QueueLength}" : "";

        public event PropertyChangedEventHandler? PropertyChanged;

        // 외부(ViewModel)에서 "화면 갱신해"라고 부를 때 쓰는 메서드
        // StatusText 변경을 통지해 바인딩된 View를 갱신
        public void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(QueueText)));
        }
    }
}
