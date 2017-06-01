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
            int maxArraySize = 4000;
            bool speedTest = false;
            LinkedList<PodatciOGreskama> analizaBlooma = new LinkedList<PodatciOGreskama>();
            LinkedList<LinkedList<string>> sve = new LinkedList<LinkedList<string>>();
            ILoader loaderMreznogPrometa = new TvzCSVLoader("/promet_veci_sa_bloom_log02032016.csv",1500);
            LinkedList<PrometData> cjelokupanPromet = loaderMreznogPrometa.Load(new string[] { "ACK(SYN)", "ACK(FIN(DOLJE))", "ACK(FIN(GORE))", "ACK(PUSH_GORE)", "ACK(PUSH_DOLJE)", "SYN-ACK" });
//            ILoader loaderMreznogPrometa = new CsvLoader("/promet_veci_sa_bloom_log02032016.csv");
//            LinkedList<PrometData> cjelokupanPromet = loaderMreznogPrometa.Load(new string[] { "ACK(SYN)","SYN-ACK" });

            Console.WriteLine("Kolicina prometa =" + cjelokupanPromet.Count);
            int VelicinaPoljaZaDodatneTestove = 3000;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/cSharpMurmurGreske.csv"))
            {
                for (int numHashes = 1; numHashes < 8; numHashes++)
                {
                    LinkedList<double> brojGresaka = new LinkedList<double>();
                    Console.WriteLine("Broj Hash funkcija" + numHashes);
                    for (int arraySize = 1; arraySize <= maxArraySize; arraySize +=200)
                    {
                        LinkedList<string> popunjenostFiltera = new LinkedList<string>();
                        Console.WriteLine("prolaz za velicinu" + arraySize);
                        LinkedList<PrometData> dodanoUBloomFilter = new LinkedList<PrometData>();
                        LinkedList<PrometData> falsePromet = new LinkedList<PrometData>();
                        LinkedList<Greske> falsePositive = new LinkedList<Greske>();
                        LinkedList<int> kolicinaCalnova = new LinkedList<int>();

                        Stopwatch stoperica = new Stopwatch();
                        CountingBloomFilter<string> bloomFilter = new CountingBloomFilter<string>(arraySize, numHashes);
                        if (speedTest)
                        {
                            stoperica.Start();
                        }
                        //LinkedList<string> popunjenostFiltera =new LinkedList<string>()
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
                                // Console.WriteLine("Dodano: "+x);
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
                                        if (arraySize == VelicinaPoljaZaDodatneTestove)
                                        {
                                            int fpDosada = falsePositive.Count == 0 ? 0 : falsePositive.Last().UkupnoGresaka;
                                            falsePositive.AddLast(new Greske()
                                            {
                                                UkupnoGresaka = fpDosada+1,
                                                PovecanjeKolicineGresaka = 1
                                            });
                                        }
                                        //Console.WriteLine("Nije bio sadrzan u listi za provjeru" + x);
                                    }
                                    else
                                    {
                                        dodanoUBloomFilter.Remove(odgovarajuciZahtjevIzListe);
                                        if (arraySize == VelicinaPoljaZaDodatneTestove)
                                        {

                                            int fpDosada = falsePositive.Count == 0 ? 0 : falsePositive.Last().UkupnoGresaka;
                                            falsePositive.AddLast(new Greske()
                                            {
                                                UkupnoGresaka = fpDosada,
                                                PovecanjeKolicineGresaka = 0
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    if (sadrzanoUListiZaProvjeru)
                                    {
                                        falsePromet.AddLast(zahtjev);
                                        if (arraySize == VelicinaPoljaZaDodatneTestove)
                                        {
                                            int fpDosada = falsePositive.Count == 0 ? 0 : falsePositive.Last().UkupnoGresaka;
                                            falsePositive.AddLast(new Greske()
                                            {
                                                UkupnoGresaka = fpDosada+1,
                                                PovecanjeKolicineGresaka = 1
                                            });
                                        }

                                        //Console.WriteLine("Bio je sadrzan u listi za provjeru a bloom je rekao da nije" + x);
                                        dodanoUBloomFilter.Remove(odgovarajuciZahtjevIzListe);

                                    }
                                    else
                                    {
                                        if (arraySize == VelicinaPoljaZaDodatneTestove)
                                        {
                                            int fpDosada = falsePositive.Count == 0 ? 0 : falsePositive.Last().UkupnoGresaka;
                                            falsePositive.AddLast(new Greske()
                                            {
                                                UkupnoGresaka = fpDosada,
                                                PovecanjeKolicineGresaka = 0
                                            });
                                        }
                                    }
                                }
                                #endregion
                            }

                            if (arraySize == VelicinaPoljaZaDodatneTestove)
                            {
                                //Console.WriteLine("ajmo" + bloomFilter.GetPopunjenost());
                             //   popunjenostFiltera.AddLast(bloomFilter.GetPopunjenost());
                                kolicinaCalnova.AddLast(bloomFilter.BrojClanova);
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
                            NumberFormatInfo nfi = new NumberFormatInfo();
                            nfi.NumberDecimalSeparator = ".";
                            brojGresaka.AddLast(((double)falsePromet.Count)/loaderMreznogPrometa.KolicinaTestnogPrometa);
                            if (falsePositive.Count > 0  || kolicinaCalnova.Count > 0)
                            {
                                analizaBlooma.AddLast(new PodatciOGreskama()
                                {
                                    brojClanova = kolicinaCalnova,
                                    falsePositiveGreske = falsePositive
                                });
                            }

                            if (popunjenostFiltera.Count > 0)
                            {
                                sve.AddLast(popunjenostFiltera);
                                //Console.WriteLine("popunjenost" + numHashes);
                            }

                            //Console.WriteLine("Broj gresaka =" + falsePositivePromet.Count);
                        }
                        if (arraySize == 1) {
                            arraySize = 0;
                        }
                    }
                    string greskeString = "";
                    for (int i = 0; i < brojGresaka.Count; i++)
                    {
                        if (i == (brojGresaka.Count - 1))
                        {
                            greskeString += (brojGresaka.Skip(i).First().ToString()).Replace(",",".");
                            break;
                        }
                        greskeString += (brojGresaka.Skip(i).First().ToString()).Replace(",", ".") + ",";
                    }
                    file.WriteLine(greskeString);
                    Console.WriteLine("greske:" + greskeString);
                }
            }
            int j = 0;
            foreach (var x in sve)
            {
                j++;
                Console.WriteLine("cSharpPopunjenostFiltera_K_" + j);
                using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/cSharpPopunjenostFiltera_K_" + j + ".csv"))
                {
                    foreach (string line in x)
                    {
                        //Console.WriteLine(line);
                        pisac.WriteLine(line);
                    }
                }
            }

            j = 0;

            //using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/cSFalseNegative.csv"))
            //{
            //    foreach (PodatciOGreskama pog in analizaBlooma)
            //    {
            //        string ispis = "";
            //        for (int t = 0; t < pog.falsenegativeGreske.Count; t++)
            //        {
            //            if (t == pog.falsePositiveGreske.Count - 1)
            //            {
            //                ispis += pog.falsenegativeGreske.ElementAt(t).PovecanjeKolicineGresaka;
            //            }
            //            else
            //            {
            //                ispis += pog.falsenegativeGreske.ElementAt(t).PovecanjeKolicineGresaka +",";
            //            }
            //        }
            //        pisac.WriteLine(ispis);
            //    }
            //}
            Console.WriteLine("cSFalsePositiveDerivacija");
            using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/cSFalsePositiveDerivacija.csv"))
            {
                foreach (PodatciOGreskama pog in analizaBlooma)
                {
                    string ispis = "";
                    for (int t= 0;t < pog.falsePositiveGreske.Count;t++)
                    {
                        if (t==pog.falsePositiveGreske.Count-1)
                        {
                            ispis += pog.falsePositiveGreske.ElementAt(t).PovecanjeKolicineGresaka;
                        }
                        else
                        {
                            ispis += pog.falsePositiveGreske.ElementAt(t).PovecanjeKolicineGresaka + ",";
                        }
                    }
                    pisac.WriteLine(ispis);
                }
            }
            Console.WriteLine("cSFalsePositiveUkupno");
            using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/cSFalsePositiveUkupno.csv"))
            {
                foreach (PodatciOGreskama pog in analizaBlooma)
                {
                    string ispis = "";
                    for (int t = 0; t < pog.falsePositiveGreske.Count; t++)
                    {
                        if (t == pog.falsePositiveGreske.Count - 1)
                        {
                            ispis += pog.falsePositiveGreske.ElementAt(t).UkupnoGresaka;
                        }
                        else
                        {
                            ispis += pog.falsePositiveGreske.ElementAt(t).UkupnoGresaka + ",";
                        }
                    }
                    pisac.WriteLine(ispis);
                }
            }
            Console.WriteLine("cSBrojClanova");
            using (var pisac = new System.IO.StreamWriter("C:/faks/zavrsni/rezultati/cSBrojClanova.csv"))
            {
                foreach (PodatciOGreskama pog in analizaBlooma)
                {

                    string ispis = "";
                    for (int i = 0; i < pog.brojClanova.Count; i++) {
                        if (i == pog.brojClanova.Count - 1) {
                            ispis += pog.brojClanova.ElementAt(i);
                        }
                        else
                        {
                            ispis += pog.brojClanova.ElementAt(i)+",";
                        }
                    }
                    //foreach (int line in pog.brojClanova)
                    //{
                    //    if (pog.brojClanova.Last.Equals(line))
                    //    {
                    //        ispis += line;
                    //    }
                    //    else
                    //    {
                    //        ispis += line + ",";
                    //    }
                    //}
                    pisac.WriteLine(ispis);
                }
            }
        }

    }
}