/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor.Reflection
{
    using System;

    internal class DependencyCheck
    {
        private IEditorField _dependee;
        private object _satisfiedBy;
        private MaskMatch _match;
        private CompareOperator _compare;
        private bool _isMask;
        private bool _isInt;

        internal DependencyCheck(IEditorField dependee, object satisfiedBy, MaskMatch match, CompareOperator compare, bool isMask)
        {
            _dependee = dependee;
            _satisfiedBy = satisfiedBy;
            _match = match;
            _compare = compare;
            _isInt = _satisfiedBy is int;
            _isMask = isMask && _isInt;
        }

        internal bool isSatisfied
        {
            get
            {
                if (_isMask)
                {
                    int cur = (int)_dependee.currentValue;
                    int test = (int)_satisfiedBy;
                    switch (_match)
                    {
                        case MaskMatch.Partial:
                        {
                            return (cur & test) > 0;
                        }

                        case MaskMatch.Strict:
                        {
                            return (cur & test) == test;
                        }

                        case MaskMatch.Equals:
                        {
                            return cur == test;
                        }

                        default:
                        {
                            return (cur & test) == 0;
                        }
                    }
                }
                else
                {
                    var lhs = _dependee.currentValue as IComparable;
                    var rhs = _satisfiedBy as IComparable;
                    if (lhs == null || rhs == null)
                    {
                        return false;
                    }

                    int res = lhs.CompareTo(rhs);
                    if (res < 0)
                    {
                        return _compare == CompareOperator.LessThan || _compare == CompareOperator.LessThanOrEquals || _compare == CompareOperator.NotEquals;
                    }
                    else if (res > 0)
                    {
                        return _compare == CompareOperator.GreaterThan || _compare == CompareOperator.GreaterThanOrEquals || _compare == CompareOperator.NotEquals;
                    }

                    return _compare == CompareOperator.Equals ||
                           _compare == CompareOperator.LessThanOrEquals ||
                           _compare == CompareOperator.GreaterThanOrEquals;
                }
            }
        }
    }
}
