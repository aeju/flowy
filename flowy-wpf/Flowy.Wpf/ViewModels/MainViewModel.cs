using Flowy.Core.Event;
using System.Collections.ObjectModel;   // ObservableCollection을 쓰기 위함
using Flowy.Core.StateMachine;          // WorkProcess를 쓰기 위함
using Flowy.Core.Simulation;            // ProductionLine을 쓰기 위함
using System.Windows.Threading;         // DispatcherTimer를 쓰기 위함

namespace Flowy.Wpf.ViewModels
{
    public class MainViewModel  // public이어야 XAML에서 접근 가능 (internal이면 바인딩 안 됨)
    {
        private readonly ProductionLine _line;
        private readonly DispatcherTimer _timer;

        // 컬렉션 변경이 자동으로 View에 통지되도록 ObservableCollection 사용
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

            // 임시: 제품 투입 버튼 구현 전, 시작하자마자 상태가 움직이도록 모든 공정에 제품 투입기 투입
            foreach (var p in workProcesses)
            {
                p.AssignProduct("P-" + p.ProcessName);
            }

            _line = new ProductionLine(workProcesses);

            Processes = new ObservableCollection<ProcessDisplayItem>();
            foreach (var process in _line.Processes)
            {
                Processes.Add(new ProcessDisplayItem(process));
            }

            // 1초마다 Tick 실행하는 타이머 (Unity Bootstrapper의 tickInterval에 대응)
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTick;
            _timer.Start();
        }

        // 타이머가 1초마다 부르는 메서드
        private void OnTick(object sender, EventArgs e)
        {
            _line.Tick();   // 모든 공정의 상태를 한 번 진행

            foreach (var item in Processes)
            {
                item.Refresh(); // 상태 변경을 View에 통지
            }
        }
    }
}
