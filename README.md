# Flowy

Unity 기반 MES(제조실행시스템) 미니 시뮬레이터로, 여러 공정(W1~W4)을 거치며 제품이 처리되는 생산 라인을 단순화해 구현했습니다. 공정 상태 관리, 실시간 시각화, 조작, 핵심 지표 집계를 다룹니다. 현재 Unity 버전을 C#/WPF 데스크톱 앱으로 포팅하고 있습니다. 

## 버전
- **Unity 버전**: 최초 구현. WebGL로 웹 배포 — 웹 시연 링크: https://aeju.github.io/flowy/
- **WPF 버전**: 제조 스택(C#/WPF)으로 포팅 중 (현재 메인)

---

## 개요

생산 라인의 각 공정(W1~W4)이 대기(Idle) → 가동(Running) → 이상(Error) 상태를 오가며, 이 상태 변화가 3D 시각화·목록·상단 KPI·하단 Alert에 실시간으로 반영됩니다. 사용자는 버튼으로 제품을 투입하거나, 라인 속도를 조절하거나, 설비를 강제로 정지/재가동시킬 수 있습니다.

가장 큰 목표는 화려한 그래픽이 아니라, **MES의 핵심 개념(실시간 상태 관리, 이벤트 기반 아키텍처, 지표 집계)을 실제로 동작하는 코드로 증명하는 것**이었습니다.

---

## 화면 구성

```
Flowy | 가동률 87% | UPH -- | 양품률 -- | OEE 87%   <- 상단 KPI
좌: 제어(조작 버튼) | 중앙: 3D 라인 뷰 | 우: 공정(상태 목록)
하단: Alert (이상/정지 안내)
```

- **중앙 3D 뷰**: 공정 4개(W1~W4)를 큐브로 표현, 상태에 따라 색이 바뀜 (가동=초록 / 대기=노랑 / 이상·정지=빨강)
- **좌측 제어**: 제품 투입, 설비 정지/재가동, 라인 가속/감속
- **우측 공정 목록**: 각 공정의 이름과 현재 상태를 텍스트로 표시
- **상단 KPI**: 가동률(실제 계산값), UPH·양품률·OEE(추가 데이터 필요로 현재 비활성화 표시)
- **하단 Alert**: 이상(Error) 또는 정지(Stopped) 상태인 공정을 실시간으로 안내

---

## 아키텍처

3개 레이어로 분리했습니다.

```
Scripts/
├── Logic/          (순수 C#, UnityEngine 미참조)
│   ├── StateMachine/   IWorkProcessState, Running/Idle/Error/Stopped, WorkProcessStateMachine, WorkProcess
│   ├── Simulation/     ProductionLine
│   ├── Event/          ProcessEventBus
│   └── Metric/         MetricsCalculator
├── View/           (Unity 의존, MonoBehaviour)
│   ├── ProcessLineView    (3D 상태 시각화)
│   ├── ProcessListView    (우측 목록)
│   ├── TopBarView         (KPI 텍스트)
│   ├── AlertView          (하단 알림)
│   └── ControlPanelView   (조작 버튼)
└── Bootstrap/
    └── Bootstrapper    (Composition Root - 객체 생성/주입/tick 구동)
```

**왜 이렇게 나눴나**: Logic 레이어는 Unity 없이도 테스트/재사용 가능해야 한다는 원칙 아래, 상태 관리·시뮬레이션·지표 계산을 전부 순수 C#으로 작성했습니다. View는 오직 "받은 데이터를 화면에 어떻게 보여줄지"만 담당하고, Bootstrapper가 모든 객체를 생성해 서로 연결(의존성 주입)합니다. 싱글톤 대신 이 방식을 택해, 각 객체가 무엇에 의존하는지가 Bootstrapper 한 곳에 명시적으로 드러나도록 했습니다.

---

## 핵심 설계 결정

### 1. State 패턴 + 상태 공유

공정 상태(Running/Idle/Error/Stopped)를 각각 별도 클래스로 구현했습니다. 상태 전환 로직이 if/switch 분기로 뒤엉키는 것을 막고, 상태가 늘어나도 분기 비용이 늘지 않게 하기 위함입니다.

성능을 고려해 상태 객체는 공정마다 새로 만들지 않고 공유 가능한 구조로 설계했습니다. 상태 자신은 개별 데이터를 갖지 않고, Enter/Tick/Exit가 매번 대상 WorkProcess를 매개변수로 받아 처리합니다. 복구 타이머 같은 개별 상태값은 상태 클래스가 아니라 WorkProcess 쪽에 둬서, 여러 공정이 상태 객체를 공유해도 데이터가 섞이지 않도록 했습니다.

Tick()은 전이가 필요 없으면 null을 반환하도록 설계했습니다. 대부분의 tick에서 상태가 유지되므로, null 반환 시 StateMachine이 즉시 스킵하도록 해 불필요한 Exit/Enter 호출을 없앴습니다.

### 2. 이벤트 기반 아키텍처 (폴링 -> 이벤트 전환)

초기에는 View가 매 프레임 상태를 폴링(polling)하는 방식이었습니다. 이를 ProcessEventBus(발행-구독 패턴)로 전환해, 상태가 실제로 바뀔 때만 View가 반응하도록 개선했습니다. 공정이 대부분의 시간 동안 상태를 유지한다는 점에서, 이 전환은 불필요한 연산을 크게 줄입니다.

전환 과정에서 발견한 문제: 이벤트는 변화 시점에만 발행되므로, 최초 상태는 아무도 알려주지 않습니다. 이를 해결하기 위해 각 View의 Initialize에서 전체 공정을 순회하며 한 번 수동으로 초기 상태를 반영하도록 했습니다.

### 3. 자동 전이 vs 강제 전이

일반 상태 전이(Tick)는 시뮬레이션이 조건을 판단해 자동으로 일으키지만, 사용자가 설비 정지/재가동 버튼으로 개입하는 경우는 조건 판단 없이 즉시 전이해야 합니다. 이를 위해 WorkProcessStateMachine에 ForceState를 별도로 두어, Tick의 자동 전이 경로와 명확히 구분했습니다.

정지 상태(StoppedState)는 처음엔 기존 ErrorState를 재사용하려 했으나, ErrorState의 자동 복구 타이머 로직이 그대로 딸려와 재가동 버튼을 누르지 않아도 자동으로 풀리는 문제가 발생했습니다. 이를 계기로 StoppedState를 별도 클래스로 분리해, 자동 복구 없이 재가동 명령이 있을 때까지 유지되도록 수정했습니다.

### 4. 시뮬레이션 속도와 프레임 속도의 분리

초기에는 매 프레임(Update) tick을 호출해, 상태 전이가 프레임 레이트에 종속되는 문제가 있었습니다(예: 5tick짜리 복구가 실제로는 0.08초 만에 끝남). Bootstrapper에 tickInterval(초 단위 간격)과 누적 타이머를 도입해, 몇 초에 한 번 시뮬레이션을 진행할지를 프레임 속도와 분리했습니다. 라인 가속/감속 버튼은 이 tickInterval 값을 조절하는 방식으로 구현했습니다.

### 5. 캡슐화

Unity Inspector에 노출해야 하는 필드는 public 대신 [SerializeField] private로 선언해, Inspector 연결은 유지하면서도 다른 클래스가 임의로 값을 변경하지 못하도록 했습니다. 도메인 모델(WorkProcess)의 데이터도 get; private set; 프로퍼티로 감싸, 상태 변경은 정해진 메서드(AssignProduct, ClearProduct 등)를 통해서만 이뤄지도록 했습니다.

---

## 지표(KPI)

| 지표 | 상태 | 설명 |
|---|---|---|
| 가동률 (Availability) | 구현 완료 | Running 상태인 공정 수 / 전체 공정 수 x 100 |
| OEE | 별도 미표시 | 정식 공식은 가동률 x 성능 x 양품률. 성능/양품률 데이터가 없어 화면에서 비활성화 처리 |
| UPH (시간당 생산 대수) | 미구현 | 제품 완료 카운트 로직 추가 시 계산 가능한 구조 (공식은 표준(완료 수/시간). 완료 카운트 로직 추가 시 계산 가능) |
| 양품률 (Quality Rate) | 미구현 | 불량 판정(Fool-Proof 검증) 로직 추가 시 계산 가능한 구조 (공식은 표준(양품 수/전체). 불량 판정 로직 추가 시 계산 가능) |

가동률만 실제로 계산해 표시하고, 나머지는 근거 없는 값을 만들지 않기 위해 "--"로 정직하게 남겼습니다.

---

## 트러블슈팅 기록

- 초기 상태 미반영: 이벤트 기반 전환 후, 최초 상태가 화면에 반영되지 않는 문제 -> Initialize 시점 수동 동기화로 해결
- 정지 상태 자동 복구 문제: ErrorState 재사용 시 의도치 않게 자동 복구되는 문제 발견 -> StoppedState 분리로 해결
- tick과 프레임 속도 결합: 매 프레임 tick 실행으로 복구 시간이 의도보다 훨씬 짧게 동작 -> tickInterval 도입으로 분리

---

## WPF 포팅 (진행 중)

제조 현장에서 널리 쓰이는 스택(C#/WPF)에 맞춰, Unity 버전을 WPF 데스크톱 앱으로 포팅하고 있습니다. 게임 엔진이 아닌 실제 제조 IT의 UI 프레임워크로 옮기는 것이 목표입니다.

### Logic 레이어 무수정 재사용 

Unity 버전에서 Logic 레이어를 "UnityEngine 미참조 순수 C#"으로 설계한 원칙 덕분에, UI 프레임워크(Unity → WPF)를 교체했지만 Logic 레이어 10개 파일은 수정 없이 이관했습니다.

```
flowy-wpf/
├── Flowy.Core/          (Unity Logic 레이어를 그대로 이관, 순수 C#)
│   └── StateMachine / Simulation / Event / Metric
└── Flowy.Wpf/           (WPF UI)
    ├── ViewModels/      MainViewModel, ProcessDisplayItem, RelayCommand
    └── MainWindow.xaml
```

### Unity → WPF 구조 대응

| Unity (원본) | WPF | 비고 |
| --- | --- | --- |
| Logic 레이어 | Flowy.Core | 무수정 이관 |
| View (MonoBehaviour) | View(XAML) + ViewModel | MVVM 패턴 적용 |
| ProcessEventBus 수동 연결 | 데이터 바인딩 (INotifyPropertyChanged) | 프레임워크 기본 기능으로 대체 |
| Bootstrapper (Update + tickInterval) | DispatcherTimer | UI 스레드 타이머로 대체 |

### 포팅하며 개선한 부분

- **설비 정지 로직**: 원본은 Running 공정만 정지 대상이라 이상(Error) 공정이 정지되지 않고 자동 복구되는 문제가 있었습니다. 현장 관점상 이상 설비도 정지 대상이라 판단해, Stopped를 제외한 전체 공정을 정지하도록 변경했습니다.
- **실시간 시계**: 실제 MES/HMI 화면이 데이터의 시점을 나타내기 위해 현재 시각을 상시 표시한다는 점을 반영해 상단에 시계를 추가했습니다. 시뮬레이션 속도(가속/감속)와 무관하게 1초 간격으로 갱신되도록 별도 타이머로 분리했습니다.

### 로드맵

- ✅ **WPF 포팅** — MVVM 구조, 제어 버튼 5종, KPI·Alert·실시간 시계
- 🔨 **SQLite 연동** — 시뮬레이션 이벤트를 SQLite+Dapper로 적재. 완료·불량 판정 로직을 추가해 비어있던 UPH·양품률 계산 (공식은 표준: 완료 수/시간, 양품 수/전체)
- ⬜ **이상 판별 파이프라인** — 적재 데이터에서 병목 등 이상 패턴 판별 → 리포트 자동 생성
- ⬜ **병목(WIP) 시각화** — 공정 앞 대기 제품 수 표시
- ⬜ **데이터 영속화 추상화** — IProductionRepository로 인메모리/SQLite 구현체 교체 가능하게 (WebGL은 인메모리, 로컬은 SQLite)
- ⬜ **상태 색상 표시** — 공정 상태를 색으로 구분 (IValueConverter)

---

## 실행 환경

- Unity 버전: WebGL 빌드로 배포 (웹 시연 링크)
- WPF 버전: .NET 8.0 데스크톱 앱 
