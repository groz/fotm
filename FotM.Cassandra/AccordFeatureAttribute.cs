using System;

namespace FotM.Cassandra
{
    public class AccordFeatureAttribute : Attribute
    {
        public AccordFeatureAttribute()
        {
            Weight = 1.0;
        }

        public double Weight { get; set; }
    }
}