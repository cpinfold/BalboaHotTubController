using System.ServiceModel;
using System.ServiceModel.Web;

namespace BalboaHotTubController
{
    [ServiceContract]
    public interface IBalboaHotTub
    {
        [OperationContract]
        [WebGet(UriTemplate = "/GetTempUnits", ResponseFormat = WebMessageFormat.Json)]
        string Get_Temperature_Unit();

        [OperationContract]
        [WebGet(UriTemplate = "/GetTargetTemp", ResponseFormat = WebMessageFormat.Json)]
        long Get_TargetTemperature();

        [OperationContract]
        [WebGet(UriTemplate = "/GetCurrentTemp", ResponseFormat = WebMessageFormat.Json)]
        long Get_CurrentTemperature();

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SetTargetTemp?value={value}", ResponseFormat = WebMessageFormat.Json)]
        void Set_TargetTemperature(string value);

        [OperationContract]
        [WebGet(UriTemplate = "/GetLED", ResponseFormat = WebMessageFormat.Json)]
        string GetLED();
        
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SetLED?value={value}", ResponseFormat = WebMessageFormat.Json)]
        void Set_LED(string value);
        
        [OperationContract]
        [WebGet(UriTemplate = "/GetJet1", ResponseFormat = WebMessageFormat.Json)]
        string Get_Jet1();

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SetJet1?value={value}", ResponseFormat = WebMessageFormat.Json)]
        void Set_Jet1(string value);
        
        [OperationContract]
        [WebGet(UriTemplate = "/GetJet2", ResponseFormat = WebMessageFormat.Json)]
        string Get_Jet2();

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SetJet2?value={value}", ResponseFormat = WebMessageFormat.Json)]
        void Set_Jet2(string value);
    }
}