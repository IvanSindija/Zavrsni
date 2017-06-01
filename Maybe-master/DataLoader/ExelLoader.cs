using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;

namespace DataLoader
{
    public class ExelLoader : ILoader
    {
        private string exelFileName;
        public ExelLoader(string fileName)
        {
            string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            exelFileName = path+fileName;
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
            Console.WriteLine("Učitavanje podataka");
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;
            int rCnt;
            int cCnt;
            int rw = 0;
            int cl = 0;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(exelFileName);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            range = xlWorkSheet.UsedRange;
            rw = range.Rows.Count;
            cl = range.Columns.Count;


            for (rCnt = 3; rCnt <= rw; rCnt++)
            {
                object vrsta = (range.Cells[rCnt, 9] as Excel.Range).Value2;
                object seq = (range.Cells[rCnt, 10] as Excel.Range).Value2;
                object ack = (range.Cells[rCnt, 11] as Excel.Range).Value2;
                object ipOd = (range.Cells[rCnt, 13] as Excel.Range).Value2;
                object portOd = (range.Cells[rCnt, 14] as Excel.Range).Value2;
                object ipZa = (range.Cells[rCnt, 15] as Excel.Range).Value2;
                //Console.WriteLine("ipZa: " + ipZa.ToString());
                object portZa = (range.Cells[rCnt, 16] as Excel.Range).Value2;

                if (olnyLoad.Contains(vrsta))
                {
                    promet.AddLast(new PrometData() {
                        Vrsta=vrsta.ToString(),
                        ASEQ=Double.Parse(ack.ToString()),
                        SEQ=seq.ToString(),
                        DestinationIP=ipZa.ToString(),
                        DestinationPort= portZa.ToString(),
                        SourcePort= portOd.ToString(),
                        SourceIP=ipOd.ToString()
                    });
                }
            }

            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);
            
            return promet;
        }
    }
}
