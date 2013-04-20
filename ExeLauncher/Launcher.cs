using System;
using System.Diagnostics;
using System.IO;

namespace ExeLauncher
{
    public class Launcher
    {
        private bool launched;

        private bool hasFixedDepth = false;

        private int depth;

        private int currentRootDepth;

        public bool Launch(string exeFileName)
        {
            launched = false;

            //determine if we have a file with extension
            bool hasExtension = exeFileName.IndexOf('.') != -1;


            //check recursion

            if (ApplicationContext.Depth != ApplicationContext.MAX_DEPTH)
            {
                depth = Int32.Parse(ApplicationContext.Depth);
                hasFixedDepth = true;
            }

            foreach (string current in ApplicationContext.Paths)
            {
                try
                {
                    if (hasFixedDepth)
                    {
                        currentRootDepth = current.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.None).Length;
                    }
                    
                    FindIt(current, exeFileName, hasExtension);

                    if (launched)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error " + ex.Message);
                }
            }

            return false;
        }

        private void FindIt(string inputPath, string exeFileName, bool hasExtension)
        {
            if (launched)
            {
                return;
            }

            if (!inputPath.EndsWith(@"\"))
            {
                inputPath += @"\";
            }


            if (hasFixedDepth)
            {
                //check depth 

                int length = inputPath.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.None).Length;

                int current = length - currentRootDepth;

                if (current > this.depth)
                {

                    Console.WriteLine("Bailing out of {0} due to depth restriction", inputPath);

                    return;
                }
            }

            //for debugging
            //Console.WriteLine(inputPath);

            string[] files;

            if (hasExtension)
            {
                //user is looking for specific file
                files = Directory.GetFiles(inputPath, exeFileName);

                if (files.Length > 0)
                {
                    RunProcess(files);

					CommandPairManager.AddCommand(new CommandPair(){Command = exeFileName, Path = files[0]});

                    return;
                }
            }
            else
            {
                //search with all extensions
                foreach (string ext in ApplicationContext.Extensions)
                {
                    files = Directory.GetFiles(inputPath, exeFileName + "." + ext);

                    if (files.Length > 0)
                    {
                        RunProcess(files);

						CommandPairManager.AddCommand(new CommandPair() { Command = exeFileName, Path = files[0] });

						return;
                    }
                }
            }

            string[] directories = Directory.GetDirectories(inputPath);

            foreach (string d in directories)
            {
                FindIt(d, exeFileName, hasExtension);
            }
        }

        public void RunProcess(string[] files)
        {
            Console.WriteLine("Running it from {0}", files[0]);

            try
            {

				ProcessStartInfo ps = new ProcessStartInfo();
	            ps.WorkingDirectory = Path.GetDirectoryName(files[0]) ?? string.Empty;
	            ps.FileName = files[0];

                Process.Start(ps);
            }
            catch (Exception e)
            {
                Console.WriteLine("Process failed to start {0}", e.Message);
            }
            finally
            {
                launched = true;
            }
        }
    }
}