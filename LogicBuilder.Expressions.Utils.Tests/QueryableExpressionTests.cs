using Contoso.Data.Entities;
using LogicBuilder.Expressions.Utils.ExpressionBuilder;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Arithmetic;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Cacnonical;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Collection;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Lambda;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Logical;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Operand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace LogicBuilder.Expressions.Utils.Tests
{
    public class QueryableExpressionTests
    {
        [Fact]
        public void BuildWhere_OrderBy_ThenBy_Skip_Take_Average()
        {
            //arrange
            var parameters = GetParameters();

            //{q => q.Where(s => ((s.ID > 1) AndAlso (Compare(s.FirstName, s.LastName) > 0))).OrderBy(v => v.LastName).ThenByDescending(v => v.FirstName).Skip(2).Take(3).Average(j => j.ID)}
            Expression<Func<IQueryable<Student>, double>> expression = new AverageOperator
            (
                parameters,
                new TakeOperator
                (
                    new SkipOperator
                    (
                        new ThenByOperator
                        (
                            parameters,
                            new OrderByOperator
                            (
                                parameters,
                                new WhereOperator
                                (//q.Where(s => ((s.ID > 1) AndAlso (Compare(s.FirstName, s.LastName) > 0)))
                                    parameters,
                                    new ParameterOperator(parameters, "q"),//q. the source operand
                                    new AndBinaryOperator//((s.ID > 1) AndAlso (Compare(s.FirstName, s.LastName) > 0)
                                    (
                                        new GreaterThanBinaryOperator
                                        (
                                            new MemberSelectorOperator("Id", new ParameterOperator(parameters, "s")),
                                            new ConstantOperator(1, typeof(int))
                                        ),
                                        new GreaterThanBinaryOperator
                                        (
                                            new MemberSelectorOperator("FirstName", new ParameterOperator(parameters, "s")),
                                            new MemberSelectorOperator("LastName", new ParameterOperator(parameters, "s"))
                                        )
                                    ),
                                    "s"//s => (created in Where operator.  The parameter type is based on the source operand underlying type in this case Student.)
                                ),
                                new MemberSelectorOperator("LastName", new ParameterOperator(parameters, "v")),
                                Strutures.ListSortDirection.Ascending,
                                "v"
                            ),
                            new MemberSelectorOperator("FirstName", new ParameterOperator(parameters, "v")),
                            Strutures.ListSortDirection.Descending,
                            "v"
                        ),
                        2
                    ),
                    3
                ),
                new MemberSelectorOperator("Id", new ParameterOperator(parameters, "j")),
                "j"
            )
            .GetExpression<IQueryable<Student>, double>(parameters, "q");

            Assert.NotNull(expression);
        }

        [Fact]
        public void BuildGroupBy_OrderBy_ThenBy_Skip_Take_Average()
        {
            //arrange
            var parameters = GetParameters();

            Expression<Func<IQueryable<Department>, IQueryable<object>>> expression1 =
                q => q.GroupBy(a => 1)
                    .OrderBy(b => b.Key)
                    .Select
                    (
                        c => new 
                        { 
                            Sum_budget = q.Where
                            (
                                d => ((d.DepartmentID == q.Count()) 
                                    && (d.DepartmentID == c.Key))
                            )
                            .ToList()
                        }
                    );

            Expression<Func<IQueryable<Department>, IQueryable<object>>> expression = new SelectOperator
            (
                parameters,
                new OrderByOperator
                (
                    parameters,
                    new GroupByOperator
                    (
                        parameters,
                        new ParameterOperator(parameters, "q"),
                        new ConstantOperator(1, typeof(int)),
                        "a"
                    ),
                    new MemberSelectorOperator("Key", new ParameterOperator(parameters, "b")),
                    Strutures.ListSortDirection.Ascending,
                    "b"
                ),
                new MemberInitOperator
                (
                    new Dictionary<string, IExpressionPart>
                    {
                        ["Sum_budget"] = new ToListOperator
                        (
                            new WhereOperator
                            (
                                parameters,
                                new ParameterOperator(parameters, "q"),
                                new AndBinaryOperator
                                (
                                    new EqualsBinaryOperator
                                    (
                                        new MemberSelectorOperator("DepartmentID", new ParameterOperator(parameters, "d")),
                                        new CountOperator(new ParameterOperator(parameters, "q"))
                                    ),
                                    new EqualsBinaryOperator
                                    (
                                        new MemberSelectorOperator("DepartmentID", new ParameterOperator(parameters, "d")),
                                        new MemberSelectorOperator("Key", new ParameterOperator(parameters, "c"))
                                    )
                                ),
                                "d"
                            )
                        )
                    }
                ),
                "c"
            )
            .GetExpression<IQueryable<Department>, IQueryable<object>>(parameters, "q");

            Assert.NotNull(expression);
        }

        [Fact]
        public void BuildGroupBy_AsQueryable_OrderBy_Select_FirstOrDefault()
        {
            //arrange
            var parameters = GetParameters();

            Expression<Func<IQueryable<Department>, object>> expression1 =
                q => q.GroupBy(item => 1)
                .AsQueryable()
                .OrderBy(group => group.Key)
                .Select
                (
                    sel => new
                    {
                        Min_administratorName = q.Where(d => (1 == sel.Key)).Min(item => string.Concat(string.Concat(item.Administrator.LastName, " "), item.Administrator.FirstName)),
                        Count_name = q.Where(d => (1 == sel.Key)).Count(),
                        Sum_budget = q.Where(d => (1 == sel.Key)).Sum(item => item.Budget),
                        Min_budget = q.Where(d => (1 == sel.Key)).Min(item => item.Budget),
                        Min_startDate = q.Where(d => (1 == sel.Key)).Min(item => item.StartDate)
                    }
                )
                .FirstOrDefault();

            Expression<Func<IQueryable<Department>, object>> expression = new FirstOrDefaultOperator
            (
                new SelectOperator
                (
                    parameters,
                    new OrderByOperator
                    (
                        parameters,
                        new AsQueryableOperator
                        (
                            new GroupByOperator
                            (
                                parameters,
                                new ParameterOperator(parameters, "q"),
                                new ConstantOperator(1, typeof(int)),
                                "item"
                            )
                        ),
                        new MemberSelectorOperator("Key", new ParameterOperator(parameters, "group")),
                        Strutures.ListSortDirection.Ascending,
                        "group"
                    ),
                    new MemberInitOperator
                    (
                        new Dictionary<string, IExpressionPart>
                        {
                            ["Min_administratorName"] = new MinOperator
                            (
                                parameters,
                                new WhereOperator
                                (
                                    parameters,
                                    new ParameterOperator(parameters, "q"),
                                    new EqualsBinaryOperator
                                    (
                                        new ConstantOperator(1, typeof(int)),
                                        new MemberSelectorOperator("Key", new ParameterOperator(parameters, "sel"))
                                    ),
                                    "d"
                                ),
                                new ConcatOperator
                                (
                                    new ConcatOperator
                                    (
                                        new MemberSelectorOperator("Administrator.LastName", new ParameterOperator(parameters, "item")), 
                                        new ConstantOperator(" ", typeof(string))
                                    ),
                                    new MemberSelectorOperator("Administrator.FirstName", new ParameterOperator(parameters, "item"))
                                ),
                                "item"
                            ),
                            ["Count_name"] = new CountOperator
                            (
                                new WhereOperator
                                (
                                    parameters,
                                    new ParameterOperator(parameters, "q"),
                                    new EqualsBinaryOperator
                                    (
                                        new ConstantOperator(1, typeof(int)),
                                        new MemberSelectorOperator("Key", new ParameterOperator(parameters, "sel"))
                                    ),
                                    "d"
                                )
                            ),
                            ["Sum_budget"] = new SumOperator
                            (
                                parameters,
                                new WhereOperator
                                (
                                    parameters,
                                    new ParameterOperator(parameters, "q"),
                                    new EqualsBinaryOperator
                                    (
                                        new ConstantOperator(1, typeof(int)),
                                        new MemberSelectorOperator("Key", new ParameterOperator(parameters, "sel"))
                                    ),
                                    "d"
                                ),
                                new MemberSelectorOperator("Budget", new ParameterOperator(parameters, "item")),
                                "item"
                            ),
                            ["Min_budget"] = new MinOperator
                            (
                                parameters,
                                new WhereOperator
                                (
                                    parameters,
                                    new ParameterOperator(parameters, "q"),
                                    new EqualsBinaryOperator
                                    (
                                        new ConstantOperator(1, typeof(int)),
                                        new MemberSelectorOperator("Key", new ParameterOperator(parameters, "sel"))
                                    ),
                                    "d"
                                ),
                                new MemberSelectorOperator("Budget", new ParameterOperator(parameters, "item")),
                                "item"
                            ),
                            ["Min_startDate"] = new MinOperator
                            (
                                parameters,
                                new WhereOperator
                                (
                                    parameters,
                                    new ParameterOperator(parameters, "q"),
                                    new EqualsBinaryOperator
                                    (
                                        new ConstantOperator(1, typeof(int)),
                                        new MemberSelectorOperator("Key", new ParameterOperator(parameters, "sel"))
                                    ),
                                    "d"
                                ),
                                new MemberSelectorOperator("StartDate", new ParameterOperator(parameters, "item")),
                                "item"
                            )
                        }
                    ),
                    "sel"
                )
            ).GetExpression<IQueryable<Department>, object>(parameters, "q");

            Assert.NotNull(expression);
        }

        [Fact]
        public void Get_Select_New()
        {
            Type queryableType = typeof(IQueryable<Department>);
            ParameterExpression param = Expression.Parameter(queryableType, "q");
            Expression exp = param.GetSelectNew<Department>
            (
                new Dictionary<string, string> { { "Name", "Name" } }
            );
            Assert.NotNull(exp);
        }

        private static IDictionary<string, ParameterExpression> GetParameters()
            => new Dictionary<string, ParameterExpression>();
    }
}
