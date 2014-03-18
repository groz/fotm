using System;
using System.Linq;
using System.Reflection;

namespace FotM.Cassandra
{
    public class FeatureAttributeDescriptor<T>: IFeatureDescriptor<T>
    {
        private readonly PropertyInfo[] _featureProperties;

        public FeatureAttributeDescriptor()
        {
            _featureProperties = typeof (T).GetProperties()
                .Where(p => p.IsDefined(typeof (AccordFeatureAttribute), false))
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

            //double w = GetFeatureWeight(prop);

            return Convert.ToDouble(prop.GetValue(obj));
        }

        private static double GetFeatureWeight(PropertyInfo featureProperty)
        {
            var attributes = (AccordFeatureAttribute[])
                featureProperty.GetCustomAttributes(typeof(AccordFeatureAttribute), false);

            return attributes.First().Weight;
        }
    }
}