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

        private bool IsLit(Vector point, Light light)
        {
			var ray = new Line(point, light.Position - point);
			var intersection = FindFirstIntersection(ray, 0.0, 5000.0);

			return !intersection.Valid || !intersection.Visible;
        }

		private bool isPointLit(Vector point)
		{
			foreach (var light in lights) {
				if (IsLit(point, light)) {
					return true;
				}
			}
			return false;
		}
        public void Render(Camera camera, int width, int height, string filename)
        {
            var background = new Color();
            var viewParallel = (camera.Up ^ camera.Direction).Normalize();

            var image = new Image(width, height);

            var distanceToViewplane = camera.Direction * camera.ViewPlaneDistance;
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
					var u = ImageToViewPlane(i, width, camera.ViewPlaneWidth);
					var v = ImageToViewPlane(j, height, camera.ViewPlaneHeight);

					var vecU = viewParallel * u;
					var vecV = camera.Up * v;

					var vecUV = vecU + vecV;
					var vecS = camera.Position + distanceToViewplane + vecUV;

					var ray = new Line(camera.Position, vecS);

					var intersection = FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);


					if (intersection.Valid && intersection.Visible) {
						var color = intersection.Geometry.Color;
						if (!isPointLit(intersection.Position)) {
                    		image.SetPixel(i, j, background + color * 0.2);
						} else {
                    		image.SetPixel(i, j, background + color);
						}
					} else {
                    	image.SetPixel(i, j, background);
					}

                }
            }

            image.Store(filename);
        }
    }
}