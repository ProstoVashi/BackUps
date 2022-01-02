using System;
using System.Threading.Tasks;
using PvBackUps.FileEventHandles;
using RestSharp;
using Tools.Utils;

namespace Sandbox {
    static class Program {
        //private const string LOCAL_TOKEN_RECEIVER_URL = "http://localhost:5092/servicerequest/token";
        
        static async Task Main(string[] args) {
            //LocalCopySandbox.Invoke();
            //RegisterHelper.SetValue(RegisterHelper.Root, "SOFTWARE/Test.some_value", "value");
            //Console.WriteLine($"Value: '{RegisterHelper.GetValue(RegisterHelper.Root, "SOFTWARE/Test.some_value")}'");
            // var client = new RestClient(LOCAL_TOKEN_RECEIVER_URL);
            // var response = await client.ExecuteAsync<string>(new RestRequest(Method.GET));
            // Console.WriteLine(response.Data);
        }
    }
}