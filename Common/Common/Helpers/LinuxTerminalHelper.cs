using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public class LinuxTerminalHelper
    {
        public LinuxTerminalHelper(string process_name)
        {
            InitializeProcess(process_name);
        }

        private Process _process_bash;
        private void InitializeProcess(string process_name)
        {
            _process_bash = new Process();
            _process_bash.StartInfo.FileName = process_name;
            _process_bash.StartInfo.UseShellExecute = false;
            _process_bash.StartInfo.RedirectStandardInput = true;
            _process_bash.StartInfo.RedirectStandardOutput = true;
            _process_bash.StartInfo.RedirectStandardError = true;
            _process_bash.StartInfo.CreateNoWindow = true;
        }

        public StreamReader ExecuteCommand(string terminalInput)
        {
            _process_bash.Start();
            _process_bash.StandardInput.WriteLine(terminalInput);
            var commandLineOutput = _process_bash.StandardOutput;
            _process_bash.Close();
            return commandLineOutput;
        }
    }
}
