﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
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
		public Vector3[,] colors1 = new Vector3[256, 256], colors2 = new Vector3[256, 256], colors3 = new Vector3[256, 256], colors4 = new Vector3[256, 256], colors = new Vector3[512,512];
		public List<Ray> rays = new List<Ray>(), shadowrays = new List<Ray>(), rays1 = new List<Ray>(), rays2 = new List<Ray>(), shadowrays1 = new List<Ray>(), shadowrays2 = new List<Ray>();
		public bool smoothdraw = true;
		int recursion = 4, antialiasing;
		float aasqrt;
		bool doaa = false;

		public int Antialiasing
		{
			get {
				return antialiasing;
			}
			set { antialiasing = value; aasqrt = (float)Math.Sqrt(value); }
		}


		public Raytracer()
		{
			Scene = new Scene();
			Antialiasing = 16;
			//Camera = new Camera();
			Camera = new Camera(new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, 1f));
			DoRaysNStuff();
		}

		public void Render()
		{
			DoRaysNStuff();
		}

		Thread t1, t2, t3, t4;

		void DoRaysNStuff()
		{
			rays1.Clear();
			rays2.Clear();
			rays.Clear();
			shadowrays1.Clear();
			shadowrays2.Clear();
			shadowrays.Clear();
			t1 = new Thread(q1);
			t2 = new Thread(q2);
			t3 = new Thread(q3);
			t4 = new Thread(q4);
			t1.Start();
			t2.Start();
			t3.Start();
			t4.Start();
			t1.Join();
			t2.Join();
			t3.Join();
			t4.Join();
			rays.AddRange(rays1);
			rays.AddRange(rays2);
			shadowrays.AddRange(shadowrays1);
			shadowrays.AddRange(shadowrays2);
			for (int x = 0; x < 256; x++)
				for(int y = 0; y < 256; y++)
				{
					colors[x, y] = colors1[x, y];
				}
			for (int x = 256; x < 512; x++)
				for (int y = 0; y < 256; y++)
				{
					colors[x, y] = colors2[x-256, y];
				}
			for (int x = 0; x < 256; x++)
				for (int y = 256; y < 512; y++)
				{
					colors[x, y] = colors3[x, y-256];
				}
			for (int x = 256; x < 512; x++)
				for (int y = 256; y < 512; y++)
				{
					colors[x, y] = colors4[x-256, y-256];
				}

		}

		void q1()
		{
			Ray ray;
			for (float x = 0; x < 256; x++)
				for (float y = 0; y < 256; y++)
				{
					colors1[(int) x, (int) y] = new Vector3();
					if ((( ( x % 2 == 0 && y % 2 == 1 ) || ( x % 2 == 1 && y % 2 == 0 ) ) && !smoothdraw) || (smoothdraw && !doaa))
					{
						ray = new Ray();
						ray.Origin = Camera.Position;
						//ray.Direction = Vector3.Normalize(new Vector3(Camera.Screen.p2.X + ((Camera.Screen.p) / 512f) * x, Camera.Screen.p2.Y - (2f / 512f) * y, Camera.Screen.p2.Z) - ray.Origin);
						Vector3 xscreen, yscreen;
						xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
						yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
						Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x ) / 256 ) + yscreen * ( ( y ) / 256 );

						ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
						colors1[(int) x, (int) y] = Trace(ray, (int) x, (int) y);

					}
					else if (smoothdraw && doaa)
					{
						Vector3[,] avg = new Vector3[(int)aasqrt, (int)aasqrt];
						for(float i = 0; i < aasqrt; i++)
						{
							for (float j = 0; j < aasqrt; j++)
							{
								ray = new Ray();
								ray.Origin = Camera.Position;
								//ray.Direction = Vector3.Normalize(new Vector3(Camera.Screen.p2.X + ((Camera.Screen.p) / 512f) * x, Camera.Screen.p2.Y - (2f / 512f) * y, Camera.Screen.p2.Z) - ray.Origin);
								Vector3 xscreen, yscreen;
								xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
								yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
								Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + i / aasqrt ) / 256 ) + yscreen * ( ( y + j / aasqrt ) / 256 );

								ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
								avg[(int)i,(int)j] = Trace(ray, (int) x, (int) y);
							}
						}
						Vector3 final = new Vector3(0,0,0);
						for (int i = 0; i < aasqrt; i++)
						{
							for (int j = 0; j < aasqrt; j++)
							{
								final += avg[i, j];
							}
						}
						final /= Antialiasing;
						colors1[(int) x, (int) y] = final;
					}
				}
		}
		void q2()
		{
			Ray ray;
			for (float x = 0; x < 256; x++)
				for (float y = 0; y < 256; y++)
				{
					colors2[(int) x, (int) y] = new Vector3();
					if (( ( ( x % 2 == 0 && y % 2 == 1 ) || ( x % 2 == 1 && y % 2 == 0 ) ) && !smoothdraw ) || ( smoothdraw && !doaa ))
					{
						ray = new Ray();
						ray.Origin = Camera.Position;
						//ray.Direction = Vector3.Normalize(new Vector3(Camera.Screen.p2.X + ((Camera.Screen.p) / 512f) * x, Camera.Screen.p2.Y - (2f / 512f) * y, Camera.Screen.p2.Z) - ray.Origin);
						Vector3 xscreen, yscreen;
						xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
						yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
						Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + 256 ) / 256 ) + yscreen * ( ( y ) / 256 );

						ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
						colors2[(int) x, (int) y] = Trace(ray, (int) x, (int) y);
						
					}
					else if (smoothdraw && doaa)
					{
						Vector3[,] avg = new Vector3[(int) aasqrt, (int) aasqrt];
						for (float i = 0; i < aasqrt; i++)
						{
							for (float j = 0; j < aasqrt; j++)
							{
								ray = new Ray();
								ray.Origin = Camera.Position;
								//ray.Direction = Vector3.Normalize(new Vector3(Camera.Screen.p2.X + ((Camera.Screen.p) / 512f) * x, Camera.Screen.p2.Y - (2f / 512f) * y, Camera.Screen.p2.Z) - ray.Origin);
								Vector3 xscreen, yscreen;
								xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
								yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
								Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + 256 + i / aasqrt ) / 256 ) + yscreen * ( ( y + j / aasqrt ) / 256 );

								ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
								avg[(int) i, (int) j] = Trace(ray, (int) x, (int) y);
							}
						}
						Vector3 final = new Vector3(0, 0, 0);
						for (int i = 0; i < aasqrt; i++)
						{
							for (int j = 0; j < aasqrt; j++)
							{
								final += avg[i, j];
							}
						}
						final /= Antialiasing;
						colors2[(int) x, (int) y] = final;
					}
				}
		}
		void q3()
		{
			Ray ray;
			for (float x = 0; x < 256; x++)
				for (float y = 0; y < 256; y++)
				{
					colors3[(int) x, (int) y] = new Vector3();
					if (( ( ( x % 2 == 0 && y % 2 == 1 ) || ( x % 2 == 1 && y % 2 == 0 ) ) && !smoothdraw ) || ( smoothdraw && !doaa ))
					{
						ray = new Ray();
						ray.Origin = Camera.Position;
						//ray.Direction = Vector3.Normalize(new Vector3(Camera.Screen.p2.X + ((Camera.Screen.p) / 512f) * x, Camera.Screen.p2.Y - (2f / 512f) * y, Camera.Screen.p2.Z) - ray.Origin);
						Vector3 xscreen, yscreen;
						xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
						yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
						Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x  ) / 256 ) + yscreen * ( ( y+256 ) / 256 );

						ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
						if (y == 0)
						{
							rays1.Add(ray);
							colors3[(int) x, (int) y] = Trace(ray, (int) x, (int) 257);
						}
						else
							colors3[(int) x, (int) y] = Trace(ray, (int) x, (int) y);
					}
					else if(smoothdraw && doaa)
					{
						Vector3[,] avg = new Vector3[(int) aasqrt, (int) aasqrt];
						for (float i = 0; i < aasqrt; i++)
						{
							for (float j = 0; j < aasqrt; j++)
							{
								ray = new Ray();
								ray.Origin = Camera.Position;
								//ray.Direction = Vector3.Normalize(new Vector3(Camera.Screen.p2.X + ((Camera.Screen.p) / 512f) * x, Camera.Screen.p2.Y - (2f / 512f) * y, Camera.Screen.p2.Z) - ray.Origin);
								Vector3 xscreen, yscreen;
								xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
								yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
								Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + i / aasqrt ) / 256 ) + yscreen * ( ( y + 256 + j / aasqrt ) / 256 );

								ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
								avg[(int) i, (int) j] = Trace(ray, (int) x, (int) y);
							}
						}
						Vector3 final = new Vector3(0, 0, 0);
						for (int i = 0; i < aasqrt; i++)
						{
							for (int j = 0; j < aasqrt; j++)
							{
								final += avg[i, j];
							}
						}
						final /= Antialiasing;
						colors3[(int) x, (int) y] = final;
					}
				}
		}
		void q4()
		{
			Ray ray;
			for (float x = 0; x < 256; x++)
				for (float y = 0; y < 256; y++)
				{
					colors4[(int) x, (int) y] = new Vector3();
					if (( ( ( x % 2 == 0 && y % 2 == 1 ) || ( x % 2 == 1 && y % 2 == 0 ) ) && !smoothdraw ) || ( smoothdraw && !doaa ))
					{
						ray = new Ray();
						ray.Origin = Camera.Position;
						//ray.Direction = Vector3.Normalize(new Vector3(Camera.Screen.p2.X + ((Camera.Screen.p) / 512f) * x, Camera.Screen.p2.Y - (2f / 512f) * y, Camera.Screen.p2.Z) - ray.Origin);
						Vector3 xscreen, yscreen;
						xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
						yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
						Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + 256 ) / 256 ) + yscreen * ( ( y+256 ) / 256 );

						ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
						
						if (y == 0)
						{
							rays2.Add(ray);
							colors4[(int) x, (int) y] = Trace(ray, (int) x, (int) 256);
						}
						else
							colors4[(int) x, (int) y] = Trace(ray, (int) x, (int) y);
					}
					else if (smoothdraw && doaa)
					{
						Vector3[,] avg = new Vector3[(int) aasqrt, (int) aasqrt];
						for (float i = 0; i < aasqrt; i++)
						{
							for (float j = 0; j < aasqrt; j++)
							{
								ray = new Ray();
								ray.Origin = Camera.Position;
								//ray.Direction = Vector3.Normalize(new Vector3(Camera.Screen.p2.X + ((Camera.Screen.p) / 512f) * x, Camera.Screen.p2.Y - (2f / 512f) * y, Camera.Screen.p2.Z) - ray.Origin);
								Vector3 xscreen, yscreen;
								xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
								yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
								Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + 256 + i / aasqrt ) / 256 ) + yscreen * ( ( y + 256 + j / aasqrt ) / 256 );

								ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
								avg[(int) i, (int) j] = Trace(ray, (int) x, (int) y);
							}
						}
						Vector3 final = new Vector3(0, 0, 0);
						for (int i = 0; i < aasqrt; i++)
						{
							for (int j = 0; j < aasqrt; j++)
							{
								final += avg[i, j];
							}
						}
						final /= Antialiasing;
						colors4[(int) x, (int) y] = final;
					}
				}

		}

		Vector3 Trace(Ray ray, int x, int y)
		{
			Intersection intersect = Scene.NearestIntersect(ray);
			if (intersect != null)
			{
				Vector3 illumination = Illumination(intersect, x, y);
				return intersect.Primitive.Color * illumination;
			}
			else
				return new Vector3(0, 0, 0);
		}

		Vector3 Illumination(Intersection intersection, int x, int saveshadow)
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
					if (saveshadow == 256)
						shadowrays1.Add(shadowRay);
					if (saveshadow == 257)
						shadowrays2.Add(shadowRay);
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
				p0 = new Vector3(-1f, 1f, 0f),
				p1 = new Vector3(1f, 1f, 0f),
				p2 = new Vector3(-1f, -1f, 0f),
				p3 = new Vector3(1f, -1f, 0f)
			};

		}

		public Camera(Vector3 position, Vector3 direction, float screenDistance = 1)
		{
			Position = position;
			Direction = direction;
			ScreenDistance = screenDistance;
			CreateScreen();
			
		}

		public void Update()
		{
			CreateScreen();
		}
		
		void CreateScreen()
		{
			Screen = new Screen();
			Vector3 perp = Vector3.Normalize(Vector3.Cross(Direction, new Vector3(0,1,0)));
			Vector3 perpz = Vector3.Normalize(Vector3.Cross(Direction, new Vector3(1,0,0)));
			Screen.p0 = Position + Direction * ScreenDistance + perp + perpz;
			Screen.p1 = Position + Direction * ScreenDistance - perp + perpz;
			Screen.p2 = Position + Direction * ScreenDistance + perp - perpz;
			Screen.p3 = Position + Direction * ScreenDistance - perp - perpz;
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
			LightSources.Add(new LightSource { Intensity = new Vector3(7f,7f,8f), Position = new Vector3( -1f,1.5f, -1f) });
			//LightSources.Add(new LightSource { Intensity = new Vector3(1, 1, 10), Position = new Vector3(0, 6,  8f) });
			//LightSources.Add(new LightSource { Intensity = new Vector3(10, 1, 10), Position = new Vector3(-2, 2, 8f) });
			//LightSources.Add(new LightSource { Intensity = new Vector3(1, 10, 10), Position = new Vector3(2, 2, 8f) });
			Primitives.Add(new Plane(new Vector3(0f,  -1.5f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1,1,1)));
			Primitives.Add(new Plane(new Vector3(0f, 0f, 7f), new Vector3(0f, 0f, -1f), new Vector3(1, 0, 1)));
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
				if (temp != null && temp.Distance < ray.Distance)
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
