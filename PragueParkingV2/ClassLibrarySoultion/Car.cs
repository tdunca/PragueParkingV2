namespace ClassLibrary
{
    public class Car : Vehicle
    {
        private int size = 4;

        public override int Size
        {
            get { return size; }
        }
        public Car(string regNumber, DataTime parkingTime)
            : base(regNumber, parkingTime)
        {
        
        }

    }
}