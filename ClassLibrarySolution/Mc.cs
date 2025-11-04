namespace ClassLibrary
{
    public class Mc : Vehicle
    {
        private int size = 2;

        public override int Size
        {
            get { return size; }
        }
        public Mc(string regNumber, DateTime parkingTime)
            : base(regNumber, parkingTime)
        {

        }

    }
}
