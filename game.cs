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
			// let the raytracer update the camera and then render the scene
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
			// draw text:
			if (raytracer.DoAA)
				screen2.Print("Anti-aliasing:" + raytracer.Antialiasing + "x", 2, 2, RGB(1, 1, 0));
			else
				screen2.Print("Anti-aliasing:" + raytracer.DoAA, 2, 2, RGB(1, 1, 0));
			screen2.Print("Recursion depth:" + raytracer.MaxRecursion, 2, 17, RGB(1, 1, 0));
			screen2.Print("Smoothdraw:" + raytracer.Smoothdraw, 2, 32, RGB(1, 1, 0));
			screen2.Print("FOV:" + raytracer.Camera.FOV, 2, 47, RGB(1, 1, 0));
			// draw the primary rays in white
			for (int x = 0; x < 512; x ++)
			{
				if (raytracer.rays.Count > x)
				{
					Ray ray = raytracer.rays[x];

					Vector3 point2;
					point2 = ray.Direction*Math.Min(ray.Distance, 100) + ray.Origin;
					screen2.Line(TX(ray.Origin.X), TZ(ray.Origin.Z), TX(point2.X), TZ(point2.Z), RGB(1, 1, 1));
				}
			}
			// draw the shadow rays in red
			for (int i = 0; i < raytracer.shadowrays.Count; i++)
			{
				Ray shadowRay = raytracer.shadowrays[i];
				if (shadowRay != null)
				{
					Vector3 point2S = (shadowRay.Direction*Math.Min(shadowRay.Distance, 100)) + shadowRay.Origin;
					screen2.Line(TX(point2S.X), TZ(point2S.Z), TX(shadowRay.Origin.X), TZ(shadowRay.Origin.Z), RGB(1, 0, 0));
				}
			}
			// draw the secondary rays in cyan
			for (int i = 0; i < raytracer.reflectrays.Count; i++)
			{
				Ray reflectRay = raytracer.reflectrays[i];
				if (reflectRay != null)
				{
					Vector3 point2S = (reflectRay.Direction*Math.Min(reflectRay.Distance, 100)) + reflectRay.Origin;
					screen2.Line(TX(point2S.X), TZ(point2S.Z), TX(reflectRay.Origin.X), TZ(reflectRay.Origin.Z), RGB(0, 1, 1));
				}
			}
			// draw the refraction rays in green
			for (int i = 0; i < raytracer.refractrays.Count; i++)
			{
				Ray refractRay = raytracer.refractrays[i];
				if (refractRay != null)
				{
					Vector3 point2S = ( refractRay.Direction * Math.Min(refractRay.Distance, 100) ) + refractRay.Origin;
					screen2.Line(TX(point2S.X), TZ(point2S.Z), TX(refractRay.Origin.X), TZ(refractRay.Origin.Z), RGB(0, 1, 0));
				}
			}
			// draw the primitives
			List<Primitive> primitives = raytracer.Scene.Primitives;
			foreach (Primitive primitive in primitives)
			{
				if (primitive.GetType() == typeof(Sphere))
				{
					Sphere temp = (Sphere) primitive;
					screen2.Circle(TX(temp.Position.X), TZ(temp.Position.Z), TL(temp.Radius), RGB(temp.Material.Color));
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
				raytracer.Smoothdraw = false;
				raytracer.Camera.Z -= 2.5f;
			}
			if (NewKeyPress(Key.Down))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.Z += 2.5f;
			}
			if (NewKeyPress(Key.Left))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.X -= 2.5f;
			}
			if (NewKeyPress(Key.Right))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.X += 2.5f;
			}
			//moving the camera
			if (NewKeyPress(Key.W))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.Position += new Vector3(raytracer.Camera.Direction.X, 0, raytracer.Camera.Direction.Z)*0.1f;
			}
			if (NewKeyPress(Key.S))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.Position -= new Vector3(raytracer.Camera.Direction.X, 0, raytracer.Camera.Direction.Z)*0.1f;
			}
			if (NewKeyPress(Key.A))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.Position -= new Vector3(raytracer.Camera.Direction.Z, 0, -raytracer.Camera.Direction.X)*0.1f;
			}
			if (NewKeyPress(Key.D))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.Position += new Vector3(raytracer.Camera.Direction.Z, 0, -raytracer.Camera.Direction.X)*0.1f;
			}
			if (NewKeyPress(Key.Z))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.Position += new Vector3(0f, -0.1f, 0f);
			}
			if (NewKeyPress(Key.X))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.Position += new Vector3(0f, 0.1f, 0f);
			}
			if (NewKeyPress(Key.P))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.FOV = Math.Min(170, raytracer.Camera.FOV + 1);
			}
			if (NewKeyPress(Key.O))
			{
				raytracer.Smoothdraw = false;
				raytracer.Camera.FOV = Math.Max(10, raytracer.Camera.FOV - 1);
			}
			if (NewKeyPress(Key.Comma))
			{
				raytracer.Smoothdraw = false;
				raytracer.MaxRecursion = Math.Min(100, raytracer.MaxRecursion + 1);
			}
			if (NewKeyPress(Key.Period))
			{
				raytracer.Smoothdraw = false;
				raytracer.MaxRecursion = Math.Max(2, raytracer.MaxRecursion - 1);
			}
			// enable smooth drawing (draws all the pixel (with anti aliasing if it's enabled)
			if (NewKeyPress(Key.Space))
			{
				raytracer.Smoothdraw = true;
			}
			// enable aa
			if (NewKeyPress(Key.T))
			{
				raytracer.Smoothdraw = true;
				raytracer.DoAA = true;
			}
			// disable aa
			if (NewKeyPress(Key.Y))
			{
				raytracer.DoAA = false;
			}
		}

		// check if a key was pressed this frame
		public bool NewKeyPress(Key key)
		{
			return currentKeyState[key] && (currentKeyState[key] != prevKeyState[key]);
		}

		// convert given x world coordinate to screen coordinates
		public int TX(float x)
		{
			x += origX;
			x += scaleX/2;
			x *= screen2.width/scaleX;
			return (int) x;
		}

		// convert given z world coordinate to screen coordinates
		public int TZ(float z)
		{
			z += origY;
			z += scaleY/2;
			z *= screen2.height/scaleY;
			z = screen2.height - z;
			return (int) z;
		}

		// translate world length to screen length
		public int TL(float l)
		{
			l *= screen2.width / scaleX;
			return (int) l;
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