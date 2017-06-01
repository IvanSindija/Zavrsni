using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLoader
{
    public class PrometData
    {
        public string Time { get; set; } 
        private string localVsta;
        public string Vrsta {get;set;}
        public string SourceIP { get; set; }
        public string DestinationIP { get; set; }
        public string SourcePort { get; set; }
        public string DestinationPort { get; set; }
        public string SEQ { get; set; }
        //aseq je int da bude lakse oduzeti 1 za cliAck
        public double ASEQ { get; set; }
    }
}
