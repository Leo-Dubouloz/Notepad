using Notepad.Objects;
using Notepad.Services;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace Notepad.Controls
{
    // Héritage de l'objet MenuStrip de la boîte à outil
    public class MainMenuStrip : MenuStrip
    {
        private const string NAME = "MainMenuStrip";
        private MainForm _form;
        private FontDialog _fontDialog;
        private OpenFileDialog _openFileDialog;
        private SaveFileDialog _saveFileDialog;

        public MainMenuStrip()
        {
            Name = NAME; // Constante car ce nom ne changera jamais
            Dock = DockStyle.Top; // place le menu en haut du formulaire

            _fontDialog = new FontDialog();
            _openFileDialog = new OpenFileDialog();
            _saveFileDialog = new SaveFileDialog();

            FileDropDownMenu();
            EditDropMenu();
            FormatDropMenu();
            ViewDropDownMenu();

            HandleCreated += (s, e) =>
            {
                _form = FindForm() as MainForm;
            };
        }

        public void FileDropDownMenu()
        {
            // Création du premier onglet du menu déroulant (Fichier)
            // Usage de var pour adoucir la lecture du code (var est défini par le programme, variable adaptative)
            var fileDropDownMenu = new ToolStripMenuItem("Fichier"); // = ToolStripMenuItem fileDropDownMenu = new ToolStripMenuItem("Fichier");
            // Création des sous onglets du menu déroulant (Fichier), avec les raccourcis clavier 
            var newFile = new ToolStripMenuItem("Nouveau", null, null, Keys.Control | Keys.N);
            var open = new ToolStripMenuItem("Ouvrir...", null, null, Keys.Control | Keys.O);
            var save = new ToolStripMenuItem("Enregister", null, null, Keys.Control | Keys.S);
            var saveAs = new ToolStripMenuItem("Enregister sous...", null, null, Keys.Control | Keys.Shift | Keys.S);
            var quit = new ToolStripMenuItem("Quitter", null, null, Keys.Alt | Keys.F4);

            newFile.Click += (s, e) =>
            {
                var tabControl = _form.MainTabControl;
                var tabCount = tabControl.TabCount;

                var fileName = $"Sans titre {tabCount + 1}";
                var file = new TextFile(fileName);
                var rtb = new CustomRichTextBox();

                tabControl.TabPages.Add(file.SafeFileName);

                var newTabPage = tabControl.TabPages[tabCount];

                newTabPage.Controls.Add(rtb);
                _form.Session.Files.Add(file);
                tabControl.SelectedTab = newTabPage;

                _form.CurrentFile = file;
                _form.CurrentRtb = rtb;
            };

            open.Click += async (s, e) =>
            {
                if(_openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var tabControl = _form.MainTabControl;
                    var tabCount = tabControl.TabCount;

                    var file = new TextFile(_openFileDialog.FileName); // récupère le chemin d'accès du fichier

                    var rtb = new CustomRichTextBox();

                    _form.Text = $"{file.FileName} - Notepad.NET";

                    using(StreamReader reader = new StreamReader(file.FileName))
                    {
                        // Asynchrone permet d'éviter un freeze de l'application si le fichier est lourd (si asynchrone, alors mot clé await)
                        // Pour utiliser le mot clé await il faut que la méthode soit elle aussi asynchrone
                        file.Contents = await reader.ReadToEndAsync();
                    }

                    rtb.Text = file.Contents;

                    tabControl.TabPages.Add(file.SafeFileName);
                    tabControl.TabPages[tabCount].Controls.Add(rtb);

                    _form.Session.Files.Add(file);
                    _form.CurrentRtb = rtb;
                    _form.CurrentFile = file;
                    tabControl.SelectedTab = tabControl.TabPages[tabCount];
                }
            };

            saveAs.Click += async (s, e) =>
            {
                if(_saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var newFileName = _saveFileDialog.FileName;
                    var alreadyExists = false;

                    foreach(var file in _form.Session.Files)
                    {
                        if(file.FileName == newFileName)
                        {
                            MessageBox.Show("Ce fichier est déjà ouvert dans Notepad.NET", "ERREUR", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            alreadyExists = true;
                            break;
                        }
                    }

                    // Si le fichier à enregistrer n'existe pas déjà
                    if(!alreadyExists)
                    {
                        var file = new TextFile(newFileName) { Contents = _form.CurrentRtb.Text };
                        var oldFile = _form.Session.Files.Where(x => x.FileName == _form.CurrentFile.FileName).First();
                        
                        _form.Session.Files.Replace(oldFile, file);

                        using (StreamWriter writer = File.CreateText(file.FileName))
                        {
                            await writer.WriteAsync(file.Contents);
                        }

                        _form.MainTabControl.SelectedTab.Text = file.SafeFileName;
                        _form.Text = file.FileName;
                        _form.CurrentFile = file;
                    }
                }
            };

            save.Click += async (s, e) =>
            {
                var currentFile = _form.CurrentFile;
                var currentRtbText = _form.CurrentRtb.Text;

                if(currentFile.Contents != currentRtbText)
                {
                    if(File.Exists(currentFile.FileName))
                    {
                        currentFile.Contents = currentRtbText;
                        using (StreamWriter writer = File.CreateText(currentFile.FileName))
                        {
                            await writer.WriteAsync(currentFile.Contents);
                        }
                        
                        _form.Text = currentFile.FileName;
                        _form.MainTabControl.SelectedTab.Text = currentFile.SafeFileName;
                    }
                    else
                    {
                        saveAs.PerformClick();
                    }
                }
            };

            quit.Click += (s, e) =>
            {
                Application.Exit();
            };

            // Intégration des sous onglets dans l'onglet principal (Fichier)
            // Ajout d'un seul sous onglet => .Add  / Ajout de plusieurs sous onglets => .AddRange
            fileDropDownMenu.DropDownItems.AddRange(new ToolStripItem[] { newFile, open, save, saveAs, quit });

            Items.Add(fileDropDownMenu);
        }

        public void EditDropMenu()
        {
            var editDropDown = new ToolStripMenuItem("Edition");

            var undo = new ToolStripMenuItem("Annuler", null, null, Keys.Control | Keys.Z);
            var redo = new ToolStripMenuItem("Restaurer", null, null, Keys.Control | Keys.Y);

            undo.Click += (s, e) => { if (_form.CurrentRtb.CanUndo) _form.CurrentRtb.Undo(); };
            redo.Click += (s, e) => { if (_form.CurrentRtb.CanRedo) _form.CurrentRtb.Redo(); };

            editDropDown.DropDownItems.AddRange(new ToolStripItem[] { undo, redo });

            Items.Add(editDropDown);
        }

        public void FormatDropMenu()
        {
            var formatDropDown = new ToolStripMenuItem("Format");
            var font = new ToolStripMenuItem("Police...");

            font.Click += (s, e) =>
            {
                _fontDialog.Font = _form.CurrentRtb.Font;
                _fontDialog.ShowDialog();

                _form.CurrentRtb.Font = _fontDialog.Font;
            };

            formatDropDown.DropDownItems.AddRange(new ToolStripItem[] { font });
            Items.Add(formatDropDown);
        }

        public void ViewDropDownMenu()
        {
            var viewDropDown = new ToolStripMenuItem("Affichage");
            var alwaysOnTop = new ToolStripMenuItem("Toujours devant");

            var zoomDropDown = new ToolStripMenuItem("Zoom");
            var zoomIn = new ToolStripMenuItem("Zoom avant", null, null, Keys.Control | Keys.Add);
            var zoomOut = new ToolStripMenuItem("Zoom arrière", null, null, Keys.Control | Keys.Subtract);
            var zoomReset = new ToolStripMenuItem("Restaurer le zoom par défaut", null, null, Keys.Control | Keys.Divide);

            //Modifier l'affichage des raccourcis clavier 
            zoomIn.ShortcutKeyDisplayString = "Ctrl+Num +";
            zoomOut.ShortcutKeyDisplayString = "Ctrl+Num -";
            zoomReset.ShortcutKeyDisplayString = "Ctrl+Num /";

            alwaysOnTop.Click += (s, e) =>
            {
                if (alwaysOnTop.Checked)
                {
                    alwaysOnTop.Checked = false;
                    Program.MainForm.TopMost = false;
                }
                else
                {
                    alwaysOnTop.Checked = true;
                    Program.MainForm.TopMost = true;
                }
            };

            zoomIn.Click += (s, e) =>
            {
                if (_form.CurrentRtb.ZoomFactor < 5F) // 5F est la taille maximum du zoom (maximum 64F mais peut utile dans ce cas)
                {
                    _form.CurrentRtb.ZoomFactor += 0.3F;
                }
            };

            zoomOut.Click += (s, e) =>
            {
                if (_form.CurrentRtb.ZoomFactor > 0.7F) // 0.7F est la taille minimal du zoom inverse
                {
                    _form.CurrentRtb.ZoomFactor -= 0.3F;
                }
            };

            zoomReset.Click += (s, e) => { _form.CurrentRtb.ZoomFactor = 1F; };

            zoomDropDown.DropDownItems.AddRange(new ToolStripItem[] { zoomIn, zoomOut, zoomReset });
            viewDropDown.DropDownItems.AddRange(new ToolStripItem[] { alwaysOnTop, zoomDropDown });
            Items.Add(viewDropDown);
        }
    }
}
