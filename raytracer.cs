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
		public List<Ray> rays = new List<Ray>(), shadowrays = new List<Ray>();

		public Raytracer()
		{
			Scene = new Scene();
			//Camera = new Camera();
			Camera = new Camera(new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, 1f));
			DoRayNStuff();
		}

		public void Render()
		{

		}

		void DoRayNStuff()
		{
			Ray ray;
			for (int x = 0; x < 512; x++)
				for (int y = 0; y < 512; y++)
				{
					ray = new Ray();
					ray.Origin = Camera.Position;
					ray.Direction = Vector3.Normalize(new Vector3(Camera.Screen.p2.X + (2f / 512f) * x, Camera.Screen.p2.Y - (2f / 512f) * y, Camera.Screen.p2.Z) - ray.Origin);
					colors[x, y] = Trace(ray, x, y);
					if (y == 256)
					{
						rays.Add( ray);
					}
				}
		}

		Vector3 Trace(Ray ray, int x, int y)
		{
			Intersection intersect = Scene.NearestIntersect(ray);
			if (intersect != null)
			{
				Vector3 illumination = Illumination(intersect, x, y == 256);
				return intersect.Primitive.Color * illumination;
			}
			else
				return new Vector3(0, 0, 0);
		}

		Vector3 Illumination(Intersection intersection, int x, bool saveshadow)
		{
			Vector3 shadows = new Vector3(0, 0, 0);
			
			for (int i = 0; i < Scene.LightSources.Count; i++)
			{
				Vector3 I = intersection.IntersectionPoint;
				Vector3 L = Scene.LightSources[i].Position - I;
				Vector3 N = intersection.IntersectionNormal;
				
				float dist =  L.Length;
				L /= dist;
				if (Vector3.Dot(N, L) > 0)
				{
					Ray shadowRay = new Ray { Origin = I, Direction = L, Distance = dist };


					Intersection result = Scene.FirstIntersect(shadowRay);
					if (result == null)
					{
						float attenuation = 1 / ( dist * dist );
						shadows += Scene.LightSources[i].Intensity * Vector3.Dot(N, L) * attenuation;
					}
					else
					{
						shadowRay.Distance = result.Distance;
					}
					if (saveshadow)
						shadowrays.Add(shadowRay);
				}
			}
			return shadows;
		}
	}

	public class Camera
	{
		public Vector3 Position { get; set; }
		public Vector3 Direction { get; set; }
		public Screen Screen;
		public float ScreenDistance;

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

		public Camera(Vector3 position, Vector3 direction, float screenDistance = 1)
		{
			Position = position;
			Direction = direction;
			ScreenDistance = screenDistance;

			Screen = new Screen
			{
				PosX1 = Position.X - 1,
				PosX2 = Position.X + 1,
				PosY1 = Position.Y - 1,
				PosY2 = Position.Y + 1,
				PosZ1 = Position.Z,
				PosZ2 = Position.Z

				/*p0 = new Vector3(Position.X - 1, Position.Y -1, Position.Z + 1),
				p1 = new Vector3(Position.X + 1, Position.Y -1, Position.Z + 1),
				p2 = new Vector3(Position.X - 1, Position.Y + 1, Position.Z + 1),
				p3 = new Vector3(Position.X + 1, Position.Y + 1, Position.Z + 1)*/
			};
			Screen.p0 = new Vector3(Screen.PosX1, Screen.PosY1, Screen.PosZ1) + ScreenDistance * Direction;
			Screen.p1 = new Vector3(Screen.PosX2, Screen.PosY1, Screen.PosZ2) + ScreenDistance * Direction;
			Screen.p2 = new Vector3(Screen.PosX1, Screen.PosY2, Screen.PosZ1) + ScreenDistance * Direction;
			Screen.p3 = new Vector3(Screen.PosX2, Screen.PosY2, Screen.PosZ2) + ScreenDistance * Direction;
		}
	}

	public class Screen
	{
		public Vector3 p0, p1, p2, p3;
		public float PosX1, PosX2, PosY1, PosY2, PosZ1, PosZ2;
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
			LightSources.Add(new LightSource { Intensity = new Vector3(7f,7f,8f), Position = new Vector3( -1f, 1.5f, -1f) });
			//LightSources.Add(new LightSource { Intensity = new Vector3(1, 1, 10), Position = new Vector3(0, 6,  8f) });
			//LightSources.Add(new LightSource { Intensity = new Vector3(10, 1, 10), Position = new Vector3(-2, 2, 8f) });
			//LightSources.Add(new LightSource { Intensity = new Vector3(1, 10, 10), Position = new Vector3(2, 2, 8f) });
			Primitives.Add(new Plane(new Vector3(0f,  -1.5f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1,1,1)));
			//Primitives.Add(new Plane(new Vector3(0f, 0f, 7f), new Vector3(0f, 0f, -1f), new Vector3(1, 0, 1)));
			Primitives.Add(new Sphere(new Vector3(-3f, 0f,5f), 1.5f, new Vector3(1,0.1f,0.1f)));
			Primitives.Add(new Sphere(new Vector3(0f, 0f, 3f), 1.5f, new Vector3(0.1f,1,0.1f)));
			Primitives.Add(new Sphere(new Vector3(3f, 0f, 5f), 1.5f, new Vector3(0.1f,0.1f,1)));
		}

		public Intersection NearestIntersect(Ray ray)
		{
			Intersection result = null;
			foreach(Primitive primitive in Primitives)
			{
				Intersection temp = primitive.Intersect(ray);
				if (temp != null)
				{
					if (temp.Distance > 0)
					{
						if (result != null)
						{
							if (temp.Distance < ray.Distance)
							{
								result = temp;
								ray.Distance = result.Distance;

							}
						}
						else
						{
							result = temp;
							ray.Distance = temp.Distance;
						}
					}
				}

			}
			
			return result;
		}

		public Intersection FirstIntersect(Ray ray)
		{
			Intersection result = null;
			foreach (Primitive primitive in Primitives)
			{
				Intersection temp = primitive.Intersect(ray);
				if (temp != null)
				{
					return temp;
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
