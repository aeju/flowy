using Flowy.Logic.StateMachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Flowy.View
{
    public class TopBarView : MonoBehaviour
    {
        [SerializeField] private TMP_Text availabilityText;   // "가동률 --%" 
        [SerializeField] private TMP_Text uphText;            // "UPH --" 
        [SerializeField] private TMP_Text qualityText;        // "양품률 --%" 
        [SerializeField] private TMP_Text oeeText;            // "OEE --%" 

        private List<WorkProcess> processes;

        public void Initialize(List<WorkProcess> processes)
        {
            this.processes = processes;
        }

        // Bootstrapper가 tick마다 호출: 계산된 값을 받아서 화면 텍스트 갱신
        public void UpdateAvailability(float availability)
        {
            availabilityText.text = $"가동률 {availability:F1}%";
        }
    }
}

