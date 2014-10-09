using System;
using System.Diagnostics;
using System.Linq;
using MonoTouch.AVFoundation;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreMedia;
using MonoTouch.CoreVideo;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ARDemo
{
	public class VideoCamera
	{
		AVCaptureSession session;

		AVCaptureDevice device;
		AVCaptureDeviceInput input;

		AVCaptureVideoDataOutput output;
		DispatchQueue queue;

		public Action<int, int, int, IntPtr> CapturedFrame;

		public VideoCamera ()
		{
			CreateSession ();
			CreateDevice ();
			CreateInput ();
			CreateOutput ();
		}

		public void Start ()
		{
			session.StartRunning ();
		}

		public void Stop ()
		{
			session.StopRunning ();
		}

		public float FieldOfView {
			get {
				return device.ActiveFormat.VideoFieldOfView;
			}
		}

		void CreateSession ()
		{
			session = new AVCaptureSession ();
			session.SessionPreset = AVCaptureSession.PresetMedium;
		}

		void CreateDevice ()
		{
			NSError error;

			device = AVCaptureDevice.DefaultDeviceWithMediaType (AVMediaType.Video);
			if (device == null) {
				throw new Exception ("No default video device");
			}

			device.LockForConfiguration(out error);
			if (error != null) {
				throw new Exception ("Could not configure. Error: " + error);
			}

			device.ActiveVideoMinFrameDuration = new CMTime (1, 30);

			device.UnlockForConfiguration();
		}

		void CreateInput ()
		{
			NSError error;

			input = AVCaptureDeviceInput.FromDevice (device, out error);
			if (input == null) {
				throw new Exception ("Could not capture from " + device + " Error: " + error);
			}

			session.AddInput (input);
		}


		void CreateOutput ()
		{
			output = new AVCaptureVideoDataOutput ();
			output.VideoSettings = new AVVideoSettings (CVPixelFormatType.CV32BGRA);

			queue = new DispatchQueue ("VideoCameraQueue");
			output.SetSampleBufferDelegateAndQueue (new VideoCameraDelegate { Camera = this }, queue);

			session.AddOutput (output);
		}

		public event Action<UIImage> FrameCaptured = delegate {};

		void OnFrameCaptured (UIImage frame)
		{
			DispatchQueue.MainQueue.DispatchAsync (() => FrameCaptured (frame));
		}

		class VideoCameraDelegate : AVCaptureVideoDataOutputSampleBufferDelegate
		{
			public VideoCamera Camera;
			public override void DidOutputSampleBuffer (AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, 
				                                        AVCaptureConnection connection)
			{
				try {
					var frame = ImageFromSampleBuffer (sampleBuffer);
					Camera.OnFrameCaptured (frame);
					sampleBuffer.Dispose ();
				} catch (Exception ex) {
					Debug.WriteLine (ex);
				}
			}

			static UIImage ImageFromSampleBuffer (CMSampleBuffer sampleBuffer)
			{
				using (var pixelBuffer = sampleBuffer.GetImageBuffer () as CVPixelBuffer){
					pixelBuffer.Lock (CVOptionFlags.None);
					var baseAddress = pixelBuffer.BaseAddress;
					int bytesPerRow = pixelBuffer.BytesPerRow;
					int width = pixelBuffer.Width;
					int height = pixelBuffer.Height;
					var flags = CGBitmapFlags.PremultipliedFirst | CGBitmapFlags.ByteOrder32Little;
					using (var cs = CGColorSpace.CreateDeviceRGB ())
					using (var context = new CGBitmapContext (baseAddress,width, height, 8, bytesPerRow, cs, (CGImageAlphaInfo) flags))
					using (var cgImage = context.ToImage ()){
						pixelBuffer.Unlock (CVOptionFlags.None);
						return UIImage.FromImage (cgImage);
					}
				}
			}
		}


	}

}