using Flowy.Core.Data;
using Microsoft.AspNetCore.Mvc;
using static Flowy.Core.Data.EventAnalyzer;

namespace Flowy.Api.Controllers
{
    /// <summary>
    /// EventAnalyzer (읽기 전용 SQL 집계)를 HTTP로 노출하는 컨트롤러
    /// Flowy.Core를 WPF와 공유하며, 같은 flowy.db를 읽기 전용으로 조회
    /// </summary>
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly EventAnalyzer _analyzer;

        // 생성자에서 EventAnalyzer를 주입받음 (Program.cs에서 등록한 인스턴스가 자동 전달)
        public AnalyticsController(EventAnalyzer analyzer)
        {
            _analyzer = analyzer;
        }

        /// <summary>
        /// 설비별, 상태별 총 체류시간(초)과 진입 횟수
        /// </summary>
        [HttpGet("state-dwell")]
        public IEnumerable<StateDwellStat> GetStateDwellStats()
        {
            return _analyzer.GetStateDwellStats();
        }

        /// <summary>
        /// 설비별 Error 체류시간 (DB 기반 누적 병목 판정용)
        /// </summary>
        [HttpGet("error-bottleneck")]
        public IEnumerable<ErrorDwellStat> GetErrorDwellStats()
        {
            return _analyzer.GetErrorDwellStats();
        }

        /// <summary>
        /// 설비별 누적 가동률 (시간가중)
        /// </summary>
        [HttpGet("cumulative-availability")]
        public IEnumerable<AvailabilityStat> GetCumulativeAvailability()
        {
            return _analyzer.GetCumulativeAvailability();
        }
    }
}
