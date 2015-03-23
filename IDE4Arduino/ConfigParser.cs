using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace IDE4Arduino
{
    class ConfigParser
    {

        public ConfigParser()
        {
        }

        public static Dictionary<string, string> parseFile(string file)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            

            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);

                foreach (string line in lines)
                {
                    if (line.Length > 1 && !line.StartsWith("#"))
                    {
                        string[] parts = line.Split(new char[] {'='}, 2);
                        parts[1] = parts[1].Replace("'", "");
                        result.Add(parts[0], parts[1]);
                    }
                }

                


            }

            return result; // parseDict(result, result);
        }

        public static string parseLine(string line, Dictionary<string, string> search)
        {
            line = Regex.Replace(line, @"\{(.*?)\}", delegate(Match match)
            {
                string mkey = match.Groups[1].Value;
                if (search.ContainsKey(mkey))
                    return search[mkey];
                else
                    return match.ToString();
            }, RegexOptions.Compiled);
            return line;
        }

        public static Dictionary<string, string> parseDict(Dictionary<string, string> input, Dictionary<string, string> values)
        {
            Dictionary<string, string> parsed = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kv in input)
            {
                parsed.Add(kv.Key, parseLine(kv.Value, values));
            }
            return parsed;
        }

    }

 
}
