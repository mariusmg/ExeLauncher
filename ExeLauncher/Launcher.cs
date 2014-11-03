using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ExeLauncher
{
	public class Launcher
	{
		private const string CLI = "/cli";

		private const string DEBUG = "/debug";

		private const string CURRENT_FOLDER = "/f";

		private int currentRootDepth;
		private int depth;
		private bool hasFixedDepth;

		private bool hasRunCLI;
		private bool hasRunInDebugMode;
		private bool launched;
		private bool hasCurrentFolder = false;

		private List<string> listArguments;

		public Launcher(List<string> args)
		{
			listArguments = args;

			hasRunCLI = HasSpecificArgument(CLI);
			hasCurrentFolder = HasSpecificArgument(CURRENT_FOLDER);
			hasRunInDebugMode = HasSpecificArgument(DEBUG);
		}

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
						currentRootDepth = current.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.None).Length;
					}

					if (exeFileName.EndsWith("*"))
					{
						FindItWithMatching(current, exeFileName);
					}
					else
					{
						FindIt(current, exeFileName, hasExtension);
					}

					if (launched)
					{
						return true;
					}
				}
				catch (Exception ex)
				{
					if (hasRunInDebugMode)
					{
						Console.WriteLine("Error " + ex.Message);
					}
				}
			}

			return false;
		}

		private void FindItWithMatching(string inputPath, string matchingFileName)
		{
			if (!inputPath.EndsWith(@"\"))
			{
				inputPath += @"\";
			}

			string[] files = Directory.GetFiles(inputPath, matchingFileName + ".exe", SearchOption.AllDirectories);

			if (files.Length > 0)
			{
				if (ApplicationContext.CacheFuzzyMatches)
				{
					CommandPairManager.AddCommand(new CommandPair { Command = matchingFileName, Path = files[0] });
				}

				//the matches for fuzzy searching are not cached
				RunProcess(files);
			}
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

				int length = inputPath.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.None).Length;

				int current = length - currentRootDepth;

				if (current > depth)
				{
					if (hasRunInDebugMode)
					{
						Console.WriteLine("Bailing out of {0} due to depth restriction", inputPath);
					}

					return;
				}
			}

			string[] files;

			if (hasExtension)
			{
				//user is looking for specific file
				files = Directory.GetFiles(inputPath, exeFileName);

				if (files.Length > 0)
				{
					RunProcess(files);

					CommandPairManager.AddCommand(new CommandPair { Command = exeFileName, Path = files[0] });

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

						CommandPairManager.AddCommand(new CommandPair { Command = exeFileName, Path = files[0] });

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
				if (hasRunCLI)
				{
					RunCliProcess(files[0]);
					return;
				}

				ProcessStartInfo ps = new ProcessStartInfo();
				ps.UseShellExecute = true;
				ps.WorkingDirectory = Path.GetDirectoryName(files[0]) ?? string.Empty;
				ps.FileName = files[0];

				if (listArguments.Count > 0)
				{
					ps.Arguments = GetArgumentsAsStrings();
				}

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

		public void RunCliProcess(string filePath)
		{
			try
			{
				ProcessStartInfo ps = new ProcessStartInfo();
				ps.RedirectStandardOutput = true;
				ps.UseShellExecute = false;

				if (hasCurrentFolder == false)
				{
					ps.WorkingDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
				}
				else
				{
					ps.WorkingDirectory = Environment.CurrentDirectory;
				}

#if DEBUG
				if (hasCurrentFolder)
				{
					Console.WriteLine("working folder is " + ps.WorkingDirectory);
				}
#endif
				ps.FileName = filePath;

				if (listArguments.Count > 0)
				{
					if (hasCurrentFolder == false)
					{
						ps.Arguments = GetArgumentsAsStrings();
					}
					else
					{
						ps.Arguments = GetArgumentsAsStrings(ps.WorkingDirectory);
					}
				}

				Process process = Process.Start(ps);

				Console.WriteLine(process.StandardOutput.ReadToEnd());
				process.WaitForExit();
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


		private string GetArgumentsAsStrings(string startupPath)
		{
			if (! startupPath.EndsWith(@"\"))
			{
				startupPath += @"\";
			}

			Console.WriteLine(startupPath);

			StringBuilder builder = new StringBuilder();

			foreach (string s in listArguments)
			{
				if (s.ToLower() != CLI && s.ToLower() != DEBUG)
				{
					if (File.Exists(startupPath + s))
					{
						Console.WriteLine(startupPath + s);
						builder.Append( startupPath + s + " ");
					}
					else
					{
						builder.Append(s + " ");
					}
				}
			}

			return builder.ToString();
		}


		private string GetArgumentsAsStrings()
		{
			StringBuilder builder = new StringBuilder();

			foreach (string s in listArguments)
			{
				if (s.ToLower() != CLI && s.ToLower() != DEBUG)
				{
					builder.Append(s + " ");
				}
			}

			return builder.ToString();
		}

		private bool HasSpecificArgument(string argument)
		{
			foreach (string s in listArguments)
			{
				if (s.ToLower() == argument)
				{
					return true;
				}
			}

			return false;
		}
	}
}