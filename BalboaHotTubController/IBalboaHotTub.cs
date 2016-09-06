using System.ServiceModel;
using System.ServiceModel.Web;

namespace BalboaHotTubController
{
    [ServiceContract]
    public interface IBalboaHotTub
    {

        [OperationContract]
        [WebGet(UriTemplate = "/SetTargetTemp?value={value}", ResponseFormat = WebMessageFormat.Json)]
        BalboaHotTub Set_TargetTemperature(string value);

       
        [OperationContract]
        [WebGet(UriTemplate = "/SetLED?value={value}", ResponseFormat = WebMessageFormat.Json)]
        BalboaHotTub Set_LED(string value);

        [OperationContract]
        [WebGet(UriTemplate = "/ToggleLEDState", ResponseFormat = WebMessageFormat.Json)]
        BalboaHotTub ToggleLEDState();
        
        [OperationContract]
        [WebGet(UriTemplate = "/SetJet1?value={value}", ResponseFormat = WebMessageFormat.Json)]
        BalboaHotTub Set_Jet1(string value);
        
        [OperationContract]
        [WebGet(UriTemplate = "/SetJet2?value={value}", ResponseFormat = WebMessageFormat.Json)]
        BalboaHotTub Set_Jet2(string value);

        [OperationContract]
        [WebGet(UriTemplate = "/StartChemicalCycle", ResponseFormat = WebMessageFormat.Json)]
        void RunChemicalCycle();

        [OperationContract]
        [WebGet(UriTemplate = "/GetHotTubstatus", ResponseFormat = WebMessageFormat.Json)]
        BalboaHotTub GetHotTubStatus();
    }
}