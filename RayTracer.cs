using System;
using System.Runtime.InteropServices;

namespace rt
{
	class RayTracer
	{
		private Geometry[] geometries;
		private Light[] lights;

		public RayTracer(Geometry[] geometries, Light[] lights)
		{
			this.geometries = geometries;
			this.lights = lights;
		}

		private double ImageToViewPlane(int n, int imgSize, double viewPlaneSize)
		{
			var u = n * viewPlaneSize / imgSize;
			u -= viewPlaneSize / 2;
			return u;
		}

		private Intersection FindFirstIntersection(Line ray, double minDist, double maxDist)
		{
			var intersection = new Intersection();

			foreach (var geometry in geometries)
			{
				var intr = geometry.GetIntersection(ray, minDist, maxDist);

				if (!intr.Valid || !intr.Visible) continue;

				if (!intersection.Valid || !intersection.Visible)
				{
					intersection = intr;
				}
				else if (intr.T < intersection.T)
				{
					intersection = intr;
				}
			}

			return intersection;
		}

		private double intersectionDelta = 0.0001;
		private bool IsLit(Vector point, Light light)
        {

            Line line = new Line(light.Position, point);
            Vector firstIntersection = FindFirstIntersection(line, 0, (point - light.Position).Length() + 1000).Position;

			if (firstIntersection == null) return false;

            return Math.Abs(firstIntersection.X - point.X) < intersectionDelta &&
                Math.Abs(firstIntersection.Y - point.Y) < intersectionDelta &&
                Math.Abs(firstIntersection.Z - point.Z) < intersectionDelta;
        }

		private bool isPointLit(Vector point)
		{
			foreach (var light in lights)
			{
				if (IsLit(point, light))
				{
					return true;
				}
			}
			return false;
		}

		private Color CalculateColor(Intersection intersection, Camera camera)
		{
			var color = new Color(0.0, 0.0, 0.0, 1.0);

			foreach (var light in lights) {
				var partialColor = intersection.Material.Ambient * light.Ambient;

				if (!IsLit(intersection.Position, light)){
					color += partialColor;
				} else {
					var normal = intersection.Normal;
					var towardsLight = (light.Position - intersection.Position).Normalize();

					if (normal * towardsLight > 0) {
						partialColor += intersection.Material.Diffuse * light.Diffuse * (normal * towardsLight);
					}

					// var towardsCamera = (intersection.Line.origin - intersection.Position).Normalize();
					var towardsCamera = (camera.Position - intersection.Position).Normalize();
					var reflection = (normal * 2 * (normal * towardsLight) - towardsLight).Normalize();

					if (towardsCamera * reflection > 0) {
						partialColor += intersection.Material.Specular * light.Specular * Math.Pow(towardsCamera * reflection, intersection.Material.Shininess);
					}

					color += partialColor * light.Intensity;
				}
			}
			return color;
		}
		public void Render(Camera camera, int width, int height, string filename)
		{
			var background = Color.GRAY;
			var viewParallel = (camera.Up ^ camera.Direction).Normalize();

			var image = new Image(width, height);

			var distanceToViewplane = camera.Direction * camera.ViewPlaneDistance;

			Console.WriteLine("Starting render for " + filename);

			for (var i = 0; i < width; i++)
			{
				for (var j = 0; j < height; j++)
				{
					var u = ImageToViewPlane(i, width, camera.ViewPlaneWidth);
					var v = ImageToViewPlane(j, height, camera.ViewPlaneHeight);

					var vecU = -viewParallel * u;
					var vecV = -camera.Up * v;

					var vecUV = vecU + vecV;
					var vecS = camera.Position + distanceToViewplane + vecUV;

					var ray = new Line(camera.Position, vecS);

					var intersection = FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);


					if (intersection.Valid && intersection.Visible)
					{
						image.SetPixel(i, j, CalculateColor(intersection, camera));
					}
					else
					{
						image.SetPixel(i, j, background);
					}
					// Console.Write(".");
				}
			}

			image.Store(filename);
		}
	}
}