using FMWData;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace FileMakerWCF
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom d'interface "IService1" à la fois dans le code et le fichier de configuration.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        [WebGet(UriTemplate = "GetData?s={s}")]
        string GetData(string s);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json,
            UriTemplate = "PostImports",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        List<Import> PostImports(List<Import> importList);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json,
        UriTemplate = "TestPath",
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.Bare)]
        string TestPath(Import import);

        [OperationContract]
        [WebInvoke(
            Method ="OPTIONS",
            RequestFormat = WebMessageFormat.Json,
            UriTemplate = "PostImports",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        void OptionsImports();

    }
}
