using FMWData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FMWClasses
{
    class APICaller
    {
        private static readonly HttpClient client = new HttpClient();

        public static void SetHeaders()
        {
            SaverLoader.ReadAPIInfos();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("username", SaverLoader.login);
            client.DefaultRequestHeaders.Add("apikey", SaverLoader.apiKey);
            client.Timeout.Add(new TimeSpan(0, 0, 5));
        }

        static readonly string ROUTE = "insertMesures";

        /// <summary>
        /// On appelle l'API avec le paramètre ServiceReset
        /// Cela a pour effet de trigger tous les webhooks déclarées dans Ellisetting
        /// Renvoyant ainsi tous les watchers à tous les FileMaker linkés
        /// Il n'est pas possible de demander à envoyer à un seul FileMaker 
        /// car on ne sait pas à l'avance quelles données sont liées au watcher appelant
        /// </summary>
        public static async Task CallAllImportsAsync()
        {
            await DoGetAsync(ROUTE + "?q=ServiceReset");
        }

        /// <summary>
        /// Retourne le corps d'une requete Http passée en paramétre
        /// </summary>
        public static async Task<string> DoGetAsync(string CollectionName)
        {
            if (SaverLoader.apiAdress == "" || SaverLoader.apiKey == "" || SaverLoader.login == "")
                SaverLoader.ReadAPIInfos();
            SetHeaders();
            try
            {
                string responseString = await client.GetStringAsync(SaverLoader.apiAdress + CollectionName);
                return responseString;
            }
            catch (Exception e)
            {
                SaverLoader.myLog.WriteEntry(e.ToString());
                return "{}";
            }
        }
        
        
        /// <summary>
        /// Envoie les données de mesure à l'application ellisetting 
        /// </summary>
        public static async void DoPostAsync(Dictionary<string,string> postData)
        {
            try
            {
                SetHeaders();
                var content = new FormUrlEncodedContent(postData);
                var response = await client.PostAsync(SaverLoader.apiAdress + ROUTE, content);
                var responseString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception exc)
            {
                SaverLoader.WriteLog("Erreur dans le Post : " + exc.ToString());
            }
        }

        //Transforme un Json en liste d'objet C#
        public static List<T> DeserializeToList<T>(string jsonString)
        {
            try {
                var array = JArray.Parse(jsonString);
                List<T> objectsList = new List<T>();
                foreach (var item in array)
                {
                    objectsList.Add(item.ToObject<T>());
                }
                return objectsList;
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }
    }
}
