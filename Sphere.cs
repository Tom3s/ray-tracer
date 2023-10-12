using System;

namespace rt
{
    public class Sphere : Geometry
    {
        private Vector Center { get; set; }
        private double Radius { get; set; }

        public Sphere(Vector center, double radius, Material material, Color color) : base(material, color)
        {
            Center = center;
            Radius = radius;
        }

        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
			bool valid = false;
			bool visible = true;
			Geometry geometry = this;
			// line
			double distance = 0.0; // distance from line.origin to intersection point

			{
				Vector v1 = Center - line.origin;
				distance = v1.Dot(line.direction);
				Vector perpendicular = (line.direction * distance) - v1;
				if (perpendicular.Length() <= Radius && distance >= minDist && distance <= maxDist) {
					valid = true;
				} else {
					visible = false;
				}
			}


            return new Intersection(
				valid,
				visible,
				geometry,
				line,
				distance
			);
        }

        public override Vector Normal(Vector v)
        {
            var n = v - Center;
            n.Normalize();
            return n;
        }
    }
}