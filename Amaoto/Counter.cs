using System;
using DxLibDLL;

namespace Amaoto
{
    /// <summary>
    /// カウンタークラス。
    /// </summary>
    public class Counter
    {
        public static decimal totalCount;

        public Action Looped;

        public Action Ended;

        public decimal NowTime;

        public long Begin;

        public long End;

        public double Interval;

        public TimerState State;

        private double _value;

        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public bool IsLoop { get; }

        public Counter(double begin, double end, double interval, bool isLoop)
        {
            NowTime = totalCount;
            Begin = (long)begin;
            End = (long)end;
            Interval = interval;
            Value = (long)begin;
            IsLoop = isLoop;
            State = TimerState.Stopped;
        }

        public long Tick()
        {
            int tickCount = 0;
            decimal nowTime = totalCount;
            if (State == TimerState.Stopped)
            {
                NowTime = nowTime;
                return 0L;
            }
            decimal diffTime = nowTime - NowTime;
            if (diffTime < 0m)
            {
                diffTime = nowTime + (9223372036854775807m - NowTime);
            }
            for (decimal inter_decimal = new decimal(Interval); diffTime >= inter_decimal; diffTime -= inter_decimal)
            {
                Value += 1.0;
                tickCount++;
                if (Value >= (double)End)
                {
                    if (IsLoop)
                    {
                        Value = Begin;
                        Looped?.Invoke();
                    }
                    else
                    {
                        Value = End;
                        Stop();
                        Ended?.Invoke();
                    }
                }
            }
            NowTime = nowTime - diffTime;
            return tickCount;
        }

        public void Start(bool isReset = false)
        {
            if (isReset)
            {
                Reset();
            }
            if (State != TimerState.Started)
            {
                Tick();
                State = TimerState.Started;
            }
        }

        public void Stop()
        {
            if (State != TimerState.Stopped)
            {
                State = TimerState.Stopped;
            }
        }

        public void Reset(bool isStop = false)
        {
            if (isStop)
            {
                Stop();
            }
            NowTime = totalCount;
            Value = Begin;
        }

        public void ChangeInterval(double interval)
        {
            Interval = interval;
        }

        public void ChangeEnd(double end)
        {
            End = (long)end;
            if ((double)End < Value)
            {
                Value = End;
            }
        }

        public void ChangeBegin(double begin)
        {
            Begin = (long)begin;
            if ((double)Begin > Value)
            {
                Value = Begin;
            }
        }
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