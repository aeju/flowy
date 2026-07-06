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

        public WorkProcess(string processName)
        {
            ProcessName = processName;
            StateMachine = new WorkProcessStateMachine();
            hasProduct = false;
            CurrentProduct = null;
        }
    }
}


