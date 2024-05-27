using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Mo3RegUI.Tasks
{
    public class ForegroundLockTimeoutTaskParameter : ITaskParameter
    {
    }
    public class ForegroundLockTimeoutTask : ITask
    {
        public string Description => "Проверка длительности блокировки переднего плана";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is ForegroundLockTimeoutTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(ForegroundLockTimeoutTaskParameter p)
        {
            using var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            using var key = hkcu.OpenSubKey(@"Control Panel\Desktop", writable: true);
            object val = key?.GetValue("ForegroundLockTimeout");
            int valDword = val is null ? 0 : Convert.ToInt32(val, CultureInfo.InvariantCulture);
            int valDefault = 0x30d40;
            if (valDword >= valDefault)
            {
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = $"Блокировка переднего плана происходит не менее чем на {valDefault} миллисекунд. Никаких действий не требуется." });
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = $"Блокировка переднего плана состовляет {valDefault} миллисекунд, что может привести к возврату на рабочий стол в игре. Это исправимо ..." });
                key.SetValue("ForegroundLockTimeout", valDefault, RegistryValueKind.DWord);
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = $"Задержка успешно устранена. Время блокировки переднего плана изменено на {valDefault} миллисекунд." });
            }
        }

    }
}
