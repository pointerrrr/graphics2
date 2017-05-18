using System;
using System.IO;
using OpenTK;
using System.Collections.Generic;

namespace Template {

	class Game
	{
		// member variables
		public Surface screen;
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
					screen.Plot(x, y, RGB(color.X, color.Y, color.Z));
				}
			for(int x = 0; x < 512; x += 10)
			{
				Ray ray = raytracer.rays[x];
				screen.Line(256 + 512, 512, (int)(ray.Direction.X * 255) + 512 + 256, 512 - (int)(ray.Direction.Y  * 255), RGB(1, 1, 1));
			}
			List<Primitive> primitives = raytracer.Scene.Primitives;
			foreach(Primitive primitive in primitives)
			{
				if (primitive.GetType() == typeof(Sphere))
				{
					Sphere temp = (Sphere) primitive;
					screen.Circle(512 + 256 +(int)(temp.Position.X * 64), 256 + (int)(temp.Position.Y * 64), (int)(temp.Radius * 64) , RGB(temp.Color));
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