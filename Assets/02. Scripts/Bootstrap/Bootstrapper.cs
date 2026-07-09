using UnityEngine;
using Flowy.Logic.Simulation;
using Flowy.Logic.StateMachine;
using System.Collections.Generic;

namespace Flowy.Bootstrap
{
    /// <summary>
    /// 유일한 진입점
    /// Logic 객체 생성 및 주입, tick 구동
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        private ProductionLine productionLine;

        private void Awake()
        {
            // 1. W1 ~ W4 생성
            var w1 = new WorkProcess("W1");
            var w2 = new WorkProcess("W2");
            var w3 = new WorkProcess("W3");
            var w4 = new WorkProcess("W4");

            // 2. 리스트로 묶기
            var processes = new List<WorkProcess> { w1, w2, w3, w4 };

            // 3. ProductionLine 생성 + 리스트 주입
            productionLine = new ProductionLine(processes);
        }

        private void Start()
        {
            // 지금은 사용 안 함
        }

        // 매 프레임 Logic의 Tick 구동
        private void Update()
        {
            productionLine.Tick();
        }
    }
}

