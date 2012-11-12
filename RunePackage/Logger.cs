using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RuneSharp.RunePackage
{
    public static class Logger
    {
        public static List<string> Logs = new List<string>();
        public static bool Silence = false;
        public static void Log(string s)
        {
            Log(s, LogType.Message);
        }
        public static void Log(string s, LogType type)
        {
            if (Silence) return;
            ConsoleColor messageColor = ConsoleColor.White;
            switch (type)
            {
                case LogType.Message:
                    messageColor = ConsoleColor.White;
                    break;
                case LogType.Error:
                    messageColor = ConsoleColor.Red;
                    break;
                case LogType.Warning:
                    messageColor = ConsoleColor.Yellow;
                    break;
                case LogType.Fatal:
                    messageColor = ConsoleColor.DarkRed;
                    break;
                case LogType.Success:
                    messageColor = ConsoleColor.Green;
                    break;
            }
            Console.ForegroundColor = messageColor;
            Console.Write("[" + type.ToString() + "] ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(s);
            //Logs.Add("<span style='color:" + messageColor.ToString().ToLower() + ";'>[" + type.ToString() + "]</span> " + s + "<br/>");
            if (type == LogType.Fatal)
            {
                Console.WriteLine("Press any key to continue...");
                Logger.PushLogs();
                Console.Read();
                Environment.Exit(0);
            }
        }
        public static void PushLogs()
        {
            return;
            string outputHeader = "";
            if (!File.Exists("Log.html"))
            {
                outputHeader += "<h1>RunePackage Log</h1>\n<style type='text/css'>body{font-family:'Verdana',sans-serif;}";
                outputHeader += ".date{border-bottom:1px dotted darkblue;padding-bottom:5px;}</style>\n";
            }
            outputHeader += "<span class='date'>Logs pushed at " + DateTime.Now.ToString()+"</span><br/><br/>";
            string output = outputHeader + "\n";
            foreach (string l in Logs)
            {
                output += l + "\n";
            }
            FileStream fs = File.Open("Log.html", FileMode.Append);
            fs.Write(Encoding.UTF8.GetBytes(output), 0, Encoding.UTF8.GetBytes(output).Length);
            fs.Close();
            Logs.Clear();
        }
    }
    public enum LogType
    {
        Message,
        Error,
        Warning,
        Fatal,
        Success
    }
}
