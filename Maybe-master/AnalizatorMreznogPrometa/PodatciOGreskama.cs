using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalizatorMreznogPrometa
{
    public class PodatciOGreskama
    {
       public LinkedList<Greske> falsePositiveGreske = new LinkedList<Greske>();
       public LinkedList<Greske> falsenegativeGreske = new LinkedList<Greske>();
       public LinkedList<int> brojClanova = new LinkedList<int>();
    }
}
