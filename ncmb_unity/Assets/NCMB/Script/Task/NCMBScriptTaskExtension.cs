#if (NET_4_6)
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCMB.Tasks
{
    public static class NCMBScriptTaskExtension
    {
        /// <summary>
        /// 非同期処理でスクリプトの実行を行います。
        /// </summary>
        /// <param name="header">リクエストヘッダー.</param>
        /// <param name="body">リクエストボディ</param>
        /// <param name="query">クエリパラメーター</param>
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
