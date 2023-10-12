namespace rt
{
    public class Line
    {
        public Vector origin { get; set; }
        public Vector direction { get; set; }

        public Line()
        {
            origin = new Vector(0.0, 0.0, 0.0);
            direction = new Vector(1.0, 0.0, 0.0);
        }

        public Line(Vector x0, Vector x1)
        {
            origin = new Vector(x0);
            direction = new Vector(x1 - x0);
            direction.Normalize();
        }

        public Vector CoordinateToPosition(double t)
        {
            return new Vector(direction * t + origin);
        }
    }
}