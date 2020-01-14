using System;

namespace runasfromfile
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0) {
                cat(args[0]);
            } else {
                System.Console.WriteLine("Please give a file name as argument");
            }
        }

        static bool MatchHeaderAndNullValue(string line, string header, string value)
        {
            return ((null == value)
                    && line.StartsWith(header,
                                       StringComparison.CurrentCultureIgnoreCase));
        }

        static string ButFirst(string x)
        {
            string[] parts = x.Split(" \t".ToCharArray(), 2);
            return parts[1].Trim();
        }

        static void cat(string arg)
        {
            string line;
            string username = null;
            string password = null;

            using(System.IO.StreamReader file =
                  new System.IO.StreamReader(arg)) {
                while((line = file.ReadLine()) != null)
                {
                    if (MatchHeaderAndNullValue(line, "username", username)) {
                        username = ButFirst(line);
                    } else if (MatchHeaderAndNullValue(line, "password", password)) {
                        password = ButFirst(line);
                    } else {
                        System.Console.WriteLine(line);
                    }
                }
                if ((null != username) && (null != password)) {
                    System.Console.WriteLine(username + ":" + password);
                } else if ((null == username) && (null == password)) {
                    System.Console.WriteLine("Neither username nor password found in " + arg);
                } else if (null != password) {
                    System.Console.WriteLine("Password found: " + new String('*', password.Length));
                } else if (null != username) {
                    System.Console.WriteLine("Username found: " + username);
                }
            }
        }
    }
}
