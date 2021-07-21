using System;
using System.Collections.Generic;

namespace WebStats.Persistence.Model {
    public class AggregateInfo {
        public int ServiceId { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}