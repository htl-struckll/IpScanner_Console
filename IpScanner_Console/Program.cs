using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace IpScanner_Console
{
    class Program
    {
        private static CountdownEvent _countdown;
        static int _upCount;
        private static readonly object LockObj = new object();

        static void Main()
        {
            _countdown = new CountdownEvent(1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //string ipBase = "192.168.195."; //todo Change for wpf application (custom user input)
            string ipBase = "";
            for (int i = 1; i < 255; i++)
            {
                string ip = ipBase + i;
                Ping p = new Ping();
                p.PingCompleted += PingCompleted;
                _countdown.AddCount();
                p.SendAsync(ip, 100000, ip);
            }
            _countdown.Signal();
            _countdown.Wait();
            sw.Stop();
            Console.WriteLine("Took {0} milliseconds. {1} hosts active.", sw.ElapsedMilliseconds, _upCount);
            Console.ReadLine();
        }
        
        /// <summary>
        /// When the ping was complete it gets handeled here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                string name;
                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                    name = hostEntry.HostName;
                }
                catch (SocketException ex)
                {
                    name = "?";
                }

                Console.WriteLine("{0} ({1}) is up: ({2} ms)", ip, name, e.Reply.RoundtripTime);
                lock (LockObj)
                {
                    _upCount++;
                }
            }
            else if (e.Reply == null)
            {
                Console.WriteLine("Pinging {0} failed. (Null Reply object?)", ip);
            }
            _countdown.Signal();
        }
    }
}
