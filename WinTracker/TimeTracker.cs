using System;
using System.Collections.Generic;
using WinTracker.Models;

namespace WinTracker
{
    public class TimeTracker : IDisposable
    {
        private ActiveWindowTracker _activeWindowTracker;
        private TimeRecordModel _currentRecord;
        private List<TimeRecordModel> _records;

        public IReadOnlyList<TimeRecordModel> Records => _records.AsReadOnly();

        public TimeTracker()
        {
            _records = new List<TimeRecordModel>();
            _activeWindowTracker = new ActiveWindowTracker();
            _activeWindowTracker.ActiveWindowChanged += OnActiveWindowChanged;
        }

        private void OnActiveWindowChanged(object sender, ActiveWindowEventArgsModel e)
        {
            DateTime now = DateTime.Now;

            if (_currentRecord != null)
            {
                _currentRecord.Duration = now - _currentRecord.StartTime;
                _records.Add(_currentRecord);
            }

            if (e.ProcessId != 0)
            {
                _currentRecord = new TimeRecordModel
                {
                    ProcessId = e.ProcessId,
                    ProcessName = e.ProcessName,
                    StartTime = now
                };
            }
            else
            {
                _currentRecord = null;
            }
        }

        public void Dispose()
        {
            if (_activeWindowTracker != null)
            {
                _activeWindowTracker.ActiveWindowChanged -= OnActiveWindowChanged;
                _activeWindowTracker.Dispose();
                _activeWindowTracker = null;
            }
        }
    }
}
