using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppModel;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            NoodleDContext db = new NoodleDContext();

            db.SaveChanges();
            db.Dispose(); db = null;

        }
    }
}
