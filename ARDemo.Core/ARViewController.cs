using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.ES11;


namespace ARDemo
{
	public class ARViewController : UIViewController
	{
		VideoCamera videoCamera;
		protected readonly UIImageView videoCameraView;
		SizeF videoImageSize;

		LocationSensor locationSensor;
		protected readonly OrientationSensor orientationSensor;

		MapOverlay map;

		public ARViewController ()
		{
			locationSensor = new LocationSensor ();
			orientationSensor = new OrientationSensor ();

			videoCameraView = new UIImageView {
				Transform = CGAffineTransform.MakeRotation ((float)Math.PI/2),
			};

			videoCamera = new VideoCamera ();

			map = new MapOverlay ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			locationSensor.LocationReceived += HandleMotion;
			locationSensor.Start ();

			orientationSensor.OrientationReceived += HandleMotion;
			orientationSensor.Start ();

			View.AddSubview (videoCameraView);
			videoCamera.FrameCaptured += HandleFrame;
			videoCamera.Start ();

			View.AddSubview (map);
		}

		protected virtual void HandleFrame (UIImage f)
		{
			projectionMatrix = GetProjectionMatrix (videoCamera, f);
			
			videoCameraView.Image = f;
			var rotSize = f.Size;
			var newSize = new SizeF (rotSize.Height, rotSize.Width);
			if (newSize != videoImageSize) {
				videoImageSize = newSize;
				LayoutViews ();
			}
		}

		protected virtual void HandleMotion (object sender, EventArgs e)
		{
			modelViewMatrix = GetModelView (locationSensor.Location, orientationSensor.Orientation);
		}

		void SetOpenGLCamera ()
		{
			GL.MatrixMode (All.Projection);
			GL.PushMatrix ();
			var p = ToMatrix4 (ref projectionMatrix);
			GL.LoadMatrix (ref p.Row0.X);
			GL.MatrixMode (All.Modelview);
			GL.PushMatrix ();
			var mv = ToMatrix4 (ref modelViewMatrix);
			GL.LoadMatrix (ref mv.Row0.X);
		}

		static Matrix4 ToMatrix4 (ref Matrix4d m)
		{
			return new Matrix4 (
				ToVector4 (ref m.Row0),
				ToVector4 (ref m.Row1),
				ToVector4 (ref m.Row2),
				ToVector4 (ref m.Row3));
		}

		static Vector4 ToVector4 (ref Vector4d v)
		{
			return new Vector4 ((float)v.X, (float)v.Y, (float)v.Z, (float)v.W);
		}

		protected SizeF viewSize = new SizeF (0, 0);
		protected Matrix4d projectionMatrix = Matrix4d.Identity;
		protected Matrix4d modelViewMatrix = Matrix4d.Identity;

		public static Matrix4d GetProjectionMatrix (VideoCamera videoCamera, UIImage frame)
		{
			// Aspect is inverted because we rotate the image
			return Matrix4d.CreatePerspectiveFieldOfView (
				fovy: videoCamera.FieldOfView * Math.PI / 180,
				aspect: frame.Size.Height / frame.Size.Width,
				zNear: 0.01,
				zFar: 4700);
		}

		public static Matrix4d GetModelView (Location location, Matrix4d orientation)
		{
			// 1. Calculate position in 3D cartesian world
			// 2. Find "up"
			// 3. Orient to face the north pole
			// 4. Apply the device orientation

			//
			// 1. Calculate position in 3D cartesian world
			//
			var pos = location.Position;

			//
			// 2. Find "up"
			//
			var up = location.OffsetAltitude (100).Position - pos;
			up.Normalize ();

			//
			// 3. Orient to face the north pole
			//
			var northPos = new Location (location.Latitude + 0.1, location.Longitude, location.Altitude).Position;

			var northZAxis = (pos - northPos);
			northZAxis.Normalize ();

			var northYAxis = up;
			var northXAxis = Vector3d.Cross (northYAxis, northZAxis);

			northXAxis.Normalize ();
			northZAxis = Vector3d.Cross (northXAxis, northYAxis);
			northZAxis.Normalize ();

			northYAxis = Vector3d.Cross (northZAxis, northXAxis);
			northYAxis.Normalize ();

			var lookNorthI = new Matrix4d (
				new Vector4d(northXAxis),
				new Vector4d(northYAxis),
				new Vector4d(northZAxis),
				Vector4d.UnitW);

//			var lookTest = Matrix4d.LookAt (pos, new Location (47.656680, -122.365480).Position, up);



			//
			// 4. Apply the device orientation
			//
			var newOrient = new Matrix4d (
				-orientation.Column1,
				orientation.Column2,
				-orientation.Column0,
				Vector4d.UnitW);

			var newOrientI = newOrient;
			newOrientI.Transpose ();

			var modelViewI = (newOrientI * lookNorthI);
			modelViewI.Row3 = new Vector4d (pos.X, pos.Y, pos.Z, 1);
			var modelView = modelViewI;
			try {
				modelView.Invert ();
				return modelView;
			}
			catch (InvalidOperationException) {
				var lookNorth = lookNorthI;
				lookNorth.Invert ();
				return lookNorth;
			}
		}

		void LayoutViews ()
		{
			Console.WriteLine ("Layout views");

			var bounds = View.Bounds;
			var scale = bounds.Width / videoImageSize.Width;
			if (scale * videoImageSize.Height < bounds.Height) {
				scale = bounds.Height / videoImageSize.Height;
			}

			var size = new SizeF (videoImageSize.Width * scale, videoImageSize.Height * scale);

			videoCameraView.Frame = new RectangleF (
				(bounds.Width - size.Width) / 2,
				(bounds.Height - size.Height) / 2,
				size.Width,
				size.Height);

			viewSize = size;
		}

		public override bool ShouldAutorotate ()
		{
			return false;
		}
	}
}

