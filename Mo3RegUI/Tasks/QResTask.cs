using System;
using System.IO;
using System.Linq;

namespace Mo3RegUI.Tasks
{
    public class QResTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class QResTask : ITask
    {
        public string Description => "Проверка проблем масштабирования QRes с высоким DPI";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is QResTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(QResTaskParameter p)
        {
            string QResPath = Path.Combine(p.GameDir, "qres.dat");
            byte[] QResOldVersionSha2 = HexToByteArray("D9BB2BFA4A3F1FADA6514E1AE7741439C3B85530F519BBABC03B4557B5879138");
            using var hash = System.Security.Cryptography.SHA256.Create();
            try
            {
                using var file = new FileStream(QResPath, FileMode.Open);
                byte[] digest = hash.ComputeHash(file);
                if (digest.SequenceEqual(QResOldVersionSha2))
                {
                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Warning,
                        Text = "Обнаружен неисправленный QRes, запуск игры в оконном режиме на мониторе с высоким DPI может привести к ошибке разрешения или ошибке «Screen mode not found», если не используется патч рендеринга или используется старый патч рендеринга. Рекомендуется обновить файл qres.dat или использовать современный патч рендеринга, если это возможно.",
                    });

                }
            }
            catch (Exception ex)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    Text = "Проверка QRes не удалась. " + ex.Message + " Запуск игры в оконном режиме может вызвать проблемы, если не используется патч рендеринга или используется древний патч рендеринга. Рекомендуется обновить файл qres.dat или использовать современный патч рендеринга, если это возможно.",
                });

            }
        }
        private static byte[] HexToByteArray(string hex) => Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();

    }
}
