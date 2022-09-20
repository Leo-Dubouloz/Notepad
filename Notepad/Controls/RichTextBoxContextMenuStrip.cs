using System.Windows.Forms;

namespace Notepad.Controls
{
    public class RichTextBoxContextMenuStrip : ContextMenuStrip
    {
        private RichTextBox _richTextBox;
        private const string NAME = "RtbContextMenuStrip";

        public RichTextBoxContextMenuStrip(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
            Name = NAME;

            var cut = new ToolStripMenuItem("Couper");
            var copy = new ToolStripMenuItem("Copier");
            var paste = new ToolStripMenuItem("Coller");
            var selectAll = new ToolStripMenuItem("Sélectionner tout");

            //Création de l'événement clic
            cut.Click += (s, e) => _richTextBox.Cut();  // += = définir un nouvel événement, s = sender, e = event  
            copy.Click += (s, e) => _richTextBox.Copy();
            paste.Click += (s, e) => _richTextBox.Paste();
            selectAll.Click += (s, e) => _richTextBox.SelectAll();

            Items.AddRange(new ToolStripItem[] { cut, copy, paste, selectAll });
        }
    }
}
