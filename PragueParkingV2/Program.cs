using System.Text.Json;
using ClassLibrary;
using Spectre.Console;

namespace PrageParkingV2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string filepath = "../../../";
            ParkingGarage pragueParking = new ParkingGarage();
            ParkingSpot[] parkingSpots = pragueParking.ReadParkingSpotsFromJson();
            parkingSpots = pragueParking.GarageSizeChange(parkingSpots);
            SaveParkingSpots();
            bool exit = false; //Styr huvudloopen
            while (!exit)
            {
                FigletPragueParking();
                ShowParkingSpaces();
                TablePriceMenu();

                //Meny
                var selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                    .PageSize(7)
                    .AddChoices(new[]
                    {
                        "Park vehicle",
                        "Retrieve vehicle",
                        "Move vehicle",
                        "Locate vehicle",
                        "Clear garage",
                        "Reload config",
                        "Exit program"
                    }

                    ));

                switch (selection)
                {
                    case "Park vehicle":
                        {
                            ParkVehicle();
                            break;
                        }
                    case "Retrieve vehicle":
                        {
                            RetrieveVehicle();
                            break;
                        }
                    case "Move vehicle":
                        {
                            MoveVehicle();
                            break;
                        }
                    case "Locate vehicle":
                        {
                            LocateVehicle();
                            break;
                        }
                    case "Reload config":
                        {
                            ReloadConfig();
                            break;
                        }
                    case "Clear garage":
                        {
                            ClearGarage();
                            break;
                        }
                    case "Exit program":
                        {
                            exit = true;
                            break;
                        }
                }
                if (!exit)
                {
                    var table1 = new Table();
                    table1.AddColumn("[green] Press enter - Return to menu[/]");
                    AnsiConsole.Write(table1);
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            void ParkVehicle()
            {
                int type = ChooseVehicleType();

                if (type == 1)
                {
                    string regNumber = GetRegNumber(); //Validering och dubblettkontroll
                    if (regNumber == "error")
                    {
                        return;
                    }
                    DateTime parkingTime = DateTime.Now;
                    Car newCar = new Car(regNumber, parkingTime);

                    bool tryPark = newCar.ParkVehicle(parkingSpots);
                    if (tryPark == false)
                    {
                        Console.WriteLine("Sorry, the parking garage is full");
                    }
                    SaveParkingSpots();
                }
                else if (type == 2)
                {
                    string regNumber = GetRegNumber();
                    if (regNumber == "error")
                    {
                        return;
                    }
                    DateTime parkingTime = DateTime.Now;
                    Mc newMc = new Mc(regNumber, parkingTime);

                    bool tryPark = newMc.ParkVehicle(parkingSpots);
                    if (tryPark == false)
                    {
                        Console.WriteLine("Sorry, the parking garage is full");
                    }
                    SaveParkingSpots();
                }
            }
            int ChooseVehicleType()
            {
                int type = 0;
                var typeChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .PageSize(4)
                        .AddChoices(new[] {
                            "Car",
                            "Mc",
                        }));
                if (typeChoice == "Car")
                {
                    type = 1;
                }
                else if (typeChoice == "Mc")
                {
                    type = 2;
                }
                return type;

            }
            string GetRegNumber()
            {

                while (true)
                {
                    Console.Write("Please enter the registration number: ");
                    string regNumber = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(regNumber) | regNumber.Length < 1 | regNumber.Length > 10 | pragueParking.ContainsSpecialCharacters(regNumber))
                    {
                        AnsiConsole.MarkupLine("[red]This is not a valid registration number, try again.[/]");
                        continue; 
                    }

                    //dubblettkontroll, kollar varje spot om dubblett
                    bool regNumberExists = parkingSpots.Any(spot => spot.ContainsVehicle(regNumber));

                    if (regNumberExists)
                    {
                        Console.WriteLine("This vehicle is already registred");
                        return "error";
                    }
                    else
                    {
                        return regNumber;
                    }
                }
            }
            void RetrieveVehicle()
            {
                string regNumber;
                do
                {
                    Console.Write("Please, enter registration number: ");
                    regNumber = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(regNumber))
                    {
                        var table2 = new Table();
                        table2.AddColumn("[red] Vehicle not found, try again.[/]");
                        AnsiConsole.Write(table2);
                        return;
                    }
                }
                while (string.IsNullOrEmpty(regNumber)); //Fortsätter ge om input tills användaren skriver något 
                ParkingSpot currentSpot = null;
                Vehicle vehicleToRemove = null;
                int currentSpotIndex = -1;

                for (int i = 1; i < parkingSpots.Length; i++)
                {
                    var spot = parkingSpots[i]; //hämtar nuvarande plats i loopen
                    vehicleToRemove = spot.parkingSpot.FirstOrDefault(v => v.RegNumber == regNumber);

                    if (vehicleToRemove != null)
                    {
                        currentSpot = spot;
                        currentSpotIndex = i;
                        break;
                    }
                }
                if (currentSpot == null || vehicleToRemove == null)
                {
                    var table2 = new Table();
                    table2.AddColumn("[red]Vehicle not found, try again.[/]");
                    AnsiConsole.Write(table2);
                    return;
                }
                DateTime currentTime = DateTime.Now;
                TimeSpan parkingDuration = currentTime - vehicleToRemove.ParkingTime; //Räknar ut hur länge fordonet stått

                double price = CalculateParkingCost(vehicleToRemove, parkingDuration);
                Console.WriteLine($"Duration of parking: {parkingDuration.TotalMinutes:F1} minutes.");
                Console.WriteLine($"The total cost is: {price:F2} CZK");

                Console.WriteLine("Do you wish to retrieve and remove this vehicle?");
                var confirm = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .PageSize(4).AddChoices(new[] { "Yes", "No" }));
                if (confirm == "Yes")
                {
                    currentSpot.parkingSpot.Remove(vehicleToRemove);
                    currentSpot.CurrentSize -= vehicleToRemove.Size;

                    Console.WriteLine($"Vehicle with registration: {regNumber} has been collected from spot {currentSpotIndex}");

                    SaveParkingSpots();
                }
            }

            //Flyttar fordon
            void MoveVehicle()
            {
                string regNumber;
                do
                {
                    Console.Write("Enter registration number of vehicle: ");
                    regNumber = Console.ReadLine().Trim();
                    if (string.IsNullOrEmpty(regNumber))
                    {
                        var table2 = new Table();
                        table2.AddColumn("[red]Vehicle not found, try again [/]");
                        AnsiConsole.Write(table2);
                        return;
                    }

                } while (string.IsNullOrEmpty(regNumber)); //loopar tills giligt regnr ges

                ParkingSpot currentSpot = null; //sparar nuvarande
                Vehicle vehicleToMove = null; //sparar fordon som ska flyttas 
                int currentSpotIndex = -1; //index för nuvarande

                for (int i = 1; i < parkingSpots.Length; i++)
                {
                    var spot = parkingSpots[i]; //hämtar aktuell plats
                    vehicleToMove = spot.parkingSpot.FirstOrDefault(Vehicle => Vehicle.RegNumber == regNumber); 
                    if (vehicleToMove != null)
                    {
                        currentSpot = spot; //sparar platsreferens
                        currentSpotIndex = i; //sparar index
                        break;
                    }
                }
                if (currentSpot == null)
                {
                    var table3 = new Table();
                    table3.AddColumn("[red]Vehicle not found, try again[/]");
                    AnsiConsole.Write(table3);
                    return;
                }
                Console.WriteLine($"Vehicle with registration number '{regNumber} is in spot '{currentSpotIndex}'");
                int newSpotIndex;

                bool isValidToCheckOut = true; //loopar tills giltig ny plats
                do
                {
                    Console.Write("Please enter the new parking spot number: ");
                    if (int.TryParse(Console.ReadLine(), out newSpotIndex) && newSpotIndex > 0 && newSpotIndex < parkingSpots.Length) //kollar om plats har utrymme
                    {
                        var newSpot = parkingSpots[newSpotIndex];

                        if (newSpot.CurrentSize + vehicleToMove.Size <= newSpot.MaxSize)
                        {
                            currentSpot.parkingSpot.Remove(vehicleToMove); //Tar bort fordonet
                            currentSpot.CurrentSize -= vehicleToMove.Size; //Fixar storlek

                            newSpot.parkingSpot.Add(vehicleToMove); //Lägger till på ny plats
                            newSpot.CurrentSize += vehicleToMove.Size;
                            Console.WriteLine($"Vehicle with registration number '{regNumber}' is moved to spot '{newSpotIndex}"); 

                            SaveParkingSpots();
                            isValidToCheckOut = false;
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]There is not enough space in this parking spot[/]");
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]This is an invalid parking spot number. Try again[/]");

                    }
                } while (isValidToCheckOut);
            }

            //Söker efter fordon i garaget, anger plats, tid och pris 
            void LocateVehicle()
            {
                Console.Write("Please enter the registration number of the vehicle: ");
                String regnumber = Console.ReadLine()?.Trim();
                bool found = false;
                for (int i = 1; i < parkingSpots.Length; i++)
                {
                    var Spot = parkingSpots[i];
                    var vehicle = Spot?.parkingSpot.FirstOrDefault(v => v.RegNumber == regnumber); // letar upp fordonet i rutan
                    if (vehicle != null)
                    {
                        DateTime currentTime = DateTime.Now; //nuvarande tid
                        TimeSpan duration = currentTime - vehicle.ParkingTime; //parkerad tid 
                        double price = CalculateParkingCost(vehicle, duration); //pris

                        Console.WriteLine($"The vehicle with '{regnumber}' is in spot number '{i}'");
                        Console.WriteLine($"Duration of parking: '{duration.TotalMinutes:F1}' minutes");
                        Console.WriteLine($"The total cost is {price:F2} CZK");
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    AnsiConsole.MarkupLine("[red]Vehicle not found, try again[/]");
                }
            }

            //räknar ut kostnad och returnerar i CZK
            double CalculateParkingCost(Vehicle vehicle, TimeSpan duration)
            {
                const double freetime = 10;
                double rate = 0;
                if (duration.TotalMinutes <= freetime)
                {
                    return 0;
                }
                else
                {
                    if (vehicle.Size == 2)
                    {
                        rate = pragueParking.McPrice;
                    }
                    else if (vehicle.Size == 4)
                    {
                        rate = pragueParking.CarPrice;
                    }
                } //räknar kostnad över freetime
                return ((duration.TotalMinutes - freetime) / 60) * rate;
            }

            //loopar igenom alla platser och räknas ur status
            void ShowParkingSpaces()
            {
                int emptyCount = 0;
                int halfFullCount = 0;
                int fullCount = 0;

                for (int i = 1; i < parkingSpots.Length; i++)
                {
                    var spot = parkingSpots[i];
                    if (spot == null) continue;

                    if (spot.CurrentSize <= 0) //tomma
                    {
                        emptyCount++;
                    }
                    else if (spot.CurrentSize < spot.MaxSize) //halvfulla
                    {
                        halfFullCount++;
                    }
                    else 
                    {
                        fullCount++; //fulla
                    }
                }

                var chart = new BreakdownChart()
                    .FullSize()
                    .AddItem("Empty", emptyCount, Color.Green)
                    .AddItem("Half Full", halfFullCount, Color.Yellow)
                    .AddItem("Full", fullCount, Color.Red);

                AnsiConsole.Write(new Markup("[gray bold]Parking space[/]\n"));
                AnsiConsole.Write(chart); 
            }

            //Serialiserar hela parkingSpots-arrayen till JSON på disk
            void SaveParkingSpots()
            {
                string updatedParkingArrayJsonString = JsonSerializer.Serialize(parkingSpots, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filepath + "ParkingArray.json", updatedParkingArrayJsonString);
            }
            //Läser config, anpassar arraystorleken och sparar status
            void ReloadConfig()
            {
                pragueParking.ReloadConfigTxt();
                parkingSpots = pragueParking.GarageSizeChange(parkingSpots);
                SaveParkingSpots();
            }

            //Skapar banner och tabeller 
            #region 
            void FigletPragueParking()
            {
                AnsiConsole.Write(
                    new FigletText("Prague Parking")
                    .Centered()
                    .Color(Color.White));
                Console.WriteLine("\n\n");
            }
            void TableStatusVehicle()
            {
                Table table = new Table();
                table.AddColumns("[green] Empty [/]", "[yellow] Half full [/]", "[green] Full [/]")
                    .Collapse().Centered().Expand();
                AnsiConsole.Write(table);
            }

            //Visar prislista baserat på config
            void TablePriceMenu()
            {
                var table = new Table();
                table.AddColumn("Vehicle type: ");
                table.AddColumn(new TableColumn("Price per hour: ").Centered());
                table.AddRow("Free", "For the duration of the first 10 minutes");
                table.AddRow("Car", $"{pragueParking.CarPrice} CZK per hour");
                table.AddRow("Motorbike", $"{pragueParking.McPrice} CZK per hour");
                AnsiConsole.Write(table.SimpleBorder().Alignment(Justify.Left));
            }
            #endregion
            void ClearGarage()
            {
                Console.WriteLine("Would you like to empty the entire garage?");
                var confirm = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .PageSize(4).AddChoices(new[] { "Yes", "No" }));
                if (confirm == "Yes")
                {
                    for (int i = 1; i < parkingSpots.Length; i++)
                    {
                        parkingSpots[i].parkingSpot.Clear(); //rensar fordon ur ruta
                        parkingSpots[i].CurrentSize = 0; //nollställer kapacitet
                    }
                    SaveParkingSpots();
                }
                else
                {
                    return;
                }
            }
        }
    }
}