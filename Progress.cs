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
    /// Usage example
    /// </summary>
    class ProgressExample : Progress
    {
        readonly public Stage _LoadingPOs = new Stage { Step = 100, AsymptoticDelta = 1000 };
        readonly public Stage _LoadingInvoices = new Stage { Step = 10 };
        readonly public Stage _Recording = new Stage { };

        void exampleCode()
        {
            ProgressExample progress = new ProgressExample() { MaxProgress = 1000 };
            progress.OnProgress += delegate (Progress.Stage stage)
            {
                //MainForm.This.SetProgress(progress.GetProgress1(), stage.Maximum, stage.Value);
            };
            //...
            progress._LoadingPOs.Maximum = 100;
            for (int i = 1; i <= 100; i++)
                progress._LoadingPOs.Value = i;
        }
    }

    /// <summary>
    /// Used to display progress bar with multiple tasks.
    /// </summary>
    public class Progress
    {
        public class Stage
        {
            public string Name { get; internal set; }

            public float Weight
            {
                get
                {
                    return weight;
                }
                set
                {
                    if (value <= 0)
                        throw new Exception("Weight cannot be a non-positive number.");
                    weight = value;
                }
            }
            float weight = 1;

            public int Maximum
            {
                get
                {
                    return maximum;
                }
                set
                {
                    if (AsymptoticDelta != null)
                        throw new Exception("Maximum cannot be set when AsymptoticFactor is on.");
                    maximum = value;
                }
            }
            int maximum = 100;

            public int Value
            {
                get
                {
                    return value;
                }
                set
                {
                    lock (this)
                    {
                        if (value <= 0)
                            throw new Exception("Value cannot be a negative number.");
                        if (AsymptoticDelta == null)
                        {
                            if (value > Maximum)
                                value = Maximum;
                            this.value = value;
                        }
                        else
                        {
                            this.value = value;
                            maximum = (int)(this.value + AsymptoticDelta.Value);
                        }
                        if (value % Step == 0 && progress.OnProgress != null)
                            progress.OnProgress(this);
                    }
                }
            }
            int value;

            /// <summary>
            /// Used when Maximum cannot be determined at the beginning.
            /// </summary>
            public float? AsymptoticDelta { get; set; } = null;


            public int Step = 1;


            internal Progress progress;

            public Stage(string name = null)
            {
                Name = name;
            }
        }

        //public Progress(params Stage[] stages)
        //{
        //    this.stages = stages.ToList();
        //    this.stages.ForEach(a => a.progress = this);
        //}   
        
        //public Stage this[string stageName]
        //{
        //    get
        //    {
        //        return stages.FirstOrDefault(a => a.Name == stageName);
        //    }
        //}

        /// <summary>
        /// Auto-detects Stages in the deriving custom class
        /// </summary>
        public Progress()
        {
            stages = GetType()
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(a => typeof(Stage).IsAssignableFrom(a.FieldType))
                .Select(a =>
                {                    
                    Stage s = (Stage)a.GetValue(this);
                    if (s.Name == null)
                        s.Name = a.Name;
                    s.progress = this;
                    return s;
                })
                .ToList();
        }

        List<Stage> stages;

        public int MaxProgress = 100;

        public event Action<Stage> OnProgress;

        public void Reset()
        {
            stages.ForEach(a => a.Value = 0);
        }

        /// <summary>
        /// [0:1]
        /// </summary>
        /// <returns></returns>
        public float GetProgress1()
        {
            lock (this)
            {
                return stages.Sum(a => a.Weight * a.Value / a.Maximum) / stages.Sum(a => a.Weight);
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