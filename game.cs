using System;
using System.IO;
using OpenTK;
using System.Collections.Generic;

namespace Template {

	class Game
	{
		// member variables
		public Surface screen1, screen2;

		private Raytracer raytracer;
		float scaleX = 10.0f;
		float scaleY = 10.0f;
		//location of the center
		float origX = 0.0f, origY = -4f;
		//current rotation of the square


		// initialize
		public void Init()
		{
			raytracer = new Raytracer();
			
		}

		// tick: renders one frame
		public void Tick()
		{
			for (int x = 0; x < 512; x++)
				for (int y = 0; y < 512; y++)
				{
					Vector3 color = raytracer.colors[x, y];
					screen1.Plot(x, y, RGB(color.X, color.Y, color.Z));
				}
			for(int x = 0; x < 512; x += 10)
			{
				if (x > 40)
					;
				Ray ray = raytracer.rays[x];
				Vector3 point2 =  (ray.Direction * Math.Min( ray.Distance, 100)) + ray.Origin;
				screen2.Line(256, 512, (TX(point2.X)),  (TY(point2.Z)), RGB(1, 1, 1));
			}
			List<Primitive> primitives = raytracer.Scene.Primitives;
			foreach(Primitive primitive in primitives)
			{
				if (primitive.GetType() == typeof(Sphere))
				{
					Sphere temp = (Sphere) primitive;
					screen2.Circle(TX(temp.Position.X), TY(temp.Position.Z), (int)(temp.Radius * 51.2 ), RGB(temp.Color));
				}
			}
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