using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;

namespace Flowy.Core.Metric
{
    // 병목 분석 결과 한 건 (공정별 평균 대기열 + 병목 지목)
    public class BottleneckReport
    {
        public int ObservedSeconds { get; set; }            // 관측 시간(초)
        public List<ProcessQueueStat> Stats { get; set; }   // 공정별 평균 대기열 
        public string BottleneckName { get; set; }          // 병목으로 지목된 공정

        public string ToText()
        {
            var sb = new StringBuilder();

            sb.AppendLine("[병목 분석 리포트]");

            sb.AppendLine($"생성 시각: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"관측 시간: {ObservedSeconds}초");
            sb.AppendLine();

            sb.AppendLine("공정별 평균 대기열:");
            foreach (var s in Stats)
            {
                // 병목 공정엔 화살표 표시
                string mark = s.ProcessName == BottleneckName ? " <- 병목" : "";
                sb.AppendLine($" {s.ProcessName} (C/T {s.CycleTimeSeconds}초): {s.AvgQueue:F2}{mark}");
            }
            sb.AppendLine();

            sb.AppendLine($"판정: {BottleneckName}가 병목 공정");
            sb.AppendLine("근거: 관측 기간 평균 대기열이 가장 높음. 앞 공정 대비 처리 속도가 라인 처리량을 제약");

            return sb.ToString();
        }
    }

    // 공정 하나의 통계
    public class ProcessQueueStat
    {
        public string ProcessName { get; set; }
        public int CycleTimeSeconds { get; set; }
        public double AvgQueue { get; set; }        // 평균 대기열
    }
}
