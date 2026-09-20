using Dapper;
using Microsoft.Data.Sqlite;

namespace Flowy.Core.Data
{
    /// <summary>
    /// MachineEvent 이력에서 SQL 집계로 분석 지표를 산출 
    /// EventRepository(쓰기)와 분리된 읽기 전용 분석 경계
    /// </summary>
    public  class EventAnalyzer
    {
        private readonly string _connectionString;

        // Repository와 동일한 connectionString을 공유 (같은 DB를 읽기 전용으로 바라봄)
        public EventAnalyzer(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 설비별/상태별로, 그 상태에 총 몇 초 머물렀는지, 몇 번 들어갔는지 계산
        /// 체류시간 = 다음 이벤트 Timestamp - 현재 이벤트 Timestamp
        /// 각 세션의 마지막 이벤트는 다음이 없어(NULL) 집계에서 제외
        /// lagacy(세션 도입 전 데이터)는 세션 경계가 불명확해 분석 대상에서 제외
        /// </summary>
        /// <returns></returns>
        public IEnumerable<StateDwellStat> GetStateDwellStats()
        {
            using var conn = new SqliteConnection(_connectionString);
            return conn.Query<StateDwellStat>(@"
                WITH ordered As (
                    SELECT 
                        MachineName, 
                        ToState, 
                        SessionId,
                        Timestamp AS StartTs, 
                        LEAD(Timestamp) OVER (
                            PARTITION BY MachineName, SessionId ORDER BY Timestamp
                        ) AS EndTs
                    FROM MachineEvent
                    WHERE SessionId != 'legacy'
                )
                SELECT 
                    MachineName, 
                    ToState, 
                    COUNT(*) AS EnterCount, 
                    SUM((julianday(EndTs) - julianday(StartTs)) * 86400) AS TotalSeconds
                FROM ordered
                WHERE EndTs IS NOT NULL
                GROUP BY MachineName, ToState
                ORDER BY MachineName, ToState;");
        }

        /// <summary>
        /// 설비별 Error 총 체류시간(초)과 발생 횟수 -> DB 기반 병목 판정용
        /// Stopped는 사용자 정지(시뮬 종료 시 전 설비 동시 Stopped)라 제외, Error만 집계
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ErrorDwellStat> GetErrorDwellStats()
        {
            using var conn = new SqliteConnection(_connectionString);
            return conn.Query<ErrorDwellStat>(@"
                WITH ordered As (
                    SELECT 
                        MachineName, 
                        ToState,
                        SessionId,
                        Timestamp AS StartTs, 
                        LEAD(Timestamp) OVER (
                            PARTITION BY MachineName, SessionId ORDER BY Timestamp
                        ) AS EndTs
                    FROM MachineEvent
                    WHERE SessionId != 'legacy'
                )
                SELECT 
                    MachineName, 
                    COUNT(*) AS ErrorCount, 
                    SUM((julianday(EndTs) - julianday(StartTs)) * 86400) AS TotalErrorSeconds
                FROM ordered
                WHERE ToState = 'Error'
                    AND EndTs IS NOT NULL
                GROUP BY MachineName
                ORDER BY TotalErrorSeconds DESC;");
        }

        /// <summary>
        /// 설비별 누적 가동률 = Running 총 체류시간 / 전체 관측시간 * 100
        /// 관측시간 = 각 설비별 첫 이벤트 ~ 마지막 이벤트 구간 (세션 내에서만)
        /// 실시간 가동률(스냅샷)과 달리, 시간가중 평균으로 "전체 구간 중 얼마나 돌았나"를 보여줌
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AvailabilityStat> GetCumulativeAvailability()
        {
            using var conn = new SqliteConnection(_connectionString);
            return conn.Query<AvailabilityStat>(@"
                WITH ordered AS (
                    SELECT 
                        MachineName, 
                        ToState,
                        SessionId,
                        Timestamp AS StartTs, 
                        LEAD(Timestamp) OVER (
                            PARTITION BY MachineName, SessionId ORDER BY Timestamp
                        ) AS EndTs
                    FROM MachineEvent
                    WHERE SessionId != 'legacy'
                ),
                dwell AS (
                    SELECT 
                        MachineName, 
                        ToState, 
                        (julianday(EndTs) - julianday(StartTs)) * 86400 AS Seconds
                    FROM ordered
                    WHERE EndTs IS NOT NULL
                )
                SELECT 
                    MachineName, 
                    SUM(CASE WHEN ToState = 'Running' THEN Seconds ELSE 0 END) AS RunningSeconds,
                    SUM(Seconds) AS ObservedSeconds,
                    (SUM(CASE WHEN ToState = 'Running' THEN Seconds ELSE 0 END)  -- Running일 때만 그 초를 세고, 아니면 0
                        / SUM(Seconds)) * 100 AS AvailabilityPercent
                FROM dwell
                GROUP BY MachineName
                ORDER BY MachineName;");
        }

        // 설비별/상태별 체류 통계
        public class StateDwellStat
        {
            public string MachineName { get; set; } = "";
            public string ToState { get; set; } = "";
            public int EnterCount { get; set; }
            public double TotalSeconds { get; set; }
        }

        // 설비별 Error 상태 체류 통계
        public class ErrorDwellStat
        {
            public string MachineName { get; set; } = "";
            public int ErrorCount { get; set; }
            public double TotalErrorSeconds { get; set; }
        }

        // 설비별 누적 가동률 한 건 (GetCumulativeAvailability() 결과 한 줄)
        // Running 시간 / 전체 관측 시간 * 100 -> 시간가중 가동률
        public class  AvailabilityStat
        {
            public string MachineName { get; set; } = "";
            public double RunningSeconds { get; set; }
            public double ObservedSeconds { get; set; }
            public double AvailabilityPercent { get; set; }
        }
    }
}
