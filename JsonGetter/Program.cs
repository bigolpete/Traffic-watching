using System;
using System.Configuration;
using Newtonsoft.Json; // Used to make JSON data retrevial easier. Followed from http://www.codeproject.com/Tips/397574/Use-Csharp-to-get-JSON-Data-from-the-Web-and-Map-i 
using System.Net; // Used to make web requests
using System.IO.Ports; // Used for serial interface

namespace JsonGetter
{
    class Program
    {
        static void Main(string[] args)
        {
            string googKey = ConfigurationManager.AppSettings.Get("googleKey");
            string origin = ConfigurationManager.AppSettings.Get("origin");
            string destination = ConfigurationManager.AppSettings.Get("destination");
            var url = "https://maps.googleapis.com/maps/api/directions/json?origin=" + origin + "&departure_time=now&destination=" + destination + "&key=" + googKey; // Enter the google maps API call
            int updateFreq; // How often the update sequence is ran

            SerialPort serialPort1 = new SerialPort();  // Establishes serial port variable
            serialPort1.PortName = GetPort();  // Picks array point for serial
            serialPort1.BaudRate = GetBaud(); // Will chance this to console code eventually.

            Console.Write("How many minutes between google API calls : ");
            updateFreq = Convert.ToInt32(Console.ReadLine());

            if (!serialPort1.IsOpen) // If serial is not running, starts the serial connection
            {
                serialPort1.Open();
            }

            if (!serialPort1.IsOpen) return; // Doesnt really do anything right now...

            var timer = new System.Threading.Timer( // Every 2 minutes, kicks off the event to run SerialSendTraffic (Includes the info update dump)
                e => SerialSendTraffic(serialPort1, url),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(updateFreq));

            Console.Read(); // Pauses the program so the console doesnt close. I hate this. 
        }

        private static string GetPort()
        {
            string[] serialPorts = SerialPort.GetPortNames(); // Populate the COM ports
            Console.Write("The following COM ports were found : ");
            foreach (string y in serialPorts)
            {
                Console.Write(y + " ");
            }
            Console.WriteLine();
            Console.Write("Please pick a COM port: ");
            string port = Console.ReadLine();
            return port;
        }

        private static int GetBaud()
        {

            int[] serialBaud = { 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 }; //Populate baud list choices
            Console.Write("The following Baud rates are available : ");

            foreach (int y in serialBaud)
            {
                Console.Write(y + " ");
            }
            Console.WriteLine();
            Console.Write("Please pick a BAUD rate: ");
            int baud = Convert.ToInt32(Console.ReadLine());
            return baud;
        }

        private static T _download_serialized_json_data<T>(string url) where T : new() // Followed code from http://www.codeproject.com/Tips/397574/Use-Csharp-to-get-JSON-Data-from-the-Web-and-Map-i 
        {
            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                try // attempt to download JSON data as a string
                {
                    json_data = w.DownloadString(url);
                }
                catch (Exception) { Console.Write("fail"); }
                // if string with JSON data is not empty, deserialize it to class and return its instance 
                return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T();
            }
        }

        static void SerialSendTraffic(SerialPort _serialPort, string _url) // Takes two arguments - Which serial port to send to and which URL to pull from
        {
            MapJson.Rootobject _rootJson = _download_serialized_json_data<MapJson.Rootobject>(_url); // Downloads JSON to json variable
            int _routeMinutes = Convert.ToInt32(_rootJson.routes[0].legs[0].duration_in_traffic.value) / 60; // converts the string value of the route duration to int32 

            if (!_serialPort.IsOpen) return; // Ends if serial is closed.
            _serialPort.Write(Convert.ToString(_routeMinutes)); // Writes to the serialport as a string (required)
            Console.WriteLine("Traffic from home to work is currently a {0} minute drive.", _routeMinutes); //posts the update for debugging
        }
    }
}

/* Super genaric arduino code. Will revise.

    int minutes;

void setup() {
  pinMode(5, OUTPUT);  // Green LED
  pinMode(6, OUTPUT); // Yellow LED
  pinMode(7, OUTPUT); // Red LED
  Serial.begin(9600); // Baud rate
}

void loop() {
  while(Serial.available()) { // As long as there is serial data available to read, the following is run:
  minutes = Serial.parseInt(); //.parseInt is used instead of .read() to ensure int is captured correctly. 
  if (minutes <30) // If, green light! Traffic is lookin good!
  {
    green();
  }
  else if(minutes >=30 && minutes <40) //Traffic is not looking too hot.
  {
    yellow();
  }
  else // Fuck traffic
  {
    red();
  }
//  Serial.write(minutes);
 }
}
  
// The following turn off all other LEDs and light up their own when called.
void green(){
  digitalWrite(5, HIGH);
  digitalWrite(6, LOW); 
  digitalWrite(7, LOW);
}
void yellow(){
  digitalWrite(5, LOW);
  digitalWrite(6, HIGH);
  digitalWrite(7, LOW);

}
void red(){
  digitalWrite(5, LOW);
  digitalWrite(6, LOW);
  digitalWrite(7, HIGH);

}
*/
