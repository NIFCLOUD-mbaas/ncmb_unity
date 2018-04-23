#if (NET_4_6)
using System.Threading.Tasks;

namespace NCMB.Task
{
    public static class NCMBObjectTaskExtension
    {
        public static Task<T> FetchTaskAsync<T>(this T origin) where T : NCMBObject
        {
            var tcs = new TaskCompletionSource<T>();
            origin.FetchAsync(error =>
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

        public static Task<T> SaveTaskAsync<T>(this T origin) where T : NCMBObject
        {
            var tcs = new TaskCompletionSource<T>();
            origin.SaveAsync(error =>
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

        public static Task<T> DeleteTaskAsync<T>(this T origin) where T : NCMBObject
        {
            var tcs = new TaskCompletionSource<T>();
            origin.DeleteAsync(error =>
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