using Flowy.Logic.StateMachine;      
using System.Collections.Generic;
using UnityEngine;

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

        // Bootstrapper가 호출 (받아온 리스트를 필드에 저장)
        public void Initialize(List<WorkProcess> processes)
        {
            this.processes = processes;
        }

        // processCubes와 processes를 짝지어 각 공정 상태에 맞게 큐브 색을 바꿈
        // 상태에 따라 색 결정 (Idle=노랑, Running=초록, Error=빨강)
        private void Update()
        {
            // 아직 초기화 안 됐으면 건너뜀
            if (processes == null) return;        

            for (int i = 0; i < processCubes.Length; i++)
            {
                var stateType = processes[i].StateMachine.CurrentStateType;   // 리스트에서 인덱스로 접근

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
                        color = Color.red;
                        break;
                    default:
                        color = Color.white;
                        break;
                }
                processCubes[i].GetComponent<Renderer>().material.color = color;
            }
        }
    }
}

    
    
