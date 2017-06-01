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
            int validanOdgovor = 0;
            int falsePromet = 0;

            using (StreamReader sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                {
                    string[] line = sr.ReadLine().Split(';');

                    if (olnyLoad.Contains(line[8]))
                    {
                        PrometData linija = new PrometData()
                        {
                            Vrsta = line[8],
                            SEQ = line[9],
                            ASEQ = Double.Parse(line[10]),
                            DestinationIP = line[14],
                            DestinationPort = line[15],
                            SourcePort = line[13],
                            SourceIP = line[12]
                        };
                        if (linija.Vrsta.Equals("SYN-ACK"))
                        {
                            if (validanPromet < n)
                            {
                                promet.AddLast(linija);
                                validanPromet++;
                            }
                        }
                        else if (linija.Vrsta.Equals("ACK(SYN)"))
                        {
                            bool pronaden = false;
                            foreach (PrometData zahtjevIzListe in promet)
                            {
                                if (zahtjevIzListe.SourceIP
                                        .Equals(linija.DestinationIP)
                                        && zahtjevIzListe.SourcePort
                                                .Equals(linija.DestinationPort)
                                        && zahtjevIzListe.DestinationIP
                                                .Equals(linija.SourceIP)
                                        && zahtjevIzListe.DestinationPort
                                                .Equals(linija.SourcePort)
                                        && zahtjevIzListe.ASEQ == Int64.Parse(linija.SEQ)
                                        && Int64
                                                .Parse(zahtjevIzListe.SEQ) == (linija.ASEQ - 1))
                                {
                                    pronaden = true;
                                }
                            }
                            if (!pronaden)
                            {
                                //		System.out.println("Desilo se sranje !"+line);
                            }
                            if (pronaden)
                            {
                                promet.AddLast(linija);
                                validanOdgovor++;
                            }
                        }
                        else
                        {
                            promet.AddLast(linija);
                            falsePromet++;
                        }
                    }
                    if (validanOdgovor == n)
                    {
                        Console.WriteLine("svi odgovori popunjeni sve OK");
                        break;
                    }
                }
            }
            Console.WriteLine("Validan promet=" + validanPromet + " Validan odgovor=" + validanOdgovor + " False promet=" + falsePromet);
            KolicinaTestnogPrometa = validanOdgovor + falsePromet;
            return promet;
        }
    }
}
