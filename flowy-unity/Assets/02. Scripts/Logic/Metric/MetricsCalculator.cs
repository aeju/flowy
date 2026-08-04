using Flowy.Logic.StateMachine;
using System.Collections.Generic;
using System.Linq;

namespace Flowy.Logic.Metric
{
    /// <summary>
    /// 공정 리스트를 바탕으로 가동률(Availability)을 계산
    /// (UPH·양품률·OEE는 완료 카운트·불량 판정 로직이 없어 화면에서 "--"로 유지)
    /// (OEE = 가동률 × 성능 × 양품률)
    /// </summary>
    public class MetricsCalculator 
    {
        private List<WorkProcess> processes;

        public MetricsCalculator(List<WorkProcess> processes)
        {
            this.processes = processes;
        }

        // 가동률(%) 계산: Running 상태인 공정 수 / 전체 공정 수 × 100
        public float CalculateAvailability()
        {
            // processes 중 CurrentStateType이 Running인 것의 개수 세기
            int runningCount = processes.Count(p => p.StateMachine.CurrentStateType == ProcessStateType.Running);

            float availability = runningCount / (float)processes.Count * 100f;
            return availability;
        }
    }
}

