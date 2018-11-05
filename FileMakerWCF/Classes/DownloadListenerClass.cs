using System;
using System.Collections.Generic;
using System.IO;

namespace FMWClasses
{
    public class DownloadListenerClass
    {
        private string rep_dep;
        public string Repertoire_depart
        {
            get
            {
                return rep_dep;
            }
            set
            {
                rep_dep = value;
                CreateWatcher();
            }
        }
        
        public string repertoire_destination;
        public string nom_fichier;
        public string nom_extension;
        public string text;
        public DateTime lastFile;

        FileSystemWatcher watcher = new FileSystemWatcher();

        public DownloadListenerClass()
        {
            text = "part;{part};\n"+
                "machine;{machine};\n" +
                "program;{program};\n" +
                "context;{context};\n" +
                "order;{FO};\n" +
                "context;{context};\n" +
                "contexte description;{contexte description};\n" +
                "{values}";
            lastFile = DateTime.Now;
        }

        public void CreateWatcher()
        {
            watcher.Path = rep_dep;
            try
            {
                // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Only watch text files.
                watcher.Filter = "*.csv";

                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                //watcher.Deleted += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);

                // Begin watching.
                watcher.EnableRaisingEvents = true;
            }
            catch(Exception ex)
            {
                SaverLoader.WriteLog("Error: " + ex.Message);
            }
        }

        // Create a new FileSystemWatcher and set its properties.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            int milliSeconds = (int)(DateTime.Now.Subtract(lastFile)).TotalMilliseconds;
            if(milliSeconds < 1000)
                return;
            try
            {
                string[] lines = File.ReadAllLines(e.FullPath);
                if (!CheckIntegrity(lines))
                {
                    SaverLoader.WriteLog("On Changed -> File Format does not correspond");
                    return;
                }

                bool correction = false;
                List<string> correctionLines = new List<string>();
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (string s in lines)
                {
                    try
                    {
                        if (correction)
                        {
                            correctionLines.Add(s);
                        }
                        string[] values = s.Split(';');
                        if (values.Length == 2)
                        {
                            dict.Add(values[0], values[1]);
                        }
                        if (s.IndexOf("values") > -1)
                        {
                            correction = true;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                string path = repertoire_destination + "\\" + ChangeText(nom_fichier, dict) + @"." + ChangeText(nom_extension,dict);
                //System.IO.File.WriteAllLines(path, lines);
                string[] linesToWrite = text.Split('\n');
                using (StreamWriter file = new StreamWriter(path))
                {
                    foreach (string line in linesToWrite)
                    {
                        // If the line doesn't contain the word 'Second', write the line to the file.
                        file.WriteLine(ChangeText(line, dict, correctionLines));
                    }
                }
                lastFile = DateTime.Now;
            }
            catch(Exception ex)
            {
                SaverLoader.WriteLog("Error: " + ex.Message);
            }
        }

        private bool CheckIntegrity(string[] lines)
        {
            bool part = false;
            bool values = false;
            bool machine = false;
            foreach (string s in lines)
            {
                if(s.IndexOf("part") > -1)
                {
                    part = true;
                }
                if (s.IndexOf("values") > -1)
                {
                    values = true;
                }
                if (s.IndexOf("machine") > -1)
                {
                    machine = true;
                }
            }
            return part & values & machine;
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        private string ChangeText(string line, Dictionary<string, string> dict)
        {
            foreach (KeyValuePair<string, string> entry in dict)
            {
                var key = "{" + entry.Key + "}";
                // do something with entry.Value or entry.Key
                if (line.IndexOf(key) > -1)
                {
                    line = line.Replace(key, entry.Value);
                }
            }
            return line;
        }

        private string ChangeText(string line, Dictionary<string, string> dict, List<string> correctionLines)
        {
            if (line.IndexOf("{values}")> -1){
                string result = "";
                foreach (string s in correctionLines)
                {
                    // If the line doesn't contain the word 'Second', write the line to the file.
                    result += s + "\n";
                }
                return result;
            }
            return ChangeText(line, dict);
        }

    }
}
