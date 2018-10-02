using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Re.Base.Queryables.File
{
    public class FileQueryable<TModel> : IQueryable<TModel>, IQueryable, IEnumerable<TModel>, IEnumerable, IOrderedQueryable<TModel>, IOrderedQueryable
    {
        public FileQueryable(string databaseLocation)
        {
            var provider = new FileQueryProvider(databaseLocation);
            var expression = Expression.Constant(this);

            if (!typeof(IQueryable<TModel>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }

            this.Provider = provider ?? throw new ArgumentException("provider");
            this.Expression = expression ?? throw new ArgumentException("expression");
        }

        public FileQueryable(FileQueryProvider provider, Expression expression)
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
