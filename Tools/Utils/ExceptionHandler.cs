using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tools.Utils {
    public static class ExceptionHandler {
        public static async Task<T> SafetyExec<T>(Func<Task<T>> func, ILogger logger, bool throwFurther = true) {
            try {
                return await func();
            } catch (Exception ex) {
                logger.LogError(ex, "Failed: ");
                if(throwFurther) {
                    throw;
                }
                return default;
            }
        }

        public static async Task<T> SafetyExec<T>(Func<Task<T>> func, ILogger logger, string methodName, object request, bool throwFurther = true) {
            try {
                LogRequest(logger, methodName, request);
                var result = await func();
                LogResult(logger, methodName, true);
                return result;
            } catch (Exception ex) {
                logger.LogError(ex, "Method-\"{Method}\" with request:\n{Request}\nfailed by reason:\n{Message}", 
                                methodName, request as string ?? JsonSerializer.Serialize(request), ex.Message);
                LogResult(logger, methodName, false);
                
                if(throwFurther) {
                    throw;
                }
                return default;
            }
        }
        
        public static async Task<T> SafetyExec<T>(Func<Task<T>> func, Action<string> errorAction, bool throwFurther = true) {
            try {
                return await func();
            } catch (Exception ex) {
                errorAction?.Invoke(ex.ToString());
                if(throwFurther) {
                    throw;
                }
                return default;
            }
        }

        private static void LogRequest(ILogger logger, string methodName, object request) {
            logger.LogInformation("Method \"{Method}\" handles request:\n\"{Request}\"",
                                  methodName, request as string ?? JsonSerializer.Serialize(request));
        }

        private static void LogResult(ILogger logger, string method, bool success) {
            string msgPart = success ? "finished successfully." : "failed.";
            logger.LogInformation("Method \"{Method}\" {MsgPart}", method, msgPart);
        }
    }
}