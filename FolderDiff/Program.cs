using System;
using System.IO;

namespace FolderDiff
{
    class Program
    {
        private String _originPath;
        private String _newPath;
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Please provide exactly two arguments.");
                Console.WriteLine("FolderDiff ORIGIN_PATH NEW_PATH");
                Console.WriteLine("This progrem will check for files that are newer in the ORIGIN_PATH than in the NEW_PATH.");
                Console.WriteLine("Any files that are in ORIGIN_PATH but not in NEW_PATH will also be listed");
                Console.WriteLine("MIS xxxx <- Indicates a file is missing");
                Console.WriteLine("CHA xxxx <- Indicates a file in ORIGIN_PATH was changed later than the same file in NEW_PATH");
                Console.WriteLine("File size, file contents and any other metadata are ignored.");
                return;
            }

            String originPath = args[0];
            String newPath = args[1];

            Boolean bothExist = Directory.Exists(originPath) && Directory.Exists(newPath);
            if (!doCheck("Checking existence of both locations", bothExist))
                return;

            //we are good
            Program p = new Program(originPath, newPath);
            p.ProcessDirectory(originPath);

        }

        public Program(String originPath, String newPath)
        {
            _originPath = originPath;
            _newPath = newPath;
        }

        public void ProcessDirectory(string targetDirectory)
        {
            String newTargetDirectory = originToNewPath(targetDirectory);

            if (!Directory.Exists(newTargetDirectory))
            {
                Console.WriteLine("MIS\t" + originToRelative(targetDirectory)+"/");
                return;
            }
            
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here.
        private void ProcessFile(string fullOriginPath)
        {
            String status = "OK ";
            Boolean fileHasIssue = false;

            String fullNewPath = originToNewPath(fullOriginPath);

            if (!File.Exists(fullNewPath))
            {
                status = "MIS";
                fileHasIssue = true;
            }
            else
            {
                //check timestamp
                DateTime originDateTime = File.GetLastWriteTimeUtc(fullOriginPath);
                DateTime newDateTime = File.GetLastWriteTimeUtc(fullNewPath);

                if(newDateTime > originDateTime)
                {
                    status = "OKN";
                }
                else if(newDateTime < originDateTime)
                {
                    status = "CHA";
                    fileHasIssue = true;
                }

            }


            if (fileHasIssue)
            {
                Console.WriteLine(status + "\t" + originToRelative(fullOriginPath));
            }
            
        }

        private String removeRootFromOriginPath(String fullOriginPath)
        {
            if (fullOriginPath.StartsWith(_originPath))
            {
                return fullOriginPath.Substring(_originPath.Length);
            }
            else
            {
                throw new Exception("Illegal path: " + fullOriginPath + " is not child of " + _originPath);
            }
        }

        private String originToNewPath(string fullOriginPath)
        {
            return _newPath + removeRootFromOriginPath(fullOriginPath);            
        }

        private String originToRelative(String fullOriginPath)
        {
            String relativePath = removeRootFromOriginPath(fullOriginPath);
            if (relativePath.Length == 0)
            {
                relativePath = ".";
            }
            return relativePath;
            
        }

        private static bool doCheck(String message, Boolean isOkay)
        {
            Console.Write(message+": ");
            if (isOkay)
            {
                Console.WriteLine("OK");
                return true;
            }
            else
            {
                Console.WriteLine("Fail");
                return false;
            }
        }
    }
}
