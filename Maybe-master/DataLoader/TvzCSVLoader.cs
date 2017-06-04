using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLoader
{
    public class TvzCSVLoader : ILoader
    {
        private int n;
        private string fileName;
        public int KolicinaTestnogPrometa { get; set; }
        public TvzCSVLoader(string fileName, int n)
        {
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            this.fileName = path + fileName;
            this.n = n;
        }

        public LinkedList<PrometData> Load(string[] olnyLoad)
        {
            LinkedList<PrometData> promet = new LinkedList<PrometData>();
            int validanPromet = 0;

            using (StreamReader sr = new StreamReader(fileName))
            {
                int brojac = 0;
                while (!sr.EndOfStream)
                {
                    if (brojac >= n)
                    {
                        break;
                    }
                    brojac++;
                    string[] line = sr.ReadLine().Split(';');

                    if (olnyLoad.Contains(line[8]))
                    {
                        PrometData linija = new PrometData()
                        {
                            Time = line[3],
                            Vrsta = line[8],
                            SEQ = line[9],
                            ASEQ = Double.Parse(line[10]),
                            DestinationIP = line[14],
                            DestinationPort = line[15],
                            SourcePort = line[13],
                            SourceIP = line[12]
                        };
                        promet.AddLast(linija);
                        if (!linija.Vrsta.Equals("SYN-ACK"))
                        {
                              validanPromet++;
                        }
                    }
                }
            }
            KolicinaTestnogPrometa = validanPromet;
            return promet;
        }
    }
}
