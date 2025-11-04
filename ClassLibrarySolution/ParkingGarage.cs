
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ClassLibrary
{
    public class ParkingGarage
    {
        public int McPrice { get; set; }
        public int CarPrice { get; set; }
        public int GarageSize { get; set; }
        public ParkingGarage()
        {
            string filepath = "../../../";
            var configValues = new Dictionary<string, int>();

            foreach (var line in File.ReadLines(filepath + "config.txt"))
            {
                if (string.IsNullOrEmpty(line) || line.TrimStart().StartsWith("#")) continue;
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim().Split('#')[0].Trim();
                    configValues[key] = int.Parse(value);
                }
            }
            //läser ut värden om nycklar finns 
            configValues.TryGetValue("CarPrice", out int configCarPrice);
            configValues.TryGetValue("McPrice", out int configMcPrice);
            configValues.TryGetValue("GarageSize", out int configGarageSize);

            this.CarPrice = configCarPrice;
            this.McPrice = configMcPrice;
            this.GarageSize = configGarageSize;
        }
        public void ReloadConfigTxt()
        {
            string filepath = "../../../";
            var configValues = new Dictionary<string, int>();

            //samma logik som i konstruktorn
            foreach (var line in File.ReadLines(filepath + "config.txt"))
            {
                if (string.IsNullOrEmpty(line) || line.TrimStart().StartsWith("#")) continue;

                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim().Split('#')[0].Trim();
                    configValues[key] = int.Parse(value);
                }
            }
            //uppdaterar klassvärden
            configValues.TryGetValue("CarPrice", out int configCarPrice);
            configValues.TryGetValue("McPrice", out int configMcPrice);
            configValues.TryGetValue("GarageSize", out int configGarageSize);

            this.CarPrice = configCarPrice;
            this.McPrice = configMcPrice;
            this.GarageSize = configGarageSize;
        }

        //justerar arraystorlek beroende på garagesize
        public ParkingSpot[] GarageSizeChange(ParkingSpot[] input)
        {
            bool isEmpty = true;
            ParkingSpot[] output;
            
            //loopar igenom
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].CurrentSize > 0)
                {
                    isEmpty = false;
                    break;
                }
            }
            if (this.GarageSize < input.Length && isEmpty == false)
            {
                Console.WriteLine("The garage is not empty. Number of spots are the same. \n + You must empty the garage before decreasing it's size");
                return input;
            }
            else if (this.GarageSize < input.Length && isEmpty == true)
            {
                
                output = new ParkingSpot[this.GarageSize];
                for (int i = 0; i < output.Length; i++)
                {
                    output[i] = new ParkingSpot(0);
                }
            }
            else
            {
                output = new ParkingSpot[this.GarageSize];
                Array.Copy(input, output, input.Length);
                for (int i = input.Length; i < output.Length; i++)
                {
                    output[i] = new ParkingSpot(0);
                }
            }
            return output;
        }
        
        //läser Json med sparad status
        public ParkingSpot[] ReadParkingSpotsFromJson()
        {
            string filepath = "../../../";
            ParkingSpot[] parkingSpots;
            if (File.Exists(filepath + "ParkingArray.json"))
            {
                string parkingJsonString = File.ReadAllText(filepath + "ParkingArray.json");
                parkingSpots = JsonSerializer.Deserialize<ParkingSpot[]>(parkingJsonString);
            }
            else
            {
                DateTime testDateTime = DateTime.Now;
                parkingSpots = new ParkingSpot[this.GarageSize];
                for (int i = 0; i < parkingSpots.Length; i++)
                {
                    parkingSpots[i] = new ParkingSpot(0);
                }
                
            }
            return parkingSpots;
        }

        //kollar om ej tillåtna tecken
        public bool ContainsSpecialCharacters(string regNumber)
        {
            return Regex.IsMatch(regNumber, @"[^\p{L}\p{N}]");
        }
        public bool FileExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            return File.Exists(fileName);
        }
    }
}