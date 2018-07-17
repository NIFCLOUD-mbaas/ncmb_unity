#if NET_4_6
using System.Threading.Tasks;

namespace NCMB
{
    public partial class NCMBUser : NCMBObject
    {
        /// <summary>
        /// 非同期処理でユーザ名とパスワードを指定して、ユーザのログインを行います。<br/>
        /// </summary>
        /// <param name="name">ユーザ名</param>
        /// <param name="password">パスワード</param>
        public static Task<NCMBUser> LogInTaskAsync(string name, string password)
        {
            var tcs = new TaskCompletionSource<NCMBUser>();
            LogInAsync(name, password, error =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(CurrentUser);
                }
            });
            return tcs.Task;
        }

        /// <summary>
        /// 非同期処理で指定したメールアドレスに対して、<br/>
        /// 会員登録を行うためのメールを送信するよう要求します。<br/>
        /// </summary>
        /// <param name="email">メールアドレス</param>
        public static Task<bool> RequestAuthenticationMailTaskAsync(string email)
        {
            var tcs = new TaskCompletionSource<bool>();
            RequestAuthenticationMailAsync(email, error =>
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
        /// 非同期処理でユーザのログアウトを行います。<br/>
        /// </summary>
        public static Task<bool> LogOutTaskAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            LogOutAsync(error =>
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
        /// 非同期処理でメールアドレスとパスワードを指定して、ユーザのログインを行います。<br/>
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="password">パスワード</param>
        public static Task<NCMBUser> LogInWithMailAddressTaskAsync(string email, string password)
        {
            var tcs = new TaskCompletionSource<NCMBUser>();
            _ncmbLogIn(null, password, email, error =>
            {
                if (error != null)
                {
                    tcs.SetException(error);
                }
                else
                {
                    tcs.SetResult(CurrentUser);
                }
            });
            return tcs.Task;
        }
    }
}
#endif
