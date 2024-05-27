using System;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class FalsePositiveTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class FalsePositiveTask : ITask
    {
        public string Description => "Проверка файлов игры (грубая)";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is FalsePositiveTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(FalsePositiveTaskParameter p)
        {
            foreach (string avExe in Constants.VulnerableAvExes)
            {
                string avExeReplaced = avExe.Replace('/', Path.DirectorySeparatorChar);
                if (!File.Exists(Path.Combine(p.GameDir, avExeReplaced)))
                {
                    throw new Exception($"Файлы игры неполные. Файл {avExeReplaced} не удалось найти. Пожалуйста, проверьте журналы антивируса, добавьте каталог игры в белый список антивируса (или используйте другой антивирус), а затем переустановите игру.");
                }
            }
        }
    }
}
