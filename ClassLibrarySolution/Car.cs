namespace ClassLibrary
{
    public class Car : Vehicle
    {
        private int size = 4;
        private DateTime parkingTime;

        public override int Size
        {
            get { return size; }
        }
        public Car(string regNumber, DateTime parkingTime)
            : base(regNumber, parkingTime)
        {
        
        }

    }
}