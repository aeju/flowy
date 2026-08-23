using Flowy.Core.Event;
using Flowy.Core.StateMachine;

namespace Flowy.Core.Data
{
    /// <summary>
    /// ProcessEventBus를 구독해, 공정 상태가 바뀔 때마다 그 사실을 DB에 이력으로 남김
    /// 기존 로직(StateMachine, 버스)을 수정하지 않고 버스에 "얹혀서" 동작
    /// 로깅을 켜고 끄는 것 = 이 객체를 만드느냐 마느냐로 끝남
    /// </summary>
    public class EventLogger
    {
        private readonly EventRepository _repository;

        /// <summary>
        /// 생성 시점에 버스 구독을 검
        /// 이후 발행되는 모든 상태 변화가 자동으로 기록됨
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="repository"></param>
        public EventLogger(ProcessEventBus bus, EventRepository repository)
        {
            _repository = repository;
            bus.OnProcessStateChanged += HandleStateChanged; // 구독 등록
        }

        /// <summary>
        /// 버스가 상태 변화를 알려올 때마다 호출됨
        /// 넘어온 WorkProcess에서 이름과 현재 상태를 꺼내 이력 한 건으로 저장
        /// </summary>
        /// <param name="process"></param>
        private void HandleStateChanged(WorkProcess process)
        {
            _repository.Insert(new MachineEvent
            {
                MachineName = process.ProcessName,
                ToState = process.StateMachine.CurrentStateType.ToString(),
                Timestamp = DateTime.Now
            });
        }
    }
}
