using System;

namespace GeneratorPrometa
{
    public class PrometData
    {
        public string Vrsta{get; set;}
        public string SourceIP { get; set; }
        public string DestinationIP { get; set; }
        public UInt16 SorcePort { get; set; }
        public UInt16 DestinationPort { get; set; }
        public int SEQ { get; set; }
        //aseq je int da bude lakse oduzeti 1 za cliAck
        public int ASEQ { get; set; }

        public string ToString()
        {
            return Vrsta + "," + SEQ.ToString() + "," + ASEQ.ToString() +","+ SourceIP + "," + SorcePort.ToString() + "," + DestinationIP + "," + DestinationPort.ToString();
        }
    }
}