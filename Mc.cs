namespace ClassLibrary
{
    public class Mc : Vehicle
    {
        private int size = 2;

        public override int Size
        {
            get { return size; }
        }
        public Mc(string regNumber, DataTime parkingTime)
            : base(regNumber, parkingTime)
        {

        }

    }
}
