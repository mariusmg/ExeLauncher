
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;

namespace ExeLauncher
{
	[DataContract]
	public class CommandPair
	{
		[DataMember]
		public string Command
		{
			get;
			set;
		}

		[DataMember]
		public string Path
		{
			get;
			set;
		}
	}

	public class CommandPairManager
	{
		private const string COMMANDS_FILENAME = "commands.json";

		private static List<CommandPair> commands = new List<CommandPair>();
		private static FileStream fileStream;

		public static void LoadCommands()
		{
			commands = new List<CommandPair>();

			FileStream fs = null;

			try
			{
				string filePath = GetFilePath();

				if (File.Exists(filePath))
				{
					try
					{
						DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<CommandPair>));

						fs = File.Open(filePath, FileMode.Open);

						commands = (List<CommandPair>)s.ReadObject(fs);
					}
					catch (Exception ex)
					{
						//Console.WriteLine("Failed to load commands from ");
					}
				}
			}
			catch
			{
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
				}
			}
		}

		public static void Persist()
		{
			if (commands.Count == 0)
			{
				return;
			}

			FileStream fs = null;

			try
			{
				string filePath = GetFilePath();


				DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<CommandPair>));

				fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write);

				s.WriteObject(fs, commands);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to cache commands" + ex);
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
				}
			}
		}


		private static string GetFilePath()
		{
			string currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "ExeLauncher";

			if (!Directory.Exists(currentDirectory))
			{
				try
				{
					Directory.CreateDirectory(currentDirectory);
				}
				catch (Exception ex)
				{

				}
			}

			string filePath = currentDirectory + Path.DirectorySeparatorChar + COMMANDS_FILENAME;

			return filePath;
		}

		public static void AddCommand(CommandPair pair)
		{
			CommandPair command = GetCommand(pair.Command);

			if (command != null)
			{
				command = pair;
			}
			else
			{
				commands.Add(pair);
			}
		}

		public static CommandPair GetCommand(string name)
		{
			if (commands == null)
			{
				return null;
			}

			return commands.FirstOrDefault(pair => pair.Command.ToLower() == name.ToLower());
		}
	}
}