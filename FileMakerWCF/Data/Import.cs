using System;
using System.IO;

namespace FMWData
{
    public class Import
    {
        public string _id;
        public string pieceId;
        public string path;
        public int isNew;
        public string machineId;

        public Import()
        {

        }

        public Import(string _id,  string pieceId, string path,int isNew, string machineId)
        {
            this._id = _id;
            this.pieceId = pieceId;
            this.path = path;
            this.isNew = isNew;
            this.machineId = machineId;
        }


        /// <summary>
        /// Check que l'import possède les valeurs nécessaires à l'ajout dans la liste des APIClass
        /// </summary>
        /// <returns></returns>
        internal bool CheckAllData()
        {
            bool a = isNew > -2 && isNew < 2 && path != null && _id != null && pieceId != null && machineId != null;
            return a;
        }

        /// <summary>
        /// Essaie de trouver le dossier vers lequel le path de l'import pointe
        /// </summary>
        /// <returns></returns>
        internal bool CheckPath()
        {
            try
            {
                return Directory.Exists(path.Substring(0, path.LastIndexOf('\\')));
            }
            catch (Exception)
            {
                return false;
            }
            
        }
    }

}