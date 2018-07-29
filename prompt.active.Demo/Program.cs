using prompt.active.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace prompt.active.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            PromptActive.Start<BaseProptSample>();
        }
    }


    public class BaseProptSample : BasePromptApp
    {
        [Autocomplete(nameof(Create), 2)]
        [Autocomplete(nameof(Delete), 1)]
        public List<string> DriverList { get; } = new List<string>() { };

        [Autocomplete(nameof(Delete), 2)]
        public List<string> valueList { get; } = new List<string>() { "1", "10", "100" };

        public override void Dispose()
        {
            Console.WriteLine("Disposed");
        }

        [CustomHint("Cumstom hint")]
        [PromptCommand]
        public void List()
        {
            Console.WriteLine("Command List");
        }

        public void Show()
        {
            Console.WriteLine("Command Show");
        }

        [PromptCommand]
        [Description("Create driver")]
        public void Create(testEnum enums, string driver, int value)
        {
            Console.WriteLine("Command Create");
        }

        [PromptCommand]
        [Description("Delete driver")]
        public void Delete(string driver, int value = 1)
        {
            Console.WriteLine("Command Delete");
        }
    }

    public enum testEnum
    {
        test1,
        test2
    }
}
