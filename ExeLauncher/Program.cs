using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading;

namespace ExeLauncher
{
	internal class Program
	{
		private static void Main(string[] args)
		{

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			Console.WriteLine("ExeLauncher");
			Console.WriteLine("(c) 2012-2014 Marius Gheorghe");
			Console.WriteLine("");

			try
			{
				if (ParseConfig() == false)
				{
					return;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error {0}", ex.Message);
				return;
			}

			//any paths
			if (ApplicationContext.Paths.Count == 0)
			{
				Console.WriteLine("No valid input paths found. Modify the config file and enter the paths.");
				return;
			}

			//is it help ?
			if (args.Length == 1 && args[0] == "?")
			{
				Console.WriteLine("Sample: el starcraft.exe");
				return;
			}


			List<string> listArguments = new List<string>();

			for (int i = 1; i < args.Length; i++)
			{
				listArguments.Add(args[i]);
			}

			//load previously cached commands
			CommandPairManager.LoadCommands();

			//look it up
			string command = args[0];

			//look it in the cache
			CommandPair commandPair = CommandPairManager.GetCommand(command);

			if (commandPair != null)
			{
				if (File.Exists(commandPair.Path))
				{
					Console.WriteLine("found it in cache");

					(new Launcher(listArguments)).RunProcess(new[] { commandPair.Path });

					return;
				}
			}

			bool result = (new Launcher(listArguments)).Launch(command);

			if (result == false)
			{
				Console.WriteLine("No luck....Make sure you typed the exe filename correctly");
			}

			CommandPairManager.Persist();
		}



		private static bool ParseConfig()
		{
			string input = ConfigurationManager.AppSettings["Paths"];

			string extensions = ConfigurationManager.AppSettings["Extensions"];

			string depth = ConfigurationManager.AppSettings["FolderDepth"];

			string cacheFuzzySearches = ConfigurationManager.AppSettings["CacheFuzzyMatching"];

			ApplicationContext.CacheFuzzyMatches = Convert.ToBoolean(cacheFuzzySearches);

			if (string.IsNullOrEmpty(depth))
			{
				Console.WriteLine("Invalid depth setting. Must be * for everything or positive number for folder depth");
				return false;
			}

			if (depth != ApplicationContext.MAX_DEPTH)
			{
				int i;

				if (Int32.TryParse(depth, out i) == false)
				{
					Console.WriteLine("Invalid value for depth setting.Must be positive numeric value");
					return false;
				}
			}

			ApplicationContext.Depth = depth;

			if (string.IsNullOrEmpty(input))
			{
				Console.WriteLine("No paths specified in config file. Please enter at least one path");
				return false;
			}

			if (string.IsNullOrEmpty(extensions))
			{
				Console.WriteLine("No file extensions specified in config file. Please specify at least one extension.");
				return false;
			}

			string[] paths = input.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

			string[] exts = extensions.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

			ApplicationContext.Extensions.AddRange(exts);

			foreach (string path in paths)
			{
				if (!Directory.Exists(path))
				{
					Console.WriteLine("Path {0} is invalid", path);
					continue;
				}

				ApplicationContext.Paths.Add(path);
			}

			return true;
		}
	}
}