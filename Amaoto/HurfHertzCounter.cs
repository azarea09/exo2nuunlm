using Taiko;

namespace Amaoto;

internal class HurfHertzCounter
{
    internal Action Looped;

    internal Action Ended;

    private double MaxFrame;

    internal bool IsLoop;

    private double _value;

    internal double NowTime { get; private set; }

    internal double Begin { get; private set; }

    internal double End { get; private set; }

    internal double Interval { get; set; }

    private bool _isReversing; // ★ 逆再生中かどうかのフラグ

    internal double Value
    {
        get
        {
            return _value;
        }
        set
        {
            if (value < Begin)
            {
                _value = Begin;
            }
            else if (End < value)
            {
                _value = End;
            }
            else
            {
                _value = value;
            }
        }
    }

    internal TimerState State { get; private set; }

    internal HurfHertzCounter(double begin, double end, double interval, bool isLoop = false, bool isDefaultEnd = false)
    {
        NowTime = Program.NowTime;
        Begin = begin;
        End = end;
        Interval = interval;
        Value = (isDefaultEnd ? end : begin);
        IsLoop = isLoop;
        State = TimerState.Stopped;
        _isReversing = false;
    }

    internal HurfHertzCounter(double begin, double end, bool isLoop = false)
    {
        NowTime = Program.NowTime;
        Begin = begin;
        End = end;
        Interval = Utils.Time(60.0);
        Value = begin;
        IsLoop = isLoop;
        State = TimerState.Stopped;
        _isReversing = false;
    }

    internal void SetReverse(bool reverse)
    {
        _isReversing = reverse;
        Value = End;
    }

    internal long Tick()
    {
        int tickCount = 0;
        double nowTime = Program.NowTime;

        if (State == TimerState.Stopped)
        {
            NowTime = nowTime;
            return 0L;
        }

        double diffTime = nowTime - NowTime;
        if (diffTime < 0.0)
        {
            diffTime = nowTime + (9.223372036854776E+18 - NowTime);
        }

        while (diffTime >= Interval)
        {
            if (_isReversing)
            {
                Value--;
                if (Value <= Begin)
                {
                    if (IsLoop)
                    {
                        Value = End;
                        Looped?.Invoke();
                    }
                    else
                    {
                        Value = Begin;
                        Stop();
                        Ended?.Invoke();
                        break;
                    }
                }
            }
            else
            {
                Value++;
                if (Value >= End)
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
                        break;
                    }
                }
            }
            tickCount++;
            diffTime -= Interval;
        }

        NowTime = nowTime - diffTime;
        return tickCount;
    }

    internal void Start(bool reset = false)
    {
        if (reset)
        {
            Reset();
        }
        if (State != TimerState.Started)
        {
            NowTime = Program.NowTime;
            Tick();
            State = TimerState.Started;
        }
    }

    internal void Stop()
    {
        if (State != TimerState.Stopped)
        {
            State = TimerState.Stopped;
        }
    }

    internal void Reset(bool IsStop = false)
    {
        if (IsStop)
        {
            Stop();
        }
        NowTime = Program.NowTime;
        Value = Begin;
    }

    internal void ChangeInterval(double interval)
    {
        Interval = interval;
    }

    internal void ChangeEnd(double end)
    {
        End = end;
        if (End < Value)
        {
            Value = End;
        }
    }

    internal void ChangeBegin(double begin)
    {
        Begin = begin;
        if (Begin > Value)
        {
            Value = Begin;
        }
    }
}
