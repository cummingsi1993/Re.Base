using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Re.Base.Queryables.Data
{
    internal class QueryTranslator : ExpressionVisitor
    {

        private RebaseQuery query;

        internal RebaseQuery Translate(Expression expression)
        {
            query = new RebaseQuery();
            this.Visit(expression);
            return query;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                query.AddSource(q.ElementType);
                return c;
            }
            else
            {
                return base.VisitConstant(c);
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {

            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                this.Visit(m.Arguments[0]);

                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                var compiled = lambda.Compile();
                query.AddWhereFilter(model => (bool)compiled.DynamicInvoke(model));

                this.Visit(lambda.Body);

                return m;
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Select")
            {
                this.Visit(m.Arguments[0]);

                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                var compiled = lambda.Compile();
                query.AddSelectClause(model => compiled.DynamicInvoke(model));

                this.Visit(lambda.Body);

                return m;
            }
            else
            {
                return base.VisitMethodCall(m);
            }

        }

    }
}
