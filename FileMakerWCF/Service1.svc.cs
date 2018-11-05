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
        //Constructeur appellé à chaque démarrage du service
        static Service1()
        {
            //Création des logs
            if (!EventLog.SourceExists("FileMaker"))
            {
                EventLog.CreateEventSource("FileMaker", "FileMakerLogs");
            }
            //For restart purposes
            if (SaverLoader.list_api == null)
            {
                SaverLoader.list_api = new List<APIClass>();
                Task.Run(() => SaverLoader.InitializeAsync()).Wait();
                SaverLoader.WriteLog("Init Done");
            }
        }

        public string GetData(string s)
        {
            return "You rebooted the system:"+ s;
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
            List<APIClass> toRemoveApi = new List<APIClass>();
            //on supprime les watchers et les classes d'api obsolètes
            foreach (var api in SaverLoader.list_api)
            {
                if (toRemove.Exists(x => x._id == api.import._id))
                {
                    api.StopWatcher();
                    toRemove.Remove(toRemove.Where(x => x._id == api.import._id).Single());
                    toRemoveApi.Add(api);
                }
            }
            SaverLoader.list_api.RemoveAll( x=> toRemoveApi.Exists(y=>y.import._id == x.import._id));

            //On ajoute la liste des imports à enlever n'ayant pas trouver de correspondances
            failList.AddRange(toRemove.Where(x=>x.isNew!=1));
            //On ajoute les éléments à ajouter
            List<Import> toAdd = importList.Where(x => x.isNew == 1).ToList();
            foreach (var import in toAdd)
            {
                if (import.CheckAllData() && import.CheckPath())
                    SaverLoader.list_api.Add(new APIClass(import));
                else
                    //On ajoute à la liste d'échecs les imports d'ajouts non conformes
                    failList.Add(import);
            }
            /*
             * Debug Purposes
            int tmp = 0;
            foreach (var api in SaverLoader.list_api)
            {
                SaverLoader.WriteLog("Num : " + tmp + " Api : " + api.ToString());
                tmp++;
            }*/
            return failList;
        }
    }
}
