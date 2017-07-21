using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace StalkHard.Models
{
    public class Intent
    {
        public string intent { get; set; }

        public double score { get; set; }
    }
}