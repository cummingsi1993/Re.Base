using Re.Base.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Re.Base.Queryables.Data
{
    public class DataQueryable<TModel> : IQueryable<TModel>, IQueryable, IEnumerable<TModel>, IEnumerable, IOrderedQueryable<TModel>, IOrderedQueryable
    {
        public DataQueryable(DataStore store)
        {
            var provider = new DataQueryProvider(store);
            var expression = Expression.Constant(this);

            if (!typeof(IQueryable<TModel>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }

            this.Provider = provider ?? throw new ArgumentException("provider");
            this.Expression = expression ?? throw new ArgumentException("expression");
        }

        public DataQueryable(DataQueryProvider provider, Expression expression)
        {
            if (!typeof(IQueryable<TModel>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }

            this.Provider = provider ?? throw new ArgumentException("provider");
            this.Expression = expression ?? throw new ArgumentException("expression");
        }

        public Type ElementType => typeof(TModel);

        public Expression Expression { get; private set; }

        public IQueryProvider Provider { get; private set; }

        public IEnumerator<TModel> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<TModel>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Provider.Execute<IEnumerable>(Expression).GetEnumerator();
        }
    }
}
