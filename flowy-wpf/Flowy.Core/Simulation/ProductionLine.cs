using System.Collections.Generic;
using Flowy.Core.StateMachine;
using System.Linq;

namespace Flowy.Core.Simulation
{
    /// <summary>
    /// Line 전체 tick 관리
    /// </summary>
    public class ProductionLine
    {
        // 이 라인이 관리하는 공정들 목록
        private List<WorkProcess> processes;

        // 외부(ViewModel)에서 공정 목록을 읽을 수 있게 노출- 읽기 전용
        public IReadOnlyList<WorkProcess> Processes => processes;

        private int _completedCount; // W4 통과 완성품 누적
        private int _elapsedSeconds;   // 시뮬레이션 경과(1 tick = 1초)
        public int CompletedCount => _completedCount;
        public int ElapsedSeconds => _elapsedSeconds;

        // 각 공정의 큐 길이를 매 tick 누적(합계). 평균 = 합계 / 경과 tick
        // 공정 이름 -> 큐 길이 누적 합계
        private readonly Dictionary<string, long> _queueSum = new Dictionary<string, long>();

        // Bootstrapper가 생성한 processes 리스트를 주입받음 
        public ProductionLine(List<WorkProcess> processes)
        {
            this.processes = processes;

            // W1 -> W2 -> W3 -> W4 순으로 연결 (마지막은 Next 없음 = 라인 끝)
            for (int i = 0; i < processes.Count - 1; i++)
            {
                processes[i].Next = processes[i + 1];
            }

            // 큐 누적 딕셔너리를 공정별 0으로 초기화
            foreach (var process in processes)
            {
                _queueSum[process.ProcessName] = 0;
            }
        }

        // 특정 공정의 큐 누적 합계 (평균 계산용)
        public long GetQueueSum(string processName) => _queueSum[processName];

        // 매 tick 호출되는 메서드 (Bootstrapper에서 호출)
        // 리스트 안의 모든 WorkProcess를 순회하며 각 공정의 StateMachine.Tick을 호출
        public void Tick()
        {
            _elapsedSeconds++; // 매 tick 경과 시간 누적

            // 각 공정의 현재 큐 길이를 누적 (나중에 평균 계산용)
            foreach (var process in processes)
            {
                _queueSum[process.ProcessName] += process.QueueLength;
            }

            // 1) 처리 진행 + 완료 시 다음 공정으로 전달
            // 뒤 공정 부터 처리해야 이번 tick에 W1 -> W2 -> ...가 한 칸씩만 호출
            // (앞부터 하면 한 tick에 여러 공정을 통과해버려 흐름이 왜곡됨)
            for (int i = processes.Count - 1; i >= 0; i--)
            {
                var process = processes[i];
                bool completed = process.AdvancedProcessing(); // 처리 중이면 tick 누적
                
                if (completed)
                {
                    var product = process.CurrentProduct;
                    process.ClearProduct();             // 이 공정 비움

                    if (process.Next != null)
                        process.Next.Enqueue(product);  // 다음 공정 앞 대기열로 
                    else
                        _completedCount++; // W4 통과 = 완성 1개
                }
            }

            // 2) 비어있는 공정은 자기 대기열에서 다음 제품을 꺼내 처리 시작
            foreach (var process in processes)
            {
                process.TryStartNext();
            }

            // 3) 기존 상태 전이 (Running/Idle/Error 판정)
            foreach (var process in processes)
            {
                process.StateMachine.Tick(process);
            }

            // 임시 확인용 - 나중에 지움
            System.Diagnostics.Debug.WriteLine(string.Join(" | ", processes.Select(p => $"{p.ProcessName}:큐{p.QueueLength}")));
        }
    }
}

