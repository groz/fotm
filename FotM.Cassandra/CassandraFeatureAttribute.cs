using System;

namespace FotM.Cassandra
{
    public class CassandraFeatureAttribute : Attribute
    {
        public CassandraFeatureAttribute()
        {
            Weight = 1.0;
            Normalize = true;
        }

        public double Weight { get; set; }
        public bool Normalize { get; set; }
    }
}