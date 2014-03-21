using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FotM.Utilities
{
    public static class ReflectionUtils
    {
        public static AttributedProperty<T, TAttribute>[] GetAttributedProperties<T, TAttribute>() where TAttribute : Attribute
        {
            return typeof(T).GetProperties()
                .Where(p => p.IsDefined(typeof(TAttribute), false))
                .Select(p => new AttributedProperty<T, TAttribute>(p, p.GetCustomAttribute<TAttribute>()))
                .ToArray();
        }
    }
}
