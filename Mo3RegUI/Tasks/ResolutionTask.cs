﻿using System;
using System.Globalization;
using System.IO;

namespace Mo3RegUI.Tasks
{

    public class ResolutionTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class ResolutionTask : ITask
    {
        public string Description => "Установка разрешения";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is ResolutionTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(ResolutionTaskParameter p)
        {
            var Resolution = GetHostingScreenSize();
            lock (Locks.RA2MO_INI)
            {
                MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, Constants.GameConfigIniName), ini =>
                {
                    var videoSection = MyIniParserHelper.GetSectionOrNew(ini, "Video");
                    videoSection["ScreenWidth"] = Resolution.Width.ToString(CultureInfo.InvariantCulture);
                    videoSection["ScreenHeight"] = Resolution.Height.ToString(CultureInfo.InvariantCulture);
                });
            }
            ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = "Установка разрешение игры на " + Resolution.Width.ToString() + "×" + Resolution.Height.ToString() + "." });
        }

        private struct Resolution
        {
            public int Width;
            public int Height;
        }

        private static Resolution GetHostingScreenSize()
        {
            // Note: declare DPI awareness in the manifest file, otherwise the result is wrong
            int monitor_width = NativeMethods.GetSystemMetrics(NativeConstants.SM_CXSCREEN);
            int monitor_height = NativeMethods.GetSystemMetrics(NativeConstants.SM_CYSCREEN);

            return new Resolution() { Width = monitor_width, Height = monitor_height };
        }
    }
}
