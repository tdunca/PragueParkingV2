using System;

namespace ClassLibrary
{
    public class Vehicle
    {
        public string RegNumber { get; set; }
        public DataTime ParkingTime { get; set; }

        public virtual int Size { get; set; }
        public Vehicle(string regNumber, DataTime parkingTime)
        {
            RegNumber = regNumber;
            ParkingTime = parkingTime;
        }

        public bool ParkVehicle(ParkingSpot[] parkingspots)
        {
            for (int i = 1; i < parkingspots.Length; i++)
            {    
                if (parkingspots[i].TakeVehicle (this))
                {
                    Console.WriteLine("Vehicle '{0}' is registred to parking spot '{1}'", this.RegNumber, i);
                    return true;
                }

            }
            return false;
        }


    }

}