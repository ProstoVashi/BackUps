using System;
using Tools.Utils;

namespace Sandbox {
    static class Program {
        static void Main(string[] args) {
            //LocalCopySandbox.Invoke();
            //RegisterHelper.SetValue(RegisterHelper.Root, "SOFTWARE/Test.some_value", "value");
            Console.WriteLine($"Value: '{RegisterHelper.GetValue(RegisterHelper.Root, "SOFTWARE/Test.some_value")}'");
        }
    }
}