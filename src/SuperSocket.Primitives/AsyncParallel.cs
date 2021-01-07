using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket
{
    public static class AsyncParallel
    {
        /// <summary>
        /// 迭代器
        /// </summary>
        /// <typeparam name="TItem">数据类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="operation">操作</param>
        /// <param name="maxDegreeOfParallelism">最大并行</param>
        /// <returns></returns>
        public static async Task ForEach<TItem>(IEnumerable<TItem> source, Func<TItem, Task> operation, int maxDegreeOfParallelism = 5)
        {
            await ForEach(source, operation, new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                CancellationToken = CancellationToken.None
            });
        }
        /// <summary>
        /// 迭代器
        /// </summary>
        /// <typeparam name="TItem">数据类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="operation">操作</param>
        /// <param name="parallelOptions">并行设置</param>
        /// <returns></returns>
        public static async Task ForEach<TItem>(IEnumerable<TItem> source, Func<TItem, Task> operation, ParallelOptions parallelOptions)
        {
            var allTasks = new List<Task>();
            var throttler = new SemaphoreSlim(initialCount: parallelOptions.MaxDegreeOfParallelism);

            foreach (var item in source)
            {
                await throttler.WaitAsync(parallelOptions.CancellationToken);

                if (parallelOptions.CancellationToken.IsCancellationRequested)
                    break;

                allTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await operation(item);
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }, parallelOptions.CancellationToken));
            }

            await Task.WhenAll(allTasks);
        }
    }
}