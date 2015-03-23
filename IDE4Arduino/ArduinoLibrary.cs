using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IDE4Arduino
{
    class ArduinoLibrary : IComparable
    {
        private string _name;
        private string _path;

        private List<string> _cppFiles;
        private List<string> _subDirs;

        private List<string> _keyWords;

        public List<string> cppFiles
        {
            get { return _cppFiles; }
        }

        public List<string> keyWords
        {
            get { return _keyWords; }
        }

        public string path
        {
            get { return _path; }
        }

        public string name
        {
            get { return _name; }
        }

        public ArduinoLibrary(string path)
        {
            _cppFiles = new List<string>();
            _subDirs = new List<string>();
            _keyWords = new List<string>();
            
            string[] files = Directory.GetFiles(path, "*.cpp", SearchOption.AllDirectories);
            _cppFiles.AddRange(files);
            _cppFiles.AddRange(Directory.GetFiles(path, "*.c", SearchOption.AllDirectories));
            string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
            
            _subDirs.AddRange(dirs);

            _name = Path.GetFileName(path);
            _name = name.Replace("_Library","");
            _path = path.Replace('\\', '/');

            _subDirs.RemoveAll(delegate(string t)
            {
                return Path.GetFileName(t).StartsWith(".") || Path.GetFileName(t).Equals("examples") || Path.GetFileName(t).Equals("docs");
            });

            _cppFiles = _cppFiles.Select(x => x.Replace('\\', '/')).ToList();
            _subDirs = _subDirs.Select( x => x.Replace('\\','/')).ToList();

            // load keywords.txt
            string p = Path.Combine(path, "keywords.txt");

            if (File.Exists(p))
            {
                string lines = File.ReadAllText(p);
                Regex reg = new Regex(@"(\w+)\s\w+", RegexOptions.Compiled);


                foreach (Match m in reg.Matches(lines))
                {
                    _keyWords.Add(m.Groups[1].Value);
                }
                _keyWords.Sort();
            }
            _cppFiles.Sort();
            _subDirs.Sort();
            

        }

        public string getIncludePath()
        {
            StringBuilder temp = new StringBuilder();

            temp.Append(" -I\"");
            temp.Append(_path);
            temp.Append("\"");

            foreach (string dir in _subDirs)
            {
                temp.Append(" -I\"");
                temp.Append(dir);
                temp.Append("\"");
            }
            return temp.ToString();
        }

        int IComparable.CompareTo(object other)
        {
            ArduinoLibrary o = (ArduinoLibrary)other;

            return String.Compare(this.name, o.name);
        }

    }
}
