using FMWClasses;
using FMWData;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace FileMakerWCF
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" dans le code, le fichier svc et le fichier de configuration.
    // REMARQUE : pour lancer le client test WCF afin de tester ce service, sélectionnez Service1.svc ou Service1.svc.cs dans l'Explorateur de solutions et démarrez le débogage.
    public class Service1 : IService1
    {
        internal static bool hasEventLogAccess = true;

        //Constructeur appellé à chaque démarrage du service
        static Service1()
        {
            hasEventLogAccess = false;
            //Création des logs
            try
             {
                 if (!EventLog.SourceExists("FileMaker"))
                 {
                     EventLog.CreateEventSource("FileMaker", "FileMakerLogs");
                 }
             }
             catch {
                 hasEventLogAccess = false;
             }
             //For restart purposes
             if (SaverLoader.list_FileListener == null)
             {
                 SaverLoader.list_FileListener = new List<FileListener>();
                 Task.Run(() => SaverLoader.InitializeAsync()).Wait();
                 SaverLoader.WriteLog("Init Done");
             }
        }

        public string GetData(string s)
        {
            return "You rebooted the system:" + s;
        }

        public void OptionsImports()
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
        }

        public List<Import> PostImports(List<Import> importList)
        {
            //On instancie la liste des imports en échec (mauvais path ou simplement manque de data (id ...))
            List<Import> failList = new List<Import>();
            //On ajoute à la liste des échecs les imports dont l'action n'est ni l'ajout/update ni la suppression
            failList.AddRange(importList.Where(x => x.isNew != 1 && x.isNew != -1));

            //On supprime les éléments à supprimer ainsi que ceux envoyés pour modification
            List<Import> toRemove = importList.Where(x => x.isNew == -1 || x.isNew == 1 ).ToList();
            //on supprime les watchers et les classes d'api obsolètes
            foreach (var api in SaverLoader.list_FileListener)
            {
                if (toRemove.Exists(x => x._id == api.import._id))
                {
                    api.StopWatcher();
                    toRemove.Remove(toRemove.Where(x => x._id == api.import._id).Single());
                }
            }
            //On ajoute les éléments à ajouter
            List<Import> toAdd = importList.Where(x => x.isNew == 1).ToList();
            foreach (var import in toAdd)
            {
                if (import.CheckAllData() && import.CheckPath())
                    SaverLoader.list_FileListener.Add(new FileListener(import));
                else
                    //On ajoute à la liste d'échecs les imports d'ajouts non conformes
                    failList.Add(import);
            }
            return failList;
        }

        public string TestPath(Import import)
        {
            if (import.CheckAllData() && import.CheckPath())
                return "Le chemin existe";
            else
                return "Le chemin n'a pas été trouvé";
        }
    }
}