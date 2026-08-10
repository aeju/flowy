using System.Collections.Generic;
using Flowy.Core.StateMachine;

namespace Flowy.Core.Simulation
{
    /// <summary>
    /// Line 전체 tick 관리
    /// </summary>
    public class ProductionLine
    {
        // 이 라인이 관리하는 공정들 목록
        private List<WorkProcess> processes;

        // Bootstrapper가 생성한 processes 리스트를 주입받음 
        public ProductionLine(List<WorkProcess> processes)
        {
            this.processes = processes;
        }

        // 매 tick 호출되는 메서드 (Bootstrapper에서 호출)
        // 리스트 안의 모든 WorkProcess를 순회하며 각 공정의 StateMachine.Tick을 호출
        public void Tick()
        {
            foreach (var process in processes)
            {
                process.StateMachine.Tick(process);
            }
        }
    }
}

