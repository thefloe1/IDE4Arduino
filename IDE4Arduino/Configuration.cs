using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IDE4Arduino
{
    class Configuration
    {
        public string arduinoPath;
        public string buildPath;

        public string arduinoCoreLibPath;
        public string arduinoLibPath;
        public string arduinoUserLibPath;

        public List<ArduinoBoard> arduinoBoards;
        public List<ArduinoLibrary> arduinoLibs;
        public List<string> keyWords;

        public Configuration(string arduinoPath)
        {
            // set path to the components
            this.arduinoPath = arduinoPath;
            buildPath = System.IO.Path.GetTempPath()+"IDE4Arduino/build/"; 

            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            arduinoCoreLibPath = Path.Combine(arduinoPath, @"hardware\arduino\avr\libraries");
            arduinoLibPath = Path.Combine(arduinoPath, @"libraries");
            arduinoUserLibPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Arduino\libraries");

            // Parse Platforms
            string[] vendors = Directory.GetDirectories(Path.Combine(arduinoPath, "hardware"));           
            string userHardwareFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Arduino\hardware");
            string[] usrVendors = Directory.GetDirectories(userHardwareFolder);

            List<ArduinoPlatform> platforms = new List<ArduinoPlatform>();
            arduinoBoards = new List<ArduinoBoard>();

            vendors = vendors.Concat(usrVendors).ToArray();

            foreach (string vendor in vendors)
            {
                string vendor_name = Path.GetFileName(vendor);

                if (vendor_name.Equals("tools"))
                    continue;

                string[] archs = Directory.GetDirectories(vendor);

                foreach (string arch in archs)
                {
                    string arch_name = Path.GetFileName(arch);

                    try
                    {
                        ArduinoPlatform p = new ArduinoPlatform(arduinoPath, Path.Combine(arch, "platform.txt"), vendor_name, arch_name);
                        if (p.cfg.Count < 1)
                            continue;
                        arduinoBoards.AddRange(ArduinoBoard.parseBoardFile(Path.Combine(arch, "boards.txt"), p));
                    }
                    catch
                    { }
                }
            }

            recreateLibraries();
        }

        public void recreateLibraries()
        {
            arduinoLibs = new List<ArduinoLibrary>();

            try
            {
                string[] temp = Directory.GetDirectories(arduinoCoreLibPath);
                foreach (string dir in temp)
                {
                    ArduinoLibrary lib = new ArduinoLibrary(dir);
                    arduinoLibs.Add(lib);
                }

                temp = Directory.GetDirectories(arduinoLibPath);
                foreach (string dir in temp)
                {
                    ArduinoLibrary lib = new ArduinoLibrary(dir);
                    arduinoLibs.Add(lib);
                }

                temp = Directory.GetDirectories(arduinoUserLibPath);
                foreach (string dir in temp)
                {
                    ArduinoLibrary lib = new ArduinoLibrary(dir);
                    arduinoLibs.Add(lib);
                }

            }
            catch { 
            
            }

            arduinoLibs.Sort();

            keyWords = new List<string>() { "Serial", "pinMode", "INPUT", "OUTPUT", "INPUT_PULLUP", "HIGH", "LOW", "LED_BUILTIN", "true", "false", "available", "begin","end","print", "println" };
            
            foreach (ArduinoLibrary lib in arduinoLibs)
            {
                keyWords.Add(lib.name);
                keyWords.AddRange(lib.keyWords.ToArray());
            }

            keyWords = keyWords.Distinct().ToList();
            keyWords.Sort();

        }

        public ProcessResult runCmd(string cmd)
        {
            int pos = 0;
            cmd = cmd.Replace("  ", " ");

            if (cmd[0] == '"')
            {
                pos = cmd.IndexOf('"', 1);
            }

            pos = cmd.IndexOf(' ', pos);

            string p1 = cmd.Substring(0, pos);
            string p2 = cmd.Substring(pos + 1, cmd.Length - pos - 1);

            return runCmd(p1, p2);
        }

        public ProcessResult runCmd(string cmd, string args)
        {
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = cmd;
                proc.StartInfo.Arguments = args;

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.CreateNoWindow = true;
                String sysPath = System.Environment.GetEnvironmentVariable("PATH");

                proc.StartInfo.EnvironmentVariables["PATH"] = sysPath + ";" + arduinoPath;
                proc.StartInfo.EnvironmentVariables.Add("CYGWIN", "nodosfilewarning");

                proc.OutputDataReceived +=
                    (o, e) => output.Append(e.Data).Append(Environment.NewLine);

                proc.ErrorDataReceived +=
                    (o, e) => error.Append(e.Data).Append(Environment.NewLine);

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();


                return new ProcessResult(output.ToString(), error.ToString(), proc.ExitCode);
            }
        }

        public ProcessResult linkArchive(string file, string archive_file, ArduinoBoard board)
        {
            Dictionary<string, string> linkDict = new Dictionary<string, string>();
            linkDict.Add("build.path", buildPath);
            linkDict.Add("archive_file", archive_file);
            linkDict.Add("object_file", buildPath + file + ".o");

            string cmd = ConfigParser.parseLine(board.getArchive(), linkDict);           
            return runCmd(cmd);            
        }

        public ProcessResult combine(string file, string archive_file, ArduinoBoard board)
        {
            Dictionary<string, string> compileDict = new Dictionary<string, string>();
            compileDict.Add("build.path", buildPath);
            compileDict.Add("build.project_name", "sketch");
            compileDict.Add("object_files", buildPath + "sketch.cpp.o");
            compileDict.Add("archive_file", archive_file);
            compileDict.Add("build.mcu", board.cpu);
            compileDict.Add("build.variant.path", board.variantPath);

            string cmd = ConfigParser.parseLine(board.getCombine(), compileDict);

            return runCmd(cmd);
        }

        public ProcessResult createEEProm(string file, ArduinoBoard board)
        {

            Dictionary<string, string> compileDict = new Dictionary<string, string>();
            compileDict.Add("build.path", buildPath);
            compileDict.Add("build.project_name", "sketch");

            string cmd = ConfigParser.parseLine(board.getEEProm(), compileDict);

            return runCmd(cmd);
        }

        public ProcessResult createHEX(string file, ArduinoBoard board)
        {

            Dictionary<string, string> compileDict = new Dictionary<string, string>();
            compileDict.Add("build.path", buildPath);
            compileDict.Add("build.project_name", "sketch");

            string cmd = ConfigParser.parseLine(board.getHEX(), compileDict);

            return runCmd(cmd);
        }

        public string getSize(string file, ArduinoBoard board)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("build.path", buildPath);
            dict.Add("build.project_name", "sketch");

            string cmd = ConfigParser.parseLine(board.getSize(), dict);
            ProcessResult res = runCmd(cmd);

            if (res.returnCode != 0)
                return String.Empty;
           
            var matches = Regex.Matches(res.output.ToString(), board.getSizeRegex(), RegexOptions.Multiline);
            string sout = "";

            foreach (Match m in matches)
            {
                if (m.Success)
                {
                    sout += m.ToString();
                }
            }

            return sout;
        }

        public ProcessResult compile(string file, ArduinoBoard board, string additionals = "")
        {
            string cmd;
            string baseFile = Path.GetFileName(file);

            Dictionary<string, string> compileDict = new Dictionary<string, string>();

            compileDict.Add("includes", " -I\"" + board.corePath + "\" " + " -I\"" + board.variantPath + "\"" + additionals);// + " -I\"" + avrLibcInc + "\"");
            compileDict.Add("source_file", file);
            compileDict.Add("object_file", buildPath + baseFile + ".o");
            compileDict.Add("build.mcu", board.cpu);

            switch (Path.GetExtension(file))
            {
                case ".c":
                    cmd = ConfigParser.parseLine(board.getCompilerC(), compileDict);
                    break;
                case ".cpp":
                    string cpp = board.getCompilerCPP();
                    cmd = ConfigParser.parseLine(cpp, compileDict);
                    break;
                case ".S":
                    cmd = ConfigParser.parseLine(board.getCompilerS(), compileDict);
                    break;
                default:
                    cmd = "";
                    break;
            }       

            return runCmd(cmd);
        }

        public string upload(string filename, ArduinoBoard board, string port)
        {
            string cmd = "";

            Dictionary<string, string> toolDict = new Dictionary<string, string>();

            toolDict.Add("build.path", buildPath);
            toolDict.Add("build.project_name", "sketch");
            toolDict.Add("upload.verbose", "");
            toolDict.Add("serial.port", port);
            toolDict.Add("serial.port.file", port);
            toolDict.Add("build.mcu", board.cpu);

            cmd = ConfigParser.parseLine(board.getUploadCmd(), toolDict);

            return cmd;
        }


        public string[] scanComPorts()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }

        public static string GetValidFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

    }

    class ProcessResult
    {
        private string _output;
        private string _error;
        private int _returnCode;

        public string output
        {
            get { return _output; }
        }

        public string error
        {
            get { return _error; }
        }

        public int returnCode
        {
            get { return _returnCode; }
        }

        public ProcessResult(string output, string error, int returnCode)
        {
            _output = output;
            _error = error;
            _returnCode = returnCode;
        }


    }
}
