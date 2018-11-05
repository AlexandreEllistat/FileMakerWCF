using System;

namespace FMWClasses
{
    public class SurfaceFileString
    {
        public string original_file { get; set; }
        public string name { get; set; }
        public string extension { get; set; }
        public string removePart { get; set; }
        public string name_init { get; set; }

        public SurfaceFileString(string _name)
        {
            original_file = _name;
            name = System.IO.Path.GetFileNameWithoutExtension(_name);
            extension = System.IO.Path.GetExtension(_name);
            name_init = name;
        }

        public SurfaceFileString(string _name, string toRemove)
        {
            original_file = _name;
            name = System.IO.Path.GetFileNameWithoutExtension(_name);
            extension = System.IO.Path.GetExtension(_name);
            name_init = name;
            refactor(toRemove);
        }

        public SurfaceFileString(string _original_file, string _name, string _name_init, string _extension, string toRemove)
        {
            original_file = _original_file;
            name = _name;
            extension = _extension;
            name_init = _name_init;
            removePart = toRemove;
        }

        public void refactor(string toRemove)
        {
            
            removePart = toRemove;
            string nc = String.Copy(name_init);
            if (toRemove == ""|| toRemove == null)
            {
                name = nc;
                return;
            }
            name = nc.Replace(toRemove, "");
        }

        public bool exists(string toAdd, string path)
        {
            var file = getPath(toAdd, path);
            return System.IO.File.Exists(file);
        }

        public string getPath(string toAdd, string path)
        {
            return path + "\\" + name + toAdd + extension;
        }


    }

   
}
