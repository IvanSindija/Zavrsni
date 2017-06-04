using DataLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using Maybe.BloomFilter;
using System.Diagnostics;
using System.Globalization;

namespace AnalizatorMreznogPrometa
{
    class Program
    {
        static void Main(string[] args)
        {
            int maxArraySize = 1 << 12;
            bool speedTest = false;
            int brojIntervala = 60;
            LinkedList<LinkedList<Int32>> sveKolicinePodataka = new LinkedList<LinkedList<Int32>>();
            LinkedList<LinkedList<Int32>> sveGreske = new LinkedList<LinkedList<Int32>>();
            LinkedList<LinkedList<string>> svePopunjenostiFiltera = new LinkedList<LinkedList<string>>();

            LinkedList<string> xTime = new LinkedList<string>();
            bool timeUzeto = false;
            ILoader loaderMreznogPrometa = new TvzCSVLoader("/promet_veci_sa_bloom_log02032016.csv",Int32.MaxValue);
            LinkedList<PrometData> cjelokupanPromet = loaderMreznogPrometa.Load(new string[] { "ACK(SYN)", "ACK(FIN(DOLJE))", "ACK(FIN(GORE))", "ACK(PUSH_GORE)", "ACK(PUSH_DOLJE)", "SYN-ACK" });
            //ILoader loaderMreznogPrometa = new CsvLoader("/promet.csv");
          //  LinkedList<PrometData> cjelokupanPromet = loaderMreznogPrometa.Load(new string[] { "ACK(SYN)", "SYN-ACK" });

            Console.WriteLine("Kolicina prometa =" + cjelokupanPromet.Count);
            int VelicinaPoljaZaDodatneTestove = 1 << 11;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/cSharpMurmurGreske.csv"))
            {
                for (int numHashes = 1; numHashes < 8; numHashes++)
                {
                    LinkedList<double> brojGresaka = new LinkedList<double>();
                    Console.WriteLine("Broj Hash funkcija" + numHashes);
                    for (int arraySize = 1; arraySize <= maxArraySize; arraySize += 128)
                    {
                        LinkedList<string> popunjenostFiltera = new LinkedList<string>();
                        Console.WriteLine("prolaz za velicinu" + arraySize);
                        LinkedList<PrometData> dodanoUBloomFilter = new LinkedList<PrometData>();
                        LinkedList<PrometData> falsePromet = new LinkedList<PrometData>();
                        LinkedList<int> kolicinaCalnova = new LinkedList<int>();
                        LinkedList<Int32> kolicinaGresaka = new LinkedList<Int32>();

                        Stopwatch stoperica = new Stopwatch();
                        CountingBloomFilter<string> bloomFilter = new CountingBloomFilter<string>(arraySize, numHashes);
                        if (speedTest)
                        {
                            stoperica.Start();
                        }
                        #region obrada prometa
                        foreach (PrometData zahtjev in cjelokupanPromet)
                        {
                            if (zahtjev.Vrsta.Equals("SYN-ACK"))
                            {
                                //dodaj u filter <SIP, DIP, SP, DP, SEQ, ASEQ>
                                if (!speedTest)
                                {
                                    dodanoUBloomFilter.AddLast(zahtjev);
                                }
                                string x = zahtjev.SourceIP + zahtjev.DestinationIP + zahtjev.SourcePort + zahtjev.DestinationPort
                                    + zahtjev.SEQ + zahtjev.ASEQ;
                                bloomFilter.Add(x);
                            }
                            else
                            {
                                //provjeri dali je vec dodoano onda makni iz filtera <DIP, SIP, DP, SP, ASEQ−1, SEQ>
                                string x = zahtjev.DestinationIP + zahtjev.SourceIP + zahtjev.DestinationPort + zahtjev.SourcePort
                                    + (zahtjev.ASEQ - 1) + zahtjev.SEQ;
                                // Console.WriteLine("Provjeravam: " + x);
                                bool sadrzanoUBloomFIltru = bloomFilter.Contains(x);
                                if (speedTest)
                                {
                                    if (sadrzanoUBloomFIltru)
                                    {
                                        bloomFilter.Remove(x);
                                    }
                                    continue;
                                }
                                #region provjera
                                PrometData odgovarajuciZahtjevIzListe = dodanoUBloomFilter.Where(
                                    zahtjevIzListe => zahtjevIzListe.SourceIP.Equals(zahtjev.DestinationIP) &&
                                                     zahtjevIzListe.SourcePort.Equals(zahtjev.DestinationPort) &&
                                                     zahtjevIzListe.DestinationIP.Equals(zahtjev.SourceIP) &&
                                                     zahtjevIzListe.DestinationPort.Equals(zahtjev.SourcePort) &&
                                                     zahtjevIzListe.ASEQ == Double.Parse(zahtjev.SEQ) &&
                                                     Double.Parse(zahtjevIzListe.SEQ) == zahtjev.ASEQ - 1
                                    ).FirstOrDefault();//nebi se trebalo destit da se dobije tu vise od 1 clan ali razmisliti o poboljsanju
                                bool sadrzanoUListiZaProvjeru = odgovarajuciZahtjevIzListe != null;//ovo bi trebalo uvijek biti sadrzano ali za svaki slucaj

                                if (sadrzanoUBloomFIltru)
                                {
                                    bloomFilter.Remove(x);
                                    if (!sadrzanoUListiZaProvjeru)
                                    {
                                        falsePromet.AddLast(zahtjev);
                                    }
                                    else
                                    {
                                        dodanoUBloomFilter.Remove(odgovarajuciZahtjevIzListe);
                                    }
                                }
                                else
                                {
                                    if (sadrzanoUListiZaProvjeru)
                                    {
                                        falsePromet.AddLast(zahtjev);
                                        dodanoUBloomFilter.Remove(odgovarajuciZahtjevIzListe);

                                    }
                                }
                                #endregion
                            }

                            if (arraySize == VelicinaPoljaZaDodatneTestove)
                            {
                                if (!timeUzeto)
                                {
                                    xTime.AddLast(zahtjev.Time);
                                }
                                popunjenostFiltera.AddLast(bloomFilter.GetPopunjenost());
                                kolicinaCalnova.AddLast(bloomFilter.BrojClanova);
                                kolicinaGresaka.AddLast(falsePromet.Count());
                            }
                        }
                        #endregion


                        if (speedTest)
                        {
                            stoperica.Stop();
                            Console.WriteLine("Proslo je vremena" + stoperica.ElapsedMilliseconds + "ms");
                        }
                        else
                        {
                            brojGresaka.AddLast(((double)falsePromet.Count) / loaderMreznogPrometa.KolicinaTestnogPrometa);

                            if (kolicinaCalnova.Count > 0)
                            {
                                timeUzeto = true;
                                sveKolicinePodataka.AddLast(kolicinaCalnova);
                                sveGreske.AddLast(kolicinaGresaka);
                            }
                        }
                        if (arraySize == 1)
                        {
                            arraySize = 0;
                        }
                    }
                    string greskeString = "";
                    for (int i = 0; i < brojGresaka.Count; i++)
                    {
                        if (i == (brojGresaka.Count - 1))
                        {
                            greskeString += (brojGresaka.Skip(i).First().ToString()).Replace(",", ".");
                            break;
                        }
                        greskeString += (brojGresaka.Skip(i).First().ToString()).Replace(",", ".") + ",";
                    }
                    file.WriteLine(greskeString);
                    Console.WriteLine("greske:" + greskeString);
                }
            }

            int j = 0;
            foreach (var popunjenostK in svePopunjenostiFiltera)
            {
                j++;
                Console.WriteLine("cSharpPopunjenostFiltera_K_" + j);
                using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/cSharpPopunjenostFiltera_K_" + j + ".csv"))
                {
                    foreach (string lineTakta in popunjenostK)
                    {
                        //Console.WriteLine(line);
                        pisac.WriteLine(lineTakta);
                    }
                }
            }
            
            Console.WriteLine("cSBrojClanova");
            using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/cSBrojClanova.csv"))
            {
                foreach (var kolicinaK in sveKolicinePodataka)
                {

                    string ispis = "";
                    for (int i = 0; i < kolicinaK.Count; i++)
                    {
                        if (i == kolicinaK.Count - 1)
                        {
                            ispis += kolicinaK.ElementAt(i);
                        }
                        else
                        {
                            ispis += kolicinaK.ElementAt(i) + ",";
                        }
                    }
                    pisac.WriteLine(ispis);
                }
            }

            Console.WriteLine("sve greske");
            using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/greskeMurMur.csv"))
            {
                foreach (var greskeK in sveGreske)
                {
                    string ispis = "";
                    for (int i = 0; i < greskeK.Count; i++)
                    {
                        if (i == greskeK.Count - 1)
                        {
                            ispis += greskeK.ElementAt(i);
                        }
                        else
                        {
                            ispis += greskeK.ElementAt(i) + ",";
                        }

                    }
                    pisac.WriteLine(ispis);
                }
            }
            //using (var outfile = new StreamWriter(myRemoteFilePath, false, Encoding.ASCII, 0x10000))
            //brže pisanje
            Console.WriteLine("sve murmurVrijeme");
            LinkedList<Int64> microsecondsTime = new LinkedList<Int64>();

            using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/murmurVrijeme.csv"))
            {
                string ispis = "";
                for (int i = 0; i < xTime.Count; i++)
                {            
                    String[] timeRazdjeljeno = xTime.ElementAt(i).Split(':');
                    Int64 timeStamp = Int64.Parse(timeRazdjeljeno[3]) + Int64.Parse(timeRazdjeljeno[2]) * 1000000 +
                            Int64.Parse(timeRazdjeljeno[1]) * 100 + Int64.Parse(timeRazdjeljeno[0]) * 60;
                    microsecondsTime.AddLast(timeStamp);

                    if (i == xTime.Count - 1)
                    {
                        ispis += timeStamp.ToString();
                    }
                    else
                    {
                        ispis += timeStamp.ToString() + ",";
                    }
                }
                pisac.WriteLine(ispis);
            }
            Console.WriteLine("Napravi vrijeme" + xTime.Count);
            //nadi intervale gresaka
            if (sveGreske.Count > 0)
            {
                LinkedList<Int32> intervali = new LinkedList<Int32>();
                LinkedList<Int64> taktIntervala = new LinkedList<Int64>();
                long korak = microsecondsTime.Last.Value / brojIntervala;
                long trazimVrijednost = korak;
                Console.WriteLine("Korak je" + korak+"last="+ microsecondsTime.Last.Value+"broj intervala"+brojIntervala);
                //trazim koji je zadnji indeks zahtjeva na kraju vremenskog intervala 
                foreach (Int64 takt in microsecondsTime)
                {
                    if (takt >= trazimVrijednost)
                    {
                        intervali.AddLast(microsecondsTime.Select((item, inx) => new { item, inx }).First(x=>x.item==takt).inx);
                        taktIntervala.AddLast(takt);
                        trazimVrijednost += korak;
                    }
                }
                string linijaTaktovaIntervala = "";
                for(int i = 0; i < taktIntervala.Count; i++)
                {
                    if (i == taktIntervala.Count - 1)
                    {
                        linijaTaktovaIntervala += taktIntervala.ElementAt(i);
                    }
                    else
                    {
                        linijaTaktovaIntervala += taktIntervala.ElementAt(i)+",";
                    }
                }
                using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/murmurVremenaIntervaliGresaka.csv"))
                {
                    pisac.WriteLine(linijaTaktovaIntervala);
                }

                Console.WriteLine("Broj intervala" + intervali.Count);
                using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/murmurIntervaliGresaka.csv"))
                {   
                    int i = 0;
                    foreach (LinkedList<Int32> greskeK in sveGreske)
                    {
                        String linija = "";
                        i++;
                        Console.WriteLine("Dodajem interval greske za K=" + i);
                        for ( j = 0; j < intervali.Count; j++)
                        {
                            if (j == 0)
                            {
                                linija += greskeK.ElementAt(intervali.ElementAt(j)) + ",";
                            }
                            else if (j == intervali.Count - 1)
                            {
                                linija += (greskeK.ElementAt(intervali.ElementAt(j)) - greskeK.ElementAt(intervali.ElementAt(j - 1)));
                            }
                            else
                            {
                                linija += (greskeK.ElementAt(intervali.ElementAt(j)) - greskeK.ElementAt(intervali.ElementAt(j - 1))) + ",";
                            }
                        }
                        pisac.WriteLine(linija);
                    }
                }
                //postotci intervala
                using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/murmurPostociIntervaliGresaka.csv"))
                {
                    int i = 0;
                    foreach (LinkedList<Int32> greskeK in sveGreske)
                    {
                        String linija = "";
                        i++;
                        Console.WriteLine("Dodajem interval greske za K=" + i);

                        for ( j = 0; j < intervali.Count; j++)
                        {
                            if (j == 0)
                            {
                                linija += ((Double)greskeK.ElementAt(intervali.ElementAt(j)) / intervali.ElementAt(j)).ToString().Replace(",", ".") + ",";
                            }
                            else if (j == intervali.Count - 1)
                            {
                                linija += ((Double)(greskeK.ElementAt(intervali.ElementAt(j)) - greskeK.ElementAt(intervali.ElementAt(j - 1))) 
                                    / (intervali.ElementAt(j) - intervali.ElementAt(j - 1))).ToString().Replace(",", ".");
                            }
                            else
                            {
                                linija += ((Double)(greskeK.ElementAt(intervali.ElementAt(j)) - greskeK.ElementAt(intervali.ElementAt(j - 1)))
                                    / (intervali.ElementAt(j) - intervali.ElementAt(j - 1))).ToString().Replace(",", ".") + ",";
                            }
                        }
                        pisac.WriteLine(linija);
                    }
                } 
            }

        }
    }
}