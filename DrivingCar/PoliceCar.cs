namespace DrivingCar
{
    public class PoliceCar : Car
    {
        private const double SpeedMultiplier = 0.5; // 🟢 Police car speed is 2x

        public PoliceCar(string imagePath, int xPos, int yPos, double speed = DefaultSpeed * SpeedMultiplier,
                         int carWidth = DefaultWidth, int carHeight = DefaultHeight)
            : base(imagePath, xPos, yPos, speed, carWidth, carHeight)
        {
        }
    }
}
