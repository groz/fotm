using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MoreLinq;

namespace FotM.Cassandra
{
    public class FeatureAttributeDescriptor<T>: IFeatureDescriptor<T>
    {
        private readonly Dictionary<string, int> _featureIndices;
        private readonly PropertyInfo[] _featureProperties;
        private readonly AccordFeatureAttribute[] _attributeValues;

        public FeatureAttributeDescriptor()
        {
            _featureProperties = typeof (T).GetProperties()
                .Where(p => p.IsDefined(typeof (AccordFeatureAttribute), false))
                .ToArray();

            _featureIndices = _featureProperties
                .Select((prop, idx) => new { prop, idx })
                .ToDictionary(pi => pi.prop.Name, pi => pi.idx);

            _attributeValues = _featureProperties
                .Select(p => p.GetCustomAttribute<AccordFeatureAttribute>(false))
                .ToArray();
        }

        public HashSet<string> Features
        {
            get { return _featureIndices.Keys.ToHashSet(); }
        }

        public int TotalFeatures
        {
            get
            {
                return _featureProperties.Length;
            }
        }

        public int GetFeatureIndex(string name)
        {
            int idx;

            if (_featureIndices.TryGetValue(name, out idx))
            {
                return idx;
            }

            string msg = string.Format("Feature with name {0} couldn't be found", name);
            throw new ArgumentException(msg);
        }

        public double GetFeatureValue(string featureName, T obj)
        {
            var idx = GetFeatureIndex(featureName);
            return GetFeatureValue(idx, obj);
        }

        public double GetFeatureValue(int featureIdx, T obj)
        {
            var prop = _featureProperties[featureIdx];

            //double w = _attributeValues[featureIdx].Weight;

            return Convert.ToDouble(prop.GetValue(obj));
        }
    }
}