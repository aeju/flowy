using Flowy.Bootstrap;
using Flowy.Logic.StateMachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Flowy.View
{
    public class ControlPanelView : MonoBehaviour
    {
        [SerializeField] private Bootstrapper bootstrapper;

        [SerializeField] private Button injectButton; // 제품 투입 버튼 

        [SerializeField] private Button speedUpButton; // 라인 가속 버튼 
        [SerializeField] private Button speedDownButton; // 라인 감속 버튼 

        [SerializeField] private Button stopButton; // 설비 정지 버튼 
        [SerializeField] private Button restartButton; // 설비 재가동 버튼 

        // 명령을 보낼 대상 (WorkProcess 리스트)
        private List<WorkProcess> processes;

        public void Initialize(List<WorkProcess> processes)
        {
            this.processes = processes;

            // 버튼 클릭 이벤트 등록
            injectButton.onClick.AddListener(OnInjectButtonClicked);
            speedUpButton.onClick.AddListener(OnSpeedUpButtonClicked);
            speedDownButton.onClick.AddListener(OnSpeedDownButtonClicked);
            stopButton.onClick.AddListener(OnStopClicked);
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnInjectButtonClicked()
        {
            // processes 중에서 CurrentStateType이 Idle인 첫 번째 공정 찾기
            var targetProcess = processes.FirstOrDefault(p => p.StateMachine.CurrentStateType == ProcessStateType.Idle);

            if (targetProcess != null)
            {
                targetProcess.AssignProduct("P-" + Random.Range(1000, 9999));
            }
        }
        private void OnSpeedUpButtonClicked()
        {
            bootstrapper.SpeedUp();
        }

        private void OnSpeedDownButtonClicked()
        {
            bootstrapper.SpeedDown();
        }

        private void OnStopClicked()
        {
            var runningProcesses = processes.Where(p => p.StateMachine.CurrentStateType == ProcessStateType.Running).ToList();
            foreach (var p in runningProcesses)
            {
                p.StateMachine.ForceState(p, new StoppedState());
            }
        }

        private void OnRestartClicked()
        {
            var stoppedProcesses = processes.Where(p => p.StateMachine.CurrentStateType == ProcessStateType.Stopped).ToList();
            foreach (var p in stoppedProcesses)
            {
                p.StateMachine.ForceState(p, new IdleState());
            }
        }
    }
}
    
