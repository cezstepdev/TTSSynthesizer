using System;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using System.Speech.Synthesis;
using WebSocketSharp;
using System.Configuration;
using Newtonsoft.Json;

namespace AIFTTSService
{
    public partial class Service1 : ServiceBase
    {
        private WebSocket webSocket;
        
        public void webSocketCreate()
        {
            try
            {
                String url = ConfigurationManager.AppSettings["url"];
                WriteToFile(url);
                webSocket = new WebSocket(url);
                webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                webSocket.OnOpen += WebSocket_OnOpen;
                webSocket.OnMessage += WebSocket_OnMessage;

                webSocket.Connect();
            }
            catch(Exception e)
            {
                WriteToFile(e.ToString());
            }
        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            AIFTTS("połączyłam");
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            String text = decodeJSON(e.Data);
            WriteToFile(text);
            AIFTTS(text);
        }

        public String decodeJSON(String jtext)
        {
            dynamic stuff = JsonConvert.DeserializeObject(jtext);
            string text;
            return text = stuff.text;
        }

        public void AIFTTS(String text)
        {
            SpeechSynthesizer tts = new SpeechSynthesizer();
            tts.SelectVoiceByHints(VoiceGender.Female);
            tts.Volume = 100;
            tts.Rate = 0;
            tts.Speak("\"" + text + "\"");
        }

        public Service1()
        {
            InitializeComponent();
        }

        
        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            webSocketCreate();
            AIFTTS("start");
        }
        
        protected override void OnStop()
        {
            AIFTTS("żegnam");
            WriteToFile("Service is stopped at " + DateTime.Now);
            webSocket.Close();
        }
        
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service is recall at " + DateTime.Now);
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
