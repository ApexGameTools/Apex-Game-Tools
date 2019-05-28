/* Copyright © 2014 Apex Software. All rights reserved. */
#pragma warning disable 1591
namespace Apex.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class DebugTimer
    {
        private static Stack<Stopwatch> _watches = new Stack<Stopwatch>();
        private static Stopwatch _avgWatch;
        private static float _iterations;
        private static int _count;
        private static float _avg;

        [Conditional("UNITY_EDITOR")]
        public static void Start()
        {
            var sw = new Stopwatch();
            _watches.Push(sw);
            sw.Start();
        }

        [Conditional("UNITY_EDITOR")]
        public static void EndTicks(string label)
        {
            _watches.Peek().Stop();
            var sw = _watches.Pop();

            UnityEngine.Debug.Log(string.Format(label, sw.ElapsedTicks));
        }

        [Conditional("UNITY_EDITOR")]
        public static void EndMilliseconds(string label)
        {
            _watches.Peek().Stop();
            var sw = _watches.Pop();

            UnityEngine.Debug.Log(string.Format(label, sw.ElapsedMilliseconds));
        }

        [Conditional("UNITY_EDITOR")]
        public static void StartAverage(int iterations)
        {
            if (_count <= 0)
            {
                _avg = 0f;
                _iterations = _count = iterations;
                _avgWatch = Stopwatch.StartNew();
            }
            else
            {
                _avgWatch.Reset();
                _avgWatch.Start();
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void EndAverageTicks(string label)
        {
            _avgWatch.Stop();
            var tmp = (_avgWatch.ElapsedTicks / _iterations);

            //Skip the first call as it is always off
            if (_count < _iterations)
            {
                _avg += tmp;
            }

            if (--_count == 0)
            {
                UnityEngine.Debug.Log(string.Format(label, _avg));
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void EndAverageMilliseconds(string label)
        {
            _avgWatch.Stop();
            var tmp = (_avgWatch.ElapsedMilliseconds / _iterations);

            //Skip the first call as it is always off
            if (_count < _iterations)
            {
                _avg += tmp;
            }

            if (--_count == 0)
            {
                UnityEngine.Debug.Log(string.Format(label, _avg));
            }
        }
    }
}
