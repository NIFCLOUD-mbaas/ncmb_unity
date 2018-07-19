#if (NET_4_6)
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCMB.Tasks
{
    public static class NCMBUserTaskExtension
    {
        /// <summary>
        /// 非同期処理でオブジェクトの取得を行います。
        /// </summary>
        public static Task<NCMBUser> FetchTaskAsync(this NCMBUser user)
        {
            var tcs = new TaskCompletionSource<NCMBUser>();
            user.FetchAsync(error =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(user);
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
        public static Task<NCMBUser> SaveTaskAsync(this NCMBUser user)
        {
            var tcs = new TaskCompletionSource<NCMBUser>();
            user.Save(error =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(user);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// オブジェクトの削除を行います。
        /// </summary>
        public static Task<bool> DeleteTaskAsync(this NCMBUser user)
        {
            var tcs = new TaskCompletionSource<bool>();
            user.DeleteAsync(error =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(true);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 非同期処理で現在ログインしているユーザのauthDataの削除を行います。<br/>
        /// </summary>
        /// <param name="provider">SNS名</param>
        public static Task<NCMBUser> UnLinkWithAuthDataTaskAsync(this NCMBUser user, string provider)
        {
            var tcs = new TaskCompletionSource<NCMBUser>();
            user.UnLinkWithAuthDataAsync(provider, error =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(user);
                }
            });
            return tcs.Task;

        }

        /// <summary>
        /// 非同期処理で現在ログインしているユーザに、authDataの追加を行います。<br/>
        /// authDataが登録されていないユーザならログインし、authDataの登録を行います。<br/>
        /// authDataが登録されているユーザなら、authDataの追加を行います。<br/>
        /// </summary>
        /// <param name="linkParam">authData</param>
        public static Task<NCMBUser> LinkWithAuthDataTaskAsync(this NCMBUser user, Dictionary<string, object> linkParam)
        {
            var tcs = new TaskCompletionSource<NCMBUser>();
            user.LinkWithAuthDataAsync(linkParam, error =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(user);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 非同期処理でauthDataを用いて、ユーザを登録します。<br/>
        /// 既存会員のauthData登録はLinkWithAuthDataAsyncメソッドをご利用下さい。<br/>
        /// </summary>
        public static Task<NCMBUser> LogInWithAuthDataTaskAsync(this NCMBUser user)
        {
            var tcs = new TaskCompletionSource<NCMBUser>();
            user.LogInWithAuthDataAsync(error =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(user);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 非同期処理でユーザを登録します。<br/>
        /// オブジェクトIDが登録されていない新規会員ならログインし、登録を行います。<br/>
        /// オブジェクトIDが登録されている既存会員ならログインせず、更新を行います。<br/>
        /// 既存会員のログインはLogInAsyncメソッドをご利用下さい。<br/>
        /// </summary>
        public static Task<NCMBUser> SignUpTaskAsync(this NCMBUser user)
        {
            var tcs = new TaskCompletionSource<NCMBUser>();
            user.SignUpAsync(error =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(user);
                }
            });
            return tcs.Task;
        }
    }
}
#endif
