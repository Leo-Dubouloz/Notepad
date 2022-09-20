using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Notepad.Objects
{
    public class Session
    {
        private const string FILENAME = "session.xml";
        private static string _applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string _applicationPath = Path.Combine(_applicationDataPath, "Notepad.NET");

        private readonly XmlWriterSettings _writerSettings;

        public static string BackupPath = Path.Combine(_applicationPath, "Notepad.NET", "backup");

        /// <summary>
        /// Chemin d'accès et nom du fichier représentant la session.
        /// </summary>
        public static string FileName { get; } = Path.Combine(_applicationPath, FILENAME);

        [XmlAttribute(AttributeName = "ActiveIndex")]
        /// <summary>
        /// Représente l'index de l'onglet sélectionné lors de la fermeture de l'application.
        /// </summary>
        public int ActiveIndex { get; set; } = 0;

        [XmlElement(ElementName = "File")]
        public List<TextFile> Files { get; set; } // Les fichiers ouverts, les nouveaux fichiers sont stocker dans cette liste

        public Session()
        {
            Files = new List<TextFile>();

            // Structure du fichier xml
            _writerSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                OmitXmlDeclaration = true
            };

            // Si le dossier n'existe pas, alors on en créer un
            if (!Directory.Exists(_applicationPath))
            {
                Directory.CreateDirectory(_applicationPath);
            }
        }

        public static async Task<Session> Load()
        {
            var session = new Session();
            if(File.Exists(FileName))
            {
                var serializer = new XmlSerializer(typeof(Session));
                var streamReader = new StreamReader(FileName);

                try
                {
                    session = (Session)serializer.Deserialize(streamReader);
                    foreach(var file in session.Files)
                    {
                        var fileName = file.FileName;
                        var backupFileName = file.BackupFileName;

                        file.SafeFileName = Path.GetFileName(fileName);

                        // Fichier existant sur le disque
                        if(File.Exists(fileName))
                        {
                            using (StreamReader reader = new StreamReader(fileName))
                            {
                                file.Contents = await reader.ReadToEndAsync();
                            }
                        }

                        // Fichier Backup du dossier Backup
                        if (File.Exists(backupFileName))
                        {
                            using (StreamReader reader = new StreamReader(backupFileName))
                            {
                                file.Contents = await reader.ReadToEndAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Une erreur s'est produite :" + ex.Message);
                }
                streamReader.Close();
            }

            return session;
        }

        public void Save()
        {
            var emptyNamespace = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }); // enlève la surcouche d'url dans le fichier xml
            var serializer = new XmlSerializer(typeof(Session));

            using (XmlWriter writer = XmlWriter.Create(FileName, _writerSettings))
            {
                serializer.Serialize(writer, this, emptyNamespace);
            }
        }

        public async void BackupFile(TextFile file)
        {
            if(!Directory.Exists(BackupPath))
            {
                await Task.Run(() => Directory.CreateDirectory(BackupPath)); // Task.Run permet d'éxécuter des méthodes qui ne sont pas nativement asynchrones.
            }
            if(file.FileName.StartsWith("Sans titre"))
            {
                using(StreamWriter writer = File.CreateText(file.BackupFileName))
                {
                    await writer.WriteAsync(file.Contents);
                }
            }
        }

    }
}
