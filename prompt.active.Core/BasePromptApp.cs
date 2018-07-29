using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace prompt.active.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AutocompleteAttribute : Attribute
    {
        public string Function { get; }
        public int Order { get; }

        public AutocompleteAttribute(string functionName, int argOrder)
        {
            Function = functionName;
            Order = argOrder;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CustomHintAttribute : Attribute
    {
        public string Hint { get; }

        public CustomHintAttribute(string customHint)
        {
            Hint = customHint;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PromptCommandAttribute : Attribute
    {
    }

    public abstract class BasePromptApp : IDisposable
    {
        internal readonly Dictionary<string, MethodInfo> CommandList = new Dictionary<string, MethodInfo>();
        internal readonly Dictionary<string, string> DescriptionList = new Dictionary<string, string>();
        internal readonly Dictionary<string, string> HintList = new Dictionary<string, string>();
        internal readonly Dictionary<string, Dictionary<int, List<string>>> AutoCompleteList = new Dictionary<string, Dictionary<int, List<string>>>() { { "", new Dictionary<int, List<string>>() { { 0, new List<string>() } } } };

        public abstract void Dispose();

        [PromptCommand]
        [Description("Disply useage text.")]
        public void Help()
        {

        }

        [PromptCommand]
        [Description("Exit prompt.")]
        public void Exit()
        {
            PromptActive.isRunning = false;
        }
    }
}
