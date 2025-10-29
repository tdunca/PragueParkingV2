using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json;
using System.Transactions;
using System.Xml.Linq;
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
            bool exit = false;
            while (!exit)
            {
                FigletPragueParking();
                ShowParkingSpaces();
                TablePriceMenu();

                var selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                    .PageSize(7)
                    .AddChoices(new[] 
                    { 
                        "Park vehicle", 
                        "Retrieve vehicle",
                        "Move vehicle", 
                        "Find vehicle",
                        "Clear Garage",
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
                    case "Clear Garage":
                        {
                            ClearGarage();
                            break;
                        }
                    case "Reload config":
                        {
                            ReloadConfig();
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
                    string regNumber = GetRegNumber();
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
                    string regNumber = Console.ReadLine();

                    if (string.IsNullOrEmpty(regNumber) | regNumber.Length < 1 | regNumber.Length > 10 | pragueParking.ContainsSpecialCharacters(regNumber))
                    {
                        Console.WriteLine("[red]This is not a valid registration number, try again.[/]");
                        continue;
                    }

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
                    regNumber = Console.ReadLine();
                    if (string.IsNullOrEmpty(regNumber))
                    {
                        var table2 = new Table();
                        table2.AddColumn("[red] Vehicle not found, try again.[/]");
                        AnsiConsole.Write(table2);
                        return;
                    }
                }
                while (string.IsNullOrEmpty(regNumber));
                ParkingSpot currentSpot = null;
                Vehicle vehicleToRemove = null;
                int currentSpotIndex = -1;

                for (int i = 1; i < parkingSpots.Length; i++)
                {
                    var spot = parkingSpots[i];
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
                TimeSpan parkingDuration = currentTime - vehicleToRemove.ParkingTime;

                double price = CalculateParkingCost(vehicleToRemove, parkingDuration);
                Console.WriteLine($"Parking duration: {parkingDuration.TotalMinutes:F1} minutes.");
                Console.WriteLine($"Total cost: {price:F2}CZK");

                Console.WriteLine("Do you wish to retrieve and remove this vehicle?");
                var confirm = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .PageSize(4).AddChoices(new[] { "Yes", "No" }));
                if (confirm == "Yes")
                {
                    currentSpot.parkingSpot.Remove(vehicleToRemove);
                    currentSpot.CurrentSize - = vehicleToRemove.Size;

                    Console.WriteLine($"Vehicle with registration: {regNumber} has been collected from spot {currentSpotIndex}");

                    SaveParkingSpots();
                }
            }
            void MoveVehicle()
            {
                string regNumber;
                do
                {
                    Console.Write("Enter registration number of vehicle: ");
                    regNumber = Console.ReadLine();
                    if (string.IsNullOrEmpty(regNumber))
                    {
                        var table2 = new Table();
                        table2.AddColumn("[red]Vehicle not found, try again [/]");
                        AnsiConsole.Write(table2);
                        return;
                    }

                } while (string.IsNullOrEmpty (regNumber));

                ParkingSpot currentSpot = null;
                Vehicle vehicleToMove = null;
                int currentSpotIndex = -1;

                for (int i = 1; i < parkingSpots.Length; i++)
                {
                    var spot = parkingSpots[i];
                    vehicleToMove = spot.parkingSpot.FirstOrDefault(Vehicle => Vehicle.RegNumber == regNumber);
                    if (vehicleToMove ! == null)
                    {
                        currentSpot = spot;
                        currentSpotIndex = i;
                        break;
                    }
                }
                if (currentSpot == null)
                {
                    var table3 = new Table();
                    table3.AddColumn($"[red]Vehicle not found[/]");
                    AnsiConsole.Write(table3);
                    return;
                }
                Console.WriteLine($"Vehicle with registration number '{regNumber} is in spot '{currentSpotIndex}'");
                int newSpotIndex;

                bool isValidToCheckOut = true;
                do
                {
                    Console.Write("Please enter the new parking spot number: ");
                    if (int.TryParse(Console.ReadLine(), out newSpotIndex) && newSpotIndex > 0 && newSpotIndex < parkingSpots.Length)
                    {
                        var newSpot = parkingSpots[newSpotIndex];

                        if (newSpot.CurrentSize + vehicleToMove.Size <= newSpot.MaxSize)
                        {
                            currentSpot.parkingSpot.Remove(vehicleToMove);
                            currentSpot.CurrentSize -= vehicleToMove.Size;

                            newSpot.parkingSpot.Add(vehicleToMove);
                            newSpot.CurrentSize += vehicleToMove.Size;
                            Console.WriteLine($"Vehicle with registration number '{regNumber}' is moved to spot ''{newSpotIndex}");

                            SaveParkingSpots();
                            isValidToCheckOut = false;
                        }
                        else
                        {
                            Console.WriteLine("[red]There is not enoug space in this parking spot[/]");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[red]This is an invalid parking spot number. Try again[/]");

                    }
                } while ( isValidToCheckOut );
            }
        }
    }
