using MilkChess.Engine;

namespace MilkChess.Helper;

public class ExceptionUtil
{
    public static Exception GetInnerMostException(Exception exception)
    {
        while (true)
        {
            if (exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1)
            {
                exception = aggregateException.InnerExceptions[0];
                continue;
            }

            if (exception.InnerException != null)
            {
                exception = exception.InnerException;
                continue;
            }

            return exception;
        }
    }
}