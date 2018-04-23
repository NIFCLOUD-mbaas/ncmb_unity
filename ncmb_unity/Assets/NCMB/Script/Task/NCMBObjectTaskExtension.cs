#if (NET_4_6)
using System.Threading.Tasks;

namespace NCMB.Tasks
{
    public static class NCMBObjectTaskExtension
    {
        /// <summary>
        /// 非同期処理でオブジェクトの取得を行います。<br/>
        /// </summary>
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

        /// <summary>
        /// 非同期処理でオブジェクトの保存を行います。<br/>
        /// SaveAsync()を実行してから編集などをしていなく、保存をする必要が無い場合は通信を行いません。<br/>
        /// オブジェクトIDが登録されていない新規オブジェクトなら登録を行います。<br/>
        /// オブジェクトIDが登録されている既存オブジェクトなら更新を行います。<br/>
        /// </summary>
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

        /// <summary>
        /// オブジェクトの削除を行います。<br/>
        /// </summary>
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
