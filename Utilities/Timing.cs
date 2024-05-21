using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Nebula
{
    public static class Timing
    {
        // 'interval' is float in seconds.
        public static async Task SetInterval(Action callback, float interval, CancellationToken cancellationToken) // Todo: Allow for callback parameters.
        {
            while (true)
            {
                await Task.Delay((int)(interval * 1000));

                cancellationToken.ThrowIfCancellationRequested();

                callback();
            }
        }

        // 'interval' is float in seconds.
        public static async Task SetTimeout(Action callback, float interval) // Todo: Allow for callback parameters.
        {
            await Task.Delay((int)(interval * 1000));
            callback();
        }
    }

    public class NebulaTimer
    {
        private Task _timerTask = null;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public void SetInterval(Action callback, float interval)
        {
            _timerTask = Timing.SetInterval(callback, interval, _cancellationTokenSource.Token);
        }

        public void Cancel() { _cancellationTokenSource.Cancel(); }
        ~NebulaTimer() { this.Cancel(); }
    }
}
