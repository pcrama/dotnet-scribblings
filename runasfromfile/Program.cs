// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

[assembly: System.Resources.NeutralResourcesLanguage("en-US")]

namespace RunAsFromFile
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics; // System.Diagnostics.Process.dll
    using runasfromfile.Properties;

    /// <summary>
    ///   Read credentials from a plain text file and start a process as that user.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Moved into a gist for usage in smallcliutils (see
    ///     https://github.com/pcrama/scoop-buckets/):
    ///     https://gist.github.com/pcrama/a0480922ba7e4a0082c50a97335011f0/.
    ///   </para>
    /// </remarks>
    public static class Program
    {
        /// <summary>
        ///   Program entry point: validate command line args and start processing them.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

if (args.Length > 1)
            {
                var commandLine = new ArraySegment<string>(args, 1, args.Length - 1);
                Cat(
                    args[0],
                    (u, p) => { StartProcessAs(commandLine, u, p); });
            }
            else
            {
                Console.WriteLine(Resources.CommandLineArgumentError);
            }
        }

        private static void SplitUserAndDomain(string fullUserName, out string domain, out string user)
        {
            var defaultDomain = System.Environment.MachineName;
            var parts = fullUserName.Split('\\');
            if (parts.Length == 1)
            {
                domain = defaultDomain;
                user = fullUserName;
            }
            else if (parts[0] == ".")
            {
                domain = defaultDomain;
                user = parts[1];
            }
            else
            {
                domain = parts[0];
                user = parts[1];
            }
        }

        private static void StartProcessAs(ArraySegment<string> cmdLine, string fullUserName, string password)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.FileName = cmdLine.Array[cmdLine.Offset];
            startInfo.Arguments = string.Join(" ", cmdLine.Array, cmdLine.Offset + 1, cmdLine.Count - 1);
            string domain, user;
            SplitUserAndDomain(fullUserName, out domain, out user);
            startInfo.Domain = domain;
            startInfo.UserName = user;
            startInfo.PasswordInClearText = password;
            startInfo.WorkingDirectory = System.IO.Path.GetPathRoot(System.IO.Directory.GetCurrentDirectory());
            var proc = Process.Start(startInfo);
        }

        /// <summary>
        ///   Checks that a line starts with given header and value has not been set before.
        /// </summary>
        private static bool MatchHeaderAndNotSet(string line, string header, SetOnce<string> value)
        {
            return value.NotSet
                && line.StartsWith(
                    header,
                    StringComparison.CurrentCultureIgnoreCase);
        }

        private static string ButFirst(string x)
        {
            var parts = x.Split(" \t".ToCharArray(), 2);
            return parts[1].Trim();
        }

        private static void Cat(string arg, Action<string, string> callWithCredentials)
        {
            const string USERNAME = "username";
            const string PASSWORD = "password";
            var data = new Dictionary<string, SetOnce<string>>();
            data.Add(USERNAME, new SetOnce<string>());
            data.Add(PASSWORD, new SetOnce<string>());

            using (var file =
                  new System.IO.StreamReader(arg))
                  {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    foreach (var item in data)
                    {
                        if (MatchHeaderAndNotSet(line, item.Key, item.Value))
                        {
                            item.Value.Set(ButFirst(line));
                            break;
                        }
                    }
                }

                var username = data[USERNAME];
                var password = data[PASSWORD];
                if (username.IsSet && password.IsSet)
                {
                    callWithCredentials(username.Value, password.Value);
                }
                else if (username.NotSet && password.NotSet)
                {
                    Console.WriteLine(Resources.NoUsernameNorPasswordFound + arg);
                }
                else if (password.IsSet)
                {
                    Console.WriteLine(Resources.PasswordFound + new string('*', password.Value.Length));
                }
                else if (username.IsSet)
                {
                    Console.WriteLine(Resources.UsernameFound + username.Value);
                }
                else
                {
                    Console.WriteLine(Resources.NotReached);
                }
            }
        }
    }
}
