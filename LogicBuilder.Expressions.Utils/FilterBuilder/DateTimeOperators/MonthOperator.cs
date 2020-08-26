﻿using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.FilterBuilder.DateTimeOperators
{
    public class MonthOperator : FilterPart
    {
        public MonthOperator(FilterPart operand)
        {
            Operand = operand;
        }

        public FilterPart Operand { get; private set; }

        public override Expression Build() => Build(Operand.Build());

        private Expression Build(Expression operandExpression) 
            => operandExpression.MakeMonthSelector();
    }
}
