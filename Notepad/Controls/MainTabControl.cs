using System.Linq;
using System.Windows.Forms;

namespace Notepad.Controls
{
    public class MainTabControl : TabControl
    {
        private const string NAME = "MainTabControl";
        private ContextMenuStrip _contextMenuStrip; // Déclaration d'un objet privé ContextMenuStrip
        private MainForm _form;
        public MainTabControl()
        {
            _contextMenuStrip = new TabControlContextMenuStrip(); // va permettre de lier avec la classe TabControlContextMenuStrip
            Name = NAME;
            ContextMenuStrip = _contextMenuStrip; // Fait la liaison entre les deux classes. ContextMenuStrip est une propriété du tabcontrol permettant l'usage du clic droit
            Dock = DockStyle.Fill;

            HandleCreated += (s, e) =>
            {
                _form = FindForm() as MainForm;
            };

            SelectedIndexChanged += (s, e) =>
            {
                _form.CurrentFile = _form.Session.Files[SelectedIndex];
                _form.CurrentRtb = (CustomRichTextBox)_form.MainTabControl.TabPages[SelectedIndex].Controls.Find("RtbTextFileContents", true).First();
                _form.Text = $"{_form.CurrentFile.FileName} - Notepad.NET";
            };

            // Ajout de la fonctionnalité suivante : lors du clic droit sur un onglet, on bascule sur celui-ci
            MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    for (int i = 0; i < TabCount; i++)
                    {
                        var rect = GetTabRect(i);

                        if(rect.Contains(e.Location))
                        {
                            SelectedIndex = i;
                            break;
                        }
                    }
                }
            };
        }
    }
}
