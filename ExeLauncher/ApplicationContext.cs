using System.Collections.Generic;

namespace ExeLauncher
{
    public class ApplicationContext
    {
        public const string MAX_DEPTH = "*";

        public static List<string> Paths;

        public static List<string> Extensions; 

        static ApplicationContext()
        {
            Paths = new List<string>();
            Extensions = new List<string>();
        }


        public static string Depth;

    }
}