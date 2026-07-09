using Flowy.Logic.StateMachine;
using System;

namespace Flowy.Logic.Event
{
    /// <summary>
    /// 공정(Process) 상태가 바뀔 때, 이를 구독한 대상에게 알림
    /// </summary>
    public class ProcessEventBus
    {
        // 공정 상태가 바뀔 때마다 호출됨
        public event Action<WorkProcess> OnProcessStateChanged;

        // 등록된 구독자들 전부에게 "이 공정의 상태가 바뀌었다"고 알림
        public void PublishStateChanged(WorkProcess process)
        {
            OnProcessStateChanged?.Invoke(process);
        }
    }
}

    
