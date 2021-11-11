using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace scan_util
{
    internal class Program
    {
        static int Main(string[] args)
        {
            string path;
            Scanner scanner = new Scanner();

            if (args.Length == 0)
                path = ".";
            else if (System.IO.Directory.Exists(args[0]))
                path = args[0];
            else
            {
                Console.WriteLine("Please enter the path to the existing directory");
                return 1;
            }

            if (args.Length >= 2 && int.TryParse(args[1], out int numOfThreads) && numOfThreads > 0)
            {
                scanner.SetNumberOfThreads(numOfThreads);
            }

            try
            {
                var sortedBySize = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).OrderBy(f => new FileInfo(f).Length);
                scanner.sortedFiles = sortedBySize.ToList();
                scanner.InitThreads();
                scanner.PrintReport();
            }
            catch
            {
                Console.WriteLine("Insufficient permissions to scan content of the directory or the subdirectory");
                return 1;
            }

            return 0;
        }
    }
}
