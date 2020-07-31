
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using DesktopAssistant.BL.ProcessWatch;

namespace ProcessMonitoring.Models
{
    public class ProcessWatch : IProcessWatch
    {
        private IProcessReporter _reporter;
        private bool _isRunning;
        private Process _process;
        
        public ProcessWatch(string processName, bool capturePacketExchange, Process process = null)
        {
            ProcessName = processName;
            DoCapture = capturePacketExchange;
            Process = process;
            IsRunning = process != null;
        }

        public void RegisterReporter(IProcessReporter reporter)
        {
            _reporter = reporter;
        }

        public bool DoCapture { get; set; }

        public bool IsRunning
        {
            get { return _isRunning; }
            set 
            {
                if (!_isRunning && value) 
                    _reporter?.ReportProcess(ProcessName);
                _isRunning = value;
            }
        }

        public Process Process
        {
            get { return _process;}
            set
            {
                _process = value;
                IsRunning = !(_process is null);
            }
        }
        
        public string ProcessName { get; }
        
        public byte Data { get; set; }
        public IEnumerable<int> Ports { get; set; }        
        
    }
}