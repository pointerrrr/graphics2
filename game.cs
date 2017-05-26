using System;
using System.IO;
using OpenTK;
using OpenTK.Input;
using System.Collections.Generic;
using System.Threading;

namespace Template {

	class Game
	{
		// member variables
		public Surface screen1, screen2;

		private Raytracer raytracer;
		private KeyboardState prevKeyState, currentKeyState;
		float scaleX = 10.0f;
		float scaleY = 10.0f;
		//location of the center
		float origX = 0.0f, origY = -3f;
		//current rotation of the square


		// initialize
		public void Init()
		{
			raytracer = new Raytracer();
			
		}
		// tick: renders one frame
		public void Tick()
		{
			raytracer.Camera.Update();
			raytracer.Render();
			for (int x = 0; x < 512; x++)
				for (int y = 0; y < 512; y++)
				{
					Vector3 color = raytracer.colors[x, y];
					screen1.Plot(x, y, RGB(color.X, color.Y, color.Z));
				}
			screen2.Clear(0x0);
			
			for(int x = 0; x < 512; x += 10)
			{
				if (raytracer.rays.Count > x)
				{
					Ray ray = raytracer.rays[x];

					Vector3 point2;
					point2 = ( ray.Direction * Math.Min(ray.Distance, 100) ) + ray.Origin;
					screen2.Line(TX(ray.Origin.X), TY(ray.Origin.Z), ( TX(point2.X) ), ( TY(point2.Z) ), RGB(1, 1, 1));

					for (int i = 0; i < raytracer.shadowrays.Count; i += 10)
					{
						Ray shadowRay = raytracer.shadowrays[i];
						if (shadowRay != null)
						{
							Vector3 point2S = ( shadowRay.Direction * Math.Min(shadowRay.Distance, 100) ) + shadowRay.Origin;
							screen2.Line(( TX(point2S.X) ), ( TY(point2S.Z) ), TX(shadowRay.Origin.X), TY(shadowRay.Origin.Z), RGB(1, 0, 0));
						}
					}

					for (int i = 0; i < raytracer.reflectray.Count; i += 10)
					{
						Ray reflectRay = raytracer.reflectray[i];
						if (reflectRay != null)
						{
							Vector3 point2S = ( reflectRay.Direction * Math.Min(reflectRay.Distance, 100) ) + reflectRay.Origin;
							screen2.Line(( TX(point2S.X) ), ( TY(point2S.Z) ), TX(reflectRay.Origin.X), TY(reflectRay.Origin.Z), RGB(0, 0.5f, 1));
						}
					}
				}
			}
			List<Primitive> primitives = raytracer.Scene.Primitives;
			foreach(Primitive primitive in primitives)
			{
				if (primitive.GetType() == typeof(Sphere))
				{
					Sphere temp = (Sphere) primitive;
					screen2.Circle(TX(temp.Position.X), TY(temp.Position.Z), (int)(temp.Radius * 51.2 +1 ), RGB(temp.Material.Color));
				}
			}
			Screen traceScreen = raytracer.Camera.Screen;
			int x11 = TX(traceScreen.p2.X);
			int y11 = TY(traceScreen.p2.Z);
			int x12 = TX(traceScreen.p3.X);
			int y12 = TY(traceScreen.p3.Z);

			screen2.Line(x11, y11 - 1, x12, y12 - 1, RGB(1, 0, 1));
			screen2.Line(x11, y11, x12, y12, RGB(1, 0, 1));
			screen2.Line(x11, y11 + 1, x12, y12 + 1, RGB(1, 0, 1));
		}

		public void Controls(KeyboardState keys)
		{
			currentKeyState = keys;
			if (NewKeyPress(Key.Up))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.Direction = Vector3.Normalize(raytracer.Camera.Direction + new Vector3(0, raytracer.Camera.Direction.Z, -raytracer.Camera.Direction.X) * ( (float) Math.PI / 180 ));
			}
			if (NewKeyPress(Key.Down))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.Direction = Vector3.Normalize(raytracer.Camera.Direction - new Vector3(0, raytracer.Camera.Direction.Z, -raytracer.Camera.Direction.X) * ( (float) Math.PI / 180 ));
			}
			if (NewKeyPress(Key.Left))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.Direction = Vector3.Normalize(raytracer.Camera.Direction - new Vector3(raytracer.Camera.Direction.Z, 0, -raytracer.Camera.Direction.X) * ( (float) Math.PI / 180 ));
			}
			if (NewKeyPress(Key.Right))
			{
				raytracer.smoothdraw = false;
				raytracer.Camera.Direction = Vector3.Normalize(raytracer.Camera.Direction + new Vector3(raytracer.Camera.Direction.Z, 0, -raytracer.Camera.Direction.X) * ( (float) Math.PI / 180 ));
			}
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
			if (NewKeyPress(Key.Space))
			{
				raytracer.smoothdraw = true;
			}
			if (NewKeyPress(Key.T))
			{
				raytracer.doaa = true;
			}
			if (NewKeyPress(Key.Y))
			{
				raytracer.doaa = false;
			}
		}

		public float rotateX(float x, float y, float a)
		{
			float rx = (float) ( x * Math.Cos(a) - y * Math.Sin(a) );
			return rx;
		}

		//returns the rotated y value of the given point a degrees
		public float rotateY(float x, float y, float a)
		{
			float ry = (float) ( x * Math.Sin(a) + y * Math.Cos(a) );
			return ry;
		}

		public bool NewKeyPress(Key key)
		{
			return ( currentKeyState[key] && ( currentKeyState[key] != prevKeyState[key] ) );
		}

		//convert given x value to screen coordinates
		public int TX(float x)
		{
			x += origX;
			x += scaleX / 2;
			x *= screen2.width / scaleX;
			return (int) x;
		}

		//convert given y value to screen coordinates
		public int TY(float y)
		{
			y += origY;
			y += scaleY / 2;
			y *= screen2.height / scaleY;
			y = screen2.height - y;
			return (int) y;
		}

		int RGB (float r, float g, float b)
		{
			int rint = (int) (Math.Min(1, r) * 255);
			int gint = (int) (Math.Min(1, g) * 255);
			int bint = (int) (Math.Min(1, b) * 255);
			return (rint << 16) + (gint << 8) + (bint);
		}

		int RGB( Vector3 color)
		{
			int rint = (int) ( Math.Min(1, color.X) * 255 );
			int gint = (int) ( Math.Min(1, color.Y) * 255 );
			int bint = (int) ( Math.Min(1, color.Z) * 255 );
			return ( rint << 16 ) + ( gint << 8 ) + ( bint );
		}
	}

} // namespace Template