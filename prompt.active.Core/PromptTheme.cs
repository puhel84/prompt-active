using System;
using System.Collections.Generic;
using System.Text;

namespace prompt.active.Core
{
    public class PromptTheme
    {
        public ConsoleColor PromptColor { get; set; }
        public ConsoleColor CommandColor { get; set; }
        public ConsoleColor HintColor { get; set; }
        public ConsoleColor ErrorColor { get; set; }

        private static PromptTheme s_LightTheme;
        public static PromptTheme LightTheme
        {
            get => s_LightTheme = s_LightTheme ?? new PromptTheme()
            {
                PromptColor = ConsoleColor.Green,
                CommandColor = ConsoleColor.Black,
                HintColor = ConsoleColor.Gray,
                ErrorColor = ConsoleColor.Red,
            };
        }

        private static PromptTheme s_DarkTheme;
        public static PromptTheme DarkTheme
        {
            get => s_DarkTheme = s_DarkTheme ?? new PromptTheme()
            {
                PromptColor = ConsoleColor.Yellow,
                CommandColor = ConsoleColor.White,
                HintColor = ConsoleColor.DarkGray,
                ErrorColor = ConsoleColor.Red,
            };
        }
    }
}
