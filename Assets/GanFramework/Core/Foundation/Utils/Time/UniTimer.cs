using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GanFramework.Core.Utils.Time
{
    /// <summary>
    /// 通用计时器（正计时 / 倒计时均可）
    /// 全程 UniTask，无 GC，支持暂停、继续、提前终止
    /// </summary>
    public sealed class UniTimer
    {
        public float Duration { get; private set; } // 总时长
        public float Elapsed { get; private set; } // 已走时间
        public float Remaining => Mathf.Max(0, Duration - Elapsed); // 剩余时间

        public bool IsRunning { get; private set; } // 是否正在运行
        public bool IsPaused { get; private set; } // 是否暂停

        /// <summary> 每帧回调：参数为当前已走时间 </summary>
        public event Action<float> OnTick;

        /// <summary> 计时结束回调 </summary>
        public event Action OnComplete;

        private CancellationTokenSource _cts;

        private UniTaskCompletionSource<bool> _taskSource;

        /*------------------------------------------------------*/
        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="duration">时长（秒）</param>
        /// <param name="isCountDown">true=倒计时；false=正计时</param>
        /// <param name="onTick">可选：每帧刷新</param>
        /// <param name="onComplete">可选：结束回调</param>
        public UniTask StartAsync(float duration,
            bool isCountDown = false,
            Action<float> onTick = null,
            Action onComplete = null)
        {
            Stop(); // 先清理旧任务
            _taskSource = new UniTaskCompletionSource<bool>(); // 新建
            Duration = duration;
            Elapsed = isCountDown ? duration : 0f;
            OnTick = onTick;
            OnComplete = onComplete;
            IsRunning = true;
            IsPaused = false;
            _cts = new CancellationTokenSource();

            return RunAsync(_cts.Token, isCountDown);
        }

        /// <summary> 立即终止（不可恢复） </summary>
        public void Stop()
        {
            if (!IsRunning) return;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            IsRunning = false;
            _taskSource?.TrySetCanceled(); // 提前终止也通知
            IsPaused = false;
        }

        /// <summary> 暂停 </summary>
        public void Pause() => IsPaused = true;

        /// <summary> 继续 </summary>
        public void Resume() => IsPaused = false;

        /*------------------------------------------------------*/
        private async UniTask RunAsync(System.Threading.CancellationToken token, bool countDown)
        {
            while (!token.IsCancellationRequested)
            {
                if (!IsPaused)
                {
                    float dt = UnityEngine.Time.deltaTime;
                    Elapsed += countDown ? -dt : dt;

                    // 触发回调
                    OnTick?.Invoke(Elapsed);

                    // 结束条件
                    if (countDown ? Elapsed <= 0f : Elapsed >= Duration)
                    {
                        Elapsed = countDown ? 0f : Duration;
                        OnTick?.Invoke(Elapsed); // 最后再给一次精确值
                        break;
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            IsRunning = false;
            _taskSource?.TrySetResult(true); // 广播“结束”
            if (!token.IsCancellationRequested)
                OnComplete?.Invoke();
        }

        public UniTask WaitForFinishAsync() => _taskSource?.Task ?? UniTask.CompletedTask;
    }
}