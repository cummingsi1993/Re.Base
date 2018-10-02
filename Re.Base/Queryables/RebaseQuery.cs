using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Queryables
{
    internal class RebaseQuery
    {

        private List<Func<object, bool>> filters = new List<Func<object, bool>>();
        private List<Func<object, object>> selects = new List<Func<object, object>>();
        private Type source;

        internal void AddWhereFilter(Func<object, bool> filter)
        {
            filters.Add(filter);
        }

        internal void AddSelectClause(Func<object, object> projection)
        {
            selects.Add(projection);
        }

        internal void AddSource(Type sourceType)
        {
            source = sourceType;
        }

        internal bool ApplyFilter(object model)
        {
            foreach (Func<object, bool> filter in filters)
            {
                if (!filter(model))
                {
                    return false;
                }
            }

            return true;
        }

        internal object ApplyProjection(object model)
        {
            object currentObject = model;
            foreach(Func<object, object> projection in selects)
            {
                currentObject = projection(currentObject);
            }

            return currentObject;
        }

        internal Type GetSourceType()
        {
            return source;
        }



    }
}
