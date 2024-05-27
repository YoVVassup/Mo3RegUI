using System;

namespace Mo3RegUI.Tasks
{

    public class EncodingCheckTaskParameter : ITaskParameter
    {
    }
    public class EncodingCheckTask : ITask
    {
        public string Description => "Проверка кодировки ANSI системы";

        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is EncodingCheckTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(EncodingCheckTaskParameter p)
        {
            uint codepage = NativeMethods.GetACP();
            if (codepage == 65001)
            {
                string message = "Текущая кодировка ANSI - UTF-8. Это хороший вариант, но, к сожалению, неанглийские символы не будут правильно набираться в Mental Omega, и такие компоненты, как редактор карт, не смогут полностью отображать имена, содержащие неанглийские символы. Кроме того, это один из факторов, влияющих на то, будет ли интерфейс меню игры иметь искажения и наложение кнопок при высоком DPI.";
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = message });
            }
        }
    }
}
