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
    }
}

