using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDE4Arduino
{

    class ArduinoPlatform
    {

        public string platform_path;

        public string vendor;
        public string arch;

  

        public Dictionary<string, string> cfg;


        public ArduinoPlatform(string arduinoPath, string filename, string vendor, string arch)
        {
            cfg = ConfigParser.parseFile(filename);

            // overwrite platform dependend stuff
            Dictionary<string, string> deps = cfg.Where(kv => kv.Key.EndsWith("windows")).ToDictionary(kv => kv.Key.Replace(".windows", ""), kv => kv.Value);
            foreach (var kv in deps)
            {
                cfg[kv.Key] = kv.Value;
            }

            Dictionary<string, string> info = new Dictionary<string, string>();

            this.platform_path = Path.GetDirectoryName(filename);
            this.vendor = vendor;
            this.arch = arch.ToUpper().Trim();

            info.Add("runtime.ide.path", arduinoPath);
            info.Add("runtime.tools.avr-gcc.path", arduinoPath + @"\hardware\tools\avr");
            info.Add("runtime.ide.version", "10600");
            info.Add("build.arch", this.arch);            
            info.Add("build.system.path", platform_path + @"\system");
            cfg = ConfigParser.parseDict(cfg, info);
        }

    }



}
