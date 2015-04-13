/* IDE4Arduino
 * by thefloe (thefloe@ptmx.org)
 * 
 * Editor is based on Scintilla.NET
 * Recent Files is handled by MRU List manager from Alex Farber
 * Docking is acomplished using Dckpanelsuite by WeiFen Luo
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WeifenLuo.WinFormsUI.Docking;


namespace IDE4Arduino
{
    public partial class MainForm : Form, IMRUClient
    {
        [DllImport("shell32.dll")]
        static extern int FindExecutable(string lpFile, string lpDirectory, [Out] StringBuilder lpResult);

        Configuration cfg;
        string _text;
      
        private MRUManager mruManager;
        
        private HelpBrowser hlp;
        private TerminalWindow terminal;


        private ColoredListBox outputListBox;
        private DockAnyComponent output;

        private bool rebuildAll;

        public MainForm()
        {
            InitializeComponent();

            rebuildAll = false;


            dockPanel1.Theme = vS2012LightTheme1;
            ToolStripManager.Renderer = new VS2012ToolStripRenderer();


            outputListBox = new ColoredListBox();
            outputListBox.Text = "Output";

            output = new DockAnyComponent(outputListBox,"Output", DockAreas.DockBottom);
            output.CloseButtonVisible = false;
            output.DockAreas = DockAreas.DockBottom;
            output.Show(dockPanel1, DockState.DockBottom);

            terminal = new TerminalWindow();
            terminal.DockAreas = DockAreas.DockBottom | DockAreas.DockRight | DockAreas.DockTop;
            terminal.Show(dockPanel1, DockState.DockRight);

            hlp = new HelpBrowser();
            terminal.DockAreas = DockAreas.DockBottom | DockAreas.DockRight | DockAreas.DockLeft;
            hlp.Show(dockPanel1, DockState.DockRight);

        }

        public void OpenMRUFile(string fileName)
        {
            if (File.Exists(fileName))
                openFile(fileName);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            mruManager = new MRUManager();
            mruManager.Initialize(this, mnuFileMRU, @"Software\IDE4Arduino\RecentFiles");

            // check if path to Arduino IDE was alread set and is valid
            if (Properties.Settings.Default.arduinoIDEPath.Length < 1 || !Directory.Exists(Properties.Settings.Default.arduinoIDEPath))
            {                
                StringBuilder path = new StringBuilder();
                string arduinoPath = string.Empty;

                // if not then try to guess the path by creating a .ino file and asking win api with what to open it
                string tmp = Path.ChangeExtension(Path.GetTempFileName(), ".ino");
                File.WriteAllText(tmp, "void setup() {}\n void loop() {}\n");

                int res = FindExecutable(tmp, string.Empty, path);
                File.Delete(tmp);

                if (res >= 32)
                    arduinoPath = Path.GetDirectoryName(path.ToString());
                
                // check if valid path and structure follows structure of arduino IDE > 1.6
                while (!Directory.Exists(Path.Combine(arduinoPath, "hardware/arduino/avr/libraries")))
                {
                    // if not, then let the user select the path
                    FolderBrowserDialog dlg = new FolderBrowserDialog();
                    dlg.Description = "Select Path to the Arduino IDE v1.6";
                    dlg.ShowNewFolderButton = false;

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        arduinoPath = dlg.SelectedPath;
                    }
                    else
                    {
                        this.Close();
                        //Application.Exit();
                    }
                }

                // save the path in settings
                Properties.Settings.Default.arduinoIDEPath = arduinoPath;
                Properties.Settings.Default.Save();
            }

            // create the new configuration
            cfg = new Configuration(Properties.Settings.Default.arduinoIDEPath);

            // build the examples menu
            foreach (ArduinoLibrary lib in cfg.arduinoLibs)
            {
                string path = Path.Combine(lib.path, "examples");
                if (Directory.Exists(path))
                {
                    ToolStripMenuItem libItem = new ToolStripMenuItem(lib.name);
                    string[] files = Directory.GetFiles(path, "*.ino", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {

                        ToolStripMenuItem example = new ToolStripMenuItem(Path.GetFileNameWithoutExtension(file));
                        example.ToolTipText = file;
                        example.Click += example_Click;
                        libItem.DropDownItems.Add(example);
                    }

                    examplesToolStripMenuItem.DropDownItems.Add(libItem);
                }
            }
            

            // fill list of boards
            string[] boardNames = cfg.arduinoBoards.Select(b => b.name).ToArray();
            cobBoard.Items.AddRange(boardNames);
            cobBoard.SelectedItem = "Arduino Uno";
            cobCPU.Enabled = false;

            // fill list of com ports
            cobPorts.Items.AddRange(cfg.scanComPorts());
            try
            {
                cobPorts.SelectedIndex = 0;
            }
            catch { }

            // make window visible
            this.Visible = true;
            this.BringToFront();
            this.Focus();

            // open a new editor tab
            createNewEditor();

            // register handler for output list double click. e.g. jumping to selected error
            outputListBox.MouseDoubleClick += listBox1_MouseDoubleClick;
        }

        // fired when a exampl sketch from menu is selected
        void example_Click(object sender, EventArgs e)
        {
            ToolStripItem item = (ToolStripItem)sender;
            System.Console.WriteLine(item.ToolTipText);
            openFile(item.ToolTipText);
            
        }

        // handle double clicks on errors in the output listbox
        void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                string filename="";
                int lineNum=-1, pos=0;

                string item = outputListBox.SelectedItem.ToString();
                string[] parts = item.Split(':');

                if (parts.Length > 1)
                {
                    filename = parts[0];
                    lineNum = int.Parse(parts[1]);
                    pos = int.Parse(parts[2]);
                    
                    foreach (EditorPage page in dockPanel1.Documents)
                    {
                        if (Path.GetFileName(page.fileName).Equals(filename))
                        {
                            System.Console.WriteLine("goto: " + lineNum.ToString());
                            
                            page.Activate();
                            page.Focus();
                            page.Editor.GoTo.Line(lineNum - 1);
                            page.Editor.CurrentPos = page.Editor.CurrentPos + pos;
                            //page.Editor.GoTo.Position(pos);

                            return;
                        }
                    }
                }
            } 
            catch {}
        }

        // we want to capture F1 presses on the mainform to load the help page
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                char[] badChars = new char[] { ' ', '\t', '\\', '/', '\n', '\r', '(', ')', '{', '}' };

                EditorPage page = dockPanel1.ActiveDocument as EditorPage;
                if (page != null && page.IsActivated)
                {
                    string selection = page.Editor.Selection.Text;

                    // check if a word is selected
                    if (selection == string.Empty)
                    {
                        // nothing selected so try to find beginning and end of current word

                        int pos = page.Editor.NativeInterface.GetCurrentPos();                        
                        int start = pos;

                        if (badChars.Contains(page.Editor.NativeInterface.GetCharAt(start)))
                            start--;

                        while (!badChars.Contains(page.Editor.NativeInterface.GetCharAt(start)))
                        {
                            start--;
                        }

                        int stop = page.Editor.NativeInterface.WordEndPosition(pos, true);

                        selection = page.Editor.Text.Substring(start, stop - start);
                        selection = selection.Trim(badChars);
                    }
                    // replace dots by space for better googeling
                    string search = selection.Replace(".", "%20");
                    hlp.loadReference(search);

                }
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void createNewEditor()
        {
            EditorPage page = new EditorPage();

            page.Editor.Lexing.SetKeywords(3, String.Join(" ", cfg.keyWords));
            
            List<string> list;

            list = page.Editor.AutoComplete.List;
            list.AddRange(cfg.keyWords.ToArray());
            list.Sort();
            page.Editor.AutoComplete.List = list;
            page.Text = "new File";

            page.Show(dockPanel1, DockState.Document);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createNewEditor();
        }

        private void openFile(string file)
        {
            try
            {
                EditorPage page = new EditorPage();
                page.load(file);
                page.Editor.Lexing.SetKeywords(3, String.Join(" ", cfg.keyWords));

                List<string> list;
                list = page.Editor.AutoComplete.List;

                list.AddRange(cfg.arduinoLibs.Select(x => x.name).ToArray());
                foreach (ArduinoLibrary l in cfg.arduinoLibs)
                    list.AddRange(l.keyWords);
                list.Sort();
                list = list.Distinct().ToList();
                list.Sort();
                page.Editor.AutoComplete.List = list;

                page.Show(dockPanel1, DockState.Document);
                mruManager.Add(file);
            }
            catch
            {
                mruManager.Remove(file);
            }
        }
        
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Arduino Sketch (*.ino)|*.ino";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                openFile(dlg.FileName);
            }
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null)
            {
                page.save();
                mruManager.Add(page.fileName);
            }
        }

        private void cobBoard_SelectedIndexChanged(object sender, EventArgs e)
        {
            rebuildAll = true;
            cobCPU.Items.Clear();

            ArduinoBoard cur = cfg.arduinoBoards.First(b => b.name.Equals(cobBoard.SelectedItem.ToString()));

            if (cur.cpu_names.Length > 0)
            {
                cobCPU.Items.AddRange(cur.cpu_names);
                cobCPU.SelectedIndex = 0;
                cobCPU.Enabled = true;
            }
            else
                cobCPU.Enabled = false;
        }

        private void cobFunctions_Click(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null)
            {
                cobFunctions.Items.Clear();

                List<FunctionListItem> functions = EditorPage.parseFunctions(page.Editor.Text);
                cobFunctions.Items.AddRange(functions.ToArray());
            }

        }

        private void cobFunctions_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null && cobFunctions.SelectedIndex >= 0)
            {
                FunctionListItem f = (FunctionListItem)cobFunctions.SelectedItem;
                
                page.Editor.GoTo.Position(f.pos);            
            }
        }

        private void bwCompiler_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = false;

            ArduinoBoard board = (ArduinoBoard)e.Argument;

            string archive_name = Configuration.GetValidFileName(board.name + board.cpu).Replace(" ", "")+".a";
            System.Console.WriteLine("Archive Name: " + archive_name);

            List<ArduinoLibrary> usedLibs = new List<ArduinoLibrary>();

            Regex myRegex = new Regex(@"#include [<""](\S+)[>""]", RegexOptions.None);
            foreach (Match myMatch in myRegex.Matches(_text))
            {
                if (myMatch.Success)
                {
                    string incName = myMatch.Groups[myMatch.Groups.Count - 1].ToString();

                    if (incName.Contains("/") || incName.Contains("\\"))
                        continue;

                    string libName = Path.GetFileNameWithoutExtension(incName);
                    
                    System.Console.WriteLine("searching for: " + libName);

                    try 
                    {
                        ArduinoLibrary f = cfg.arduinoLibs.First(l => l.name.Equals(libName));
                        System.Console.WriteLine("Found by name: "+f.name);
                        usedLibs.Add(f);
                    }
                    catch
                    {
                        bool found = false;
                        foreach (ArduinoLibrary l in cfg.arduinoLibs)
                        {

                            int idx = l.cppFiles.FindIndex(x => Path.GetFileNameWithoutExtension(x).Equals(libName));
                            if (idx != -1)
                            {
                                System.Console.WriteLine("Found by Include: "+l.name);
                                usedLibs.Add(l);
                                found = true;
                                break ;
                            }
                        }

                        if (!found)
                        {
                            setListBox1("Library: " + libName + " not found, assuming system library", Color.Blue);
                        }
                    }
                }
            }

            foreach (ArduinoLibrary l in usedLibs)
                setListBox1(l.name+": "+l.path, Color.Green);

            List<FunctionListItem> functions = EditorPage.parseFunctions(_text); ;

            if (!Directory.Exists(cfg.buildPath))
                Directory.CreateDirectory(cfg.buildPath);

            string[] lines = _text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            int lineCnt = 0;
            bool incomment = false;

            StringBuilder temp = new StringBuilder();
            EditorPage page = (EditorPage)dockPanel1.ActiveDocument;

            temp.AppendLine("#line 1 \""+Path.GetFileName(page.fileName)+"\"");

            foreach (string line in lines)
            {
                lineCnt++;
                string test = line.Trim();

                if (test.Length < 2)
                {
                    temp.AppendLine();
                    continue;
                }

                if (!incomment)
                {
                    if (test[0] == '#' || test.StartsWith("//"))
                    {
                        temp.AppendLine(line);
                        continue;
                    }

                    if (test.StartsWith("/*"))
                    {
                        temp.AppendLine();
                        incomment = true;
                        continue;
                    }
                    
                    break;
                }
                else
                {
                    temp.AppendLine();
                    if (test.EndsWith("*/"))
                        incomment = false;
                }
            }

            temp.AppendLine("#include \"Arduino.h\"");

            foreach (FunctionListItem func in functions)
                temp.AppendLine(func.name + ";");

            temp.AppendLine("#line " + lineCnt.ToString()); // +" \"sketch.ino\""

            for (int i = lineCnt-1; i < lines.Length; i++)
                temp.AppendLine(lines[i]);

            File.WriteAllText(cfg.buildPath + "sketch.ino", _text);
            File.WriteAllText(cfg.buildPath+"sketch.cpp", temp.ToString());

            string[] arduinoSrc = Directory.GetFiles(board.corePath, "*.cpp", SearchOption.AllDirectories);
            arduinoSrc = arduinoSrc.Concat(Directory.GetFiles(board.corePath, "*.c", SearchOption.AllDirectories)).ToArray();
            arduinoSrc = arduinoSrc.Concat(Directory.GetFiles(board.variantPath, "*.cpp", SearchOption.TopDirectoryOnly)).ToArray();
            arduinoSrc = arduinoSrc.Concat(Directory.GetFiles(board.variantPath, "*.c", SearchOption.TopDirectoryOnly)).ToArray();

            string libInc = "";

            foreach (ArduinoLibrary lib in usedLibs)
                libInc += lib.getIncludePath();

            setListBox1("compiling sketch.cpp");

            ProcessResult res = cfg.compile(cfg.buildPath+"sketch.cpp", board, libInc);
            if (res.returnCode == 0)
            {
                if (!File.Exists(cfg.buildPath + archive_name) || rebuildAll)
                {
                    System.Console.WriteLine("Rebuilding Archive: " + cfg.buildPath + archive_name);
                    foreach (string file in arduinoSrc)
                    {
                        string baseFile = Path.GetFileName(file);
                        setListBox1("compiling: " + baseFile);
                        res = cfg.compile(file, board);

                        if (res.returnCode != 0)
                        {
                            setListBox1("Error: " + res.error, Color.Red);

                            return;
                        }
                    }

                    foreach (ArduinoLibrary l in usedLibs)
                    {
                        foreach (string file in l.cppFiles)
                        {
                            setListBox1("compiling: " + file);
                            res = cfg.compile(file, board, libInc);
                            if (res.returnCode != 0)
                            {
                                setListBox1("Error: " + res.error, Color.Red);
                                return;
                            }
                        }
                    }

                    foreach (string file in arduinoSrc)
                    {
                        string baseFile = Path.GetFileName(file);
                        setListBox1("linking " + baseFile + ".o");
                        res = cfg.linkArchive(baseFile,archive_name, board);
                        if (res.returnCode != 0)
                        {
                            setListBox1("Error: " + res.error, Color.Red);
                            return;
                        }

                    }

                    foreach (ArduinoLibrary l in usedLibs)
                    {
                        foreach (string file in l.cppFiles)
                        {
                            string baseFile = Path.GetFileName(file);
                            setListBox1("linking " + baseFile + ".o");
                            res = cfg.linkArchive(baseFile,archive_name, board);
                            if (res.returnCode != 0)
                            {
                                setListBox1("Error: " + res.error, Color.Red);
                                return;
                            }

                        }
                    }
                }

                setListBox1("linking sketch.cpp.o");
                res = cfg.combine("sketch.cpp",archive_name, board);
                if (res.returnCode != 0)
                {
                    setListBox1("Error: " + res.error, Color.Red);
                    return;
                }

                /*
                setListBox1("Creating EEPROM file");
                res = cfg.createEEProm("sketch.cpp", board);
                if (res.returnCode != 0)
                {
                    setListBox1("Error: " + res.error, Color.Red);
                    return;
                }*/


                setListBox1("Creating HEX file");
                res = cfg.createHEX("sketch.cpp", board);
                if (res.returnCode != 0)
                {
                    setListBox1("Error: " + res.error, Color.Red);
                    return;
                }

                string size = cfg.getSize("sketch.cpp", board);
                if (size == String.Empty)
                {
                    setListBox1("Error: " + res.error, Color.Red);
                    return;
                }

                setListBox1(size);

                rebuildAll = false;
                setListBox1("success", Color.Green);
                e.Result = true;
            }
            else
            {
                setListBox1("Error: " + res.error, Color.Red);
                return;
            }
        }

        private void bwCompiler_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {            

            if ((bool)e.Result == true)
            {
                MessageBox.Show("build successfull", "Build", MessageBoxButtons.OK, MessageBoxIcon.Information);
                rebuildAll = false;
            }
            else
            {
                MessageBox.Show("build unsuccessfull", "Build", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void setListBox1(string text)
        {
            setListBox1(text, outputListBox.ForeColor);
        }

        void setListBox1(string text, Color col)
        {
            if (text == null)
                return;

            if (text.Length < 1)
                return;

            this.Invoke((MethodInvoker)delegate
            {
                string[] lines = text.Split(System.Environment.NewLine.ToCharArray());
                foreach (string line in lines)
                {
                    if (line.Length < 1)
                        continue;
                    outputListBox.Items.Add(new ColoredListBoxItem(col, line));
                }
                outputListBox.SelectedIndex = outputListBox.Items.Count - 1;
            });
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            newToolStripMenuItem_Click(sender, e);
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem_Click(sender, e);
        }

        private void printToolStripButton_Click(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null)
                page.Editor.Printing.PrintPreview();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {

            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null)
                page.Editor.UndoRedo.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null)
                page.Editor.UndoRedo.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null)
                page.Editor.Clipboard.Cut();
        }


        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null)
                page.Editor.Clipboard.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null)
                page.Editor.Clipboard.Paste();
        }

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            cutToolStripMenuItem_Click(sender, e);
        }

        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Click(sender, e);
        }

        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem_Click(sender, e);
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null)
                page.Editor.FindReplace.ShowFind();
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;
            if (page != null)
                page.Editor.FindReplace.ShowReplace();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cfg != null && cfg.buildPath != string.Empty)
            {
                if (Directory.Exists(cfg.buildPath))
                    Directory.Delete(cfg.buildPath, true);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cobCPU_SelectedIndexChanged(object sender, EventArgs e)
        {
            //rebuildAll = true;
            ArduinoBoard cur = cfg.arduinoBoards.First(b => b.name.Equals(cobBoard.SelectedItem.ToString()));
            cur.setCPU(cobCPU.SelectedItem.ToString());

            //cur.cpu = cobCPU.SelectedItem.ToString();
            //curCfg = cfg.buildCurrentConfig(cobBoard.SelectedItem.ToString(), cobCPU.SelectedItem.ToString());
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.ShowDialog();
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            aboutToolStripMenuItem_Click(sender, e);
        }


        private void bwUpload_DoWork(object sender, DoWorkEventArgs ex)
        {

            
            string cmd = (string)ex.Argument;
            int pos = 0;
            cmd = cmd.Replace("  ", " ");

            if (cmd[0] == '"')
            {
                pos = cmd.IndexOf('"', 1);
            }

            pos = cmd.IndexOf(' ', pos);

            string p1 = cmd.Substring(0, pos);
            string p2 = cmd.Substring(pos + 1, cmd.Length - pos - 1);

            using (Process proc = new Process())
            {
                proc.StartInfo.FileName = p1;
                proc.StartInfo.Arguments = p2;
                
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.CreateNoWindow = true;
                
                String sysPath = System.Environment.GetEnvironmentVariable("PATH");

                proc.StartInfo.EnvironmentVariables["PATH"] = sysPath + ";" + cfg.arduinoPath;
                proc.StartInfo.EnvironmentVariables.Add("CYGWIN", "nodosfilewarning");

                proc.OutputDataReceived +=
                    (o, e) => setListBox1(e.Data, Color.Black);
                        //output.Append(e.Data).Append(Environment.NewLine);

                proc.ErrorDataReceived +=
                    (o, e) => setListBox1(e.Data, Color.Red);//error.Append(e.Data).Append(Environment.NewLine);

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                if (!proc.WaitForExit(30000)) // wait 30 seconds
                {  
                    proc.Kill();
                    setListBox1("Upload terminated because of timeout", Color.Red);
                    ex.Result = false;
                }
                else
                {
                    ex.Result = true;
                }
                //proc.Close();
            }
        }

        private void bwUpload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripButton2.Enabled = true;
            cobPorts.Enabled = true;

            if (terminal.autoReconnect)
                terminal.connect();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (outputListBox.SelectedIndex < 0)
                return;

            string tmp = outputListBox.SelectedItem.ToString();
            System.Windows.Forms.Clipboard.SetText(tmp);

        }

        private void copyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder tmp = new StringBuilder();

            foreach (var item in outputListBox.Items)
                tmp.AppendLine(item.ToString());
            
            if (tmp.Length > 0)
                System.Windows.Forms.Clipboard.SetText(tmp.ToString());
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            outputListBox.Items.Clear();
        }



        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            buildToolStripMenuItem_Click(sender, e);
        }

        private void buildToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (bwCompiler.IsBusy)
                return;

            if (cobBoard.SelectedIndex < 0 || (cobCPU.Items.Count > 0 && cobCPU.SelectedIndex < 0))
            {
                MessageBox.Show("Please select platform / processor", "Build Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            EditorPage page = dockPanel1.ActiveDocument as EditorPage;

            if (page != null)
            {
                if (!page.save())
                    setListBox1("Could not save " + page.fileName, Color.Red);

                outputListBox.Items.Clear();

                string filename = Path.GetFileNameWithoutExtension(page.fileName);

                if (!new DirectoryInfo(cfg.buildPath).Name.Equals(filename))
                    cfg.buildPath += filename + "/";

                _text = page.Editor.Text;

                ArduinoBoard board = cfg.arduinoBoards.First(b => b.name.Equals(cobBoard.SelectedItem.ToString()));


                bwCompiler.RunWorkerAsync(board);
            }

        }

        private void rebuildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rebuildAll = true;
            buildToolStripMenuItem_Click(sender, e);
        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (cobPorts.SelectedIndex < 0)
            {
                MessageBox.Show("Select a COM Port first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(cfg.buildPath + "sketch.hex") && !File.Exists(cfg.buildPath + "sketch.bin"))
            {
                MessageBox.Show("HEX File not found, run build first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            if (cobPorts.SelectedItem.ToString() == terminal.port)
            {
                if (terminal.isConnected)
                    terminal.disconnect();
            }

            ArduinoBoard board = cfg.arduinoBoards.First(b => b.name.Equals(cobBoard.SelectedItem.ToString()));

            string cmd = cfg.upload("sketch", board, cobPorts.SelectedItem.ToString());
            System.Console.WriteLine(cmd);
            setListBox1("Starting upload...", Color.Green);
            bwUpload.RunWorkerAsync(cmd);
            toolStripButton2.Enabled = false;
            cobPorts.Enabled = false;
        }


        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            uploadToolStripMenuItem_Click(sender, e);
        }

        private void helpBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {          
            if (hlp.IsDisposed)
            {                
                hlp = new HelpBrowser();
                hlp.Show(dockPanel1, DockState.DockRight);
            }

            if (hlp.VisibleState == DockState.Hidden)
                hlp.VisibleState = hlp.DockState;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;

            if (page != null)
            {
                page.Close();
            }
        }

        private void dockPanel1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            EditorPage page = dockPanel1.ActiveDocument as EditorPage;

            if (page != null)
            {
                cobFunctions.SelectedIndex = -1;
                cobFunctions.SelectedText = "";
                cobFunctions.Items.Clear();


                List<FunctionListItem> functions = EditorPage.parseFunctions(page.Editor.Text);
                cobFunctions.Items.AddRange(functions.ToArray());
            }
        }

        private void openBuildDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = cfg.buildPath.Replace("/", "\\");
            try
            {
                Process.Start(path);
            }
            catch { }
        }

        private void toolStripLabel2_Click(object sender, EventArgs e)
        {
            cobPorts.Items.Clear();
            cobPorts.Items.AddRange(cfg.scanComPorts());
            try
            {
                cobPorts.SelectedIndex = 0;
            }
            catch { }

        }

    }

    internal class VS2012ToolStripRenderer : ToolStripProfessionalRenderer
    {
        public VS2012ToolStripRenderer()
            : base(new VS2012ColorTable())
        {
        }
    }

    class VS2012ColorTable : ProfessionalColorTable
    {
        public override Color ButtonSelectedHighlight
        {
            get { return ButtonSelectedGradientMiddle; }
        }
        public override Color ButtonSelectedHighlightBorder
        {
            get { return ButtonSelectedBorder; }
        }
        public override Color ButtonPressedHighlight
        {
            get { return ButtonPressedGradientMiddle; }
        }
        public override Color ButtonPressedHighlightBorder
        {
            get { return ButtonPressedBorder; }
        }
        public override Color ButtonCheckedHighlight
        {
            get { return ButtonCheckedGradientMiddle; }
        }
        public override Color ButtonCheckedHighlightBorder
        {
            get { return ButtonSelectedBorder; }
        }
        public override Color ButtonPressedBorder
        {
            get { return ButtonSelectedBorder; }
        }
        public override Color ButtonSelectedBorder
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color ButtonCheckedGradientBegin
        {
            get { return Color.FromArgb(255, 254, 254, 254); }
        }
        public override Color ButtonCheckedGradientMiddle
        {
            get { return Color.FromArgb(255, 254, 254, 254); }
        }
        public override Color ButtonCheckedGradientEnd
        {
            get { return Color.FromArgb(255, 254, 254, 254); }
        }
        public override Color ButtonSelectedGradientBegin
        {
            get { return Color.FromArgb(255, 254, 254, 254); }
        }
        public override Color ButtonSelectedGradientMiddle
        {
            get { return Color.FromArgb(255, 254, 254, 254); }
        }
        public override Color ButtonSelectedGradientEnd
        {
            get { return Color.FromArgb(255, 254, 254, 254); }
        }
        public override Color ButtonPressedGradientBegin
        {
            get { return Color.FromArgb(255, 32, 172, 232); }
        }
        public override Color ButtonPressedGradientMiddle
        {
            get { return Color.FromArgb(255, 32, 172, 232); }
        }
        public override Color ButtonPressedGradientEnd
        {
            get { return Color.FromArgb(255, 32, 172, 232); }
        }
        public override Color CheckBackground
        {
            get { return Color.FromArgb(255, 254, 254, 254); }
        }
        public override Color CheckSelectedBackground
        {
            get { return Color.FromArgb(255, 254, 254, 254); }
        }
        public override Color CheckPressedBackground
        {
            get { return Color.FromArgb(255, 32, 172, 232); }
        }
        public override Color GripDark
        {
            get { return Color.FromArgb(255, 221, 226, 236); }
        }
        public override Color GripLight
        {
            get { return Color.FromArgb(255, 204, 204, 219); }
        }
        public override Color ImageMarginGradientBegin
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color ImageMarginGradientMiddle
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color ImageMarginGradientEnd
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color ImageMarginRevealedGradientBegin
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color ImageMarginRevealedGradientMiddle
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color ImageMarginRevealedGradientEnd
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color MenuStripGradientBegin
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color MenuStripGradientEnd
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color MenuItemSelected
        {
            get { return Color.FromArgb(255, 248, 249, 250); }
        }
        public override Color MenuItemBorder
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color MenuBorder
        {
            get { return Color.FromArgb(255, 204, 206, 219); }
        }
        public override Color MenuItemSelectedGradientBegin
        {
            get { return Color.FromArgb(255, 254, 254, 254); }
        }
        public override Color MenuItemSelectedGradientEnd
        {
            get { return Color.FromArgb(255, 254, 254, 254); }
        }
        public override Color MenuItemPressedGradientBegin
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color MenuItemPressedGradientMiddle
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color MenuItemPressedGradientEnd
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color RaftingContainerGradientBegin
        {
            get { return Color.FromArgb(255, 186, 192, 201); }
        }
        public override Color RaftingContainerGradientEnd
        {
            get { return Color.FromArgb(255, 186, 192, 201); }
        }
        public override Color SeparatorDark
        {
            get { return Color.FromArgb(255, 204, 206, 219); }
        }
        public override Color SeparatorLight
        {
            get { return Color.FromArgb(255, 246, 246, 246); }
        }
        public override Color StatusStripGradientBegin
        {
            get { return Color.FromArgb(255, 79, 146, 219); }
        }
        public override Color StatusStripGradientEnd
        {
            get { return Color.FromArgb(255, 79, 146, 219); }
        }
        public override Color ToolStripBorder
        {
            get { return Color.FromArgb(0, 0, 0, 0); }
        }
        public override Color ToolStripDropDownBackground
        {
            get { return Color.FromArgb(255, 231, 232, 236); }
        }
        public override Color ToolStripGradientBegin
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color ToolStripGradientMiddle
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color ToolStripGradientEnd
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color ToolStripContentPanelGradientBegin
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color ToolStripContentPanelGradientEnd
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color ToolStripPanelGradientBegin
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color ToolStripPanelGradientEnd
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color OverflowButtonGradientBegin
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color OverflowButtonGradientMiddle
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
        public override Color OverflowButtonGradientEnd
        {
            get { return Color.FromArgb(255, 239, 239, 242); }
        }
    }
    public partial class DockAnyComponent : DockContent
    {
        public DockAnyComponent(Control control):this(control, control.Text, (DockAreas)255)
        { }

        public DockAnyComponent(Control control, string name) : this(control, name, (DockAreas)255)
        { }

        public DockAnyComponent(Control control, string name, WeifenLuo.WinFormsUI.Docking.DockAreas areas)
        {
            this.DockAreas = areas;
            this.Text = name;
            control.Dock = DockStyle.Fill;
            this.Controls.Add(control);
        }

    }
}
