using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace FMWClasses
{
    public class SurfaceFileMakerClass
    {
        private string rep_dep;
        public string Repertoire_depart
        {
            get
            {
                return this.rep_dep;
            }
            set
            {
                this.rep_dep = value;
                CreateWatcher();
            }
        }

        private string _name = ".xml";
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = Nom_fichier + "." + Nom_extension;
            }
        }

        public string repertoire_destination;

        private string nom_fic;
        public string Nom_fichier
        {
            get
            {
                return nom_fic;
            }
            set
            {
                nom_fic = value;
                Name = "name";
            }
        }

        private string nom_ext = "xml";
        public string Nom_extension
        {
            get
            {
                return nom_ext;
            }
            set
            {
                nom_ext = value;
                Name = "name";
            }
        }

        public string nom_supprimer;
        public string text;

        ObservableCollection<SurfaceFileString> _list;
        public ObservableCollection<SurfaceFileString> List
        {
            get
            {
                return _list;
            }
            set
            {
                _list = value;
            }
        }

        public DateTime lastFile;

        public SurfaceFileMakerClass()
        {
            List = new ObservableCollection<SurfaceFileString>();
        }

        FileSystemWatcher watcher = new FileSystemWatcher();

        public void CreateWatcher()
        {
            watcher.Path = rep_dep;
            try
            {
                // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Only watch text files.
                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                //watcher.Deleted += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);

                // Begin watching.
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                SaverLoader.WriteLog("Error: " + ex.Message);
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            var file = new SurfaceFileString(e.FullPath);
            bool found = false;
            string toAdd = "";
            foreach (SurfaceFileString s in List)
            {
                Regex r = new Regex("" + s.name + "*", RegexOptions.IgnoreCase);
                Match m = r.Match(file.name);
                int matchCount = 0;
                while (m.Success)
                {
                    Console.WriteLine("Match" + (++matchCount));
                    for (int i = 1; i <= 2; i++)
                    {
                        Group g = m.Groups[i];
                        Console.WriteLine("Group" + i + "='" + g + "'");
                        CaptureCollection cc = g.Captures;
                        for (int j = 0; j < cc.Count; j++)
                        {
                            Capture c = cc[j];
                            Console.WriteLine("Capture" + j + "='" + c + "', Position=" + c.Index);
                        }
                    }
                    m = m.NextMatch();
                }
                if (matchCount > 0)
                {
                    toAdd = file.name.Replace(s.name, "");
                    found = true;
                    break;
                }
            }
            if (found)
            {
                string path = Path.GetDirectoryName(e.FullPath);
                foreach (SurfaceFileString s in List)
                {
                    if (!s.exists(toAdd, path))
                    {
                        return;
                    }
                }
                List<string> lines = new List<string>();
                lines.Add("<BCPFORMAT>");

                DateTime date = DateTime.Now;
                foreach (SurfaceFileString s in List)
                {
                    bool filelock = true;
                    int time = 0;

                    while (filelock && time < 10000)
                    {
                        time = (int)(DateTime.Now.Subtract(date)).TotalMilliseconds;
                        try
                        {
                            lines.Add("<surface name=\"" + s.name + "\">");
                            string[] lineFile = WriteSafeReadAllLines(s.getPath(toAdd, path));
                            foreach (string line in lineFile)
                            {
                                lines.Add(line);
                            }
                            lines.Add("</surface>");
                            filelock = false;

                        }
                        catch (Exception) { }
                    }
                }
                lines.Add("</BCPFORMAT>");
                CreateFile(lines);
            }
        }

        public string[] WriteSafeReadAllLines(String path)
        {
            using (var csv = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(csv))
            {
                List<string> file = new List<string>();
                while (!sr.EndOfStream)
                {
                    file.Add(sr.ReadLine());
                }

                return file.ToArray();
            }
        }

        private void OnRenamed(object source, FileSystemEventArgs e)
        {
        }

        public void CreateFile()
        {
            List<string> lines = new List<string>();

        }

        public void CreateFile(List<string> lines)
        {
            try
            {
                string path = repertoire_destination + "\\" + Nom_fichier + "." + Nom_extension;
                //System.IO.File.WriteAllLines(path, lines);
                using (StreamWriter file = new StreamWriter(path))
                {
                    foreach (string line in lines)
                    {
                        // If the line doesn't contain the word 'Second', write the line to the file.
                        file.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                SaverLoader.WriteLog("Error: " + ex.Message);
            }
        }

        public void Refactor(string changingPart)
        {
            foreach (SurfaceFileString s in List)
            {
                s.refactor(changingPart);
            }
        }

        public override string ToString()
        {
            return "file" + Nom_fichier + "." + Nom_extension;
        }

    }
}
