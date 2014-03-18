using System;
using System.Linq;
using System.Reflection;

namespace FotM.Cassandra
{
    public class FeatureAttributeDescriptor<T>: IFeatureDescriptor<T>
    {
        private readonly PropertyInfo[] _featureProperties;
        private readonly AccordFeatureAttribute[] _attributeValues;

        public FeatureAttributeDescriptor()
        {
            _featureProperties = typeof (T).GetProperties()
                .Where(p => p.IsDefined(typeof (AccordFeatureAttribute), false))
                .ToArray();

            _attributeValues = _featureProperties
                .Select(p => p.GetCustomAttribute<AccordFeatureAttribute>(false))
                .ToArray();
        }

        public int TotalFeatures
        {
            get
            {
                return _featureProperties.Length;
            }
        }

        public double GetFeatureValue(int featureIdx, T obj)
        {
            var prop = _featureProperties[featureIdx];

            //double w = _attributeValues[featureIdx].Weight;

            return Convert.ToDouble(prop.GetValue(obj));
        }
    }
}