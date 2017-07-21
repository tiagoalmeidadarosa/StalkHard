using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace StalkHard.Models
{
    public class Utterance
    {
        [DataMember]
        public string query { get; set; }

        [DataMember]
        public Intent topScoringIntent { get; set; }

        [DataMember]
        public List<Intent> intents { get; set; }

        [DataMember]
        public List<Entity> entities { get; set; }
    }
}