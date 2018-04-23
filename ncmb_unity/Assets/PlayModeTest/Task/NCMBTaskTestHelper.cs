#if NET_4_6
using System.Collections;
using System.Threading.Tasks;

public static class NCMBTaskTestHelper
{
    public static IEnumerator ToEnumerator(this Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }
    }

    public static IEnumerator ToEnumerator<T>(this Task<T> task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }
    }
}
#endif
