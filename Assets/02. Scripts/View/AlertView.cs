using Flowy.Logic.Event;
using Flowy.Logic.StateMachine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Flowy.View
{
    /// <summary>
    /// 공정 중 이상(Error) 상태가 있으면 하단 Alert에 표시
    /// </summary>
    public class AlertView : MonoBehaviour
    {
        [SerializeField] private TMP_Text alertText;

        private List<WorkProcess> processes;

        public void Initialize(List<WorkProcess> processes, ProcessEventBus eventBus)
        {
            this.processes = processes;
            eventBus.OnProcessStateChanged += OnProcessChanged;
            RefreshAlert();   
        }

        private void OnProcessChanged(WorkProcess process)
        {
            RefreshAlert();
        }

        private void RefreshAlert()
        {
            var errorProcesses = processes.Where(p => p.StateMachine.CurrentStateType == ProcessStateType.Error).ToList();
            var stoppedProcesses = processes.Where(p => p.StateMachine.CurrentStateType == ProcessStateType.Stopped).ToList();

            if (errorProcesses.Count > 0)
            {
                string names = string.Join(", ", errorProcesses.Select(p => p.ProcessName));
                alertText.text = $"⚠ {names} 이상 발생";
                alertText.color = new Color(0.85f, 0.25f, 0.2f); // 경고 색상 (빨간색)
            }
            else if (stoppedProcesses.Count > 0)
            {
                string names = string.Join(", ", stoppedProcesses.Select(p => p.ProcessName));
                alertText.text = $"⚠ {names} 정지 중";
                alertText.color = new Color(0.9f, 0.6f, 0.1f);   // 정지 알림 색상 (주황)
            }
            else
            {
                alertText.text = "이상 없음";
                alertText.color = new Color(0.3f, 0.3f, 0.3f);
            }
        }
    }
}

