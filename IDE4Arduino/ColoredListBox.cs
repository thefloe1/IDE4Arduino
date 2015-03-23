using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDE4Arduino
{
    public partial class ColoredListBox : ListBox
    {
        public ColoredListBox()
        {
            InitializeComponent();
            this.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.DrawItem += ColoredListBox_DrawItem;
            this.ItemHeight = 18;         
        }

        void ColoredListBox_DrawItem(object sender, DrawItemEventArgs e)
        {

            if (this.Items.Count < 1)
                return;

            if (e.Index < 0)
                return;

            if (!Visible)
                return;

            ColoredListBoxItem item = this.Items[e.Index] as ColoredListBoxItem;

            string text = this.Items[e.Index].ToString();
            Color color = this.ForeColor;
            
            if (item != null)
            {
                text = item.Message;
                color = item.ItemColor;
            }

            //e.DrawBackground();
            Brush brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ?
                             Brushes.LightGray : new SolidBrush(e.BackColor);
            e.Graphics.FillRectangle(brush, e.Bounds);

            e.Graphics.DrawString(text, e.Font, new SolidBrush(color), e.Bounds);

            e.DrawFocusRectangle();

        }
    }

    class ColoredListBoxItem
    {
        public Color ItemColor { get; set; }
        public string Message { get; set; }

        public ColoredListBoxItem(Color c, string m)
        { 
            ItemColor = c; 
            Message = m;
        }

        public override string ToString()
        {
            return Message;
        }
    }
}
