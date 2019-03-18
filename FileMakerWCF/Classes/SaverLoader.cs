using FileMakerWCF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;

namespace FMWClasses
{
    public static class SaverLoader
    {
        public static List<FileListener> list_FileListener;

        //Api
        public static string login ="";
        public static string apiKey ="";
        public static string apiAddress ="";

        //Logs
        public static EventLog myLog = new EventLog
        {
            Source = "FileMaker"
        };

        //Appelé dans le onStart du service
        public static async Task InitializeAsync()
        {
            list_FileListener = new List<FileListener>();
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
                                    apiAddress = reader.GetAttribute("apiAddress");
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
        

        public static void WriteLog(string log)
        {
            if(Service1.hasEventLogAccess)
            myLog.WriteEntry(log);
        }
    }
}
