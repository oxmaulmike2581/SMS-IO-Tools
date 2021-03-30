using System;
using System.IO;
using System.Linq;

namespace TEXFilePatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: TEXFilePatcher.exe input.tex");
                Console.WriteLine(Environment.NewLine + "==========" + Environment.NewLine + "ERROR: No input path given or path is invalid." + Environment.NewLine + "Please press any key to exit." + Environment.NewLine + "==========");
                Console.ReadLine();
            }

            // shitty code
            string inputPath = Path.GetFileNameWithoutExtension(args[0]);
            string outputPath = inputPath + ".dds";

            using (FileStream fstream = new FileStream(args[0], FileMode.Open))
            {
                Console.WriteLine("Reading " + inputPath);

                // Start reading from 12th byte
                fstream.Seek(32, SeekOrigin.Begin);

                // Code from MS Docs :)
                byte[] input = new byte[fstream.Length];
                int numBytesToRead = (int)fstream.Length - 12;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fstream.Read(input, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                        break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                numBytesToRead = input.Length;

                // Write the byte array to the other FileStream.
                using (FileStream fsNew = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    // Write to the output file
                    Console.WriteLine("Writing " + outputPath);
                    fsNew.Write(input, 0, numBytesToRead);
                }
            }
            Console.WriteLine("Done.");
        }
    }
}

