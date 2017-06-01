using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLoader
{
    public class CsvLoader : ILoader
    {
        private string csvFileName;
        public CsvLoader(string fileName)
        {
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            csvFileName = path + fileName;
        }

        public int KolicinaTestnogPrometa
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public LinkedList<PrometData> Load(string[] olnyLoad)
        {
            LinkedList<PrometData> promet = new LinkedList<PrometData>();

            using (StreamReader sr = new StreamReader(csvFileName))
            {
                while (!sr.EndOfStream)
                {
                    string[] line = sr.ReadLine().Split(',');

                    if (olnyLoad.Contains(line[0]))
                    {
                        promet.AddLast(new PrometData()
                        {
                            Vrsta = line[0],
                            SEQ = line[1],
                            ASEQ = Double.Parse(line[2]),
                            DestinationIP = line[5],
                            DestinationPort = line[6],
                            SourcePort = line[4],
                            SourceIP = line[3]
                        });
                    }
                }
            }

            return promet;
        }
    }
}
