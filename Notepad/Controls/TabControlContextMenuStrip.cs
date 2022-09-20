using Notepad.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;

namespace Notepad.Controls
{
    public class TabControlContextMenuStrip : ContextMenuStrip
    {
        private const string NAME = "TabControlContextMenuStrip";
        private MainForm _form;

        public TabControlContextMenuStrip()
        {
            Name = NAME;

            var closeTab = new ToolStripMenuItem("Fermer");
            var closeAllTabExceptThis = new ToolStripMenuItem("Fermer tout sauf ce fichier");
            var openFileInExplorer = new ToolStripMenuItem("Ouvrir le répertoire du fichier en cours dans l'explorateur");

            Items.AddRange(new ToolStripItem[] { closeTab, closeAllTabExceptThis, openFileInExplorer });

            HandleCreated += (s, e) =>
            {
                _form = SourceControl.FindForm() as MainForm;
            };

            closeTab.Click += (s, e) =>
            {
                var selectedTab = _form.MainTabControl.SelectedTab;

                _form.Session.Files.Remove(_form.CurrentFile);

                if(_form.MainTabControl.TabCount > 1)
                {
                    _form.MainTabControl.TabPages.Remove(selectedTab);
                    var newIndex = _form.MainTabControl.TabCount - 1;
                    _form.MainTabControl.SelectedIndex = newIndex;
                    _form.CurrentFile = _form.Session.Files[newIndex];
                }
                else
                {
                    var fileName = "Sans titre 1";
                    var file = new TextFile(fileName);

                    _form.CurrentFile = file;
                    _form.CurrentRtb.Clear();

                    _form.MainTabControl.SelectedTab.Text = file.FileName;
                    _form.Session.Files.Add(file);
                    _form.Text = "Sans titre 1 - Notepad.NET";

                }
            };

            closeAllTabExceptThis.Click += (s, e) =>
            {
                var filesToDelete = new List<TextFile>();

                if(_form.MainTabControl.TabCount > 1)
                {
                    TabPage selectedTab = _form.MainTabControl.SelectedTab;
                    // Suppression des onglets
                    foreach (TabPage tabPage in _form.MainTabControl.TabPages)
                    {
                        if(tabPage != selectedTab)
                        {
                            _form.MainTabControl.TabPages.Remove(tabPage);
                        }
                    }

                    foreach (var file in _form.Session.Files)
                    {
                        if(file != _form.CurrentFile)
                        {
                            filesToDelete.Add(file);
                        }
                    }
                    // Compare deux listes de fichiers (fichiers de ma session, et les fichiers à supprimer), et supprime les fichiers égaux
                    _form.Session.Files = _form.Session.Files.Except(filesToDelete).ToList();
                }
            };

            openFileInExplorer.Click += (s, e) =>
            {
                var arguments = $"/select, \"{_form.CurrentFile.FileName}\"";
                Process.Start("explorer.exe", arguments);
            };
        }
    }
}
