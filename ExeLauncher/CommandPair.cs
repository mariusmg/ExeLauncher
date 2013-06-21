using System.Runtime.Serialization;

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
}