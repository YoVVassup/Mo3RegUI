using Microsoft.VisualBasic.Devices;
using System;

namespace Mo3RegUI.Tasks
{
    public class BasicInfoTaskParameter : ITaskParameter
    {
    }
    public class BasicInfoTask : ITask
    {
        public string Description => "Проверка основной информации о системе";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is BasicInfoTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(BasicInfoTaskParameter p)
        {
            var computerInfo = new ComputerInfo();
            uint codepage = NativeMethods.GetACP();

            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = "Операционная система: " + computerInfo.OSFullName + " " + computerInfo.OSVersion
            });
            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = "Текущая кодировка ANSI: " + codepage.ToString()
            });
            ReportMessage(this, new TaskMessageEventArgs()
            {
                Level = MessageLevel.Info,
                Text = string.Format(
                    "Оперативная память: всего: {0:0.##} GB，доступно: {1:0.##} GB",
                    ((double)computerInfo.TotalPhysicalMemory) / 1024 / 1024 / 1024,
                    ((double)computerInfo.AvailablePhysicalMemory) / 1024 / 1024 / 1024),
            });

        }
    }
}
