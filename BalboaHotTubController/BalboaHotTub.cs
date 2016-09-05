using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using BalboaHotTubController.Properties;

namespace BalboaHotTubController
{
    public class BalboaHotTub : IDisposable, IBalboaHotTub
    {
        public string DeviceId { get; internal set; }
        private string[] _panelInfoRawData = null;
        private string[] _getPanelInfo_RawData
        {
            get
            {
                while (_panelInfoRawData == null)
                {
                    // Wait until the data is initially populated
                    Thread.Sleep(200);
                }

                return _panelInfoRawData;
            }
        }

        private string[] _deviceConfiguration_RawData = null;
        public string[] _getDeviceConfiguration_RawData
        {
            get
            {
                while (_deviceConfiguration_RawData == null)
                {
                    // Wait until the data is initially populated
                    Thread.Sleep(200);
                }

                return _deviceConfiguration_RawData;
            }
        }

        private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

        public BalboaHotTub(string deviceId)
        {
            DeviceId = deviceId;

            Task.Factory.StartNew(() =>
            {
                while (!_cancelToken.IsCancellationRequested)
                {
                    // Update our data read
                    UpdateHotTub_RawData(DeviceId);
                    Thread.Sleep(2000);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public BalboaHotTub()
        {
            DeviceId = Settings.Default.deviceId;

            Task.Factory.StartNew(() =>
            {
                while (!_cancelToken.IsCancellationRequested)
                {
                    // Update our data read
                    UpdateHotTub_RawData(DeviceId);
                    Thread.Sleep(2000);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public string Get_Temperature_Unit()
        {
            switch (_getPanelInfo_RawData[PanelInfo.TEMPERATURE_UNIT])
            {
                case "03":
                    return "c";
                case "02":
                    return "f";
                default:
                    return "";
            }
        }

        public long Get_CurrentTemperature() => ConvertHexToTemperature(_getPanelInfo_RawData[PanelInfo.CURRENT_TEMPERATURE]);

        public long Get_TargetTemperature()
        {
            return ConvertHexToTemperature(_getPanelInfo_RawData[PanelInfo.TARGET_TEMPERATURE]); 
        }
        public void Set_TargetTemperature(string value)
        {
            var data = "<sci_request version=\"1.0\"><data_service><targets>" +
                    $"<device id=\"{DeviceId}\"/></targets><requests>" +
                    $"<device_request target_name=\"SetTemp\">{value}.000000</device_request></requests></data_service></sci_request>";

            BalboaHttpRequest(data);
        }

        private static long ConvertHexToTemperature(string hexValue)
        {
            return int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber) / 2;
        }

        public HeatModeEnum HeatMode
        {
            get
            {
                switch (_getPanelInfo_RawData[PanelInfo.HEAT_MODE])
                {
                    case "00":
                        return HeatModeEnum.READY;

                    case "01":
                        return HeatModeEnum.REST;

                    default:
                        return HeatModeEnum.UNKNOWN;
                }
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public string Get_HeatMode()
        {
            return HeatMode.ToString();
        }

        public LEDState LED
        {
            get
            {
                switch (_getPanelInfo_RawData[PanelInfo.LEDS_ON])
                {
                    case "00":
                        return LEDState.Off;

                    case "03":
                        return LEDState.Cycle;

                    default:
                        throw new Exception("Unknown LED setting");
                }
            }
            set
            {
                var newS = (int)value;
                var oldS = (int)LED;

                if (newS == oldS) return;

                // Get the max number of LED states we have
                var maxColours = Enum.GetValues(typeof(LEDState)).Cast<LEDState>().Last();

                // Determine how many button pushes are required
                var buttonPushesRequired = maxColours - LED + newS;

                // Press the led lights button the appropraite number of times to reach the colour we want
                for (int i = 1; i < buttonPushesRequired; i++)
                {
                    LEDLights_Button();
                }
            }
        }

        public string GetLED()
        {
            return LED.ToString();
        }
        public void Set_LED(string value)
        {
            LED = (LEDState)Enum.Parse(typeof(LEDState), value);
        }

        private void updateJetSpeed()
        {
            switch (_getPanelInfo_RawData[PanelInfo.JETINFO])
            {
                case "00":
                    _jet1 = JetSpeed.Off;
                    _jet2 = JetSpeed.Off;
                    break;

                case "01":
                    _jet1 = JetSpeed.One;
                    _jet2 = JetSpeed.Off;
                    break;

                case "02":
                    _jet1 = JetSpeed.Two;
                    _jet2 = JetSpeed.Off;
                    break;

                case "08":
                    _jet1 = JetSpeed.Off;
                    _jet2 = JetSpeed.One;
                    break;

                case "09":
                    _jet1 = JetSpeed.One;
                    _jet2 = JetSpeed.One;
                    break;

                case "0A":
                    _jet1 = JetSpeed.Two;
                    _jet2 = JetSpeed.One;
                    break;
            }
        }

        private JetSpeed _jet1;
        public JetSpeed Jet1
        {
            get
            {
                updateJetSpeed();
                return _jet1;
            }
            set
            {
                switch (Jet1) // Get jet1's current speed, and then determine how to achieve the new state.
                {
                    case JetSpeed.Off:
                        switch (value)
                        {
                            case JetSpeed.One:
                                Jets1_Button(DeviceId);
                                break;
                            case JetSpeed.Two:
                                Jets1_Button(DeviceId);
                                Jets1_Button(DeviceId);
                                break;
                        }
                        break;

                    case JetSpeed.One:
                        switch (value)
                        {
                            case JetSpeed.Two:
                                Jets1_Button(DeviceId);
                                break;
                            case JetSpeed.Off:
                                Jets1_Button(DeviceId);
                                Jets1_Button(DeviceId);
                                break;
                        }
                        break;
                    case JetSpeed.Two:
                        switch (value)
                        {
                            case JetSpeed.Off:
                                Jets1_Button(DeviceId);
                                break;
                            case JetSpeed.One:
                                Jets1_Button(DeviceId);
                                Jets1_Button(DeviceId);
                                break;
                        }
                        break;
                }
            }
        }
        public string Get_Jet1()
        {
            return Jet1.ToString();
        }
        public void Set_Jet1(string value)
        {
            Jet1 = (JetSpeed)Enum.Parse(typeof(JetSpeed), value);
        }

        private JetSpeed _jet2;
        public JetSpeed Jet2
        {
            get
            {
                updateJetSpeed();
                return _jet2;
            }
            set
            {
                switch (Jet2)
                {
                    case JetSpeed.Off:
                        switch (value)
                        {
                            case JetSpeed.One:
                                Jets2_Button(DeviceId);
                                break;
                        }
                        break;

                    case JetSpeed.One:
                        switch (value)
                        {
                            case JetSpeed.Off:
                                Jets2_Button(DeviceId);
                                break;
                        }
                        break;

                    case JetSpeed.Two:
                        throw new Exception("Jet 2 does not support speed two");
                }
            }
        }
        public string Get_Jet2()
        {
            return Jet2.ToString();
        }
        public void Set_Jet2(string value)
        {
            Jet2 = (JetSpeed)Enum.Parse(typeof(JetSpeed), value);
        }

        private string LEDLights_Button()
        {
            return LEDLights_Button(DeviceId);
        }

        private static string PressButton(string deviceId, string buttonNumber)
        {
            // Post data which will request the panel update text
            string postData = "<sci_request version=\"1.0\">" +
                              "<data_service><targets>" +
                              $"<device id=\"{deviceId}\"/>" +
                              "</targets><requests>" +
                              $"<device_request target_name=\"Button\">{buttonNumber}</device_request>" +
                              "</requests></data_service></sci_request>";

            // Make the HTTP request
            var wr = BalboaHttpRequest(postData);

            // Parse the output
            return ParseButtonResponse(wr);
        }
        private static string Jets1_Button(string deviceId)
        {
            return PressButton(deviceId, "4");
        }
        private static string Jets2_Button(string deviceId)
        {
            return PressButton(deviceId, "5");
        }

        private static string LEDLights_Button(string deviceId)
        {
            return PressButton(deviceId, "17");
        }

        public void RunChemicalJetCycle()
        {
            Jet1 = JetSpeed.One; // Put jets 1 into speed 1
            Jet2 = JetSpeed.One; // Turn jets 2 on
            Console.WriteLine("Jets turned onto speed 1");
            Thread.Sleep(10000);

            Console.WriteLine("Sleeping 5 mins for cycle to complete");
            Thread.Sleep(int.Parse(new TimeSpan(0, 0, 2, 0).TotalMilliseconds.ToString()));

            Jet1 = JetSpeed.Two;
            Thread.Sleep(int.Parse(new TimeSpan(0, 0, 1, 0).TotalMilliseconds.ToString()));

            Jet1 = JetSpeed.One;
            Thread.Sleep(int.Parse(new TimeSpan(0, 0, 2, 0).TotalMilliseconds.ToString()));

            Jet1 = JetSpeed.Off;
            Jet2 = JetSpeed.Off;
            Console.WriteLine("Jet cycle complete");
        }

        private void UpdateHotTub_RawData(string deviceId)
        {
            // Post data which will request the panel update text
            string postData = "<sci_request version=\"1.0\"><file_system cache=\"false\" syncTimeout=\"15\"><targets>" +
                              $"<device id=\"{deviceId}\"/>" +
                              "</targets><commands>" +
                              "<get_file path=\"PanelUpdate.txt\"/>" +
                              "<get_file path=\"DeviceConfiguration.txt\"/>" +
                              "</commands></file_system></sci_request>";

            // Make the HTTP request
            var wr = BalboaHttpRequest(postData);

            Parse_PanelInfo_and_DeviceConfiguration_Response(wr);
        }

        private static HttpWebResponse BalboaHttpRequest(string postData)
        {
            while (true)
            {
                var wr = (HttpWebRequest)WebRequest.Create("https://developer.idigi.com/ws/sci");
                wr.Headers.Add("Cookie", "JSESSIONID = BC58572FF42D65B183B0318CF3B69470; BIGipServerAWS - DC - CC - Pool - 80 = 3959758764.20480.0000");
                wr.Headers.Add("Authorization", "Basic QmFsYm9hV2F0ZXJJT1NBcHA6azJuVXBSOHIh");
                wr.UserAgent = "Spa / 48 CFNetwork / 758.5.3 Darwin / 15.6.0";
                wr.Method = "POST";

                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] postDataBytes = encoding.GetBytes(postData);
                wr.ContentLength = postDataBytes.Length;

                Stream newStream = wr.GetRequestStream();
                newStream.Write(postDataBytes, 0, postDataBytes.Length);
                newStream.Close();

                try
                {
                    return wr.GetResponse() as HttpWebResponse;
                }
                catch (Exception)
                {
                    //Console.WriteLine(ex.Message);
                    Thread.Sleep(200);
                }
            }
        }
        private void Parse_PanelInfo_and_DeviceConfiguration_Response(HttpWebResponse response)
        {
            using (var reader = new StreamReader(response.GetResponseStream(), new ASCIIEncoding()))
            {
                string responseText = reader.ReadToEnd();

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(responseText);

                XmlNodeList nodes = xmldoc.SelectNodes("/sci_reply[@*]/file_system/device/commands/get_file");

                if (nodes == null) return;

                var panelUpdateXmlElement = nodes[0]["data"];
                if (panelUpdateXmlElement != null)
                {
                    _panelInfoRawData = FromBase64ToHexString(panelUpdateXmlElement.InnerText).Split('-');
                }

                var deviceConfigurationXmlElement = nodes[1]["data"];
                if (deviceConfigurationXmlElement != null)
                {
                    _deviceConfiguration_RawData = FromBase64ToHexString(deviceConfigurationXmlElement.InnerText).Split('-');
                }
            }
        }

        private static string ParseButtonResponse(HttpWebResponse response)
        {
            using (var reader = new StreamReader(response.GetResponseStream(), new ASCIIEncoding()))
            {
                string responseText = reader.ReadToEnd();

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(responseText);

                XmlNode node = xmldoc.SelectSingleNode("/sci_reply[@*]/data_service/device/requests");
                return node["device_request"].InnerText;
            }
        }

        private static string FromBase64ToHexString(string base64)
        {
            byte[] data = Convert.FromBase64String(base64);
            var hexString = BitConverter.ToString(data);
            return hexString;
        }

        public enum HeatModeEnum
        {
            REST,
            READY,
            UNKNOWN
        }

        public enum JetSpeed
        {
            Off = 1,
            One = 2,
            Two = 3
        }

        public enum LEDState
        {
            Off = 1,
            Purple = 2,
            Blue = 3,
            Red = 4,
            Green = 5,
            Cycle = 6,
            Fade = 7
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Target Temp : {Get_TargetTemperature()}{Get_Temperature_Unit()}");
            sb.AppendLine($"Current Temp: {Get_CurrentTemperature()}{Get_Temperature_Unit()}");
            sb.AppendLine($"Heat Mode   : {HeatMode}");
            sb.AppendLine($"Jet 1       : {Jet1}");
            sb.AppendLine($"Jet 2       : {Jet2}");
            sb.AppendLine($"LED status  : {LED}");

            return sb.ToString();
        }

        public void Dispose()
        {
            _cancelToken.Cancel();
        }
    }

    public class PanelInfo
    {
        public const int CURRENT_TEMPERATURE = 6;
        public const int TARGET_TEMPERATURE = 24;
        public const int HEAT_MODE = 9;
        public const int TEMPERATURE_UNIT = 13;
        public const int LEDS_ON = 18;
        public const int JETINFO = 15;
    }
}
