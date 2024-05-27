using Microsoft.Win32;
using System;
using System.Linq;

namespace Mo3RegUI.Tasks
{
    public class DDrawDLLTaskParameter : ITaskParameter
    {
    }
    public class DDrawDLLTask : ITask
    {
        public string Description => "Проверка реестра на наличие патча рендеринга";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is DDrawDLLTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }

        private void _DoWork(DDrawDLLTaskParameter p)
        {
            const string dllName = "ddraw.dll";
            using (var registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\KnownDLLs", false))
            {
                object dllItem = registryKey.GetValue(dllName);
                if (dllItem is not null)
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = $"{dllName} удалена из реестра. {dllName} не должна присутствовать в составе KnownDLLs." });
                }
                else
                {
                    ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = $"{dllName} записи в реестре соответствуют правилам." });
                }
            }
            using (var registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager", true))
            {
                const string keyName = "ExcludeFromKnownDlls";
                object exclusiveDlls = registryKey.GetValue(keyName);
                if (exclusiveDlls is null)
                {
                    registryKey.SetValue(keyName, new string[] { dllName }, RegistryValueKind.MultiString);
                    return;
                }

                var exclusiveDllsKind = registryKey.GetValueKind("ExcludeFromKnownDlls");
                if (exclusiveDllsKind != RegistryValueKind.MultiString)
                {
                    throw new Exception($"ExcludeFromKnownDlls должен иметь тип {RegistryValueKind.MultiString} который, на самом деле, является {exclusiveDllsKind}.");
                    //string message = $"ExcludeFromKnownDlls 应为 {RegistryValueKind.MultiString} 类型，实际为 {exclusiveDllsKind}。";
                    //ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Critical, Text = message });
                    //return; // throw new Exception(message);
                }

                var exclusiveDllsArray = (exclusiveDlls as string[]).ToList();
                if (!exclusiveDllsArray.Contains(dllName))
                {
                    exclusiveDllsArray.Add(dllName);
                }

                registryKey.SetValue(keyName, exclusiveDllsArray.ToArray(), RegistryValueKind.MultiString);
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Info, Text = $"Успешно добавлена {dllName} в ExcludeFromKnownDlls." });
            }
        }
    }
}
