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
                WHERE EndTs IS NOT NULL
                    AND EndTs IS NOT NULL
                GROUP BY MachineName
                ORDER BY TotalErrorSeconds DESC;");
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
    }
}
