using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;

namespace Template
{
	public class Application
	{

	}

	public class Raytracer
	{
		public Scene Scene { get; set; }
		public Camera Camera { get; set; }
		public Vector3[,] colors = new Vector3[512, 512];
		public Ray[] rays = new Ray[512];

		public Raytracer()
		{
			Scene = new Scene();
			Camera = new Camera();
			DoRayNStuff();
		}

		public void Render()
		{

		}

		void DoRayNStuff()
		{
			Ray ray;
			for(int x = 0; x < 512; x++)
				for(int y = 0; y < 512; y++)
				{
					ray = new Ray();
					ray.Origin = Camera.Position;
					ray.Direction = Vector3.Normalize ((ray.Origin - new Vector3(Camera.Screen.p2.X + (2f/512f)*x, Camera.Screen.p2.Y - (2f/512f)*y, 0)));
					colors[x,y] = Trace(ray);
					if(y == 127)
					{
						rays[x] = ray;
					}
				}
		}

		Vector3 Trace(Ray ray)
		{
			Intersection intersect = Scene.NearestIntersect(ray);
			if (intersect != null)				
				return intersect.Primitive.Color;
			else
				return new Vector3(0,0,0);
		}
	}

	public class Camera
	{
		public Vector3 Position { get; set; }
		public Vector3 Direction { get; set; }
		public Screen Screen;

		public Camera()
		{
			Position = new Vector3(0,0,-1);
			Screen = new Screen
			{
				p0 = new Vector3(-1f, -1f, 0f),
				p1 = new Vector3(1f, -1f, 0f),
				p2 = new Vector3(-1f, 1f, 0f),
				p3 = new Vector3(1f, 1f, 0f)
			};

		}

		public Camera(Vector3 position, Vector3 direction)
		{
			Position = position;
			Direction = direction;
		}
	}

	public class Screen
	{
		public Vector3 p0, p1, p2, p3;
	}


	public class Ray
	{
		public Vector3 Direction { get; set; }
		public Vector3 Origin { get; set; }
		public float Distance { get; set; } = 1e34f;

		
	}

	public class LightSource
	{
		public Vector3 Position { get; set; }
		public Vector3 Intensity { get; set; }
	}

	public class Scene
	{
		public List<Primitive> Primitives { get; set; }
		public List<LightSource> LightSources { get; set; }

		public Scene()
		{
			Primitives = new List<Primitive>();
			LightSources = new List<LightSource>();
			Primitives.Add(new Plane(new Vector3(0f, -10f, 0f), new Vector3(0,1,0), new Vector3(1,1,1)));
			Primitives.Add(new Sphere(new Vector3(-3f, 0f,6f), 1.5f, new Vector3(1,0,0)));
			Primitives.Add(new Sphere(new Vector3(0f, 0f, 5f), 1.5f, new Vector3(0,1,0)));
			Primitives.Add(new Sphere(new Vector3(3f, 0f, 4f), 1.5f, new Vector3(0,0,1)));
		}

		public Intersection NearestIntersect(Ray ray)
		{
			Intersection result = null;
			foreach(Primitive primitive in Primitives)
			{
				Intersection temp = primitive.Intersect(ray);
				if (temp != null)
				{
					if (result != null)
					{
						if (temp.Distance < ray.Distance)
						{
							if (temp.Distance != 0)
							{
								result = temp;
								ray.Distance = result.Distance;
							}
						}
					}
					else
					{
						result = temp;

					}
				}

			}
			
			return result;
		}
	}

	public class Intersection
	{
		public Vector3 IntersectionPoint { get; set; }
		public Vector3 IntersectionNormal { get; set; }
		public float Distance { get; set; }
		public Primitive Primitive { get; set; }
	}
}
