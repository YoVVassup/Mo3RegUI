﻿using Microsoft.Win32;
using System;
using System.Globalization;

namespace Mo3RegUI.Tasks
{
    public class RuntimeComponentTaskParameter : ITaskParameter
    {
    }
    public class RuntimeComponentTask : ITask
    {
        public string Description => "Проверка компонентов среды системы";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is RuntimeComponentTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }

        private const int NET_FRAMEWORK_4_5_RELEASE_KEY = 378389;
        private const int NET_FRAMEWORK_4_5_1_RELEASE_KEY = 378675;
        private const int NET_FRAMEWORK_4_5_2_RELEASE_KEY = 379893;
        private const int NET_FRAMEWORK_4_6_RELEASE_KEY = 393295;
        private const int NET_FRAMEWORK_4_6_1_RELEASE_KEY = 394254;
        private const int NET_FRAMEWORK_4_6_2_RELEASE_KEY = 394802;
        private const int NET_FRAMEWORK_4_7_RELEASE_KEY = 460798;
        private const int NET_FRAMEWORK_4_7_1_RELEASE_KEY = 461308;
        private const int NET_FRAMEWORK_4_7_2_RELEASE_KEY = 461808;
        private const int NET_FRAMEWORK_4_8_RELEASE_KEY = 528040;
        private const int NET_FRAMEWORK_4_8_1_RELEASE_KEY = 533320;

        private void _DoWork(RuntimeComponentTaskParameter p)
        {
            // Future: Remove .NET 3.5 check for MO 3.3.7
            string net35 = GetInstalledNetFramework35VersionString();
            if (net35 is null)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = ".NET Framework 3.5 не установлен.", // don't treat this as an error
                });
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = ".NET Framework " + net35 + " установлен.",
                });
            }

            string net4 = GetInstalledNetFramework4VersionString();
            if (net4 is null)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Error,
                    Text = ".NET Framework 4 не установлен.",
                });
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = ".NET Framework " + net4 + " установлен.",
                });
            }

            int? net45plus = GetInstalledNetFramework45VersionNumber();
            bool isDotNet45Installed = net45plus.GetValueOrDefault() >= NET_FRAMEWORK_4_5_RELEASE_KEY;
            // Future: Remove .NET 4.5 check for MO 3.3.7
            if (!isDotNet45Installed && Environment.OSVersion.Version.Major < 6)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Error,
                    Text = "Текущий номер версии .NET Framework 4 не соответствует версии 4.5.",
                });
            }

            // Future: Always require .NET 4.7.1 for MO 3.3.7

            // Future: Always require .NET 7 for MO 3.3.7

            bool isXna4Installed = IsXNAFramework4Installed();
            if (isXna4Installed)
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = MessageLevel.Info,
                    Text = "XNA Framework 4.0 установлен.",
                });
            }
            else
            {
                ReportMessage(this, new TaskMessageEventArgs()
                {
                    Level = (Environment.OSVersion.Version.Major < 6) ? MessageLevel.Error : MessageLevel.Info, // Future: Always prompt as Info for MO 3.3.7
                    Text = "XNA Framework 4.0 не установлен.",
                });
            }

            if (Environment.OSVersion.Version.Major < 6) // Future: Remove this part for MO 3.3.7
            {
                if (!isXna4Installed && !isDotNet45Installed)
                {
                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Error,
                        Text = "Текущий номер версии .NET Framework 4 не соответствует версии 4.5, а XNA Framework 4.0 не установлен. Клиент может работать некорректно.",
                    });
                }
            }
        }

        private static int? GetInstalledNetFramework45VersionNumber()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", false);
                object installValue = key?.GetValue("Release");
                return installValue != null ? (int)installValue : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static string GetInstalledNetFramework4VersionString()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", false);
                object versionValue = key?.GetValue("Version");
                return versionValue == null ? null : versionValue as string;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static string GetInstalledNetFramework35VersionString()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", false);
                object name = key?.GetValue("Version");
                return name == null ? null : name as string;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static bool IsXNAFramework4Installed()
        {
            try
            {
                var HKLM_32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                var xnaKey = HKLM_32.OpenSubKey(@"SOFTWARE\Microsoft\XNA\Framework\v4.0");
                object installValue = xnaKey.GetValue("Installed");
                return installValue is not null && Convert.ToInt32(installValue, CultureInfo.InvariantCulture) == 1;
            }
            catch
            {
                return false;
            }

        }
    }
}
