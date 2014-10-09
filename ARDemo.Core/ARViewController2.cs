using System;
using System.Drawing;
using MonoTouch.UIKit;
using OpenTK;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.CoreGraphics;

namespace ARDemo
{
	public class Annotation : UILabel
	{
		public Location Location { get; set; }

		public Annotation ()
		{
			BackgroundColor = UIColor.White;
			TextAlignment = UITextAlignment.Center;
			Frame = new RectangleF (0, 0, 140, 22);
		}
	}

	public class ARViewController2 : ARViewController
	{
		List<Annotation> annos;

		public ARViewController2 ()
		{
			annos = new List<Annotation> ();
		}

		public void AddAnnotation (string title, Location location)
		{
			var a = new Annotation {
				Location = location,
			};
			a.Text = title;
			annos.Add (a);
			if (IsViewLoaded) {
				View.AddSubview (a);
			}
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.AddSubviews (annos.Cast<UIView> ().ToArray ());
		}

		protected override void HandleFrame (MonoTouch.UIKit.UIImage f)
		{
			base.HandleFrame (f);
			LayoutAnnotations ();
		}

		protected override void HandleMotion (object sender, EventArgs e)
		{
			base.HandleMotion (sender, e);
			BeginInvokeOnMainThread (LayoutAnnotations);
		}

		void LayoutAnnotations ()
		{

			var w = 140;
			var hw = w / 2;
			var h = 22;
			var hh = h/2;

			foreach (var a in annos) {
				PointF p;
				if (LocationToView (a.Location, out p)) {
					a.Transform = CGAffineTransform.MakeIdentity ();
					a.Frame = new RectangleF (p.X - hw, p.Y - hh, w, h);
					a.Transform = CGAffineTransform.MakeRotation ((float)orientationSensor.Pitch);
				}
			}
		}

		public bool LocationToView (Location location, out PointF point)
		{
			// Move location to 3D earth
			var pos3d = new Vector4d (location.Position, 1);

			// Camera model
			var m = Matrix4d.Mult (modelViewMatrix, projectionMatrix);

			// Project into homogeneous 2D point
			var pos2h = Vector4d.Transform (pos3d, m);

			// Perform the perspective divide
			var pos2 = pos2h / pos2h.W;

			// Ignore points behind us
			if (pos2h.W < 0) {
				point = PointF.Empty;
				return false;
			}

//			Console.WriteLine ("{0} = {1}", "W", pos2h.W);

			// Stretch into our view
			var fr = videoCameraView.Frame;
			point = new PointF (
				fr.X + (float)((pos2.X + 1) * 0.5) * fr.Width,
				fr.Y + (float)((-pos2.Y + 1) * 0.5) * fr.Height
			);
			return true;
		}
	}
}

