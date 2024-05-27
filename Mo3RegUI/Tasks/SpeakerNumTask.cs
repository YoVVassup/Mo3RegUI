using System;

namespace Mo3RegUI.Tasks
{
    public class SpeakerNumTaskParameter : ITaskParameter
    {
    }
    public class SpeakerNumTask : ITask
    {
        public string Description => "Проверка устройства вывода звука";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is SpeakerNumTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(SpeakerNumTaskParameter p)
        {
            uint num = NativeMethods.waveOutGetNumDevs();
            if (num == 0)
            {
                throw new Exception("В текущей системе отсутствует устройство вывода звука. Пожалуйста, подключите наушники или колонки перед входом в игру, иначе игра может завершиться аварийно.");
            }
            ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = $"Найдено устройств вывода звука: {num}" });

        }
    }
}
