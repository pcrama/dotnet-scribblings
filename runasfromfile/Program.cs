using System;
using System.Collections.Generic;

namespace runasfromfile
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0) {
                cat(args[0]);
            } else {
                Console.WriteLine("Please give a file name as argument");
            }
        }

        static bool MatchHeaderAndNotSet(string line, string header, SetOnce<string> value)
        {
            return (value.NotSet
                    && line.StartsWith(header,
                                       StringComparison.CurrentCultureIgnoreCase));
        }

        static string ButFirst(string x)
        {
            var parts = x.Split(" \t".ToCharArray(), 2);
            return parts[1].Trim();
        }

        static void cat(string arg)
        {
            const string USERNAME = "username";
            const string PASSWORD = "password";
            var data = new Dictionary<string, SetOnce<string>>();
            data.Add(USERNAME, new SetOnce<string>());
            data.Add(PASSWORD, new SetOnce<string>());

            using(var file =
                  new System.IO.StreamReader(arg)) {
                string line;
                while((line = file.ReadLine()) != null)
                {
                    foreach (var item in data)
                    {
                        if (MatchHeaderAndNotSet(line, item.Key, item.Value)) {
                            item.Value.Set(ButFirst(line));
                            break;
                        }
                    }
                }
                var username = data[USERNAME];
                var password = data[PASSWORD];
                if (username.NotSet && password.NotSet) {
                    Console.WriteLine("Neither username nor password found in " + arg);
                } else if (password.IsSet) {
                    Console.WriteLine("Password found: " + new String('*', password.Value.Length));
                } else if (username.IsSet) {
                    Console.WriteLine("Username found: " + username.Value);
                } else {
                    Console.WriteLine(username.Value + ":" + password.Value);
                }
            }
        }
    }

    class SetOnce<T> {
        public bool NotSet {
            get { return !IsSet; }
            private set { IsSet = !value; }
        }
        public bool IsSet { get; private set; }
        public T Value { get; private set; }
        public SetOnce()
        {
            IsSet = false;
            Value = default(T);
        }

        public SetOnce(T v)
        {
            IsSet = true;
            Value = v;
        }

        public SetOnce<T> Set(T v)
        {
            if (NotSet)
            {
                IsSet = true;
                Value = v;
            }
            return this;
        }
    }
}
