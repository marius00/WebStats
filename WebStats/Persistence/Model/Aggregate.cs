using System.Collections.Generic;

namespace WebStats.Persistence {
    public class Aggregate {
        public List<int> Past24H { get; set; }
        public List<int> Past7d { get; set; }
        public List<int> Past14d { get; set; }
        public List<int> Past30d { get; set; }
    }
}