using Flowy.Core.StateMachine;
using System.Collections.Generic;
using System.Linq;
using Flowy.Core.Simulation;    // ProductionLine을 쓰기 위함

namespace Flowy.Core.Metric
{
    /// <summary>
    /// 공정 리스트와 라인 정보를 바탕으로 가동률·UPH를 계산
    /// (양품률·OEE는 불량 판정 로직이 없어 화면에서 "--"로 유지
    /// (OEE = 가동률 × 성능 × 양품률)
    /// </summary>
    public class MetricsCalculator 
    {
        private List<WorkProcess> processes;
        private readonly ProductionLine _line;

        public MetricsCalculator(List<WorkProcess> processes, ProductionLine line)
        {
            this.processes = processes;
            _line = line;
        }

        // 가동률(%) 계산: Running 상태인 공정 수 / 전체 공정 수 × 100
        public float CalculateAvailability()
        {
            // processes 중 CurrentStateType이 Running인 것의 개수 세기
            int runningCount = processes.Count(p => p.StateMachine.CurrentStateType == ProcessStateType.Running);

            float availability = runningCount / (float)processes.Count * 100f;
            return availability;
        }

        public float CalculateUph()
        {
            if (_line.ElapsedSeconds == 0) return 0f;

            return _line.CompletedCount / (_line.ElapsedSeconds / 3600f); 
        }

        // 관측 기간 동안의 평균 대기열로 병목 공정을 판정
        public BottleneckReport AnalyzeBottleneck()
        {
            var stats = new List<ProcessQueueStat>();
            int elapsed = _line.ElapsedSeconds;

            foreach (var process in processes)
            {
                // 평균 대기열 = 큐 누적 합계 / 경과 tick (경과 0이면 0)
                double avg = elapsed == 0 ? 0 : _line.GetQueueSum(process.ProcessName) / (double)elapsed;
                stats.Add(new ProcessQueueStat
                {
                    ProcessName = process.ProcessName,
                    CycleTimeSeconds = process.CycleTimeSeconds,
                    AvgQueue = avg,
                });
            }

            // 평균 대기열이 가장 큰 공정을 병목으로 지목
            string bottleneck = stats.OrderByDescending(s => s.AvgQueue).First().ProcessName;

            return new BottleneckReport
            {
                ObservedSeconds = elapsed,
                Stats = stats,
                BottleneckName = bottleneck,
            };
        }
    }
}

