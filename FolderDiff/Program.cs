using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FolderDiff
{
    class Program
    {
        
        // parameters
        private String _originPath;
        private String _newPath;
        private string[] _excludePaths;
        private bool _ignoreTime;

        private MD5 _md5Provider;
        static void Main(string[] args)
        {
            Program p = new Program(args);
        }

        private static void showHelp()
        {

            Console.WriteLine("NAME");
            Console.WriteLine("  FolderDiff - recursively search for files that have been changed or added"); 
            Console.WriteLine("");
            Console.WriteLine("SYNOPSIS");
            Console.WriteLine("  FolderDiff [--exclude=\"EXCLUDE_PATH\"] PATH_A PATH_B");            
            Console.WriteLine("");
            Console.WriteLine("DESCRIPTION");
            Console.WriteLine("  The following differences between the two folders are returned: ");
            Console.WriteLine("    Any files that are newer in PATH_A than in PATH_B");
            Console.WriteLine("    Any files that are in PATH_A but not in PATH_B");
            Console.WriteLine("  Returns a list with all files (and their relative paths) that were found:");            
            Console.WriteLine("    CHA xxxx <- (CHANGED) file in PATH_A is newer and different than in PATH_B");
            Console.WriteLine("    MIS xxxx <- (MISSING) file is missing in PATH_B");
            Console.WriteLine("");
            Console.WriteLine("OPTIONS");
            Console.WriteLine("  --ignore-times");
            Console.WriteLine("    Check for changes between files regardless of timestamp.");
            Console.WriteLine("    Even if files have the same timestamp or if the file in PATH_B is newer than");
            Console.WriteLine("    the file in PATH_A it will still be flagged as changed if its contents are");
            Console.WriteLine("    different.");
            Console.WriteLine("  --exclude=\"EXCLUDE_PATH\"");
            Console.WriteLine("    You can exclude parts of your folder structure with the --exclude parameter.");
            Console.WriteLine("    Paths are matched against the start of a relative path (starting with a /).");
            Console.WriteLine("    This parameter can be specified multiple times.");
            Console.WriteLine("");
            Console.WriteLine("EXAMPLES");
            Console.WriteLine("    FolderDiff --exclude=\"/exclude_me\" /test/a /test/b");
            Console.WriteLine("    Changes made in /test/a after /test/b are highlighted");
            Console.WriteLine("    Changes made in /test/b after /test/a are ignored");
            Console.WriteLine("    Any changes in /test/a/exclude_me are ignored.");
        }

        private bool parseParameters(string[] args)
        {
            int pathCounter = 0;
            List<string> excludeTempList = new List<string>();
            foreach(String arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    string argPart = (arg.Substring(2).Split("="))[0];
                    switch (argPart)
                    {
                        case "exclude":
                            string[] argParts = arg.Split("=");                            
                            if (argParts.Length == 2)
                            {
                                excludeTempList.Add(argParts[1]);                                
                            }
                            else
                            {
                                Console.WriteLine("Warning: illegal exclude argument");
                            }                            
                            break;
                        case "ignore-times":
                            _ignoreTime = true;
                            break;
                        default:
                            Console.WriteLine("Warning: unkown parameter: " + arg);
                            break;

                    }
                }
                else
                {
                    //assume path
                    if (pathCounter == 0)
                    {
                        _originPath = arg;
                    }else if(pathCounter == 1)
                    {
                        _newPath = arg;
                    }
                    else
                    {
                        Console.WriteLine("Warning: ignoring additional argument: " + arg);
                    }

                    pathCounter++;
                }
            }

            if (pathCounter >= 2)
            {
                Boolean bothExist = Directory.Exists(_originPath) && Directory.Exists(_newPath);
                if (bothExist)
                {
                    _excludePaths = excludeTempList.ToArray();                    
                    return true;
                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("Error: could not find one of the paths provided ");
                    Console.WriteLine("  " + _originPath + " or " + _newPath);
                    Console.WriteLine("");                    
                }
                    
            }
            
            return false;
            
        }

        public Program(string[] args)
        {
            if (parseParameters(args))
            {
                _md5Provider = MD5.Create();                
                ProcessDirectory(_originPath);
            }
            else
            {
                showHelp();
            }
            
        }

        private bool isExcluded(string fullOriginPath)
        {
            string relativePath = originToRelative(fullOriginPath);
            foreach(string exclude in _excludePaths)
            {
                if (relativePath.StartsWith(exclude))
                {
                    return true;
                }
            }

            return false;
        }

        private void ProcessDirectory(string targetDirectory)
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
            {
                if (!isExcluded(fileName))
                {
                    ProcessFile(fileName);
                }
                
            }
                

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                if (!isExcluded(subdirectory))
                {
                    ProcessDirectory(subdirectory);
                }
            }
                
        }

        // inspired by https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string 
        private String md5HashFromFile(String fullPath)
        {
            return BitConverter.ToString(_md5Provider.ComputeHash(ASCIIEncoding.ASCII.GetBytes(File.ReadAllText(fullPath))));
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

                if(_ignoreTime || newDateTime < originDateTime)
                {
                    //Check if file actually changed
                    String originHash = md5HashFromFile(fullOriginPath);
                    String newHash = md5HashFromFile(fullNewPath);

                    if (originHash.CompareTo(newHash) != 0)
                    {
                        status = "CHA";
                        fileHasIssue = true;
                    }
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
        
    }
}
