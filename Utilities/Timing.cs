using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nebula
{
    public static class Timing
    {
        // Todo: Allow for callback parameters.
        // 'interval' is float in seconds.
        public static async Task SetInterval(Action callback, float interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay((int)(interval * 1000));

                cancellationToken.ThrowIfCancellationRequested();

                callback();
            }
        }

        // Todo: Allow for callback parameters.
        // 'interval' is float in seconds.
        public static async Task SetTimeout(Action callback, float interval)
        {
            await Task.Delay((int)(interval * 1000));
            callback();
        }
    }
}
