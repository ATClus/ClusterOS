using System;

namespace WinTracker.Models
{
    public class ActiveWindowEventArgsModel : EventArgs
    {
        public int ProcessId { get; }
        public string ProcessName { get; }

        public ActiveWindowEventArgsModel(int processId, string processName)
        {
            ProcessId = processId;
            ProcessName = processName;
        }
    }
}
