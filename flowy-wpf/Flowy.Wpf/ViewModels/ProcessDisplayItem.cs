using Flowy.Core.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flowy.Wpf.ViewModels
{
    // 화면 표시용 래퍼.
    // WorkProcess를 그대로 바인딩하지 않고, "화면에 보여줄 형태"로 한 번 가공해서 View에 넘기는 역할.
    public class ProcessDisplayItem
    {
        public string ProcessName { get; }
        public string StatusText {  get; }

        public ProcessDisplayItem(WorkProcess process)
        {
            ProcessName = process.ProcessName;
            StatusText = process.StateMachine.CurrentStateType switch
            { 
                ProcessStateType.Idle => "대기",
                ProcessStateType.Running => "가동",
                ProcessStateType.Error => "이상",
                ProcessStateType.Stopped => "정지",
                _ => "알 수 없음"
            };
        }
    }
}
