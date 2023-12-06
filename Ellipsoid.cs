using System;


namespace rt
{
	public class Ellipsoid : Geometry
	{
		private Vector Center { get; }
		private Vector AxisScales { get; }
		private double Radius { get; }


		public Ellipsoid(Vector center, Vector axisScales, double radius, Material material, Color color) : base(material, color)
		{
			Center = center;
			AxisScales = axisScales;
			Radius = radius;
		}

		public Ellipsoid(Vector center, Vector axisScales, double radius, Color color) : base(color)
		{
			Center = center;
			AxisScales = axisScales;
			Radius = radius;
		}

		public override Intersection GetIntersection(Line line, double minDist, double maxDist)
		{
			bool valid = false;
			bool visible = false;
			double distance = 0.0;
			Vector normal = Vector.ZERO; // Initialize the normal vector.

			var a = (line.direction / AxisScales).Length2();
			var b = 2.0 * ((line.direction / AxisScales) * ((line.origin - Center) / AxisScales));
			var c = ((line.origin - Center) / AxisScales).Length2() - Radius * Radius;

			var delta = b * b - 4 * a * c;

			if (delta >= 0) {
				var t1 = (-b - Math.Sqrt(delta)) / (2 * a);
				var t2 = (-b + Math.Sqrt(delta)) / (2 * a);

				if (t1 < t2) {

					var pos = line.CoordinateToPosition(t1);

					normal = new Vector(
						2 * (pos.X - Center.X) / (AxisScales.X * AxisScales.X),
						2 * (pos.Y - Center.Y) / (AxisScales.Y * AxisScales.Y),
						2 * (pos.Z - Center.Z) / (AxisScales.Z * AxisScales.Z)
					).Normalize();
					visible = minDist <= t1 && t1 <= maxDist;
					valid = true;
					distance = t1;
				} else if (t2 < t1){

					var pos = line.CoordinateToPosition(t2);

					normal = new Vector(
						2 * (pos.X - Center.X) / (AxisScales.X * AxisScales.X),
						2 * (pos.Y - Center.Y) / (AxisScales.Y * AxisScales.Y),
						2 * (pos.Z - Center.Z) / (AxisScales.Z * AxisScales.Z)
					).Normalize();
					visible = minDist <= t2 && t2 <= maxDist;
					valid = true;
					distance = t2;
				}

			}

			return new Intersection(
				valid,
				visible,
				this,
				line,
				distance,
				normal, // Include the normal in the Intersection constructor.
				this.Material,
				this.Color
			);
		}

		private Vector CalculateNormal(Vector point)
		{
			// Calculate the normal vector at the given point on the ellipsoid's surface.
			Vector normalizedPoint = new Vector(
				point.X / (AxisScales.X * AxisScales.X),
				point.Y / (AxisScales.Y * AxisScales.Y),
				point.Z / (AxisScales.Z * AxisScales.Z)
			).Normalize();

			return normalizedPoint;
		}

	}
}
