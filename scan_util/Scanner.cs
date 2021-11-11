using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace scan_util
{
    public class Scanner
    {
        const int MAX_BUFFER_SIZE = 2048;
        static readonly Encoding enc8 = Encoding.UTF8;

        private int processed;
        private int js;
        private int rm;
        private int rundll;
        private int errors;
        private int MAX_THREADS;
        private List<Thread> searchThreadPull;
        public Stopwatch execTime;

        public List<string> sortedFiles;
        public Scanner()
        {
            execTime = new Stopwatch();
            execTime.Start();
        }
        public void SetNumberOfThreads(int num)
        {
            MAX_THREADS = num;
        }
        public void InitThreads()
        {
            searchThreadPull = new List<Thread>();
            for (int i = 0; i < MAX_THREADS; ++i)
            {
                searchThreadPull.Add(new Thread(SearchRoutine));
            }
            foreach (Thread current in searchThreadPull)
            {
                current.Start();
            }
            foreach (Thread current in searchThreadPull)
            {
                current.Join();
            }
        }
        private void SearchRoutine()
        {
            string filePath;
            while (true)
            {
                lock (sortedFiles)
                {
                    filePath = sortedFiles.FirstOrDefault();
                    if (sortedFiles.Count != 0)
                        sortedFiles.RemoveAt(0);
                }
                if (filePath == null)
                    break;
                try
                {
                    using (FileStream file = new FileStream(filePath, FileMode.Open))
                    {
                        Interlocked.Increment(ref processed);
                        string extension = Path.GetExtension(filePath);

                        byte[] convert = new byte[MAX_BUFFER_SIZE];
                        string textFromFile = null;
                        if (convert.Length <= MAX_BUFFER_SIZE)
                        {
                            file.Read(convert, 0, convert.Length);
                            textFromFile = enc8.GetString(convert);
                        }
                        else
                        {
                            textFromFile = ReadFromBuffer(file);
                        }

                        if ((extension == ".js" || extension == ".JS") &&
                            (textFromFile.Contains("<script>evil_script()</script>") ||
                            textFromFile.Contains("< s c r i p t > e v i l _ s c r i p t ( ) < / s c r i p t >")))
                            {
                                Interlocked.Increment(ref js);
                            }
                        if (textFromFile.Contains("rm -rf %userprofile%\\Documents"))
                        {
                            Interlocked.Increment(ref rm);
                        }
                        else if (textFromFile.Contains("Rundll32 sus.dll SusEntry"))
                        {
                            Interlocked.Increment(ref rundll);
                        }

                    }
                }
                catch
                {
                    Interlocked.Increment(ref errors);
                }
            }
        }
        private string ReadFromBuffer(FileStream fStream)
        {
            string output = String.Empty;
            Decoder decoder8 = enc8.GetDecoder();

            while (fStream.Position < fStream.Length)
            {
                int nBytes = fStream.Read(bytes, 0, bytes.Length);
                int nChars = decoder8.GetCharCount(bytes, 0, nBytes);
                char[] chars = new char[nChars];
                nChars = decoder8.GetChars(bytes, 0, nBytes, chars, 0);
                output += new String(chars, 0, nChars);
            }
        return output;
        }
        public void PrintReport()
        {
            execTime.Stop();
            TimeSpan ts = execTime.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
            ts.Hours, ts.Minutes, ts.Seconds);

            Console.WriteLine("====== Scan result ======");
            Console.WriteLine($"Processed files: {processed}");
            Console.WriteLine($"JS detects: {js}");
            Console.WriteLine($"rm -rf detects: {rm}");
            Console.WriteLine($"Rundll32 detects: {rundll}");
            Console.WriteLine($"Errors: {errors}");
            Console.WriteLine($"Execution time: {elapsedTime}");
            Console.WriteLine("=========================");
        }
    }
}
