using System;

namespace FotM.Cassandra
{
    public class FeatureAttribute : Attribute
    {
        public FeatureAttribute()
        {
            Weight = 1.0;
        }

        public double Weight { get; set; }
    }
}