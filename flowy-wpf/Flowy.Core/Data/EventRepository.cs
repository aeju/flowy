using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flowy.Core.Data
{
    /// <summary>
    /// 공정 이벤트 이력(MachineEvent)의 DB 저장/조회를 담당하는 저장소
    /// SQL과 DB 접근을 이 클래스 안에서 격리해, 바깥(로거/뷰)은 DB를 몰라도 되게 한다
    /// </summary>
    public class EventRepository
    {
        // DB 연결 문자열 (기본값: 실행 폴더의 flowy.db)
        private readonly string _connectionString;

        public EventRepository(string? connectionString = null)
        {
            if (connectionString == null)
            {
                // db를 항상 실행 파일과 같은 폴더에 고정 생성 (위치 헷갈림 방지)
                var dbPath = Path.Combine(AppContext.BaseDirectory, "flowy.db");
                connectionString = $"Data Source={dbPath}";
            }
            _connectionString = connectionString;
            EnsureCreate();   // 저장소 생성 시 테이블이 없으면 만든다
        }

        /// <summary>
        /// MachineEvent 테이블이 없으면 생성 (이미 있으면 아무 일 x)
        /// </summary>
        private void EnsureCreate()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS MachineEvent (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    MachineName TEXT NOT NULL,
                    ToState     TEXT NOT NULL,
                    Timestamp   TEXT NOT NULL
                );");
        }

        /// <summary>
        /// 이벤트 한 건을 DB에 저장
        /// @MachineName 등 파라미터는 인자로 받은 MachineEvent의 프로퍼티에서 자동 매핑됨
        /// </summary>
        /// <param name="e"></param>
        public void Insert(MachineEvent e)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Execute(
                "INSERT INTO MachineEvent (MachineName, ToState, Timestamp) VALUES (@MachineName, @ToState, @Timestamp);",
                e);
        }

        /// <summary>
        /// 저장된 모든 이벤트를 발생 순서(Id 오름차순)로 조회
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MachineEvent> GetAll()
        {
            using var conn = new SqliteConnection(_connectionString);
            return conn.Query<MachineEvent>(
                "SELECT Id, MachineName, ToState, Timestamp FROM MachineEvent ORDER BY Id;");
        }
    }
}
