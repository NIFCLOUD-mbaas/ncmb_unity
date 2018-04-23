#if (NET_4_6)
using System.Threading.Tasks;

namespace NCMB.Task
{
    public static class NCMBPushTaskExtension
    {
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