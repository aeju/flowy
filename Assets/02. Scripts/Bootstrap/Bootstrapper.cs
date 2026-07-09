using UnityEngine;
using Flowy.Logic.Simulation;
using Flowy.Logic.StateMachine;
using Flowy.View;
using System.Collections.Generic;
using Flowy.Logic.Event;

namespace Flowy.Bootstrap
{
    /// <summary>
    /// 유일한 진입점
    /// Logic 객체 생성 및 주입, tick 구동
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        private ProductionLine productionLine;
        private ProcessEventBus processEventBus;
        [SerializeField] private ProcessLineView processLineView;

        private void Awake()
        {
            // 0. ProcessEventBus 생성
            processEventBus = new ProcessEventBus();

            // 1. W1 ~ W4 생성 및 eventBus를 넘겨줌
            var w1 = new WorkProcess("W1", processEventBus);
            var w2 = new WorkProcess("W2", processEventBus);
            var w3 = new WorkProcess("W3", processEventBus);
            var w4 = new WorkProcess("W4", processEventBus);

            // 2. 리스트로 묶기
            var processes = new List<WorkProcess> { w1, w2, w3, w4 };

            // 3. ProductionLine 생성 + 리스트 주입
            productionLine = new ProductionLine(processes);

            // 4. ProcessLineView 생성 + 리스트 주입
            processLineView.Initialize(processes, processEventBus);

            // TODO: 나중에 지우기 (임시 테스트용)
            w1.AssignProduct("test");
        }

        // 매 프레임 Logic의 Tick 구동
        private void Update()
        {
            productionLine.Tick();
        }
    }
}

