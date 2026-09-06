using Flowy.Core.Event;
using System.Collections.Generic;   // Queue를 쓰기 위함

namespace Flowy.Core.StateMachine
{
    /// <summary>
    /// 생산 라인의 한 작업 단위(공정)
    /// 예: W1, W2, ... (WorkProcess)
    /// </summary>
    public class WorkProcess
    {
        public string ProcessName; // 식별 정보

        public WorkProcessStateMachine StateMachine; // 상태 관리 (Running/Idle/Error)

        private bool hasProduct;      // 처리 중인 제품 있는지 
        public string CurrentProduct; // 처리 중인 제품 식별자

        public int ErrorRecoveryTicks; // 이상 복구까지 남은 tick 수

        private readonly Queue<string> _inputBuffer = new Queue<string>();  // 이 공정 앞 대기열 (쌓이면 병목 신호)
        public WorkProcess Next;              // 다음 공정 (W4 는 null = 라인 끝)
        public readonly int CycleTimeSeconds; // 처리 시간(초). 1 tick = 1초로 구동
        private int _processedTicks;          // 현재 제품을 몇 tick 처리했는지

        public WorkProcess(string processName, ProcessEventBus eventBus, int cycleTimeSeconds)
        {
            ProcessName = processName;
            StateMachine = new WorkProcessStateMachine(eventBus);
            hasProduct = false;
            CurrentProduct = null;
            ErrorRecoveryTicks = -1; 
            CycleTimeSeconds = cycleTimeSeconds;
        }

        public bool HasProduct()
        {
            return hasProduct;   // 지금 제품 있는지 
        }

        public void AssignProduct(string productId)
        {
            CurrentProduct = productId;   // 제품 식별자 저장
            hasProduct = true;            // "제품 있음"으로 표시
        }

        // 처리 완료 시 비우기 (RunningState에서 쓸 예정)
        public void ClearProduct()        
        {
            CurrentProduct = null;
            hasProduct = false;
        }
        
        // 앞 대기열에 제품 투입 (앞 공정 or 사용자가 넣음)

        public void Enqueue(string productId) => _inputBuffer.Enqueue(productId);

        // 현재 앞에 쌓인 제품 수 (병목 판별의 핵심 지표)
        public int QueueLength => _inputBuffer.Count;

        // 대기열에서 하나 꺼내 처리 시작 (제품 없을 때만)
        public bool TryStartNext()
        {
            if (hasProduct || _inputBuffer.Count == 0) return false;

            AssignProduct(_inputBuffer.Dequeue());
            _processedTicks = 0;
            return true;
        }

        // 한 tick 처리. 사이클타임 채우면 완료(true) 반환
        public bool AdvancedProcessing()
        {
            if (!hasProduct) return false;

            // Running 상태가 아니면 처리 진행 안 함 (Error/Stopped)
            if (StateMachine.CurrentStateType != ProcessStateType.Running) return false;

            _processedTicks++;
            return _processedTicks >= CycleTimeSeconds;
        }
    }
}


