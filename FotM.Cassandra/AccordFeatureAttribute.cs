using System;

namespace FotM.Cassandra
{
    public class AccordFeatureAttribute : Attribute
    {
        public AccordFeatureAttribute()
        {
            Weight = 1.0;
            Normalize = true;
        }

        public double Weight { get; set; }
        public bool Normalize { get; set; }
    }
}