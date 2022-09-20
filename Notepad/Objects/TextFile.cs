using System;
using System.IO;
using System.Xml.Serialization;

namespace Notepad.Objects
{
    public class TextFile
    {
        /// <summary>
        /// Chemin d'accès et nom du fichier.
        /// </summary>
        [XmlAttribute(AttributeName = "FileName")]
        public string FileName { get; set; } // get;set permet de récupérer et définir la valeur en dehors de la class

        /// <summary>
        /// Chemin d'accès et nom du fichier backup.
        /// </summary>
        [XmlAttribute(AttributeName = "BackupFileName")]
        public string BackupFileName { get; set; } = String.Empty; // Le chemin d'accès et le nom des fichiers backup, qui sont des fichiers qui ne seront pas encore enregistrés

        /// <summary>
        /// Nom et extension du fichier. Le nom du fichier n'inclut pas le chemin d'accès.
        /// </summary>
        [XmlIgnore()]
        public string SafeFileName { get; set; } // Nom et extension des fichiers (n'inclut pas le chemin d'accès)

        /// <summary>
        /// Nom et extension du fichier backup. Le nom du fichier n'inclut pas le chemin d'accès.
        /// </summary>
        [XmlIgnore()]
        public string SafeBackupFileName { get; set; } // Pareille

        /// <summary>
        /// Contenu du fichier.
        /// </summary>
        [XmlIgnore()]
        public string Contents { get; set; } = String.Empty; //Lorsque l'on créer un fichier, le texte n'est pas NULL mais un string vide (="")

        /// <summary>
        /// Constructeur de la classe TextFile.
        /// </summary>
        public TextFile() // Besoin d'un constructeur vide pour la sérialisation et la désérialisation des objets
        { }

        /// <summary>
        /// Constructeur de la classe TextFile.
        /// </summary>
        /// <param name="fileName">Chemin d'accès et nom du fichier.</param>
        public TextFile(string fileName)
        {
            FileName = fileName;
            SafeFileName = Path.GetFileName(fileName);

            if(FileName.StartsWith("Sans titre"))
            {
                SafeBackupFileName = $"{FileName}@{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";
                BackupFileName = Path.Combine(Session.BackupPath, SafeBackupFileName);
            }
        }

    }
}
