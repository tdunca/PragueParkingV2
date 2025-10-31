using ClassLibrary;

namespace ParkingTestTemp
{
    [TestClass]
    public class SpecialCharacterTest
    {
        [TestMethod]
        public void StringDoesContainSpecialCharacter()
        {
            var parkingGarage = new ParkingGarage();
            string stringToTest = "Testing testing!";
            bool fromCall;


            fromCall = parkingGarage.ContainsSpecialCharacters(stringToTest);

            Assert.IsTrue(fromCall);
        }
        [TestMethod]
        public void StringDoesNotContainSpecialCharacter()
        {

            var parkingGarage = new ParkingGarage();
            string stringToTest = "BananaBoat";
            bool fromCall;

            fromCall = parkingGarage.ContainsSpecialCharacters(stringToTest);

            Assert.IsFalse(fromCall);
        }
    }
}
