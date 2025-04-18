using System.Threading.Tasks;
using System;

namespace SnowyRiver.Exceptions;
public static class Extensions
{
    public static bool IsTaskCanceledException(this Exception e)
    {
        if (e is TaskCanceledException)
        {
            return true;
        }

        return e.InnerException != null && IsTaskCanceledException(e.InnerException);
    }
}
