using System;
using OpenTK;
using OpenTK.Input;
using System.Collections.Generic;

namespace Template {

	class Game
	{
		// member variables
		public Surface screen1, screen2;
		// the raytracer which will do all the calculations
		private Raytracer raytracer;
		// for keyboard input
		private KeyboardState prevKeyState, currentKeyState;
		private float scaleX = 10.0f;
		private float scaleY = 10.0f;
		// location of the center
		private float origX = 0.0f, origY = -3f;

		// initialize
		public void Init()
		{
			// change the fov
			float fov = 90;
			raytracer = new Raytracer(fov);
		}

		// tick: renders one frame
		public void Tick()
		{
			// let the raytracer update the camera and then render the secne
			raytracer.Camera.Update();
			raytracer.Render();
			// draw the view
			for (int x = 0; x < 512; x++)
				for (int y = 0; y < 512; y++)
				{
					Vector3 color = raytracer.colors[x, y];
					screen1.Plot(x, y, RGB(color.X, color.Y, color.Z));
				}

			// from here it's the debug view drawing
			screen2.Clear(0x0);
			// draw the primary rays in white
			for(int x = 0; x < 512; x += 10)
			{
				if (raytracer.rays.Count > x)
				{
					Ray ray = raytracer.rays[x];

					Vector3 point2;
					point2 = ( ray.Direction * Math.Min(ray.Distance, 100) ) + ray.Origin;
					screen2.Line(TX(ray.Origin.X), TZ(ray.Origin.Z), ( TX(point2.X) ), ( TZ(point2.Z) ), RGB(1, 1, 1));
				}
			}
			// draw the shadow rays in red
			for (int i = 0; i < raytracer.shadowrays.Count; i += 10)
			{
				Ray shadowRay = raytracer.shadowrays[i];
				if (shadowRay != null)
				{
					Vector3 point2S = ( shadowRay.Direction * Math.Min(shadowRay.Distance, 100) ) + shadowRay.Origin;
					screen2.Line(( TX(point2S.X) ), ( TZ(point2S.Z) ), TX(shadowRay.Origin.X), TZ(shadowRay.Origin.Z), RGB(1, 0, 0));
				}
			}
			// draw the secondary rays in cyan
			for (int i = 0; i < raytracer.reflectray.Count; i += 10)
			{
				Ray reflectRay = raytracer.reflectray[i];
				if (reflectRay != null)
				{
					Vector3 point2S = ( reflectRay.Direction * Math.Min(reflectRay.Distance, 100) ) + reflectRay.Origin;
					screen2.Line(( TX(point2S.X) ), ( TZ(point2S.Z) ), TX(reflectRay.Origin.X), TZ(reflectRay.Origin.Z), RGB(0, 1f, 1));
				}
			}
			// draw the primitives
			List<Primitive> primitives = raytracer.Scene.Primitives;
			foreach(Primitive primitive in primitives)
			{
				if (primitive.GetType() == typeof(Sphere))
				{
					Sphere temp = (Sphere) primitive;
					screen2.Circle(TX(temp.Position.X), TZ(temp.Position.Z), (int)(temp.Radius * 51.2 +1 ), RGB(temp.Material.Color));
				}
				else if (primitive.GetType() == typeof(Triangle))
				{
					Triangle temp = (Triangle) primitive;
					screen2.Line(TX(temp.p0.X), TZ(temp.p0.Z), TX(temp.p1.X), TZ(temp.p1.Z), RGB(temp.Material.Color));
					screen2.Line(TX(temp.p1.X), TZ(temp.p1.Z), TX(temp.p2.X), TZ(temp.p2.Z), RGB(temp.Material.Color));
					screen2.Line(TX(temp.p2.X), TZ(temp.p2.Z), TX(temp.p0.X), TZ(temp.p0.Z), RGB(temp.Material.Color));
				}
			}
			// draw camera screen
			Screen traceScreen = raytracer.Camera.Screen;
			// get the coordinates
			int x11 = TX(traceScreen.p2.X);
			int y11 = TZ(traceScreen.p2.Z);
			int x12 = TX(traceScreen.p3.X);
			int y12 = TZ(traceScreen.p3.Z);
			// draw the lines (3 lines to make it a bit thicker)
			screen2.Line(x11, y11 - 1, x12, y12 - 1, RGB(1, 0, 1));
			screen2.Line(x11, y11, x12, y12, RGB(1, 0, 1));
			screen2.Line(x11, y11 + 1, x12, y12 + 1, RGB(1, 0, 1));
		}

		// input handling
		public void Controls(KeyboardState keys)
		{
			// changes the current keystate
			currentKeyState = keys;
			// rotating the camera
			if (NewKeyPress(Key.Up))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.z -= 2.5f;
			}
			if (NewKeyPress(Key.Down))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.z += 2.5f;
			}
			if (NewKeyPress(Key.Left))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.x -= 2.5f;
			}
			if (NewKeyPress(Key.Right))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.x += 2.5f;
			}
			//moving the camera
			if (NewKeyPress(Key.W))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.Position +=  new Vector3(raytracer.Camera.Direction.X, 0, raytracer.Camera.Direction.Z) * 0.1f;
			}
			if (NewKeyPress(Key.S))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.Position -= new Vector3(raytracer.Camera.Direction.X, 0, raytracer.Camera.Direction.Z) * 0.1f;
			}
			if (NewKeyPress(Key.A))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.Position -= new Vector3(raytracer.Camera.Direction.Z, 0, -raytracer.Camera.Direction.X) * 0.1f;
			}
			if (NewKeyPress(Key.D))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.Position += new Vector3(raytracer.Camera.Direction.Z, 0, -raytracer.Camera.Direction.X) * 0.1f;
			}
			if (NewKeyPress(Key.Z))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.Position += new Vector3(0f, -0.1f, 0f);
			}
			if (NewKeyPress(Key.X))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.Position += new Vector3(0f, 0.1f, 0f);
			}
			// enable smooth drawing (draws all the pixel (with anti aliasing if it's enabled)
			if (NewKeyPress(Key.Space))
			{
				raytracer.smoothdraw = true;
			}
			// enable aa
			if (NewKeyPress(Key.T))
			{
				raytracer.doaa = true;
			}
			// disable aa
			if (NewKeyPress(Key.Y))
			{
				raytracer.doaa = false;
			}
		}

		// check if a key was pressed this frame
		public bool NewKeyPress(Key key)
		{
			return ( currentKeyState[key] && ( currentKeyState[key] != prevKeyState[key] ) );
		}

		//convert given x world coordinate to screen coordinates
		public int TX(float x)
		{
			x += origX;
			x += scaleX / 2;
			x *= screen2.width / scaleX;
			return (int) x;
		}

		//convert given z world coordinate to screen coordinates
		public int TZ(float z)
		{
			z += origY;
			z += scaleY / 2;
			z *= screen2.height / scaleY;
			z = screen2.height - z;
			return (int) z;
		}

		// convert 3 floats to an rgb int
		private static int RGB (float r, float g, float b)
		{
			int rint = (int) (Math.Min(1, r) * 255);
			int gint = (int) (Math.Min(1, g) * 255);
			int bint = (int) (Math.Min(1, b) * 255);
			return (rint << 16) + (gint << 8) + (bint);
		}

		// convert a vector3 to an rgb int
		private static int RGB( Vector3 color)
		{
			int rint = (int) ( Math.Min(1, color.X) * 255 );
			int gint = (int) ( Math.Min(1, color.Y) * 255 );
			int bint = (int) ( Math.Min(1, color.Z) * 255 );
			return ( rint << 16 ) + ( gint << 8 ) + ( bint );
		}
	}

} // namespace Template