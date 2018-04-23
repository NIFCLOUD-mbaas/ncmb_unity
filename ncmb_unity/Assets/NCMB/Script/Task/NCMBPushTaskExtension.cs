#if (NET_4_6)
using System.Threading.Tasks;

namespace NCMB.Tasks
{
    public static class NCMBPushTaskExtension
    {
        /// <summary>
        /// プッシュの送信を行います。
        /// </summary>
        public static Task<NCMBPush> SendPushTaskAsync(this NCMBPush origin)
        {
            var tcs = new TaskCompletionSource<NCMBPush>();
            origin.SendPush(error =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(origin);
                }
            });
            return tcs.Task;
        }
    }
}
#endif
