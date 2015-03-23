using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace IDE4Arduino
{
    public partial class HelpBrowser : DockContent
    {
        public HelpBrowser()
        {
            InitializeComponent();
            this.KeyPreview = false;
        }

        private void HelpBrowser_Load(object sender, EventArgs e)
        {
            /*
            webBrowser1.AllowNavigation =false;
            webBrowser1.AllowWebBrowserDrop = false;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;
            */

            //this.CloseButtonVisible = false;
            //webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;
        }

        //System.Windows.Forms.WebBrowser wb;

        public void loadReference(string reference)
        {
            try
            {
                webBrowser1.ScriptErrorsSuppressed = true;

                //wb.Navigate("http://arduino.cc/en/Reference/"+reference);
                System.Console.WriteLine("loading " + reference);
                //wb.DocumentCompleted += wb_DocumentCompleted;
                //wb.Navigate(@"http://www.google.com/search?ie=UTF-8&sourceid=navclient&gfns=1&q=arduino%20" + reference);
                string url = @"http://www.google.com/search?btnI=I%27m+Feeling+Lucky&ie=UTF-8&oe=UTF-8&q=site:arduino.cc%20" + reference;
                System.Console.WriteLine(url);
                webBrowser1.Navigate(url);
            }
            catch { }
            
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //System.Console.WriteLine("completed");
            HtmlDocument doc = webBrowser1.Document;

            doc.Body.Style = "background: white; font-size: 14px;";
            HtmlElement elm = doc.GetElementById("pageheader");
            if (elm!=null)
                elm.Style += "display:none;";

            elm = doc.GetElementById("pagefooter");
            if (elm != null)
                elm.Style += "display:none;";

            foreach (HtmlElement el in doc.GetElementsByTagName("link"))
            {
                if (el.GetAttribute("rel") == "stylesheet")
                {
                    if (el.GetAttribute("href").EndsWith("common.css"))
                        el.SetAttribute("media", "print");
                }
                //System.Console.WriteLine();
            }
        }

        /*
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            System.Console.WriteLine("HLP: " + keyData.ToString());
            //return base.ProcessCmdKey(ref msg, keyData);
            return false;
        }*/

        /*
        void wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            System.Console.WriteLine(e.Url.ToString());
            if (e.Url == wb.Url)
            {
                System.Console.WriteLine("match: "+e.Url.ToString());
                HtmlElement elm = wb.Document.GetElementById("wikitext");

                if (elm != null)
                {
                    wb.DocumentCompleted -= wb_DocumentCompleted;

                    

                    webBrowser1.Navigate("about:blank");
                    if (webBrowser1.Document != null)
                    {
                        webBrowser1.Document.Write(string.Empty);
                    }
                    System.Console.WriteLine("setting html");

                        
                    webBrowser1.Document.Write("<html><head></head><body>");
                    webBrowser1.Document.Write(elm.InnerHtml);
                    webBrowser1.Document.Write("</body></html>");
                    webBrowser1.Refresh();
                }
            }
        }*/
        /*
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
          
            System.Console.WriteLine(e.Url.ToString());
            System.Console.WriteLine("completed, now styling");
            HtmlDocument doc = webBrowser1.Document;

            System.Console.WriteLine(doc.Body.InnerHtml);

            foreach (HtmlElement elm in doc.GetElementsByTagName("h2"))
            {
                System.Console.WriteLine("styling h2");
                elm.Style = "color: #333;";
            }
            foreach (HtmlElement elm in doc.GetElementsByTagName("h4"))
                elm.Style = "color: #e34c00;";

            foreach (HtmlElement elm in doc.GetElementsByTagName("div"))
            {
                if (elm.GetAttribute("classname") == "sourceblocktext")
                {
                    elm.Style = "font-family: monospace;font-size: smaller;"; 
                }
            }

            webBrowser1.DocumentCompleted -= webBrowser1_DocumentCompleted;
           
        }
         * */

    }
}
