using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDE4Arduino
{
    
    class ArduinoBoard
    {
        public string path;
        public string name;
        public string id;

        public string corePath;
        public string variantPath;

        public string[] cpu_names;

        private string _cpu;

        public string cpu
        {
            get { return _cpu; }
        }

        private Dictionary<string, string> cfg;

        public ArduinoBoard()
        {

        }

        public ArduinoBoard(string boardShort, Dictionary<string, string> data, string path, ArduinoPlatform p)
        {
            cfg = new Dictionary<string, string>(p.cfg);

            Dictionary<string, string> bcfg = data.Where(kv => kv.Key.StartsWith(boardShort)).ToDictionary(kv => kv.Key.Replace(boardShort + ".", ""), kv => kv.Value);
            bcfg.ToList().ForEach(x => cfg[x.Key] = x.Value);

            if (cfg.ContainsKey("build.usb_manufacturer"))
                cfg["build.usb_manufacturer"] = "\"\\\"" + cfg["build.usb_manufacturer"].Replace("\"","") + "\\\"\"";

            if (cfg.ContainsKey("build.usb_product"))
                cfg["build.usb_product"] = "\"\\\"" + cfg["build.usb_product"].Replace("\"", "") + "\\\"\"";

            cfg = ConfigParser.parseDict(cfg, cfg);
            cfg = ConfigParser.parseDict(cfg, cfg);           

            id = boardShort;
            name = cfg["name"];
            this.path = path;           

            corePath = Path.Combine(path, @"cores\" + cfg["build.core"]); //.Replace('\\','/');
            variantPath = Path.Combine(path, @"variants\" + cfg["build.variant"]); //.Replace('\\', '/');

            Dictionary<string, string> types = cfg.Where(kv => kv.Key.StartsWith("menu.cpu")).ToDictionary(x => x.Key.Remove(0, 9), x => x.Value);
            cpu_names = types.Keys.Select(key => key.Split('.')[0]).ToList().Distinct().ToArray();
        }

        public static ArduinoBoard[] parseBoardFile(string file, ArduinoPlatform p)
        {
            List<ArduinoBoard> boards = new List<ArduinoBoard>();

            Dictionary<string, string> tcfg = ConfigParser.parseFile(file);

            // get list of boards
            List<string> bNames = tcfg.Where(kv => kv.Key.EndsWith("name")).Select(kv => kv.Key.Split('.')[0]).ToList();
            bNames = bNames.Distinct().ToList();

            foreach (string bName in bNames)
            {
                boards.Add(new ArduinoBoard(bName, tcfg, Path.GetDirectoryName(file),p));
            }

            return boards.ToArray();
        }

        public string getToolName()
        {
            return cfg["upload.tool"];
        }

        public Dictionary<string, string> getToolConfig()
        {
            return cfg.Where(kv => (kv.Key.StartsWith("upload") || kv.Key.StartsWith("tools." + cfg["upload.tool"]))).ToDictionary(kv => kv.Key.Replace("tools." + cfg["upload.tool"]+".",""), kv => kv.Value);
        }

        public void setCPU(string cpu)
        {
            if (this.cpu_names.Length > 0)
            {
                this._cpu = cpu;

                Dictionary<string, string> cpuCfg = cfg.Where(kv => kv.Key.StartsWith("menu.cpu." + this.cpu)).ToDictionary(kv => kv.Key.Replace("menu.cpu." + this.cpu + ".", ""), kv => kv.Value);

                foreach (var kv in cpuCfg)
                {
                    cfg[kv.Key] = kv.Value;
                }
            }
        }

        public string getUploadCmd()
        {
            return this.getUploadCmd(this.getToolName());
        }

        public string getUploadCmd(string tool)
        {
            return ConfigParser.parseLine(cfg["tools."+tool+".upload.pattern"], this.getToolConfig());
        }

        public string getCompilerC()
        {
            return cfg["recipe.c.o.pattern"];
        }


        public string getCompilerCPP()
        {
            return cfg["recipe.cpp.o.pattern"];
        }

        public string getCompilerS()
        {
            return cfg["recipe.S.o.pattern"];
        }

        public string getArchive()
        {
            return cfg["recipe.ar.pattern"];
        }
             

        public string getCombine()
        {
            return cfg["recipe.c.combine.pattern"];
        }

        public string getEEProm()
        {
            return cfg["recipe.objcopy.eep.pattern"];
        }

        public string getHEX()
        {
            return cfg["recipe.objcopy.hex.pattern"];
        }

        public string getSize()
        {
            return cfg["recipe.size.pattern"];
        }

        public string getSizeRegex()
        {
            return cfg["recipe.size.regex"];
        }

    }
}
