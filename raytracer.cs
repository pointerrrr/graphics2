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
		public Ray[] rays = new Ray[512], shadowrays = new Ray[512];

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
			for (int x = 0; x < 512; x++)
				for (int y = 0; y < 512; y++)
				{
					ray = new Ray();
					ray.Origin = Camera.Position;
					ray.Direction = Vector3.Normalize(new Vector3(Camera.Screen.p2.X + (2f / 512f) * x, Camera.Screen.p2.Y - (2f / 512f) * y, 0) - ray.Origin);
					colors[x, y] = Trace(ray, x, y);
					if (y == 256)
					{
						rays[x] = ray;
					}
				}
		}

		Vector3 Trace(Ray ray, int x, int y)
		{
			Intersection intersect = Scene.NearestIntersect(ray);
			if (intersect != null)
			{
				Vector3 illumination = Illumination(intersect, intersect.Primitive.GetNormal(intersect.IntersectionPoint), x, y == 256);
				return intersect.Primitive.Color * illumination;
			}
			else
				return new Vector3(0, 0, 0);
		}

		Vector3 Illumination(Intersection intersection, Vector3 N, int x, bool saveshadow, int shadowsnr = 0)
		{
			Vector3 shadows = new Vector3(0, 0, 0);

			for (int i = 0; i < Scene.LightSources.Count; i++)
			{
				Vector3 I = intersection.IntersectionPoint;
				Vector3 L = Scene.LightSources[i].Position - I;
				float dist = L.Length;
				L /= dist;

				Ray shadowRay = new Ray { Origin = I, Direction = L, Distance = dist };
				if (saveshadow)
					shadowrays[x] = shadowRay;

				if (Scene.FirstIntersect(shadowRay) != null)
					return shadows;
				else
				{
					float attenuation = 1 / (dist * dist);
					shadows += Scene.LightSources[i].Intensity * Vector3.Dot(N, L) * attenuation;
				}
			}
			/*Vector3 shadows = new Vector3(0,0,0);
			for(int i = 0; i < Scene.LightSources.Count; i++)
			{
				Ray shadowray = new Ray();
				shadowray.Distance = (Scene.LightSources[i].Position - intersection.IntersectionPoint).Length;
				shadowray.Direction = Vector3.Normalize(Scene.LightSources[i].Position - intersection.IntersectionPoint);
				shadowray.Origin = intersection.IntersectionPoint + shadowray.Direction * 0.0001f;
				if (Scene.FirstIntersect(shadowray) == null)
				{ if (shadowray.Distance < 0 || shadowray.Distance > ( Scene.LightSources[i].Position - intersection.IntersectionPoint ).Length)
						;
					else
						shadows += Scene.LightSources[i].Intensity / ( shadowray.Distance * shadowray.Distance ) * Vector3.Dot(intersection.IntersectionNormal,  shadowray.Direction);
				}*/

			/* (saveshadow)
				shadowrays[shadowsnr] = shadowray;*/
			/*
							Vector3 I = intersection.IntersectionPoint;
							Vector3 N = intersection.IntersectionNormal;
							Vector3 L = Scene.LightSources[i].Position - I;
							float dist = L.Length;
							L *= ( 1.0f / dist );
							if (Scene.FirstIntersect(new Ray { Origin = I, Direction = L, Distance = dist}) != null) ;

							else
							{

								float attenuation = 1 / ( dist * dist );
								shadows += Scene.LightSources[i].Intensity * Vector3.Dot(N, L) * attenuation;
							}*/


			return shadows;
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
			LightSources.Add(new LightSource { Intensity = new Vector3(10,10,10), Position = new Vector3( 0, 0, 2f) });
			//LightSources.Add(new LightSource { Intensity = new Vector3(10, 10, 10), Position = new Vector3(2, 0, -1f) });
			Primitives.Add(new Plane(new Vector3(0f,  -2f, 0f), new Vector3(0,1,0), new Vector3(1,1,1)));
			Primitives.Add(new Sphere(new Vector3(-3f, 0f,5f), 1.5f, new Vector3(1,0,0)));
			Primitives.Add(new Sphere(new Vector3(0f, 0f, 5f), 1.5f, new Vector3(0,1,0)));
			Primitives.Add(new Sphere(new Vector3(3f, 0f, 5f), 1.5f, new Vector3(0,0,1)));
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
