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
        [SerializeField] private ControlPanelView controlPanelView;
        [SerializeField] private ProcessListView processListPanelView;

        private void Awake()
        {
            // 0. ProcessEventBus 생성
            processEventBus = new ProcessEventBus();

            // 1. W1 ~ W4 생성 및 (각자 eventBus를 공유해 상태 변화를 알릴 수 있게 함)
            var w1 = new WorkProcess("W1", processEventBus);
            var w2 = new WorkProcess("W2", processEventBus);
            var w3 = new WorkProcess("W3", processEventBus);
            var w4 = new WorkProcess("W4", processEventBus);

            // 2. W1~W4를 리스트로 묶기 (ProductionLine과 각 View에 공유해서 전달)
            var processes = new List<WorkProcess> { w1, w2, w3, w4 };

            // 3. ProductionLine 생성 (전체 공정을 순회하며 매 tick 진행시킴)
            productionLine = new ProductionLine(processes);

            // 4. ProcessLineView 초기화 (공정 상태를 3D 큐브 색으로 시각화)
            processLineView.Initialize(processes, processEventBus);

            // 5. controlPanelView 초기화 (버튼 클릭으로 공정에 명령 전달)
            controlPanelView.Initialize(processes);

            // 6. processListPanelView 초기화 (공정 상태를 리스트로 시각화)
            processListPanelView.Initialize(processes, processEventBus);

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

