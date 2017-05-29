using System;
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
		public Skybox Skydome { get; set; }
		public Vector3[,] colors1 = new Vector3[256, 256], colors2 = new Vector3[256, 256], colors3 = new Vector3[256, 256], colors4 = new Vector3[256, 256], colors = new Vector3[512,512];
		public List<Ray> rays = new List<Ray>(), shadowrays = new List<Ray>(), rays1 = new List<Ray>(), rays2 = new List<Ray>(), shadowrays1 = new List<Ray>(), shadowrays2 = new List<Ray>(), reflectray = new List<Ray>(), reflect1 = new List<Ray>(), reflect2 = new List<Ray>();
		public float FOV = 40;
		public bool smoothdraw = true;
		int maxrecursion = 4, antialiasing;
		float aasqrt;
		public bool doaa = false;

		public int Antialiasing
		{
			get {
				return antialiasing;
			}
			set {
				antialiasing = value; aasqrt = (float)Math.Sqrt(value);
			}
		}


		public Raytracer(float fov = 90)
		{
			FOV = fov;
			Scene = new Scene();
			Antialiasing = 16;
			//Camera = new Camera();
			Camera = new Camera(new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, 1f), fov);
			Skydome = new Skybox("../../assets/stpeters_probe.jpg");
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
			reflect1.Clear();
			reflect2.Clear();
			reflectray.Clear();
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
			reflectray.AddRange(reflect1);
			reflectray.AddRange(reflect2);
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
						colors1[(int) x, (int) y] = Trace(ray);

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
								avg[(int)i,(int)j] = Trace(ray);
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
						colors2[(int) x, (int) y] = Trace(ray);
						
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
								avg[(int) i, (int) j] = Trace(ray);
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
							colors3[(int) x, (int) y] = Trace(ray, 0, 1, 1);
						}
						else
							colors3[(int) x, (int) y] = Trace(ray);
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
								avg[(int) i, (int) j] = Trace(ray, (y == 0 && i == 0 && j == 0) ? 1 : 0);
								if (i == 0 && j == 0 && y == 0)
									rays1.Add(ray);
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
							colors4[(int) x, (int) y] = Trace(ray, 0, 2, 2);
						}
						else
							colors4[(int) x, (int) y] = Trace(ray);
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
								avg[(int) i, (int) j] = Trace(ray, ( y == 0 && i == 0 && j == 0 ) ? 2 : 0);
								if (i == 0 && j == 0 && y == 0)
									rays2.Add(ray);
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

		Vector3 Trace(Ray ray, int recursion = 0, int shadow = 0, int recurse = 0)
		{
			Intersection intersect = Scene.NearestIntersect(ray);
			if (intersect != null)
			{
				if(intersect.Primitive.Material.Reflect)
				{
					Ray newray = new Ray();
					
					newray.Direction = Reflect(ray.Direction, intersect.IntersectionNormal);
					newray.Origin = intersect.IntersectionPoint + newray.Direction * 0.01f;
					if (recurse == 1)
						reflect1.Add(newray);
					if (recurse == 2)
						reflect2.Add(newray);
					if (recursion++ < maxrecursion)
						return intersect.Primitive.Material.Color * Trace(newray, recursion);
					else
						return new Vector3(intersect.Primitive.Material.Color);
				}
				else
				{
					Vector3 illumination = Illumination(intersect, shadow);
					if (intersect.Primitive.Material.Texture == null)
					{
						return intersect.Primitive.Material.Color * illumination;
					}
					else
					{
						return intersect.Primitive.GetTexture(intersect) * illumination;
						
					}
				}
				
			}
			else
				return Skybox(ray);
		}

		public Vector3 Reflect(Vector3 direction, Vector3 normal)
		{
			return direction - 2 * Vector3.Dot(direction, normal) * normal;
		}

		Vector3 Illumination(Intersection intersection, int shadow = 0)
		{
			Vector3 shadows = new Vector3(0, 0, 0);
			
			for (int i = 0; i < Scene.LightSources.Count; i++)
			{
				LightSource lightSource = Scene.LightSources[i];
				Vector3 I = intersection.IntersectionPoint;
				Vector3 L = lightSource.Position - I;
				Vector3 N = intersection.IntersectionNormal;
				
				float dist =  L.Length;
				L /= dist;
				float NormalDot = Vector3.Dot(N, L);
				if (NormalDot > 0)
				{
					if (lightSource.GetType() == typeof(PointLight))
					{
						Ray shadowRay = new Ray { Origin = I, Direction = L, Distance = dist };

						Intersection result = Scene.FirstIntersect(shadowRay);
						if (result == null)
						{
							float attenuation = 1f / (dist * dist);
							if (lightSource.GetType() == typeof(Spotlight))
							{
								Spotlight light = (Spotlight)lightSource;
								float dot = -Vector3.Dot(light.Direction, L);
								if (dot >= light.Dot && dot > 0)
									shadows += light.Intensity * NormalDot * attenuation;
							}
							else
								shadows += lightSource.Intensity * NormalDot * attenuation;
						}
						else
						{
							shadowRay.Distance = result.Distance;
						}
						if (shadow == 1)
							shadowrays1.Add(shadowRay);
						if (shadow == 2)
							shadowrays2.Add(shadowRay);
					}

					else if (lightSource.GetType() == typeof(AreaLight))
					{
						AreaLight light = (AreaLight)lightSource;
						Vector3 Color = new Vector3(0, 0, 0);

						float yfactor = light.Area.Height / 3;
						float xfactor = light.Area.Width / 3;

						float attenuation = 1f / (dist * dist);
						for (int x = 0; x < 3; x++)
							for (int y = 0; y < 3; y++)
							{
								Vector3 lightPoint = new Vector3(light.Area.p0.X + x * xfactor, light.Area.p0.Y, light.Area.p0.Z + y * yfactor);
								Vector3 direction = Vector3.Normalize(lightPoint - I);
								Ray areaShadowRay = new Ray { Origin = I, Direction = direction, Distance = dist };
								Intersection result = Scene.FirstIntersect(areaShadowRay);
								if (result == null)
								{
									Color += (light.Intensity / 9);
								}
								else
									areaShadowRay.Distance = result.Distance;

								if (shadow == 1)
									shadowrays1.Add(areaShadowRay);
								if (shadow == 2)
									shadowrays2.Add(areaShadowRay);
							}
						shadows += Color * attenuation;
					}
				}
			}
			// minimum light level of 0.3f
			/*if (shadows.X < 0.3f)
				shadows.X = 0.3f;
			if (shadows.Y < 0.3f)
				shadows.Y = 0.3f;
			if (shadows.Z < 0.3f)
				shadows.Z = 0.3f;*/
			return shadows;
		}

		public Vector3 Skybox(Ray ray)
		{
			Vector3 d = -ray.Direction;
			float r = (float) ( ( 1d / Math.PI ) * Math.Acos(d.Z) / Math.Sqrt(d.X * d.X + d.Y * d.Y) );
			float u = r*d.X+1;
			float v = r*d.Y+1;
			
			int iu = (int) ( u * Skydome.Texture.Image.GetLength(0)  / 2) ;
			int iv = (int) ( v * Skydome.Texture.Image.GetLength(1) / 2 );
			if (iu >= Skydome.Texture.Image.GetLength(0))
				iu = 0;
			if (iu < 0)
				iu = 0;
			if (iv >= Skydome.Texture.Image.GetLength(1))
				iv = 0;
			if (iv < 0)
				iv = 0;
			return Skydome.Texture.Image[iu, iv];
		}
	}

	public class Camera
	{
		public Vector3 Position { get; set; }
		public Vector3 Direction { get; set; }
		public float x = 0, z = 90;
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

		public Camera(Vector3 position, Vector3 direction, float fov = 90)
		{
			float distance = 1 / (float)Math.Tan((fov * (Math.PI / 180)) / 2);
			Position = position;
			Direction = direction;
			ScreenDistance = distance;
			CreateScreen();
			
		}


		public Camera(Vector3 position, float x, float z, float fov = 90)
		{
			float distance = 1 / (float) Math.Tan(( fov * ( Math.PI / 180 ) ) / 2);
			Position = position;

			Directionchange(x, z);
			ScreenDistance = distance;
			CreateScreen();

		}

		void Directionchange(float x, float z)
		{
			Direction = new Vector3((float) Math.Sin(x * Math.PI / 180) * (float) Math.Sin(z * Math.PI / 180), (float) Math.Cos(z * Math.PI / 180), (float) Math.Cos(x * Math.PI / 180) * (float) Math.Sin(z * Math.PI / 180));
		}

		public void Update()
		{
			Directionchange(x, z);
			CreateScreen();
		}
		
		void CreateScreen()
		{
			Screen = new Screen();
			Vector3 perp = Vector3.Normalize(Vector3.Cross(Direction, new Vector3(0, 1, 0)));
			Vector3 perpz = -Vector3.Normalize(Vector3.Cross(Direction, perp));
			if (perpz.Y < 0)
				perpz = -perpz;
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

	public class PointLight : LightSource
	{

	}

	public class Spotlight : PointLight
	{
		public Vector3 Direction { get; set; }
		public float Angle;
		public float Dot;

		public Spotlight(Vector3 position, Vector3 intensity, Vector3 direction, float angle)
		{
			Position = position;
			Intensity = intensity;
			Direction = Vector3.Normalize(direction);
			Angle = angle / 2;
			Dot = (float)Math.Cos(Angle / 180 * Math.PI);
		}
	}

	public class AreaLight : LightSource
	{
		public Quad Area { get; set; }
		public AreaLight(Vector3 position, Vector3 direction, float height, float width, Vector3 intensity)
		{
			Area = new Quad(position, direction, height, width);
			Position = position;
			Intensity = intensity;
		}
	}

	public class Quad
	{
		public Vector3 p0, p1, p2, p3, Normal;
		public float Width, Height;
		public Quad(Vector3 position, Vector3 direction, float height, float width)
		{
			Width = width / 2;
			Height = height / 2;
			Vector3 perpx;
			if (direction == new Vector3(0, -1, 0))
				perpx = Vector3.Normalize(Vector3.Cross(direction, new Vector3(0, 0, 1)));
			else
				perpx = Vector3.Normalize(Vector3.Cross(direction, new Vector3(0, 1, 0)));

			Vector3 perpy = -Vector3.Normalize(Vector3.Cross(direction, perpx));
			perpx *= Height;
			perpy *= Width;
			if (perpy.Y < 0)
				perpy = -perpy;
			p0 = position + perpx + perpy;
			p1 = position - perpx + perpy;
			p2 = position + perpx - perpy;
			p3 = position - perpx - perpy;
			Normal = Vector3.Normalize(direction);
		}
	}
	
	public class Scene
	{
		public List<Primitive> Primitives { get; set; }
		public List<LightSource> LightSources { get; set; }

		public Scene()
		{
			Primitives = new List<Primitive>();
			LightSources = new List<LightSource>();
			//LightSources.Add(new PointLight { Intensity = new Vector3(10f,10f,10f), Position = new Vector3( 0f, 0f, 5f) });
			//LightSources.Add(new PointLight { Intensity = new Vector3(10f, 10f, 10f), Position = new Vector3(0f, 0f, -1f) });
			//LightSources.Add(new Spotlight(new Vector3(0, 0, -1), new Vector3(20f, 20f, 15f), new Vector3(0f, 0, 1), 30));
			LightSources.Add(new AreaLight(new Vector3(0, 0, 0), new Vector3(0, -1, 0), 1f, 1f, new Vector3(20f, 20f, 0)));
			Plane bottom = new Plane(new Vector3(0f, -1.5f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1, 1, 1));
			bottom.Material.Texture = new Texture("../../assets/checkers.png");
			Primitives.Add(bottom);
			Primitives.Add(new Sphere(new Vector3(-3f, 0f,5f), 1.5f, new Vector3(1,0.1f,0.1f)));
			Sphere temping = new Sphere(new Vector3(0f, 0f, 3f), 1.5f, new Vector3(0.1f, 1, 0.1f));
			temping.Material.Texture = new Texture("../../assets/uffizi_probe.jpg");
			Primitives.Add(temping);
			Primitives.Add(new Sphere(new Vector3(3f, 0f, 5f), 1.5f, new Vector3(1f, 1f, 1f), true));
			Triangle temp = new Triangle(new Vector3(1, 2, 4), new Vector3(-1, 2, 4), new Vector3(0, 2, 3), new Vector3(1,1,1));
			temp.Material.Texture = new Texture("../../assets/asdf.png");
			Primitives.Add(temp);
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
				if (temp != null && temp.Distance < ray.Distance && temp.Distance > 0.0001f)
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
