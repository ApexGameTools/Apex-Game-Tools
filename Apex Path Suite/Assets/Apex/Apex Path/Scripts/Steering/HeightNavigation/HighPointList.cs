/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Steering.HeightNavigation
{
    using System;
    using Apex.Utilities;
    using UnityEngine;

    internal class HighPointList
    {
        private Vector3[] _points;
        private int _head;
        private int _used;

        private Vector3 _lastHigh;
        private float _lastDelta = Consts.InfiniteDrop;

        internal HighPointList(int capacity, Vector3 startHigh)
        {
            _points = new Vector3[capacity];
            _lastHigh = startHigh;
        }

        internal Vector3 current
        {
            get { return _points[_head]; }
        }

        internal float currentHeight
        {
            get { return _points[_head].y; }
        }

        internal int count
        {
            get { return _used; }
        }

        internal void MoveNext()
        {
            _used--;
            _head = (_head + 1) % _points.Length;
        }

        internal void RegisterHighpoint(Vector3 proposed)
        {
            var p = _lastHigh;
            var add = (_lastDelta > 0f && proposed.y <= _lastHigh.y);
            _lastDelta = proposed.y - _lastHigh.y;
            _lastHigh = proposed;

            if (!add)
            {
                return;
            }

            var length = _points.Length;
            for (int i = 0; i < _used; i++)
            {
                var idx = (_head + i) % length;
                if (_points[idx].y <= p.y)
                {
                    _points[idx] = p;
                    _used = i + 1;
                    return;
                }
            }

            if (_used == length)
            {
                var newArray = new Vector3[2 * length];

                if (_head == 0)
                {
                    Array.Copy(_points, 0, newArray, 0, _used);
                }
                else
                {
                    Array.Copy(_points, _head, newArray, 0, length - _head);
                    Array.Copy(_points, 0, newArray, length - _head, _head);
                }

                _points = newArray;
                _head = 0;
                length = _points.Length;
            }

            var tail = (_head + _used) % length;

            _used++;
            _points[tail] = p;
        }
    }
}
