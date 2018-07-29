using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace prompt.active.Core
{
    internal static class PromptHelper
    {
        private static int CurLeft = 0;
        private static int CurTop = 0;
        private static ConsoleKeyInfo lastKey = new ConsoleKeyInfo();
        private static readonly List<List<char>> inputHistory = new List<List<char>>();

        private static void ReWrite(int left, List<char> input)
        {
            if (left >= 0)
            {
                Console.ForegroundColor = PromptActive.Theme.CommandColor;
                Console.SetCursorPosition(left, Console.CursorTop);
                Console.Write(string.Concat(input.Concat(new string(' ', Console.BufferWidth - left - input.Count))));
                Console.SetCursorPosition(CurLeft, CurTop);
            }
        }

        internal static (string cmd, List<string> args) Run(string yourPrompt, BasePromptApp yourApp)
        {
            // for result
            var cmd = "";
            var args = new List<string>();

            // for history
            var inputHistoryPosition = inputHistory.Count;

            // for auto completer
            var argOrder = 0;
            var completeTarget = "";
            IEnumerator<string> matchedList = null;

            // for input
            Console.ForegroundColor = PromptActive.Theme.PromptColor;
            Console.Write($"{Environment.NewLine}{yourPrompt}");
            var input = new List<char>();
            var inputPosition = 0;

            CurLeft = Console.CursorLeft + yourPrompt.Length;
            CurTop = Console.CursorTop;
            int startLeft = CurLeft;

            ReWrite(startLeft, input);
            Console.SetCursorPosition(startLeft, CurTop);

            var key = new ConsoleKeyInfo();
            var isRunning = true;
            while (isRunning)
            {
                key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (CurLeft - 1 >= startLeft)
                        {
                            inputPosition--;
                            CurLeft--;
                            Console.SetCursorPosition(CurLeft, CurTop);
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (inputPosition + 1 <= input.Count)
                        {
                            inputPosition++;
                            CurLeft++;
                            Console.SetCursorPosition(CurLeft, CurTop);
                        }
                        break;
                    case ConsoleKey.Backspace:
                        if (CurLeft - 1 >= startLeft)
                        {
                            inputPosition--;
                            CurLeft--;
                            input.RemoveAt(inputPosition);

                            ReWrite(startLeft, input);
                        }
                        break;
                    case ConsoleKey.Delete:
                        if (CurLeft - startLeft < input.Count)
                        {
                            input.RemoveAt(CurLeft - startLeft);

                            ReWrite(startLeft, input);
                        }
                        break;
                    case ConsoleKey.Home:
                        {
                            inputPosition = 0;
                            CurLeft = startLeft;

                            Console.SetCursorPosition(CurLeft, CurTop);
                        }
                        break;
                    case ConsoleKey.End:
                        {
                            inputPosition = input.Count;
                            CurLeft = startLeft + inputPosition;

                            Console.SetCursorPosition(CurLeft, CurTop);
                        }
                        break;
                    case ConsoleKey.UpArrow:
                        if (inputHistory.Count > 0 && inputHistoryPosition - 1 >= 0)
                        {
                            // history
                            inputHistoryPosition--;
                            input = inputHistory[inputHistoryPosition].ToList();
                            inputPosition = input.Count;
                            CurLeft = startLeft + inputPosition;

                            ReWrite(startLeft, input);
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (inputHistoryPosition + 1 < inputHistory.Count)
                        {
                            // history
                            inputHistoryPosition++;
                            input = inputHistory[inputHistoryPosition].ToList();
                            inputPosition = input.Count;
                            CurLeft = startLeft + inputPosition;

                            ReWrite(startLeft, input);
                        }
                        else if (inputHistoryPosition + 1 == inputHistory.Count)
                        {
                            inputHistoryPosition = inputHistory.Count;
                            input.Clear();
                            inputPosition = input.Count;
                            CurLeft = startLeft + inputPosition;

                            ReWrite(startLeft, input);
                        }
                        break;
                    case ConsoleKey.Tab:
                        {
                            args = string.Concat(input).Trim().Split(' ').ToList();
                            argOrder = args.Count - 1;
                            cmd = argOrder == 0 ? "" : args[0];

                            // autocomplete
                            if (lastKey.Key != ConsoleKey.Tab)
                            {
                                var thisCompleteList = yourApp.AutoCompleteList[cmd][argOrder];
                                completeTarget = args[argOrder];
                                matchedList = GetMatch(thisCompleteList, completeTarget).GetEnumerator();
                            }

                            matchedList.MoveNext();
                            while (string.IsNullOrEmpty(matchedList.Current))
                            {
                                matchedList.MoveNext();
                            }

                            var before = cmd;
                            for (int i = 0; i < args.Count - 1; i++)
                            {
                                before = $"{before} {args[i]}";
                            }

                            input = $"{before}{matchedList.Current}".ToCharArray().ToList();
                            inputPosition = input.Count;
                            CurLeft = startLeft + inputPosition;

                            ReWrite(startLeft, input);
                        }
                        break;
                    case ConsoleKey.Escape:
                        {
                            if (lastKey.Key == ConsoleKey.Escape)
                            {
                                throw new InvalidOperationException("Escape");
                            }
                            else
                            {
                                throw new OperationCanceledException("Escape");
                            }
                        }
                    case ConsoleKey.Enter:
                        {
                            // input insert
                            args = string.Concat(input).Trim().Split(' ').ToList();
                            argOrder = args.Count - 1;
                            cmd = args[0];

                            if (!string.IsNullOrWhiteSpace(cmd)) inputHistory.Add(input);
                            isRunning = false;
                        }
                        break;
                    case ConsoleKey.Spacebar:
                        {
                            if (lastKey.Key != ConsoleKey.Spacebar)
                            {
                                // input insert
                                input.Insert(inputPosition++, key.KeyChar);
                                CurLeft++;

                                ReWrite(startLeft, input);
                            }
                            // show hint
                            args = string.Concat(input).Trim().Split(' ').ToList();
                            argOrder = args.Count - 1;
                            cmd = args[0];

                            if (argOrder == 0 && yourApp.HintList.ContainsKey(cmd))
                            {
                                var hint = yourApp.HintList[cmd];
                                if (!string.IsNullOrEmpty(hint))
                                {
                                    ReWrite(startLeft, input);
                                    Console.ForegroundColor = PromptActive.Theme.HintColor;
                                    Console.Write(hint);
                                    Console.SetCursorPosition(CurLeft, CurTop);
                                }
                            }
                        }
                        break;
                    default:
                        {
                            // input insert
                            input.Insert(inputPosition++, key.KeyChar);
                            CurLeft++;

                            ReWrite(startLeft, input);
                        }
                        break;
                }
                // maxbuffer check
                if (startLeft + input.Count >= Console.BufferWidth)
                {
                    throw new OverflowException("Console");
                }

                lastKey = key;
            }
            return (cmd, args);
        }

        private static IEnumerable<string> GetMatch(List<string> completeList, string input)
        {
            var list = completeList.ToList();
            list.Add(input);
            for (int i = 0; i < list.Count; i++)
            {
                if (Regex.IsMatch(list[i], ".*(?:" + input + ").*", RegexOptions.IgnoreCase))
                {
                    yield return list[i];
                }
                if (i == list.Count - 1) i = 0;
            }
        }
    }
}
