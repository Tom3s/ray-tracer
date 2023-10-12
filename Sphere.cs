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
			bool visible = false;
			// Geometry geometry = this;
			// Line l = line;
			double distance = 0.0; // distance from line.origin to intersection point

			Vector sphereToLine = line.origin - this.Center;

			// Calculate the coefficients for the quadratic equation (ax^2 + bx + c = 0).
			double a = line.direction.Length2();  // Dot product of direction with itself.
			double b = 2.0 * (line.direction * sphereToLine);
			double c = sphereToLine.Length2() - this.Radius * this.Radius;

			// Calculate the discriminant of the quadratic equation.
			double discriminant = b * b - 4 * a * c;

			// If the discriminant is non-negative, the line intersects the sphere.
			if (discriminant >= 0)
			{
				// Calculate the two possible solutions for t (parameter along the line).
				double t1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
				double t2 = (-b - Math.Sqrt(discriminant)) / (2 * a);

				// Check if the solutions are within the specified distance range.
				if (t2 >= minDist && t2 <= maxDist)
				{
					valid = true;
					distance = t2;
				}
				else if (t1 >= minDist && t1 <= maxDist)
				{
					valid = true;
					distance = t1;
				}

				// Check if the intersection point is in front of the line's origin.
				if (distance > 0)
				{
					visible = true;
				}
			}

			return new Intersection(
				valid,
				visible,
				this,
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