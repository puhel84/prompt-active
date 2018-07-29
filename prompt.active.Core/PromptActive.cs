using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace prompt.active.Core
{
    public static class PromptActive
    {
        #region Properties
        public static PromptTheme Theme { get; set; }
        internal static bool isRunning = true;
        #endregion

        #region Constructor
        static PromptActive()
        {
            switch (Console.BackgroundColor)
            {
                case ConsoleColor.Black:
                case ConsoleColor.Blue:
                case ConsoleColor.DarkBlue:
                case ConsoleColor.DarkCyan:
                case ConsoleColor.DarkGray:
                case ConsoleColor.DarkGreen:
                case ConsoleColor.DarkMagenta:
                case ConsoleColor.DarkRed:
                    Theme = PromptTheme.DarkTheme;
                    break;
                default:
                    Theme = PromptTheme.LightTheme;
                    break;
            }
        }
        #endregion

        #region Public Methods
        public static void Start<T>(string yourPrompt = ">>") where T : BasePromptApp, new()
        {
            // Setup
            var yourApp = new T();
            Setup(yourApp);

            // For dispose
            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            Task.Run(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        // run prompt
                        var (cmd, args) = PromptHelper.Run(yourPrompt, yourApp);
                        // Delegate
                        var param = new List<object>();
                        if (!yourApp.CommandList.ContainsKey(cmd)) continue;
                        var action = yourApp.CommandList[cmd];
                        // validation
                        var actionParms = action.GetParameters();
                        try
                        {
                            for (int i = 0; i < actionParms?.Length; i++)
                            {
                                var t = actionParms[i].ParameterType;
                                if (t.IsEnum)
                                {
                                    param.Add(Enum.Parse(t, args[i + 1]));
                                }
                                else
                                {
                                    param.Add(Convert.ChangeType(args[i + 1], actionParms[i].ParameterType));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = Theme.ErrorColor;
                            Console.Write(" Wrong command.");
                            continue;
                        }
                        action.Invoke(yourApp, param.ToArray());
                    }
                    catch (InvalidOperationException)
                    {
                        isRunning = false;
                    }
                    catch (OperationCanceledException)
                    {
                        // Continue
                    }
                    catch (OverflowException)
                    {
                        // Continue
                    }
                }

                exitEvent.Set();
            });

            exitEvent.WaitOne();
            isRunning = false;
            yourApp.Dispose();
        }
        #endregion

        #region Private Methods
        //private static void Setup<T>(T app) where T : BasePromptApp
        private static void Setup(BasePromptApp yourApp)
        {
            // 1) Create CommandList
            var kvm = yourApp.GetType().GetMethods()
                ?.Where(x => x.GetCustomAttribute<PromptCommandAttribute>() != null)
                ?.Select(x => new KeyValuePair<string, MethodInfo>(x.Name, x));
            if (kvm != null)
            {
                foreach (var kv in kvm)
                {
                    yourApp.AutoCompleteList[""][0].Add(kv.Key);
                    yourApp.CommandList.Add(kv.Key, kv.Value);
                }
            }
            // 2) Create HintList
            foreach (var item in yourApp.CommandList)
            {
                // 2-1) Description list
                var description = item.Value.GetCustomAttribute<DescriptionAttribute>()?.Description;
                description = string.IsNullOrEmpty(description) ? "Not defined." : description;
                yourApp.DescriptionList.Add(item.Key, description);
                // 2-2) Custim hint available
                var customhint = item.Value.GetCustomAttribute<CustomHintAttribute>()?.Hint;
                if (!string.IsNullOrEmpty(customhint))
                {
                    yourApp.HintList.Add(item.Key, customhint);
                    continue;
                }
                // 2-2) Hint list
                var args = item.Value.GetParameters();
                var sb = "";
                if (args != null)
                {
                    for (int i = 0; i < args.Count(); i++)
                    {
                        var type = args[i].ParameterType;
                        var type_s = "";
                        if (type.IsEnum)
                        {
                            // enum
                            // Autocomplete add
                            var enumNames = type.GetEnumNames().ToList();
                            if (!yourApp.AutoCompleteList.ContainsKey(item.Key)) yourApp.AutoCompleteList.Add(item.Key, new Dictionary<int, List<string>>());
                            yourApp.AutoCompleteList[item.Key][i] = enumNames;

                            foreach (var e in enumNames)
                            {
                                type_s = $"{type_s}|{e}";
                            }
                            type_s = new string(type_s.Skip(1).ToArray());
                        }
                        else
                        {
                            type_s = type.Name;
                        }

                        if (args[i].IsOptional)
                        {
                            // optional
                            sb = $"{sb}[{args[i].Name}<{type_s}>] ";
                        }
                        else
                        {
                            sb = $"{sb}{args[i].Name}<{type_s}> ";
                        }
                    }
                }
                yourApp.HintList.Add(item.Key, sb);
            }
            // 3) Create Autocomplete list
            var kvp = yourApp.GetType().GetProperties();
            foreach (var item in kvp)
            {
                var ps = item.GetCustomAttributes<AutocompleteAttribute>();
                foreach (var p in ps)
                {
                    if (!yourApp.AutoCompleteList.ContainsKey(p.Function)) yourApp.AutoCompleteList.Add(p.Function, new Dictionary<int, List<string>>());
                    yourApp.AutoCompleteList[p.Function][p.Order] = item.GetValue(yourApp) as List<string>;
                }
            }
        }
        #endregion
    }
}
