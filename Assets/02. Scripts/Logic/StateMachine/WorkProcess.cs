namespace Flowy.Logic.StateMachine
{
    /// <summary>
    /// 생산 라인의 한 작업 단위(공정)
    /// 예: W1, W2, ... (WorkProcess)
    /// </summary>
    public class WorkProcess
    {
        public string ProcessName; // 식별 정보
        public WorkProcessStateMachine StateMachine; // 상태 관리 (Running/Idle/Error)

        private bool hasProduct; // 처리 중인 제품 있는지 
        public string CurrentProduct; // 처리 중인 제품 식별자

        public int ErrorRecoveryTicks; // 이상 복구까지 남은 tick 수

        public WorkProcess(string processName)
        {
            ProcessName = processName;
            StateMachine = new WorkProcessStateMachine();
            hasProduct = false;
            CurrentProduct = null;
            ErrorRecoveryTicks = 0;
            ErrorRecoveryTicks = -1; 
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
    }
}


