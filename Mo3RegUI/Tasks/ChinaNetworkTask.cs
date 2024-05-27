using System;
using System.Globalization;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class ChinaNetworkTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class ChinaNetworkTask : ITask
    {
        public string Description => "Отключение использования Discord";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is ChinaNetworkTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(ChinaNetworkTaskParameter p)
        {
            if (RegionInfo.CurrentRegion.ThreeLetterISORegionName == "RU")
            {
                lock (Locks.RA2MO_INI)
                {
                    MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, Constants.GameConfigIniName), ini =>
                    {
                        var section = MyIniParserHelper.GetSectionOrNew(ini, "MultiPlayer");
                        section["DiscordIntegration"] = "False";
                    });
                }
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = "Discord успешно отключен.",
                });
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = "Не удалось отключить Discord.",
                });
            }
        }
    }
}
