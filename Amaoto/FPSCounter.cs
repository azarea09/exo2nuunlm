namespace Amaoto
{
    /// <summary>
    /// FPSを計測するクラス。
    /// </summary>
    public class FPSCounter
    {
        private int NowFPS;

        private readonly Counter Counter;

        public int FPS { get; private set; }

        public FPSCounter()
        {
            NowFPS = 0;
            FPS = 0;
            Counter = new Counter(0.0, 999.0, 1000.0, isLoop: true);
        }

        private void Counter_Looped(object sender, EventArgs e)
        {
            FPS = NowFPS;
            NowFPS = 0;
        }

        public void Update()
        {
            if (Counter.State == TimerState.Stopped)
            {
                Counter.Start();
            }
            Counter.Tick();
            NowFPS++;
        }
    }
}