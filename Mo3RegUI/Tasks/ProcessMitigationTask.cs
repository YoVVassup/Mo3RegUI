using Mo3RegUI.Tasks;
using System;

namespace Mo3RegUI
{
    public class ProcessMitigationTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class ProcessMitigationTask : ITask
    {
        public string Description => "Отключение принудительного случайного распределения для образов";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is ProcessMitigationTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(ProcessMitigationTaskParameter p)
        {
            if (!(Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 16299))
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = "Версия Windows слишком стара для этой опции.",
                });
                return;
            }

            bool hasASLRTurnedOffForGamemd = false;
            {
                ConsoleCommandManager.RunConsoleCommand("powershell.exe", $"-Command \"Set-ProcessMitigation -Name {Constants.GameExeName} -Disable ForceRelocateImages\"", out int exitCode, out string stdOut, out string stdErr);

                if (!string.IsNullOrWhiteSpace(stdOut))
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = stdOut.Trim() });
                }

                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = stdErr.Trim() });
                }

                if (exitCode != 0)
                {
                    //ReportMessage(this, new TaskMessage()
                    //{
                    //    Level = MessageLevel.Warning,
                    //    Text = "尝试为 gamemd.exe 关闭强制映像虚拟化失败。",
                    //});
                }
                else
                {
                    //ReportMessage(this, new TaskMessage()
                    //{
                    //    Level = MessageLevel.Info,
                    //    Text = "成功为 gamemd.exe 关闭了强制映像虚拟化。",
                    //});
                    hasASLRTurnedOffForGamemd = true;
                }
            }

            {
                ConsoleCommandManager.RunConsoleCommand("powershell.exe", "-Command \"((Get-ProcessMitigation -System).ASLR.ForceRelocateImages -eq [Microsoft.Samples.PowerShell.Commands.OPTIONVALUE]::ON) -as [int]\"",
                      out int exitCode, out string stdOut, out string stdErr);

                // don't display stdOut

                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = stdErr.Trim() });
                }

                if (exitCode != 0)
                {
                    string message = $"Процесс возвращает значение {exitCode}. Выполнение не удалось.";
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Error, Text = message });
                }

                bool stdOutIsNumeric = int.TryParse(stdOut.Trim(), out int stdOutInt);
                if (stdOutIsNumeric && stdOutInt == 1)
                {
                    if (hasASLRTurnedOffForGamemd)
                    {
                        ReportMessage(this, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Info,
                            Text = $"Принудительное случайное распределение для образов (обязательный ASLR) включено по умолчанию, но был успешно отключено для {Constants.GameExeName}. ",
                        });
                    }
                    else
                    {
                        ReportMessage(this, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Warning,
                            Text = $"Принудительное случайное распределение для образов (обязательный ASLR) включено по умолчанию, и его отключение для {Constants.GameExeName} не удалось. Это может привести к тому, что Ares не сможет загрузиться должным образом. Найдите и отключите «Принудительное случайное распределение для образов (обязательный ASLR) на вкладке «Системные параметры» в разделе «Безопасность Windows» → «Управление приложениями/браузером» или отключите эту опцию отдельно для файлов игры на вкладке «Параметры программы».",
                        });
                    }
                }
                else if (stdOutIsNumeric && stdOutInt == 0)
                {
                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Info,
                        Text = "Принудительное случайное распределение для образов (обязательный ASLR) отключено по умолчанию.",
                    });
                }
                else
                {
                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Warning,
                        Text = "Не удалось распознать числовое значение " + stdOutIsNumeric + ". Невозможно определить состояние опции.",
                    });
                }
            }

        }
    }
}
