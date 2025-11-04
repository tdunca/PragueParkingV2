# Prague Parking
This is a school assignment for DevSecOps #C

## Requirements
- .NET SDK: .NET 9 (LTS) or .NET 6+
- OS: Windows / macOS / Linux
- Editor (optional): Visual Studio 2022 or VS Code with the C# extension

## About the project
**Prague Parking V2** is a console-based parking system that uses JSON for persistence and reads configuration values (like prices and garage size) from a text file.  
The project is structured into two main parts:

- **Console Application** (`Program.cs`) – Handles user interface and menu logic.  
- **ClassLibrary** – Contains the classes for `ParkingGarage`, `ParkingSpot`, `Vehicle`, `Car`, and `Mc`.

### Functions
- **Park Vehicle** – Add a new car or motorcycle to the first available parking spot.  
- **Retrieve Vehicle** – Remove a parked vehicle and calculate parking cost based on time.  
- **Move Vehicle** – Move a vehicle from one spot to another (if there is enough space).  
- **Locate Vehicle** – Find a vehicle by registration number and display cost/time.  
- **Clear Garage** – Empty all spots in the garage (with confirmation).  
- **Reload Config** – Reread `config.txt` to update prices and number of spots.  
- **Visualization** – Show garage status (Empty / Half Full / Full) using a colored chart.


### Installation
#### Clone the repository

#### Usage
After starting the program, you will see a menu in the console:
Use the arrow keys and Enter to navigate.

The program saves your garage status automatically in ParkingArray.json after each change.
