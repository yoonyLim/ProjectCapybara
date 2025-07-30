using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Moko
{
    public static class Timer
    {
        public static IEnumerator WaitCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        public static IEnumerator WaitCoroutine(float duration, Action onTimerEnd)
        {
            yield return new WaitForSeconds(duration);
            onTimerEnd?.Invoke();
        }
        
        public static async Task Wait(float duration)
        {
            await Task.Delay((int)(duration * 1000));
        }
        
        public static async Task Wait(float duration, CancellationToken cancellationToken)
        {
            await Task.Delay((int)(duration * 1000), cancellationToken);
        }
        
        public static async Task Wait(float duration, Action onTimerEnd)
        {
            await Task.Delay((int)(duration * 1000));
            onTimerEnd?.Invoke();
        }

        public static async Task Wait(float duration, Action onTimerEnd, CancellationToken cancellationToken)
        {
            await Task.Delay((int)(duration * 1000), cancellationToken);
            onTimerEnd?.Invoke();
        }
    }
}
