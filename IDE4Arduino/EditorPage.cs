using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScintillaNET;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using WeifenLuo.WinFormsUI.Docking;

namespace IDE4Arduino
{
    partial class EditorPage : DockContent
    {
        Scintilla editor;

        private string _fileName;
        private bool _needsSave;

        private List<string> enteredKeywords;

        public string fileName 
        { 
            get { return _fileName; }
            set { _fileName = value; }        
        }

        public Scintilla Editor
        {
            get { return editor; }
        }

        public bool needsSave
        {
            get { return _needsSave; }
        }

        public EditorPage()
        {            
            editor = new Scintilla();
            editor.ConfigurationManager.Language = "cpp";
            editor.Annotations.Visibility = ScintillaNET.AnnotationsVisibility.Standard;
            editor.ConfigurationManager.IsUserEnabled = false;
            editor.ConfigurationManager.Language = "cpp";
            editor.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            editor.IsBraceMatching = true;
            editor.Lexing.Lexer = ScintillaNET.Lexer.Cpp;
            editor.Lexing.LexerName = "cpp";

            editor.Lexing.SetProperty("fold.compact", "0");
            editor.Lexing.SetProperty("fold.comment", "1");
            editor.Lexing.SetProperty("fold.preprocessor", "0");

            editor.Margins.Margin0.Width = 20;
            editor.Margins.Margin2.Width = 16;
            editor.Name = "scintilla";
            editor.SearchFlags = ((ScintillaNET.SearchFlags)(((ScintillaNET.SearchFlags.WholeWord | ScintillaNET.SearchFlags.MatchCase)
            | ScintillaNET.SearchFlags.WordStart)));
            editor.TabIndex = 1;
            editor.Dock = DockStyle.Fill;
            editor.Text = "//Includes go here\n\nvoid setup()\n{\n\t//Add your setup code here\n}\n\nvoid loop()\n{\n\t//Add your code here\n}\n";
            editor.CharAdded += editor_CharAdded;
            
            this.Controls.Add(editor);

            this.DockAreas = DockAreas.Document;
            
            _needsSave = false;
            _fileName = "";

            enteredKeywords = new List<string>();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            
            if (_needsSave)
            {
                save();

            }

            base.OnFormClosing(e);
        }

        void editor_CharAdded(object sender, CharAddedEventArgs e)
        {

            _needsSave = true;


            if (_fileName == string.Empty)
            {
                this.Text = "new File *";
            }
            else
            {
                this.Text = Path.GetFileName(_fileName) + " *";
            }

            int pos = editor.NativeInterface.GetCurrentPos();
            int length = pos - editor.NativeInterface.WordStartPosition(pos, true);

            // add closing brakets
            if (e.Ch == '(')
            {
                editor.NativeInterface.InsertText(pos, ")");
                return;
            }
            if (e.Ch == '[')
            {
                editor.NativeInterface.InsertText(pos, "]");
                return;
            }
            if (e.Ch == '{')
            {
                editor.NativeInterface.InsertText(pos, "}");
                return;
            }
            if (e.Ch == '\"')
            {
                editor.NativeInterface.InsertText(pos, "\"");
                return;
            }

            if (e.Ch == ' ' || e.Ch == '.' || e.Ch == '=')
            {
                string word = editor.GetWordFromPosition(pos - 1).Trim();

                if (word.Length < 4)
                    return;

                int i;
                if (int.TryParse(word, out i))
                    return;

                if (!editor.AutoComplete.List.Contains(word))
                {
                    List<string> list = editor.AutoComplete.List;

                    list.Add(word);
                    list.Sort();
                    editor.AutoComplete.List = list;

                    System.Console.WriteLine("adding '" + word + "' to autocomplete");
                }
                else
                {
                    System.Console.WriteLine("Word " + word + " is in autocomplete");
                }
            }

            if (editor.AutoComplete.List.Count == 0)
                return;

            if (length < 3)
                return;


            
            editor.AutoComplete.Show(length);


        }

        public static List<FunctionListItem> parseFunctions(string text)
        {
            List<FunctionListItem> functions = new List<FunctionListItem>();
            Regex myRegex = new Regex(@"\n\s*(\b\s*(?!\b(if|while|for|else)\b)(\b\w+\b)[ \t]+\w+[ \t]*\(.*\))\s*\{");

            foreach (Match myMatch in myRegex.Matches(text))
            {

                if (myMatch.Success)
                {
                    string fName = myMatch.Groups[1].ToString();
                    functions.Add(new FunctionListItem(fName, myMatch.Groups[1].Index));
                }
            }

            functions = functions.Distinct().ToList();
            functions.Sort();
            return functions;
        }


        public bool save(string file)
        {
            _fileName = file;
            return save();
        }

        public bool save()
        {
            System.Console.WriteLine("save: "+_fileName);
            if (_fileName.Length < 1)
            {
                System.Console.WriteLine("no filename");
                this.Activate();
                this.Focus();
                this.BringToFront();
                
                string filename = "";
                if (_fileName == string.Empty)
                {
                    filename = "new File";
                }
                else
                {
                    filename = Path.GetFileName(_fileName);
                }

                DialogResult res = MessageBox.Show("File \"" + filename + "\" is not saved, save now?", "Save File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (res == DialogResult.Yes)
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Filter = "Arduino Sketch (*.ino)|*.ino";

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _fileName = dlg.FileName;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            try
            {
                File.WriteAllText(_fileName, editor.Text);
                _needsSave = false;
                this.Text = Path.GetFileName(this._fileName);

                return true;
            }
            catch { }

            return false;
        }

        public bool load(string file)
        {
            try
            {
                editor.Text = File.ReadAllText(file);
                _fileName = file;
                this.Text = Path.GetFileName(file);

                string keySearch = Regex.Replace(editor.Text, @"/\*[\s\S]*?\*/|//.*", "");
                MatchCollection col = Regex.Matches(keySearch, @"\b[a-zA-Z]\w*\b", RegexOptions.Compiled);

                HashSet<string> words = new HashSet<string>();
                foreach (Match m in col)
                {
                    if (m.Success)
                    {
                        words.Add(m.Value);
                    }
                }
                editor.AutoComplete.List.AddRange(words);
                _needsSave = false;
                return true;
            }
            catch { }
            return false;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // EditorPage
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "EditorPage";
            this.ResumeLayout(false);
        }
    }

    class FunctionListItem : IComparable
    {
        public string name;
        public int pos;
        private string shortname;

        public FunctionListItem(string name, int pos)
        {
            this.name = name;
            this.pos = pos;

            try
            {
                shortname = name.Split(' ')[1];
            }
            catch {
                shortname = this.name;
            }
        }

        public override string ToString()
        {
            return this.name;  // + " [" + this.pos.ToString()+"]"
        }

        int IComparable.CompareTo(object other)
        {
            FunctionListItem o = (FunctionListItem)other;

            return String.Compare(this.shortname,o.shortname);
        }
    }
}
