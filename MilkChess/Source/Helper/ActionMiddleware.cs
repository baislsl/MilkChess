using System.Windows.Input;

namespace MilkChess.Helper;

public static class MiddlewareHelper<T>
{
    public delegate EventHandler<T> EventHandlerMiddleware(EventHandler<T> eventHandler);

    public static EventHandler<T> BuildEventHandler(EventHandler<T> eventHandler,
                    params EventHandlerMiddleware[] middleware)
    {
        for (int i = middleware.Length - 1; i >= 0; i--)
        {
            eventHandler = middleware[i](eventHandler);
        }

        return eventHandler;
    }
}

public static class CommandMiddlewareHelper
{
    public delegate ICommand CommandMiddleware(ICommand command);

    public static ICommand BuildCommand(ICommand cmd, params CommandMiddleware[] middleware)
    {
        for (int i = middleware.Length - 1; i >= 0; i--)
        {
            cmd = middleware[i](cmd);
        }

        return cmd;
    }

    public static ICommand BuildCommand(Action action, params CommandMiddleware[] middleware)
    {
        ICommand cmd = new Command<object>(_ => action());
        return BuildCommand(cmd, middleware);
    }
}