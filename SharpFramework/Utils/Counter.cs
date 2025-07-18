﻿using Raylib_cs;

namespace SharpFramework.Utils
{
    /// <summary>
    /// カウンタークラス。
    /// </summary>
    public class Counter
    {
        /// <summary>
        /// カウンターを初期化します。
        /// </summary>
        /// <param name="begin">開始値。</param>
        /// <param name="end">終了値。</param>
        /// <param name="interval">Tickする間隔(ミリ秒)。</param>
        /// <param name="isLoop">ループするか否か。</param>
        public Counter(double begin, double end, double interval, bool isLoop)
        {
            NowTime = Raylib.GetTime();
            Begin = begin;
            End = end;
            Interval = interval;
            IntervalInSeconds = interval / 1000.0;
            Value = begin;
            IsLoop = isLoop;
            State = TimerState.Stopped;
        }

        /// <summary>
        /// Tickします。
        /// </summary>
        /// <returns>何Tickしたか。</returns>
        public long Tick()
        {
            // 何Tickしたかのカウント
            var tickCount = 0;
            var nowTime = Raylib.GetTime();

            // 停止状態の場合、現在時間をそのままプロパティに代入して、終わる。
            if (State == TimerState.Stopped)
            {
                NowTime = nowTime;
                return 0;
            }

            // 現在時間から以前Tick()したまでの時間の差
            var diffTime = nowTime - NowTime;

            while (diffTime >= IntervalInSeconds)
            {
                // 時間の差が間隔未満になるまで進める
                Value++;
                tickCount++;
                if (Value >= End)
                {
                    if (IsLoop)
                    {
                        // ループ設定かつ現在の値が終了値より大きかったら
                        Value = Begin;
                        Looped?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        // 非ループ設定かつ現在の値が終了値より大きかったら、終了値を維持してタイマーを停止する。
                        Value = End;
                        Stop();
                        Ended?.Invoke(this, EventArgs.Empty);
                    }
                }
                diffTime -= IntervalInSeconds;
            }
            // 余ったdiffTimeを引いて、次Tick()したときにちゃんとなるように
            NowTime = nowTime - diffTime;
            return tickCount;
        }

        /// <summary>
        /// タイマーを開始します。必ずこのメソッドを呼び出してください。
        /// </summary>
        public void Start()
        {
            if (State == TimerState.Started)
            {
                // すでに開始しているなら、何もしない。
                return;
            }

            // Tick()を呼び出して、NowTimeに現在の時間を代入させてからタイマーを開始する。
            Tick();
            State = TimerState.Started;
        }

        /// <summary>
        /// タイマーを停止します。
        /// </summary>
        public void Stop()
        {
            if (State == TimerState.Stopped)
            {
                // すでに停止しているなら、何もしない。
                return;
            }

            State = TimerState.Stopped;
        }

        /// <summary>
        /// タイマーをリセットします。
        /// </summary>
        public void Reset()
        {
            // 現在時間を入れる。
            NowTime = Raylib.GetTime();
            // カウンターを最小値に戻す。
            Value = Begin;
        }

        /// <summary>
        /// タイマーのTick間隔を変更します。
        /// </summary>
        /// <param name="interval">Tickする間隔(ミリ秒)。</param>
        public void ChangeInterval(long interval)
        {
            // 今までのカウンター値を更新する。
            Tick();
            // 間隔を更新する。
            Interval = interval;
            IntervalInSeconds = interval / 1000.0;
            // 間隔更新後、あまりがあるかもしれないのでもう一度カウンター値を更新する。
            Tick();
        }

        /// <summary>
        /// タイマーの終了値を変更します。
        /// </summary>
        /// <param name="end">終了値。</param>
        public void ChangeEnd(long end)
        {
            End = end;
            if (End < Value)
            {
                Value = End;
            }
        }

        /// <summary>
        /// タイマーの開始値を変更します。
        /// </summary>
        /// <param name="begin">開始値。</param>
        public void ChangeBegin(long begin)
        {
            Begin = begin;
            if (Begin > Value)
            {
                Value = Begin;
            }
        }

        /// <summary>
        /// ループした場合、イベントが発生します。
        /// </summary>
        public event EventHandler Looped;

        /// <summary>
        /// タイマーが止まった。
        /// </summary>
        public event EventHandler Ended;

        /// <summary>
        /// 現在のコンピュータの時間(秒)。
        /// </summary>
        public double NowTime { get; private set; }

        /// <summary>
        /// 開始値。
        /// </summary>
        public double Begin { get; private set; }

        /// <summary>
        /// 終了値。
        /// </summary>
        public double End { get; private set; }

        /// <summary>
        /// タイマー間隔 (ミリ秒)。
        /// </summary>
        public double Interval { get; private set; }

        /// <summary>
        /// タイマー間隔（秒。
        /// </summary>
        private double IntervalInSeconds;

        /// <summary>
        /// カウンターの現在の値。
        /// </summary>
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                // 最小値・最大値を超える場合、丸める。
                if (value < Begin)
                {
                    _value = Begin;
                    return;
                }
                if (End < value)
                {
                    _value = End;
                    return;
                }
                _value = value;
            }
        }

        /// <summary>
        /// ループするかどうか。
        /// </summary>
        public bool IsLoop { get; }

        /// <summary>
        /// 現在の状態。
        /// </summary>
        public TimerState State { get; private set; }

        private double _value;
    }

    /// <summary>
    /// タイマーの状態。
    /// </summary>
    public enum TimerState
    {
        /// <summary>
        /// 停止している。
        /// </summary>
        Stopped,

        /// <summary>
        /// 動作している。
        /// </summary>
        Started
    }
}