using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using FMWData;

namespace FMWClasses
{
    public static class SaverLoader
    {
        //A déplacer dans le launch du service
        public static List<DownloadListenerClass> list_dlc;
        public static List<SurfaceFileMakerClass> list_sfm;
        public static List<APIClass> list_api;

        //Api
        public static string login ="";
        public static string apiKey ="";
        public static string apiAdress ="";

        //Logs
        public static EventLog myLog = new EventLog
        {
            Source = "FileMaker"
        };

        //Appelé dans le onStart du service
        public static async Task InitializeAsync()
        {
            /*
            list_dlc = new List<DownloadListenerClass>
            {
                new DownloadListenerClass()
            };
            list_sfm = new List<SurfaceFileMakerClass>
            {
                new SurfaceFileMakerClass()
            };*/
            list_api = new List<APIClass>();
            //Retrieve API infos
            ReadAPIInfos();
            await APICaller.CallAllImportsAsync();
        }

        public static void ReadAPIInfos()
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(GetAPIFile()))
                {
                    // Parse the file and display each of the nodes.
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "AUTH":
                                    login = reader.GetAttribute("login");
                                    apiKey = reader.GetAttribute("apiKey");
                                    apiAdress = reader.GetAttribute("apiAdress");
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e){
                myLog.WriteEntry(e.ToString());
            }
        }


        /// <summary>
        /// Récupère les infos de connection de l'API
        /// lors du premier démarrage du service
        /// </summary>
        private static string GetAPIFile()
        {
            return AppDomain.CurrentDomain.BaseDirectory + "\\APIInfos.xml";
        }
        
        /*
        public static void WritefileRep()
        {
            string version = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;
            XmlWriterSettings xmlSettings = new XmlWriterSettings
            {
                Indent = true
            };
            using (XmlWriter writer = XmlWriter.Create("GetTempFile()", xmlSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Document", "GetTempFile()");
                writer.WriteAttributeString("Version", version);

                foreach(DownloadListenerClass dlc in list_dlc) {
                    writer.WriteStartElement("DLC");
                    writer.WriteAttributeString("FolderListened", dlc.Repertoire_depart);
                    writer.WriteAttributeString("FolderDestination", dlc.repertoire_destination);
                    writer.WriteAttributeString("filename", dlc.nom_fichier);
                    writer.WriteAttributeString("fileExtension", dlc.nom_extension);
                    writer.WriteString(dlc.text);
                    writer.WriteEndElement();
                }
                foreach (SurfaceFileMakerClass sfm in list_sfm)
                {
                    writer.WriteStartElement("SFM");
                    writer.WriteAttributeString("FolderListened", sfm.Repertoire_depart);
                    writer.WriteAttributeString("FolderDestination", sfm.repertoire_destination);
                    writer.WriteAttributeString("filename", sfm.Nom_fichier);
                    writer.WriteAttributeString("fileExtension", sfm.Nom_extension);
                    writer.WriteAttributeString("nomSupprimer", sfm.nom_supprimer);
                    foreach (SurfaceFileString fs in sfm.List)
                    {
                        writer.WriteStartElement("SFMFILE");
                        writer.WriteAttributeString("original_file", fs.original_file);
                        writer.WriteAttributeString("name",fs.name);
                        writer.WriteAttributeString("nameInit", fs.name_init);
                        writer.WriteAttributeString("extension", fs.extension);
                        writer.WriteAttributeString("removePart", fs.removePart);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                foreach (APIClass api in list_api)
                {
                    writer.WriteStartElement("API");
                    //writer.WriteAttributeString("FolderListened", api.PathOfFile);
                    //writer.WriteAttributeString("PieceId", api.piece._id);
                    writer.WriteAttributeString("ImportId", api.import._id);
                   // writer.WriteAttributeString("PathOfFile", api.PathOfFile);
                    writer.WriteEndElement();
                }
                //API part
                writer.WriteStartElement("AUTH");
                writer.WriteAttributeString("login", login);
                writer.WriteAttributeString("apiKey", Encrypt.EncryptString(apiKey));
                writer.WriteAttributeString("apiAdress", apiAdress);
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
        */

        public static void WriteLog(string log)
        {
            myLog.WriteEntry(log);
        }
    }
}
