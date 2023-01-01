//********************************************************************************************
//Author: Sergiy Stoyan
//        systoyan@gmail.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Cliver
{
    /// <summary>
    /// Used to display progress bar with multiple tasks.
    /// </summary>
    public class Progress
    {
        public class Stage
        {
            public string Name { get; }
            public float Weight { get; set; } = 1;
            public int MaxProgress { get; set; }
            public int Progress
            {
                get
                {
                    return progress;
                }
                set
                {
                    lock (this)
                    {
                        if (value > MaxProgress)
                            value = MaxProgress;
                        progress = value;
                        if (progress % Step == 0)
                            _progress.OnProgress?.BeginInvoke();
                    }
                }
            }
            int progress;

            public int Step = 1;

            internal Progress _progress;

            public Stage(string name, float weight)
            {
                Name = name;
                Weight = weight;
                //MaxProgress = maxProgress;
            }
        }

        public Progress(params Stage[] stages)
        {
            Stages = stages.ToList();
            Stages.ForEach(a => a._progress = this);
        }

        List<Stage> Stages;//{ get; private set; }

        public Stage this[string stageName]
        {
            get
            {
                return Stages.FirstOrDefault(a => a.Name == stageName);
            }
        }

        public int MaxProgress = 1;

        public event Action OnProgress;

        public void Reset()
        {
            Stages.ForEach(a => a.Progress = 0);
        }

        /// <summary>
        /// [0:1]
        /// </summary>
        /// <returns></returns>
        public float GetProgress1()
        {
            lock (this)
            {
                return Stages.Sum(a => a.Weight * a.Progress / a.MaxProgress) / Stages.Sum(a => a.Weight);
            }
        }

        /// <summary>
        /// [0:MaxProgress]
        /// </summary>
        /// <returns></returns>
        public int GetProgress()
        {
            return (int)(MaxProgress * GetProgress1());
        }
    }
}