using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace rt;

public class RawCtMask : Geometry
{
	private readonly Vector _position;
	private readonly double _scale;
	private readonly ColorMap _colorMap;
	private readonly byte[] _data;

	private readonly int[] _resolution = new int[3];
	private readonly double[] _thickness = new double[3];
	private readonly Vector _v0;
	private readonly Vector _v1;

	public RawCtMask(string datFile, string rawFile, Vector position, double scale, ColorMap colorMap) : base(Color.NONE)
	{
		_position = position;
		_scale = scale;
		_colorMap = colorMap;

		var lines = File.ReadLines(datFile);
		foreach (var line in lines)
		{
			var kv = Regex.Replace(line, "[:\\t ]+", ":").Split(':');
			if (kv[0] == "Resolution")
			{
				_resolution[0] = Convert.ToInt32(kv[1]);
				_resolution[1] = Convert.ToInt32(kv[2]);
				_resolution[2] = Convert.ToInt32(kv[3]);
			}
			else if (kv[0] == "SliceThickness")
			{
				_thickness[0] = Convert.ToDouble(kv[1]);
				_thickness[1] = Convert.ToDouble(kv[2]);
				_thickness[2] = Convert.ToDouble(kv[3]);
			}
		}

		_v0 = position;
		_v1 = position + new Vector(_resolution[0] * _thickness[0] * scale, _resolution[1] * _thickness[1] * scale, _resolution[2] * _thickness[2] * scale);

		var len = _resolution[0] * _resolution[1] * _resolution[2];
		_data = new byte[len];
		using FileStream f = new FileStream(rawFile, FileMode.Open, FileAccess.Read);
		if (f.Read(_data, 0, len) != len)
		{
			throw new InvalidDataException($"Failed to read the {len}-byte raw data");
		}
	}

	private ushort Value(int x, int y, int z)
	{
		if (x < 0 || y < 0 || z < 0 || x >= _resolution[0] || y >= _resolution[1] || z >= _resolution[2])
		{
			return 0;
		}

		return _data[z * _resolution[1] * _resolution[0] + y * _resolution[0] + x];
	}

	public override Intersection GetIntersection(Line line, double minDist, double maxDist)
	{
		var lineDir = new Vector(line.direction);
		var lineOrigin = line.origin;

		var tmin = new Vector();
		var tmax = new Vector();

		// check if intersection is in the bounding box - X axis
		if (lineDir.X == 0) {
			if (lineOrigin.X < _v0.X || lineOrigin.X > _v1.X) return Intersection.NONE;
			tmin.X = Double.MinValue;
			tmax.X = Double.MaxValue;
		}
		else if (lineDir.X < 0) {
			tmax.X = (_v0.X - lineOrigin.X) / lineDir.X;
			tmin.X = (_v1.X - lineOrigin.X) / lineDir.X;
		}
		else {
			tmin.X = (_v0.X - lineOrigin.X) / lineDir.X;
			tmax.X = (_v1.X - lineOrigin.X) / lineDir.X;
		}

		// check if intersection is in the bounding box - Y axis
		if (lineDir.Y == 0) {
			if (lineOrigin.Y < _v0.Y || lineOrigin.Y > _v1.Y) return Intersection.NONE;
			tmin.Y = Double.MinValue;
			tmax.Y = Double.MaxValue;
		}
		else if (lineDir.Y < 0) {
			tmax.Y = (_v0.Y - lineOrigin.Y) / lineDir.Y;
			tmin.Y = (_v1.Y - lineOrigin.Y) / lineDir.Y;
		}
		else {
			tmin.Y = (_v0.Y - lineOrigin.Y) / lineDir.Y;
			tmax.Y = (_v1.Y - lineOrigin.Y) / lineDir.Y;
		}

		// check if intersection is in the bounding box - Z axis
		if (lineDir.Z == 0) {
			if (lineOrigin.Z < _v0.Z || lineOrigin.Z > _v1.Z) return Intersection.NONE;
			tmin.Z = Double.MinValue;
			tmax.Z = Double.MaxValue;
		}
		else if (lineDir.Z < 0) {
			tmax.Z = (_v0.Z - lineOrigin.Z) / lineDir.Z;
			tmin.Z = (_v1.Z - lineOrigin.Z) / lineDir.Z;
		}
		else {
			tmin.Z = (_v0.Z - lineOrigin.Z) / lineDir.Z;
			tmax.Z = (_v1.Z - lineOrigin.Z) / lineDir.Z;
		}

		double tlower = getMaxComponent(tmin);
		double tupper = getMinComponent(tmax);

		if (tupper <= tlower) {
			return Intersection.NONE;
		}

		Color color = new Color(0, 0, 0, 0);
		var distance = _scale;

		// find the first point of intersection - the first point with alpha > 0
		for (double t = tlower; t < tupper; t += distance) {
			Color currColor = GetColor(line.CoordinateToPosition(t));

			if (currColor.Alpha > 0) {
				tlower = t;
				break;
			}
		}

		if (tupper <= tlower) {
			return Intersection.NONE;
		}


		double top1 = tupper;

		for (double t = tlower; t < tupper; t += distance) {
			Color currColor = GetColor(line.CoordinateToPosition(t));

			if (currColor.Alpha == 1) {
				top1 = t; break;
			}
		}

		for (double t = top1; t >= tlower; t -= distance) {
			Color currColor = GetColor(line.CoordinateToPosition(t));

			color = color * (1 - currColor.Alpha) + currColor * currColor.Alpha;
		}

		if (color.Alpha == 0) return Intersection.NONE;

		Vector norm = GetNormal(line.CoordinateToPosition(tlower));

		return new Intersection(
			true,
			true, 
			this, 
			line, 
			tlower, 
			norm, 
			Material.FromColor(color), 
			color
		);
	}


	private int[] GetIndexes(Vector v)
	{
		return new[]{
			(int)Math.Floor((v.X - _position.X) / _thickness[0] / _scale),
			(int)Math.Floor((v.Y - _position.Y) / _thickness[1] / _scale),
			(int)Math.Floor((v.Z - _position.Z) / _thickness[2] / _scale)
		};
	}
	private Color GetColor(Vector v)
	{
		int[] idx = GetIndexes(v);

		ushort value = Value(idx[0], idx[1], idx[2]);
		return _colorMap.GetColor(value);
	}

	private Vector GetNormal(Vector v)
	{
		int[] idx = GetIndexes(v);
		double x0 = Value(idx[0] - 1, idx[1], idx[2]);
		double x1 = Value(idx[0] + 1, idx[1], idx[2]);
		double y0 = Value(idx[0], idx[1] - 1, idx[2]);
		double y1 = Value(idx[0], idx[1] + 1, idx[2]);
		double z0 = Value(idx[0], idx[1], idx[2] - 1);
		double z1 = Value(idx[0], idx[1], idx[2] + 1);

		return new Vector(x1 - x0, y1 - y0, z1 - z0).Normalize();
	}

	private double getMinComponent(Vector v)
	{
		return Math.Min(v.X, Math.Min(v.Y, v.Z));
	}

	private double getMaxComponent(Vector v)
	{
		return Math.Max(v.X, Math.Max(v.Y, v.Z));
	}
}