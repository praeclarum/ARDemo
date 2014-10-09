using System;
using OpenTK;

namespace ARDemo
{
	public struct Location
	{
		public double Latitude;
		public double Longitude;
		public double Altitude;

		public Location (double latitude, double longitude, double altitude = 0)
		{
			Latitude = latitude;
			Longitude = longitude;
			Altitude = altitude;
		}

		public const double EarthRadius = 6378137.0;
		public const double EarthEccentricity = 8.1819190842622e-2;

		public static readonly Location NorthPole = new Location (90, 0, 0);
		public static readonly Location MagneticNorthPole = new Location (86.073, 206.653, 0);
		public static readonly Location SouthPole = new Location (-90, 0, 0);

		const double ToRad = Math.PI / 180;

		public Vector3d Position {
			get {
				var omega = ToRad * Longitude;
				var phi = ToRad * Latitude;
				var r = EarthRadius + Altitude;

				return new Vector3d (
					r * Math.Cos(phi) * Math.Cos(omega),
					r * Math.Cos(phi) * Math.Sin(omega),
					r * Math.Sin(phi));
			}
		}

		public Vector3d EcefPosition {
			get {
				var phi = ToRad * Latitude;
				var omega = ToRad * Longitude;

				double clat = Math.Cos (phi);
				double slat = Math.Sin (phi);
				double clon = Math.Cos (omega);
				double slon = Math.Sin (omega);

				double N = EarthRadius / Math.Sqrt ((1.0 - EarthEccentricity * EarthEccentricity) * slat * slat);

				return new Vector3d (
					(N + Altitude) * clat * clon,
					(N + Altitude) * clat * slon,
					(N * (1.0 - EarthEccentricity * EarthEccentricity) + Altitude) * slat);
			}
		}

		public Location OffsetAltitude (double offset)
		{
			return new Location (Latitude, Longitude, Altitude + offset);
		}

		public override string ToString ()
		{
			return string.Format ("[Lat={0}, Lon={1}, Alt={2}]", Latitude, Longitude, Altitude);
		}
	}
}

