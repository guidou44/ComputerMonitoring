using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public class CommandLineHelper
    {
        private Process _process_cmd;
        public CommandLineHelper()
        {
            InitializeProcess();
        }

        private void InitializeProcess()
        {
            _process_cmd = new Process();
            _process_cmd.StartInfo.FileName = "cmd.exe";
            _process_cmd.StartInfo.UseShellExecute = false;
            _process_cmd.StartInfo.RedirectStandardInput = true;
            _process_cmd.StartInfo.RedirectStandardOutput = true;
            _process_cmd.StartInfo.RedirectStandardError = true;
            _process_cmd.StartInfo.CreateNoWindow = true;
        }

        public virtual StreamReader ExecuteCommand(string commandLineInput)
        {
            _process_cmd.Start();
            _process_cmd.StandardInput.WriteLine(commandLineInput);
            _process_cmd.StandardInput.WriteLine("exit");
            var commandLineOutput = _process_cmd.StandardOutput;
            _process_cmd.Close();
            return commandLineOutput;
        }
    }
}
