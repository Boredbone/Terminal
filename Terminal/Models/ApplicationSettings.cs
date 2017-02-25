using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Terminal.Models
{
    [DataContract]
    public class ApplicationSettings
    {
        [DataMember]
        public string PortName { get; set; }


        [DataMember]
        public bool NoFeedAfterSend { get; set; }


        [DataMember]
        public List<string> CommandHistory { get; set; }
    }
}
