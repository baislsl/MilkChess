using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

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

    public delegate Func<Task> AsyncCommandMiddleware(Func<Task> next);

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
        ICommand cmd = new RelayCommand(action);
        return BuildCommand(cmd, middleware);
    }

    public static ICommand BuildCommand(Func<Task> action, params CommandMiddleware[] middleware)
    {
        ICommand cmd = new AsyncRelayCommand(action);
        return BuildCommand(cmd, middleware);
    }

    public static Func<Task> BuildAsyncCommand(
        Func<Task> action,
        params AsyncCommandMiddleware[] middleware)
    {
        for (int i = middleware.Length - 1; i >= 0; i--)
        {
            action = middleware[i](action);
        }

        return action;
    }


    public static async Task ExecuteAsync(Func<Task> action, params AsyncCommandMiddleware[] middleware)
    {
        await BuildAsyncCommand(action, middleware)();
    }

    public static void Execute(Action action, params CommandMiddleware[] middleware)
    {
        BuildCommand(action, middleware).Execute(null);
    }
}