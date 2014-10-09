using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ARDemo.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;

		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			// If you have defined a root view controller, set it here:
			var ar2 = new ARViewController2 ();

			// Atlanta annotations
			ar2.AddAnnotation ("Hyatt", new Location (33.761665, -84.387530, 0));
			ar2.AddAnnotation ("Aquarium", new Location (33.763464, -84.394514));
			ar2.AddAnnotation ("GIT", new Location (33.776579, -84.398249));

			// Seattle annotations
//			ar2.AddAnnotation ("Space Needle", new Location (47.6204, -122.3491));
//			ar2.AddAnnotation ("Gray Building", new Location (47.656680, -122.365480));
//			ar2.AddAnnotation ("Gas Works", new Location (47.645316, -122.336371));

			window.RootViewController = ar2;
			
			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

