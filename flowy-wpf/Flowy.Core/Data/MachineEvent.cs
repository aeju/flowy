using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flowy.Core.Data
{
    /// <summary>
    /// 공정 상태 변화 이력 한 건
    /// ProcessEventBus의 상태 변화 발행 시점에 기록
    /// </summary>
    public class MachineEvent
    {
        public long Id { get; set; }                    // DB PK
        public string MachineName { get; set; } = "";   // 공정 이름 (예: "W1")
        public string ToState { get; set; } = "";       // 전이 결과 상태 (Idle/Running/Error/Stopped)
        public DateTime Timestamp { get; set; }         // 발생 시각
    }
}
