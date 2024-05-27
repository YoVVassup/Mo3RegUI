using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace Mo3RegUI.Tasks
{
    public class NetworkInterfaceTaskParameter : ITaskParameter
    {
    }
    public class NetworkInterfaceTask : ITask
    {
        public string Description => "Проверка сетевого окружения";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is NetworkInterfaceTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }

        private void _DoWork(NetworkInterfaceTaskParameter p)
        {
            var interfaceIPv4s = new List<Tuple<string, List<System.Net.IPAddress>>>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var IPv4s = new List<System.Net.IPAddress>();
                if (ni.NetworkInterfaceType is NetworkInterfaceType.Wireless80211 or NetworkInterfaceType.Ethernet)
                {

                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            IPv4s.Add(ip.Address);
                        }
                    }
                }
                if (IPv4s.Count > 0)
                {
                    interfaceIPv4s.Add(new Tuple<string, List<System.Net.IPAddress>>(ni.Name, IPv4s));
                }
            }

            if (interfaceIPv4s.Count > 1)
            {
                var ips = new StringBuilder();
                foreach (var kv in interfaceIPv4s)
                {
                    string interfaceName = kv.Item1;
                    var addresses = kv.Item2;
                    foreach (var addr in addresses)
                    {
                        _ = ips.Append("\n" + interfaceName + " --- " + addr.ToString());
                    }
                }

                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = "На вашем компьютере установлено несколько сетевых карт, перечисленных ниже. Вы можете не видеть других игроков в сетевом лобби в LAN. Чтобы обойти эту проблему, временно отключите другие сетевые карты при подключении к LAN и оставьте только сетевую карту, подключенную к LAN." + ips });
            }
        }
    }
}
