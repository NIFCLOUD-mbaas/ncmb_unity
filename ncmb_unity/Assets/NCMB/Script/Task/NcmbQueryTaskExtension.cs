#if (NET_4_6)
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCMB.Tasks
{
    public static class NCMBQueryTaskExtension
    {
        /// <summary>
        /// クエリにマッチするオブジェクトを取得を行います。
        /// </summary>
        public static Task<IList<T>> FindTaskAsync<T>(this NCMBQuery<T> query) where T : NCMBObject
        {
            var tcs = new TaskCompletionSource<IList<T>>();
            query.FindAsync((objects, error) =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(objects);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 指定IDのオブジェクトを取得を行います。
        /// </summary>
        public static Task<T> GetTaskAsync<T>(this NCMBQuery<T> query, string objectId) where T : NCMBObject
        {
            var tcs = new TaskCompletionSource<T>();
            query.GetAsync(objectId, (obj, error) =>
                {

                    if (error != null)
                    {
                        tcs.SetException(error);
                    }
                    else
                    {
                        tcs.SetResult(obj);
                    }

                });
            return tcs.Task;
        }

        /// <summary>
        /// クエリにマッチするオブジェクト数の取得を行います。
        /// </summary>
        public static Task<int> CountTaskAsync<T>(this NCMBQuery<T> query) where T : NCMBObject
        {
            var tcs = new TaskCompletionSource<int>();
            query.CountAsync((count, error) =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(count);
                }
            });
            return tcs.Task;
        }
    }
}
#endif
