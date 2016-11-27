using System.Threading.Tasks;
using Windows.Foundation;

namespace WindowsPhoneMusicSync
{
    public static class Extensions
    {
        public static Task<T> AsTask<T>(this IAsyncOperation<T> op)
        {
            var ret = new TaskCompletionSource<T>();
            op.Completed = (info, status) =>
            {
                if (status == AsyncStatus.Completed)
                {
                    ret.SetResult(info.GetResults());
                }
                if (status == AsyncStatus.Canceled)
                {
                    ret.SetCanceled();
                }
                if (status == AsyncStatus.Error)
                {
                    ret.SetException(info.ErrorCode);
                }
            };
            return ret.Task;
        }

        public static Task AsTask(this IAsyncAction op)
        {
            var ret = new TaskCompletionSource<object>();
            op.Completed = (info, status) =>
            {
                if (status == AsyncStatus.Completed)
                {
                    ret.SetResult(null);
                }
                if (status == AsyncStatus.Canceled)
                {
                    ret.SetCanceled();
                }
                if (status == AsyncStatus.Error)
                {
                    ret.SetException(info.ErrorCode);
                }
            };
            return ret.Task;
        }
    }
}