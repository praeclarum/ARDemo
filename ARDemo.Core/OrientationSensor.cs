using System;
using MonoTouch.Foundation;
using MonoTouch.CoreMotion;
using OpenTK;

namespace ARDemo
{
	public class OrientationSensor
	{
		public event EventHandler OrientationReceived = delegate {};

		public Matrix4d Orientation { get; private set; }
		public double Pitch { get; private set; }

		CMMotionManager mman;

		public void Start ()
		{
			mman = new CMMotionManager {
				ShowsDeviceMovementDisplay = true,
			};

			mman.StartDeviceMotionUpdates (
			//	CMAttitudeReferenceFrame.XArbitraryZVertical,
			//	CMAttitudeReferenceFrame.XArbitraryCorrectedZVertical,
//				CMAttitudeReferenceFrame.XMagneticNorthZVertical,
				CMAttitudeReferenceFrame.XTrueNorthZVertical,
				new NSOperationQueue (), 
				(motion, error) => {
					if (error == null) {
						Orientation = ToMatrix4d (motion.Attitude.RotationMatrix);

						var roll = motion.Attitude.Roll;
						var p = motion.Attitude.Pitch;
						if (roll > 0) {
							p = (3 * Math.PI) / 2 + p;
						}
						else {
							p = Math.PI / 2 - p;
						}
						Pitch = p;

						OrientationReceived (this, EventArgs.Empty);
					}
				});
		}

		static Matrix4d ToMatrix4d (CMRotationMatrix m)
		{
			return new Matrix4d (
				new Vector4d (m.m11, m.m12, m.m13, 0),
				new Vector4d (m.m21, m.m22, m.m23, 0),
				new Vector4d (m.m31, m.m32, m.m33, 0),
				new Vector4d (0, 0, 0, 1));
		}
	}
}

