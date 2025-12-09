using System.Threading.Tasks;
using System;

namespace SnowyRiver.Exceptions;
public static class Extensions
{
    public static bool IsTaskCanceledOrOperationCanceledExceptionException(this Exception e)
    {
        if (e is TaskCanceledException or OperationCanceledException)
        {
            return true;
        }

        return e.InnerException != null && IsTaskCanceledOrOperationCanceledExceptionException(e.InnerException);
    }
}
