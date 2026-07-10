using Flowy.Logic.Event;
using Flowy.Logic.StateMachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Flowy.View
{
    /// <summary>
    /// ProcessListView: 공정 상태를 우측 목록에 텍스트로 표시하는 역할
    /// </summary>
    public class ProcessListView : MonoBehaviour
    {
        // 상태 조회 대상
        private List<WorkProcess> processes;

        // 우측 목록에 표시할 텍스트 4개
        [SerializeField] private TMP_Text[] processTexts;

        public void Initialize(List<WorkProcess> processes, ProcessEventBus eventBus)
        {
            this.processes = processes;
            eventBus.OnProcessStateChanged += OnProcessChanged; // 구독 등록

            // 초기 상태 반영
            foreach (var process in processes)
            {
                OnProcessChanged(process);
            }
        }

        // 상태에 맞는 설명을 텍스트에 적용
        private void OnProcessChanged(WorkProcess process)
        {
            if (processes == null || processTexts == null) return;

            // 상태를 한글 텍스트로 변환 (예: Idle → "대기", Running → "가동", Error → "이상")
            var stateType = process.StateMachine.CurrentStateType;

            string stateLabel;
            switch (stateType)
            {
                case ProcessStateType.Idle:
                    stateLabel = "대기";
                    break;
                case ProcessStateType.Running:
                    stateLabel = "가동";
                    break;
                case ProcessStateType.Error:
                    stateLabel = "이상";
                    break;
                case ProcessStateType.Stopped:
                    stateLabel = "정지";
                    break;
                default:
                    stateLabel = "알 수 없음";
                    break;
            }

            // tmp 배열에서의 위치 찾아서 텍스트 변경
            int labetIndex = processes.IndexOf(process);
            processTexts[labetIndex].text = $"{process.ProcessName}  {stateLabel}";
        }
    }
}

