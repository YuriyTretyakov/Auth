using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColibriWebApi.DL
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public string Picture { get; set; }
        public bool IsActive { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
