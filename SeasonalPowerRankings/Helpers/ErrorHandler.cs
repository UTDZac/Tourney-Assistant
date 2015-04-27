using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ChallongeMatchViewer.Helpers
{
    class ErrorHandler
    {
        public const string ERROR_PATH = "Errors.txt";

        public static void LogMessageAndExit(string message, params object[] args)
        {
            LogMessage(message, args);

            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void LogMessageAndExit(string message)
        {
            LogMessage(message);

            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void LogMessage(string message, params object[] args)
        {
            LogMessage(string.Format(message, args));
        }

        public static void LogMessage(string message)
        {
            message = string.Format("[{0}] {1}", DateTime.Now, message);

            using (StreamWriter writer = new StreamWriter(ERROR_PATH))
            {
                Console.WriteLine();
                Console.WriteLine("=== ERROR ===");
                Console.WriteLine(message);
                writer.WriteLine(message);
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
            }
        }
    }
}
