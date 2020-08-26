﻿using System;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.FilterBuilder.Cacnonical
{
    public class IndexOfOperator : FilterPart
    {
        public IndexOfOperator(FilterPart left, FilterPart right)
        {
            Left = left;
            Right = right;
        }

        public FilterPart Left { get; private set; }
        public FilterPart Right { get; private set; }

        public override Expression Build() => Build(Left.Build());

        private Expression Build(Expression leftExpression)
        {
            if (leftExpression.Type == typeof(string))
                return leftExpression.GetStringIndexOfCall(Right.Build());
            else
                throw new ArgumentException(nameof(Left));
        }
    }
}
