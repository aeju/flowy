using Flowy.Logic.StateMachine;      
using System.Collections.Generic;
using UnityEngine;
using Flowy.Logic.Event;

namespace Flowy.View
{
    /// <summary>
    /// ProcessLineView: Process W1~W4에 해당하는 큐브 색을 바꾸는 역할
    /// </summary>
    public class ProcessLineView : MonoBehaviour
    {
        // 상태 조회 대상
        private List<WorkProcess> processes; 

        // Process W1~W4에 해당하는 큐브
        [SerializeField] private GameObject[] processCubes;

        // Bootstrapper가 호출
        // 공정 리스트 저장, 이벤트 구독 등록, 초기 상태 반영
        public void Initialize(List<WorkProcess> processes, ProcessEventBus eventBus)
        {
            this.processes = processes;

            // 구독 등록 (상태가 바뀔 때마다 OnProcessChanged가 불리도록 연결)
            eventBus.OnProcessStateChanged += OnProcessChanged;

            // 초기 상태를 한 번 반영 (이벤트가 없으니 수동으로)
            foreach (var process in processes)
            {
                OnProcessChanged(process);
            }
        }

        // 공정 상태 변화 이벤트를 구독해 자동으로 호출됨
        // 상태가 바뀐 공정을 전달받아, 그 상태에 맞는 색을 해당 큐브에 적용 
        private void OnProcessChanged(WorkProcess process)
        {
            if (processes == null) return;

            // 해당 공정의 상태 확인 
            var stateType = process.StateMachine.CurrentStateType;  
            Color color = Color.white;
            switch (stateType)
            {
                case ProcessStateType.Idle:
                    color = Color.yellow;
                    break;
                case ProcessStateType.Running:
                    color = Color.green;
                    break;
                case ProcessStateType.Error:
                case ProcessStateType.Stopped:
                    color = Color.red;
                    break;
                default:
                    color = Color.white;
                    break;
            }

            // 큐브 배열에서의 위치 찾아서 색상 변경
            int cubeIndex = processes.IndexOf(process);
            processCubes[cubeIndex].GetComponent<Renderer>().material.color = color;
        }
    }
}

    
    
