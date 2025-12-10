using System.Threading.Tasks;
using System;
using System.Linq;

namespace SnowyRiver.Exceptions;
public static class Extensions
{
    /// <param name="ex">要检查的异常</param>
    extension(Exception ex)
    {
        /// <summary>
        /// 判断异常或其内部异常是TaskCanceledException或OperationCanceledException
        /// </summary>
        /// <returns>如果是取消异常返回true，否则返回false</returns>
        public bool IsCanceledException()
        {
            if (ex == null) return false;

            var current = ex;
            while (current != null)
            {
                switch (current)
                {
                    case TaskCanceledException:
                    case OperationCanceledException:
                    case AggregateException aggregateEx when aggregateEx.InnerExceptions.Any(IsCanceledException):
                        return true;
                    case AggregateException:
                        return false;
                    default:
                        current = current.InnerException;
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断异常是否不是取消异常（与IsCanceledException相反）
        /// </summary>
        public bool IsNotCanceledException()
        {
            return !IsCanceledException(ex);
        }
    }
}
