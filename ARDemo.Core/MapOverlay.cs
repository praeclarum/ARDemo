using System;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using System.Drawing;
using MonoTouch.CoreLocation;

namespace ARDemo
{
	public class MapOverlay : UIView
	{
		readonly MKMapView map;

		public MapOverlay () : base (new RectangleF (0, 0, 144, 144))
		{
			map = new MKMapView (Bounds) {
				AutoresizingMask = UIViewAutoresizing.FlexibleDimensions,
				ShowsUserLocation = true,
				MapType = MKMapType.Satellite,
			};
			map.SetRegion (MKCoordinateRegion.FromDistance (new CLLocationCoordinate2D (47,-122), 1, 1), false);
			map.UserTrackingMode = MKUserTrackingMode.FollowWithHeading;
			Alpha = 0.5f;
			AddSubview (map);
		}
	}
}

