using Microsoft.Win32;
using System;
using System.Globalization;

namespace Mo3RegUI.Tasks
{
    public class XboxGameBarTaskParameter : ITaskParameter
    {
    }
    public class XboxGameBarTask : ITask
    {
        public string Description => "Проверка игровой панели Xbox";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is XboxGameBarTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(XboxGameBarTaskParameter p)
        {
            if (!(Environment.OSVersion.Version.Major >= 10))
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = "Версия Windows слишком стара для этой функции.",
                });
                return;
            }

            bool gameBarEnabled = IsGameBarEnabled();
            if (gameBarEnabled)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Warning,
                    Text = "Игровая панель включена, но некоторые области игры могут быть не кликабельны на некоторых компьютерах и при некоторых патчах рендеринга. В разделе Меню „Пуск“ → „Настройки“ → „Игры“ → „Xbox Game Bar“ найдите соответствующие настройки и отключите игровую панель.",
                });
            }

        }

        private static bool IsGameBarEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"System\GameConfigStore", false);
                object name = key?.GetValue("GameDVR_Enabled");
                return name != null && Convert.ToInt32(name, CultureInfo.InvariantCulture) == 1;

            }
            catch (Exception)
            {
            }
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR", false);
                object name = key?.GetValue("AppCaptureEnabled");
                return name != null && Convert.ToInt32(name, CultureInfo.InvariantCulture) == 1;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}

