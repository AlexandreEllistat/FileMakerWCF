using FMWData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FMWClasses
{
    public class APIClass
    {
        public Import import;
        FileSystemWatcher watcher = new FileSystemWatcher();
        public DateTime lastFile;
        public string filter;

        //ctor
        public APIClass()
        {
            lastFile = DateTime.Now;
        }

        public APIClass(Import i)
        {
            lastFile = DateTime.Now;
            import = i;
            filter = FindFilter();
            CreateWatcher();
        }

        /// <summary>
        /// Créé le filtre à appliquer aux fichiers du dossier à surveiller
        /// </summary>
        /// <returns>la chaine servant de filtre</returns>
        public string FindFilter()
        {
            
            filter = import.path.Substring(import.path.LastIndexOf('\\') + 1);
            try
            {
                //On récupère l'extension du fichier
                string ext = import.path.Substring(import.path.LastIndexOf('.') + 1);
                //On ne garde que le nom du fichier (sans le chemin ni l'extension)
                string tmpPath = import.path.Substring(import.path.LastIndexOf('\\') + 1, import.path.LastIndexOf('.') - import.path.LastIndexOf('\\') - 1);
                tmpPath = tmpPath.Replace("*", "(.*)");
                filter = tmpPath + "." + ext;
            }
            catch (Exception e)
            {
                SaverLoader.WriteLog("Erreur de filtre : " + e.ToString());
            }
            return filter;
        }

        //Watch the specified rep_dep directory
        public void CreateWatcher()
        {
            try
            {
                watcher.Path = import.path.Substring(0, import.path.LastIndexOf("\\"));
                // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.
                watcher.Created += new FileSystemEventHandler(OnChanged);
                // Begin watching.
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                SaverLoader.WriteLog("Error: " + ex.Message);
            }
        }

        // Create a new FileSystemWatcher and set its properties.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Regex rgx = new Regex(filter, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(e.Name);
            if (matches.Count < 1)
            {
                return;
            }
            // Specify what is done when a file is changed, created, or deleted.
            int milliSeconds = (int)(DateTime.Now.Subtract(lastFile)).TotalMilliseconds;
            if (milliSeconds < 1000)
                return;
            try{
                //read all bytes???
                
                string lines = File.ReadAllText(e.FullPath);
                
                //Construction du corps du post ->
                PostData postData = new PostData(import.pieceId, import._id, lines);
                Dictionary<string,string> postDataDict = BuildData(postData);

                //API calls here ->
                APICaller.DoPostAsync(postDataDict);
            }
            catch (Exception ex)
            {
                SaverLoader.WriteLog("OnChanged Error: " + ex.Message);
            }
        }

        private Dictionary<string, string> BuildData(PostData postData)
        {
            Dictionary<string, string> Done = new Dictionary<string, string>
            {
                { "PieceId", postData.PieceId },
                { "ImportId", postData.ImportId },
                { "FileContent", postData.FileContent }
            };
            return Done;
        }

        public override string ToString()
        {
            return "Watcher on file : " + import.path; 
        }

        /// <summary>
        /// Détruit un watcher
        /// </summary>
        internal void StopWatcher()
        {
            watcher.EnableRaisingEvents = false;
        }

    }
}
