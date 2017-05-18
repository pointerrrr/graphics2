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
				Ray ray = raytracer.rays[x];
				Vector3 point2 = ray.Direction * Math.Min( ray.Distance, 20);
				screen2.Line(256, 512, (int)(point2.X * 255) + 256, 512 - (int)(point2.Y  * 255), RGB(1, 1, 1));
			}
			List<Primitive> primitives = raytracer.Scene.Primitives;
			foreach(Primitive primitive in primitives)
			{
				if (primitive.GetType() == typeof(Sphere))
				{
					Sphere temp = (Sphere) primitive;
					screen2.Circle(256 +(int)(temp.Position.X * 51.2), 512 - (int)(temp.Position.Z * 51.2), (int)(temp.Radius * 51.2) , RGB(temp.Color));
				}
			}
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