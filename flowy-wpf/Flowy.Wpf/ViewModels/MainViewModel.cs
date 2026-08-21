using Flowy.Core.Event;
using Flowy.Core.Metric;
using System.Collections.ObjectModel;   // ObservableCollection을 쓰기 위함
using Flowy.Core.StateMachine;          // WorkProcess를 쓰기 위함
using Flowy.Core.Simulation;            // ProductionLine을 쓰기 위함
using System.Windows.Threading;         // DispatcherTimer를 쓰기 위함
using System.Windows.Input;
using System.ComponentModel;             // ICommand를 쓰기 위함
using System.Runtime.CompilerServices;

namespace Flowy.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ProductionLine _line;
        private readonly DispatcherTimer _timer;
        private readonly MetricsCalculator _metrics;

        // 각 버튼이 바인딩할 Command
        public ICommand AssignProductCommand { get; } // 제품 투입
        public ICommand StopCommand { get; }          // 설비 정지
        public ICommand RestartCommand { get; }       // 재가동 
        public ICommand SpeedUpCommand { get; }       // 가속
        public ICommand SpeedDownCommand { get; }     // 감속

        // 컬렉션 변경이 자동으로 View에 통지되도록 ObservableCollection 사용 
        // 일반 List<string>을 쓰면 항목 추가/삭제가 화면에 자동 반영이 안 됨
        public ObservableCollection<ProcessDisplayItem> Processes { get; }  // { get; } = 외부에서 읽기만 가능

        private string _availabilityText = "가동률 --";
        public string AvailabilityText
        {
            get => _availabilityText;
            private set { _availabilityText = value; OnPropertyChanged(); }
        }

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

            _line = new ProductionLine(workProcesses);
            _metrics = new MetricsCalculator(workProcesses);

            Processes = new ObservableCollection<ProcessDisplayItem>();
            foreach (var process in _line.Processes)
            {
                Processes.Add(new ProcessDisplayItem(process));
            }

            AssignProductCommand = new RelayCommand(AssignProduct);
            StopCommand = new RelayCommand(Stop);
            RestartCommand = new RelayCommand(Restart);
            SpeedUpCommand = new RelayCommand(SpeedUp);
            SpeedDownCommand = new RelayCommand(SpeedDown);

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
                item.Refresh(); // 상태 변경을 View에 통지 (각 항목 개별 갱신)
            }

            AvailabilityText = $"가동률 {_metrics.CalculateAvailability():F1}%"; // 전체 기준
        }

        // Idle 상태인 첫 번째 공정 하나에만 제품 투입 
        private void AssignProduct()
        {
            var target = _line.Processes.FirstOrDefault(
            p => p.StateMachine.CurrentStateType == ProcessStateType.Idle);

            if (target != null)
            {
                target.AssignProduct("P-" + new Random().Next(1000, 9999));
            }
        }

        // Running 공정 전부 강제 정지 
        private void Stop()
        {
            foreach (var p in _line.Processes)
            {
                var state = p.StateMachine.CurrentStateType;
                // 이미 정지된 것만 빼고 전부 정지 (Running·Error·Idle 다 멈춤)
                if (state != ProcessStateType.Stopped)
                    p.StateMachine.ForceState(p, new StoppedState());
            }
        }

        // Stopped 공정 전부 재가동 → Idle로 
        private void Restart()
        {
            foreach (var p in _line.Processes)
            {
                if (p.StateMachine.CurrentStateType == ProcessStateType.Stopped)
                    p.StateMachine.ForceState(p, new IdleState());
            }
        }

        // tick 간격 줄이기 = 가속 (최소 0.1초)
        private void SpeedUp()
        {
            var next = _timer.Interval.TotalSeconds - 0.1;
            if (next < 0.1) next = 0.1;
            _timer.Interval = TimeSpan.FromSeconds(next);
        }

        // tick 간격 늘리기 = 감속
        private void SpeedDown()
        {
            _timer.Interval = TimeSpan.FromSeconds(_timer.Interval.TotalSeconds + 0.1);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // 호출한 프로퍼티 이름을 자동으로 넘겨 변경을 통지 ([callerMemberName])
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
