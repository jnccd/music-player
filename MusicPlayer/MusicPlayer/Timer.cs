using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicPlayer
{
    public class Timer
    {
        bool Started;
        public event EventHandler Finished;
        int Counter;
        int EndCount;

        public Timer(int Startcount, int EndCount, EventHandler Finished)
        {
            this.Counter = Startcount;
            this.EndCount = EndCount;
            this.Finished = Finished;
        }

        public void Start() { Started = true; }

        public void Update()
        {
            if (Started)
            {
                Counter++;

                if (Counter > EndCount)
                {
                    Finished(this, EventArgs.Empty);
                    Started = false;
                    Counter = 0;
                }
            }
        }
    }
}
