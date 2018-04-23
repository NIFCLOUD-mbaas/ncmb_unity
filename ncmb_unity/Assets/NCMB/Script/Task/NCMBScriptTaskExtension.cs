#if (NET_4_6)
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCMB.Tasks
{
    public static class NCMBScriptTaskExtension
    {
        public static Task<byte[]> ExecuteTaskAsync(this NCMBScript script,
            IDictionary<string, object> header, IDictionary<string, object> body, IDictionary<string, object> query)
        {
            var tcs = new TaskCompletionSource<byte[]>();
            script.ExecuteAsync(header, body, query, (data, error) =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(data);
                }
            });
            return tcs.Task;
        }
    }
}
#endif