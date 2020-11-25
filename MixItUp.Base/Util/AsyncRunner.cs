﻿using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public static class AsyncRunner
    {
        public static async Task<T> RunAsync<T>(Task<T> task)
        {
            try
            {
                await task;
                return task.Result;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, includeStackTrace: true);
            }
            return default(T);
        }

        public static async Task RunAsync(Func<Task> task)
        {
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, includeStackTrace: true);
            }
        }

        public static async Task RunAsyncBackground(Action action, CancellationToken token)
        {
            await Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }, token);
        }

        public static async Task<T> RunAsyncBackground<T>(Func<T> function, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return function();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                return default(T);
            }, token);
        }

        public static Task RunAsyncBackground(Func<Task> task)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await task();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex, includeStackTrace: true);
                }
            });
        }

        public static Task RunAsyncBackground(CancellationToken token, Func<CancellationToken, Task> backgroundTask)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await backgroundTask(token);
                }
                catch (ThreadAbortException) { return; }
                catch (OperationCanceledException) { return; }
                catch (Exception ex) { Logger.Log(ex); }
            });
        }

        public static Task RunAsyncBackground(CancellationToken token, int delayInMilliseconds, Func<CancellationToken, Task> backgroundTask)
        {
            return Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await backgroundTask(token);
                    }
                    catch (ThreadAbortException) { return; }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex) { Logger.Log(ex); }

                    try
                    {
                        if (delayInMilliseconds > 0 && !token.IsCancellationRequested)
                        {
                            await Task.Delay(delayInMilliseconds, token);
                        }
                    }
                    catch (ThreadAbortException) { return; }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex) { Logger.Log(ex); }
                }
            });
        }
    }
}