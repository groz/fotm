using System.Reflection;

namespace FotM.Utilities
{
    public class AttributedProperty<T, TAttribute>
    {
        public PropertyInfo PropertyInfo { get; private set; }
        public TAttribute Attribute { get; private set; }

        public AttributedProperty(PropertyInfo propertyInfoInfo, TAttribute attribute)
        {
            this.PropertyInfo = propertyInfoInfo;
            this.Attribute = attribute;
        }

        public TProperty GetValue<TProperty>(T obj)
        {
            return (TProperty)PropertyInfo.GetValue(obj);
        }
    }
}