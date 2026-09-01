using Flowy.Core.Event;
using Flowy.Core.Metric;
using System.Collections.ObjectModel;   // ObservableCollection을 쓰기 위함
using Flowy.Core.StateMachine;          // WorkProcess를 쓰기 위함
using Flowy.Core.Simulation;            // ProductionLine을 쓰기 위함
using System.Windows.Threading;         // DispatcherTimer를 쓰기 위함
using System.Windows.Input;
using System.ComponentModel;            // ICommand를 쓰기 위함
using System.Runtime.CompilerServices;
using Flowy.Core.Data;                  // EventRepository, EventLogger를 쓰기 위함
using System.Windows;                   // Application을 쓰기 위함

namespace Flowy.Wpf.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ProductionLine _line;
        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _clockTimer;        // 현재 시각 표시용 타이머
        private readonly MetricsCalculator _metrics;

        private readonly EventLogger _eventLogger;

        // 각 버튼이 바인딩할 Command
        public ICommand AssignProductCommand { get; } // 제품 투입
        public ICommand StopCommand { get; }          // 설비 정지
        public ICommand RestartCommand { get; }       // 재가동 
        public ICommand SpeedUpCommand { get; }       // 가속
        public ICommand SpeedDownCommand { get; }     // 감속
        public ICommand AnalyzeBottleneckCommand { get; } // 병목 분석

        // 컬렉션 변경이 자동으로 View에 통지되도록 ObservableCollection 사용 
        // 일반 List<string>을 쓰면 항목 추가/삭제가 화면에 자동 반영이 안 됨
        public ObservableCollection<ProcessDisplayItem> Processes { get; }  // { get; } = 외부에서 읽기만 가능

        // 이력 그리드가 바인딩할 컬렉션 (최신이 위로 오도록 Insert(0) 사용)
        public ObservableCollection<MachineEvent> EventHistory { get; } = new ObservableCollection<MachineEvent>();

        // OEE 3요소 중 가동률·UPH는 계산해 표시
        // 양품률은 불량 판정 데이터가 없어 미표시
        private string _availabilityText = "가동률 --";
        public string AvailabilityText
        {
            get => _availabilityText;
            private set { _availabilityText = value; OnPropertyChanged(); }
        }

        private string _uphText = "UPH --";
        public string UphText
        {
            get => _uphText;
            private set { _uphText = value; OnPropertyChanged(); }
        }

        public string QualityText => "양품률 --";

        private string _alertText = "이상 없음";
        public string AlertText
        {
            get => _alertText;
            private set { _alertText = value; OnPropertyChanged(); }
        }

        private string _currentTime = "";
        public string CurrentTime
        {
            get => _currentTime;
            private set { _currentTime = value; OnPropertyChanged(); }
        }

        private string _elapsedText = "가동 00:00";
        public string ElapsedText
        {
            get => _elapsedText;
            private set { _elapsedText = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            // 임시: 직접 4개 공정을 만듦.
            // 나중에 Bootstrapper 역할을 하는 클래스로 옮길 예정
            var eventBus = new ProcessEventBus();

            // 이벤트 버스를 구독해 상태 변화를 DB에 기록 (EventLogger는 저장만 담당)
            var eventRepository = new EventRepository();
            _eventLogger = new EventLogger(eventBus, eventRepository);

            // 시작 시 기존 이력을 DB에서 불러와 그리드 초기화 (최신이 위로)
            EventHistory = new ObservableCollection<MachineEvent>(eventRepository.GetAll().Reverse());

            // VM도 같은 버스를 구독 -> 화면 갱신만 담당 (저장은 EventLogger가 이미 함)
            eventBus.OnProcessStateChanged += OnStateChangedForHistory;

            // 공정별 사이클타임(초). W2가 8초로 가장 느려 병목 지점이 됨
            var workProcesses = new List<WorkProcess>
            {
                new WorkProcess("W1", eventBus, 3),
                new WorkProcess("W2", eventBus, 8),
                new WorkProcess("W3", eventBus, 4),
                new WorkProcess("W4", eventBus, 5)
            };

            _line = new ProductionLine(workProcesses);
            _metrics = new MetricsCalculator(workProcesses, _line); // 라인 전달

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
            AnalyzeBottleneckCommand = new RelayCommand(AnalyzeBottleneck);

            // 1초마다 Tick 실행하는 타이머 (Unity Bootstrapper의 tickInterval에 대응)
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTick;
            _timer.Start();

            // 현재 시각 표시용 타이머 (시뮬레이션 속도와 무관하게 1초 고정)
            _clockTimer = new DispatcherTimer { 
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _clockTimer.Start();
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
            UphText = $"UPH {_metrics.CalculateUph():F0}";

            var sec = _line.ElapsedSeconds;
            ElapsedText = $"가동 {sec / 60:D2}:{sec % 60:D2} · 완성 {_line.CompletedCount}개"; // (분:초)

            UpdateAlert(); // 이상/정지 공정 알림 갱신
        }

        // Error/Stopped 공정을 찾아 알림 텍스트 갱신
        private void UpdateAlert()
        {
            var errors = _line.Processes
                .Where(p => p.StateMachine.CurrentStateType == ProcessStateType.Error)
                .Select(p => p.ProcessName).ToList();
            var stopped = _line.Processes
                .Where(p => p.StateMachine.CurrentStateType == ProcessStateType.Stopped)
                .Select(p => p.ProcessName).ToList();

            if (errors.Count > 0)
                AlertText = $"⚠ {string.Join(", ", errors)} 이상 발생";
            else if (stopped.Count > 0)
                AlertText = $"⛔ {string.Join(", ", stopped)} 정지 중";
            else
                AlertText = "이상 없음";
        }

        // Idle 상태인 첫 번째 공정 하나에만 제품 투입 
        // 제품은 항상 라인 시작점(W1) 대기열로 투입
        private void AssignProduct()
        {
            var first = _line.Processes[0]; // W1
            first.Enqueue("P-" + new Random().Next(1000, 9999));
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

        // 병목 분석 -> 리포트 테스트 -> 파일 저장
        private void AnalyzeBottleneck()
        {
            var report = _metrics.AnalyzeBottleneck();
            string text = report.ToText();

            // 파일명에 생성 시각을 붙여 매번 새 파일로 저장 (분석 이력 누적)
            var fileName = $"bottleneck_report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var path = System.IO.Path.Combine(System.AppContext.BaseDirectory, fileName);
            System.IO.File.WriteAllText(path, text);

            // 저장 완료를 하단 Alert에 알림
            AlertText = $"병목 분석 완료: {report.BottleneckName}가 병목 · {fileName} 저장됨"; 
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // 호출한 프로퍼티 이름을 자동으로 넘겨 변경을 통지 ([callerMemberName])
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // 상태 변화가 발생할 때마다 이력 그리드 맨 위에 한 줄 추가
        private void OnStateChangedForHistory(WorkProcess process)
        {
            var entry = new MachineEvent
            {
                MachineName = process.ProcessName,
                ToState = process.StateMachine.CurrentStateType.ToString(),
                Timestamp = DateTime.Now
            };

            // 버스 이벤트가 다른 스레드에서 올 수 있으므로 UI 스레드로 넘겨 안전하게 추가
            Application.Current.Dispatcher.Invoke(() =>
            {
                EventHistory.Insert(0, entry);
            });
        }
    }
}
