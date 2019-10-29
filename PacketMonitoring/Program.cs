using SharpPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using PacketDotNet;

namespace PacketMonitoring
{
    public class ProcessPerformanceInfo
    {
        public ProcessPerformanceInfo(int pid)
        {
            ProcessID = pid;
        }
        public int ProcessID { get; set; }
        public long NetSendBytes { get; set; }
        public long NetRecvBytes { get; set; }
        public long NetTotalBytes { get; set; }

    }

    class Program
    {
        static ProcessPerformanceInfo ProcInfo;
        static void Main(string[] args)
        {
            Process[] allprocesses = Process.GetProcesses();
            int process_toWatch_id = allprocesses.Where(P => P.ProcessName == "USBHelperLauncher").SingleOrDefault().Id;

            //int process_toWatch_id = allprocesses.Where(P => P.ProcessName == "chrome").FirstOrDefault().Id;
            ProcInfo = new ProcessPerformanceInfo(process_toWatch_id);




            #region get TCP/UDP ports 
            // Execute cmd : netstat -a, get the results that are TCP and UDP and corresponding to process ID

            Process process_cmd = new Process();
            process_cmd.StartInfo.FileName = "cmd.exe";
            process_cmd.StartInfo.UseShellExecute = false;
            process_cmd.StartInfo.RedirectStandardInput = true;
            process_cmd.StartInfo.RedirectStandardOutput = true;
            process_cmd.StartInfo.RedirectStandardError = true;
            process_cmd.StartInfo.CreateNoWindow = true;
            process_cmd.Start();
            process_cmd.StandardInput.WriteLine("netstat -ano");
            process_cmd.StandardInput.WriteLine("exit");


            Regex reg = new Regex("\\s+", RegexOptions.Compiled);
            string line = null;
            


            int pid = ProcInfo.ProcessID;
            List<int> ports = new List<int>();
            ports.Clear();

            while ((line = process_cmd.StandardOutput.ReadLine()) != null) //parsing command line output
            {
                line = line.Trim();
                if (line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
                {
                    line = reg.Replace(line, ",");
                    string[] arr = line.Split(',');
                    if (arr[4] == pid.ToString()) //if one of the processes corresponds to our process Id of interrest
                    {
                        string socket = arr[1]; //Socket == (Local_address:TCP_port, ex: 0.0.0.0:135)
                        int pos = socket.LastIndexOf(':');
                        int port = int.Parse(socket.Substring(pos + 1));
                        ports.Add(port);
                    }
                }
                else if (line.StartsWith("UDP", StringComparison.OrdinalIgnoreCase))
                {
                    line = reg.Replace(line, ",");
                    string[] arr = line.Split(',');
                    if (arr[3] == pid.ToString())
                    {
                        string socket = arr[1];
                        int pos = socket.LastIndexOf(':');
                        int port = int.Parse(socket.Substring(pos + 1));
                        ports.Add(port);
                    }
                }
            }
            process_cmd.Close();

            #endregion

            //get ip address of local computer
            IPAddress[] addrList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            string IP = addrList[3].ToString();
            //int pos_prcent = IP.LastIndexOf("%");
            //IP = IP.Replace(IP.Substring(pos_prcent), "");

            //var devices = SharpPcap.WinPcap.WinPcapDeviceList.Instance;
            var devices = CaptureDeviceList.Instance.Where(CD => !CD.Description.Contains("Atheros")); ;

            // for all devices found, check for all packets exchange between ports listed and local ip

            int count = devices.Count();
            if (count < 1)
            {
                Console.WriteLine("No device found on this machine");
                return;
            }

            var device = CaptureDeviceList.New()[0];

            foreach (var port in ports)
            {
                CaptureFlowRecv(IP, port, device);
                CaptureFlowSend(IP, port, device);
            }



            while (true)
            {
                Console.WriteLine("proc NetTotalBytes : " + ProcInfo.NetTotalBytes);
                Console.WriteLine("proc NetSendBytes : " + ProcInfo.NetSendBytes);
                Console.WriteLine("proc NetRecvBytes : " + ProcInfo.NetRecvBytes);

                //Call refresh function every 1s 
                RefershInfo();
            }

        }

        private static void CaptureFlowSend(string IP, int portID, ICaptureDevice device)
        {


            device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrivalSend);
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
            string filter = "src host " + IP + " and src port " + portID;
            device.Filter = filter;
            device.StartCapture();

        }

        private static void device_OnPacketArrivalSend(object sender, CaptureEventArgs e)
        {
            //DateTime time = e.Packet.Timeval.Date;
            //int len = e.Packet.Data.Length;
            //Console.WriteLine("{0}:{1}:{2},{3} Len={4}",
            //    time.Hour, time.Minute, time.Second, time.Millisecond, len);
            var len = e.Packet.Data.Length;
            ProcInfo.NetSendBytes += len;
        }

        private static void CaptureFlowRecv(string IP, int portID, ICaptureDevice device)
        {
            device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrivalRecv);

            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

            string filter = "dst host " + IP + " and dst port " + portID;
            device.Filter = filter;
            device.StartCapture();

        }
        private static void device_OnPacketArrivalRecv(object sender, CaptureEventArgs e)
        {
            var len = e.Packet.Data.Length;
            ProcInfo.NetRecvBytes += len;
        }
        public static void RefershInfo()
        {
            ProcInfo.NetRecvBytes = 0;
            ProcInfo.NetSendBytes = 0;
            ProcInfo.NetTotalBytes = 0;
            Thread.Sleep(1000);
            ProcInfo.NetTotalBytes = ProcInfo.NetRecvBytes + ProcInfo.NetSendBytes;
        }

    }
}
