using Flowy.Core.Event;
using System.Collections.ObjectModel;   // ObservableCollection을 쓰기 위함
using Flowy.Core.StateMachine;          // WorkProcess를 쓰기 위함
using Flowy.Core.Simulation;            // ProductionLine을 쓰기 위함

namespace Flowy.Wpf.ViewModels
{
    public class MainViewModel  // public이어야 XAML에서 접근 가능 (internal이면 바인딩 안 됨)
    {
        // ObservableCollection = "변경 시 화면에 자동 알림"이 내장된 목록 (= 이벤트 버스의 WPF 기본형)
        // 일반 List<string>을 쓰면 항목 추가/삭제가 화면에 자동 반영이 안 됨
        public ObservableCollection<ProcessDisplayItem> Processes { get; }  // { get; } = 외부에서 읽기만 가능

        public MainViewModel()
        {
            // 임시: 직접 4개 공정을 만듦.
            // 나중에 Bootstrapper 역할을 하는 클래스로 옮길 예정
            var eventBus = new ProcessEventBus();
            var workProcesses = new List<WorkProcess>
            {
                new WorkProcess("W1", eventBus),
                new WorkProcess("W2", eventBus),
                new WorkProcess("W3", eventBus),
                new WorkProcess("W4", eventBus)
            };

            var line = new ProductionLine(workProcesses);

            Processes = new ObservableCollection<ProcessDisplayItem>();
            foreach (var process in line.Processes)
            {
                Processes.Add(new ProcessDisplayItem(process));
            }
        }
    }
}
