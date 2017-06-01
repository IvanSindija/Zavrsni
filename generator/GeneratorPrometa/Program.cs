using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorPrometa
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] tipoviZahtjev = new string[] { "SYN-ACK", "ACK(SYN)" };
            int brojRedovaPoadatak = 10000;
            LinkedList<PrometData> cekaPotvrdu = new LinkedList<PrometData>();
            Random rand = new Random(DateTime.Now.Second);
            //vrsta seq 32bit ack32bit 4*8bit ipOd portOd uint16 iPza portZa
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("C:/faks/zavrsni/promet.csv"))
            {
                PrometData linijaPodataka = new PrometData();

                int postotakZahtjeva = 60;
                for (int i = 0; i < brojRedovaPoadatak; i++)
                {
                    int randVal = rand.Next(100);
                    //generiranje prometa
                    if (i == brojRedovaPoadatak * 0.5)
                    {
                        postotakZahtjeva = 30;
                        Console.WriteLine("broj zahtjeva" + cekaPotvrdu.Count);
                    }
                    //generiraj vise zahtjeva nego potvrda
                    if (randVal < postotakZahtjeva)
                    {
                        //stvori zahtjev
                        linijaPodataka = Util.GenerirajPromet("SYN-ACK", null, rand);
                        cekaPotvrdu.AddLast(linijaPodataka);
                    }
                    else if (randVal < postotakZahtjeva + 10 && randVal >= postotakZahtjeva)
                    {
                        //stvori laznu potvrdu
                        linijaPodataka = Util.GenerirajPromet("ACK(SYN)", null, rand);
                    }
                    else
                    {
                        //stvori potvrdu
                        PrometData saljemPotvrduZa = cekaPotvrdu.Skip(rand.Next(cekaPotvrdu.Count)).FirstOrDefault();
                        if (saljemPotvrduZa == null)
                        {
                            Console.WriteLine("Nemam što potvrditi");
                            linijaPodataka = Util.GenerirajPromet("SYN-ACK", null, rand);
                            cekaPotvrdu.AddLast(linijaPodataka);
                        }
                        else
                        {
                            linijaPodataka = Util.GenerirajPromet("ACK(SYN)", saljemPotvrduZa, rand);
                            cekaPotvrdu.Remove(saljemPotvrduZa);
                        }
                    }
                    //Console.WriteLine(linijaPodataka.ToString());
                    file.WriteLine(linijaPodataka.ToString());
                    if (brojRedovaPoadatak - i < cekaPotvrdu.Count-1)
                    {
                        //mora ih poceti zatvarai ili nece uspijeti
                        Console.WriteLine("Nasilno zatvaraj");
                        break;
                    }
                }
                Console.WriteLine("Preostalo za potvrditi"+cekaPotvrdu.Count);
                //zatvori preostale zahtjeve
                for (int i = 0; i < cekaPotvrdu.Count; i++)
                {
                    PrometData saljemPotvrduZa = cekaPotvrdu.Skip(i).FirstOrDefault();
                    linijaPodataka = Util.GenerirajPromet("ACK(SYN)", saljemPotvrduZa, rand);
                    file.WriteLine(linijaPodataka.ToString());
                }
            }
        }
    }

    public static class Util
    {
        public static PrometData RandomPrometData(this Random rng)
        {
            PrometData data = new PrometData();
            data.ASEQ = rng.Next();
            data.SEQ = rng.Next();
            data.SorcePort = (UInt16)rng.Next(UInt16.MaxValue);
            data.DestinationPort = (UInt16)rng.Next(UInt16.MaxValue);
            data.DestinationIP = rng.Next(1<<8).ToString() + "." + rng.Next(1<<8).ToString() + "." +
                rng.Next(1<<8).ToString() + "." + rng.Next(1<<8).ToString() + ".";
            data.SourceIP = rng.Next(1<<8).ToString() + "." + rng.Next(1<<8).ToString() + "." +
                rng.Next(1<<8).ToString() + "." + rng.Next(1<<8).ToString() + ".";
            return data;
        }

        public static PrometData GenerirajPromet(string vrsta, PrometData zahtjev, Random rng)
        {
            PrometData data = new PrometData();
            if (vrsta.Equals("SYN-ACK"))
            {
                data = rng.RandomPrometData();
                data.Vrsta = "SYN-ACK";
            }
            else
            {
                if (zahtjev == null)
                {
                    //generiraj lažnu potvrdu
                    //možda nema smisla raditi to ali tu ce postaojati mogucnost za false pozitive
                    data = rng.RandomPrometData();
                    data.Vrsta = "ACK(SYN)";
                }
                else
                {
                    //generirj validni respose za zahtjev
                    data.Vrsta = "ACK(SYN)";
                    data.DestinationIP = zahtjev.SourceIP;
                    data.DestinationPort = zahtjev.SorcePort;
                    data.SourceIP = zahtjev.DestinationIP;
                    data.SorcePort = zahtjev.DestinationPort;
                    data.SEQ = zahtjev.ASEQ;
                    data.ASEQ = zahtjev.SEQ + 1;
                }
            }
            return data;
        }
    }
}
