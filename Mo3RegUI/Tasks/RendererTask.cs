using System;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class RendererTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class RendererTask : ITask
    {
        public string Description => "Настройка патча рендеринга";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is RendererTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(RendererTaskParameter p)
        {
            if (Environment.OSVersion.Version.Major >= 7 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2))
            {
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "Установка патча рендеринга CnC-DDraw." });

                // Set "singlecpu=false" to support multi-core. Renderer should not determine the affinity but CnC-DDraw did. So the option is turned off in this task.
                lock (Locks.CnC_DDraw_INI)
                {
                    MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, "Resources", Constants.CnCDDrawIniName), ini =>
                    {
                        var section = MyIniParserHelper.GetSectionOrNew(ini, "ddraw");
                        section["singlecpu"] = "false";
                    });
                }

                // Apply CnC-DDraw
                bool success = true;
                try
                {
                    File.Copy(Path.Combine(p.GameDir, "Resources", Constants.CnCDDrawDllName), Path.Combine(p.GameDir, "ddraw.dll"), true);
                    File.Copy(Path.Combine(p.GameDir, "Resources", Constants.CnCDDrawIniName), Path.Combine(p.GameDir, "ddraw.ini"), true);
                }
                catch (Exception ex)
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = "Проблема с установкой патча рендеринга." + ex.Message });
                    success = false;
                }
                if (success)
                {
                    lock (Locks.RA2MO_INI)
                    {
                        MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, Constants.GameConfigIniName), ini =>
                        {
                            var section = MyIniParserHelper.GetSectionOrNew(ini, "Compatibility");
                            section["Renderer"] = "CnC_DDraw";
                        });
                    }
                }

            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "Патч рендеринга не установлен." });
            }

            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = $"Совет: При необходимости настройки патча рендеринга можно изменить из клиента {Constants.GameName}. \n На системах Windows 8/10/11 рекомендуется всегда использовать современные патчи рендеринга, такие как TS-DDraw, CnC-DDraw."
            });

        }
    }
}
