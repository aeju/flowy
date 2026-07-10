using Flowy.Logic.Event;
using Flowy.Logic.Metric;
using Flowy.Logic.Simulation;
using Flowy.Logic.StateMachine;
using Flowy.View;
using System.Collections.Generic;
using UnityEngine;

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
        private MetricsCalculator metricsCalculator;

        [SerializeField] private ProcessLineView processLineView;
        [SerializeField] private ControlPanelView controlPanelView;
        [SerializeField] private ProcessListView processListPanelView;
        [SerializeField] private TopBarView topBarView;

        // 추가: 몇 초마다 한 번 tick 할지 (조절 가능하게)
        private float tickInterval = 0.5f;   // 기본값 0.5초에 한 번
        // 추가: 마지막 tick 이후 얼마나 시간이 지났는지 누적
        private float timer = 0f;

        private void Awake()
        {
            // 1. 이벤트 버스 생성 + 공정 생성 
            processEventBus = new ProcessEventBus();
            var w1 = new WorkProcess("W1", processEventBus);
            var w2 = new WorkProcess("W2", processEventBus);
            var w3 = new WorkProcess("W3", processEventBus);
            var w4 = new WorkProcess("W4", processEventBus);
            var processes = new List<WorkProcess> { w1, w2, w3, w4 };

            // 2. 공정 데이터를 다루는 로직 객체 생성
            productionLine = new ProductionLine(processes);
            metricsCalculator = new MetricsCalculator(processes);

            // 3. View 초기화
            processLineView.Initialize(processes, processEventBus);
            controlPanelView.Initialize(processes);
            processListPanelView.Initialize(processes, processEventBus);
            topBarView.Initialize(processes);
        }

        // 매 tick마다 시뮬레이션 진행 + 가동률 재계산 + 화면 반영
        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= tickInterval) 
            {
                timer = 0f;
                productionLine.Tick();

                float availability = metricsCalculator.CalculateAvailability();
                topBarView.UpdateAvailability(availability);
            }
        }

        // 가속: interval을 줄임 (최소 0.1초로 제한)
        public void SpeedUp()
        {
            tickInterval = Mathf.Max(0.1f, tickInterval - 0.1f);
        }

        // 감속: interval을 늘림
        public void SpeedDown()
        {
            tickInterval += 0.1f;
        }
    }
}

