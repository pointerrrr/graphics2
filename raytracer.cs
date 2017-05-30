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
	// the actual raytracer class
	public class Raytracer
	{
		// the scene
		public Scene Scene { get; set; }
		// the camera
		public Camera Camera { get; set; }
		// the skydome
		public Skybox Skydome { get; set; }
		// 5 lists of colors (1 for the total, 4 for the 4 seperate threads)
		public Vector3[,] colors = new Vector3[512, 512], colors1 = new Vector3[256, 256], colors2 = new Vector3[256, 256], colors3 = new Vector3[256, 256], colors4 = new Vector3[256, 256];
		// list of primary rays, shadow rays and secondary rays
		public List<Ray> rays = new List<Ray>(), shadowrays = new List<Ray>(), rays1 = new List<Ray>(), rays2 = new List<Ray>(), shadowrays1 = new List<Ray>(), shadowrays2 = new List<Ray>(), reflectrays = new List<Ray>(), reflect1 = new List<Ray>(), reflect2 = new List<Ray>();
		// float for field of view (in degrees)
		public float FOV = 90;
		// are we drawing all pixels, or just half for speed
		public bool Smoothdraw = true;
		// are we drawing with anti aliazsing
		public bool DoAA = false;
		// the maximum recursion depth for reflections
		public int MaxRecursion = 16;
		// private int of antialiasing, used for the public property
		private int antialiasing;
		// the sqrt of anti aliasing, used for calculations when using anti aliasing
		private float aasqrt;
		// amount of rays per pixel for anti aliasing
		public int Antialiasing
		{
			get {
				return antialiasing;
			}
			set {
				antialiasing = value; aasqrt = (float)Math.Sqrt(value);
			}
		}
		// 4 seperate threads for concurrent ray tracing
		Thread t1, t2, t3, t4;

		// initialize the raytracer
		public Raytracer(float fov = 90)
		{
			FOV = fov;
			Scene = new Scene();
			Antialiasing = 16;
			Camera = new Camera(new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, 1f), fov);
			Skydome = new Skybox("../../assets/stpeters_probe.jpg");
			Render();
		}

		// render the scene
		public void Render()
		{
			// call all the sub-functions of render
			ClearRays();
			StartThreads();
			JoinThreads();
			MergeRayLists();
			MergeScreen();
		}

		// clear the list of rays
		private void ClearRays()
		{
			rays1.Clear();
			rays2.Clear();
			rays.Clear();
			shadowrays1.Clear();
			shadowrays2.Clear();
			shadowrays.Clear();
			reflect1.Clear();
			reflect2.Clear();
			reflectrays.Clear();
		}

		// start the rendering threads
		private void StartThreads()
		{
			// setup the threads for tracing the rays for screen-quarters
			// upper-left
			t1 = new Thread(q1);
			// upper-right
			t2 = new Thread(q2);
			// lower-left
			t3 = new Thread(q3);
			// lower-right
			t4 = new Thread(q4);
			// start the threads
			t1.Start();
			t2.Start();
			t3.Start();
			t4.Start();
		}

		// join the rendering threads
		private void JoinThreads()
		{
			t1.Join();
			t2.Join();
			t3.Join();
			t4.Join();
		}

		// merge the ray lists to the main ray lists
		private void MergeRayLists()
		{
			// merge rays1 and rays2 into rays for drawing primary rays in debug view
			rays.AddRange(rays1);
			rays.AddRange(rays2);
			// merge shadowrays1 and shadowrays2 into shadowrays for drawing shadow rays in debug view
			shadowrays.AddRange(shadowrays1);
			shadowrays.AddRange(shadowrays2);
			// merge reflect1 and reflect2 into reflectrays for drawing secondary rays in debug view
			reflectrays.AddRange(reflect1);
			reflectrays.AddRange(reflect2);
		}

		// merge the 4 quarters of the screen into one
		private void MergeScreen()
		{
			// upper-left
			for (int x = 0; x < 256; x++)
				for (int y = 0; y < 256; y++)
				{
					colors[x, y] = colors1[x, y];
				}
			// uppper-right
			for (int x = 256; x < 512; x++)
				for (int y = 0; y < 256; y++)
				{
					colors[x, y] = colors2[x - 256, y];
				}
			// lower-left
			for (int x = 0; x < 256; x++)
				for (int y = 256; y < 512; y++)
				{
					colors[x, y] = colors3[x, y - 256];
				}
			// lower-right
			for (int x = 256; x < 512; x++)
				for (int y = 256; y < 512; y++)
				{
					colors[x, y] = colors4[x - 256, y - 256];
				}
		}

		// trace upper-left
		private void q1()
		{
			// ray used for tracing
			Ray ray;
			// trace a 256x256 area
			for (float x = 0; x < 256; x++)
				for (float y = 0; y < 256; y++)
				{
					// clear the current pixel
					colors1[(int) x, (int) y] = new Vector3();
					// trace the current pixel if aa is disabled, or if smoothdraw is disabled (but only half of them then)
					if ((( ( x % 2 == 0 && y % 2 == 1 ) || ( x % 2 == 1 && y % 2 == 0 ) ) && !Smoothdraw) || (Smoothdraw && !DoAA))
					{
						// finding the point on the camera screen to shoot the ray through
						Vector3 xscreen, yscreen;
						xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
						yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
						Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x ) / 256 ) + yscreen * ( ( y ) / 256 );
						// setup the ray
						ray = new Ray();
						ray.Origin = Camera.Position;
						ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
						// shoot the ray and save the result
						colors1[(int) x, (int) y] = Trace(ray);
					}
					// draw with antialiasing
					else if (Smoothdraw && DoAA)
					{
						// temporary 2d array for saving all the result of the anti aliasing rays
						Vector3[,] avg = new Vector3[(int)aasqrt, (int)aasqrt];
						// shoot rays
						for(float i = 0; i < aasqrt; i++)
						{
							for (float j = 0; j < aasqrt; j++)
							{
								// finding the point on the camera screen to shoot the ray through
								Vector3 xscreen, yscreen;
								xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
								yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
								Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + i / aasqrt ) / 256 ) + yscreen * ( ( y + j / aasqrt ) / 256 );
								// setup the ray
								ray = new Ray();
								ray.Origin = Camera.Position;
								ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
								avg[(int)i,(int)j] = Trace(ray);
							}
						}
						// the final color
						Vector3 final = new Vector3(0,0,0);
						// add all the temporary rays to the final ray
						for (int i = 0; i < aasqrt; i++)
						{
							for (int j = 0; j < aasqrt; j++)
							{
								final += avg[i, j];
							}
						}
						// devide the final ray by the amount of rays to get the average
						final /= Antialiasing;
						// save the final ray
						colors1[(int) x, (int) y] = final;
					}
				}
		}

		// trace upper-right (same explanations as q1)
		private void q2()
		{
			Ray ray;
			for (float x = 0; x < 256; x++)
				for (float y = 0; y < 256; y++)
				{
					colors2[(int) x, (int) y] = new Vector3();
					if (( ( ( x % 2 == 0 && y % 2 == 1 ) || ( x % 2 == 1 && y % 2 == 0 ) ) && !Smoothdraw ) || ( Smoothdraw && !DoAA ))
					{
						Vector3 xscreen, yscreen;
						xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
						yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
						Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + 256 ) / 256 ) + yscreen * ( ( y ) / 256 );
						ray = new Ray();
						ray.Origin = Camera.Position;
						ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
						colors2[(int) x, (int) y] = Trace(ray);
					}
					else if (Smoothdraw && DoAA)
					{
						Vector3[,] avg = new Vector3[(int) aasqrt, (int) aasqrt];
						for (float i = 0; i < aasqrt; i++)
						{
							for (float j = 0; j < aasqrt; j++)
							{
								Vector3 xscreen, yscreen;
								xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
								yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
								Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + 256 + i / aasqrt ) / 256 ) + yscreen * ( ( y + j / aasqrt ) / 256 );
								ray = new Ray();
								ray.Origin = Camera.Position;
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

		// trace lower-left (same explanations as q1, except lines 294-298 and 318- 320, these are used for saving the rays for debug drawing)
		private void q3()
		{
			Ray ray;
			for (float x = 0; x < 256; x++)
				for (float y = 0; y < 256; y++)
				{
					colors3[(int) x, (int) y] = new Vector3();
					if (( ( ( x % 2 == 0 && y % 2 == 1 ) || ( x % 2 == 1 && y % 2 == 0 ) ) && !Smoothdraw ) || ( Smoothdraw && !DoAA ))
					{
						Vector3 xscreen, yscreen;
						xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
						yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
						Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x  ) / 256 ) + yscreen * ( ( y+256 ) / 256 );
						ray = new Ray();
						ray.Origin = Camera.Position;
						ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
						if (y == 0)
						{
							rays1.Add(ray);
							colors3[(int) x, (int) y] = Trace(ray, 0, 1, 1);
						}
						else
							colors3[(int) x, (int) y] = Trace(ray);
					}
					else if(Smoothdraw && DoAA)
					{
						Vector3[,] avg = new Vector3[(int) aasqrt, (int) aasqrt];
						for (float i = 0; i < aasqrt; i++)
						{
							for (float j = 0; j < aasqrt; j++)
							{
								Vector3 xscreen, yscreen;
								xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
								yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
								Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + i / aasqrt ) / 256 ) + yscreen * ( ( y + 256 + j / aasqrt ) / 256 );
								ray = new Ray();
								ray.Origin = Camera.Position;
								ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
								int saverays = (y == 0 && i == 0 && j == 0) ? 1 : 0;
								avg[(int) i, (int) j] = Trace(ray, 0, saverays, saverays);
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

		// trace lower-right (same explanations as q1, except lines 354-358 and 376-378, these are used for saving the rays for debug drawing)
		private void q4()
		{
			Ray ray;
			for (float x = 0; x < 256; x++)
				for (float y = 0; y < 256; y++)
				{
					colors4[(int) x, (int) y] = new Vector3();
					if (( ( ( x % 2 == 0 && y % 2 == 1 ) || ( x % 2 == 1 && y % 2 == 0 ) ) && !Smoothdraw ) || ( Smoothdraw && !DoAA ))
					{
						Vector3 xscreen, yscreen;
						xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
						yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
						Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + 256 ) / 256 ) + yscreen * ( ( y+256 ) / 256 );
						ray = new Ray();
						ray.Origin = Camera.Position;
						ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
						if (y == 0)
						{
							rays2.Add(ray);
							colors4[(int) x, (int) y] = Trace(ray, 0, 2, 2);
						}
						else
							colors4[(int) x, (int) y] = Trace(ray);
					}
					else if (Smoothdraw && DoAA)
					{
						Vector3[,] avg = new Vector3[(int) aasqrt, (int) aasqrt];
						for (float i = 0; i < aasqrt; i++)
						{
							for (float j = 0; j < aasqrt; j++)
							{
								Vector3 xscreen, yscreen;
								xscreen = Vector3.Normalize(Camera.Screen.p1 - Camera.Screen.p0);
								yscreen = Vector3.Normalize(Camera.Screen.p2 - Camera.Screen.p0);
								Vector3 onscreen = Camera.Screen.p0 + xscreen * ( ( x + 256 + i / aasqrt ) / 256 ) + yscreen * ( ( y + 256 + j / aasqrt ) / 256 );
								ray = new Ray();
								ray.Origin = Camera.Position;
								ray.Direction = Vector3.Normalize(onscreen - ray.Origin);
								int saverays = ( y == 0 && i == 0 && j == 0 ) ? 2 : 0;
								avg[(int) i, (int) j] = Trace(ray, 0, saverays, saverays);
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

		// trace a ray, the three integers are for checking: do we save the reflection ray (and where), do we save the shadow ray (and where) and how far are we in the recursion
		private Vector3 Trace(Ray ray, int recursion = 0, int shadow = 0, int recurse = 0)
		{
			// find the first primitive the ray hits
			Intersection intersect = Scene.NearestIntersect(ray);
			// if we hit something
			if (intersect != null)
			{
				// reflection
				if(intersect.Primitive.Material.Reflect)
				{
					// setting up the reflection ray
					Ray reflectray = new Ray();
					reflectray.Direction = Reflect(ray.Direction, intersect.IntersectionNormal);
					reflectray.Origin = intersect.IntersectionPoint + reflectray.Direction * 0.01f;
					// do we save this ray in reflect1
					if (recurse == 1)
						reflect1.Add(reflectray);
					// do we save this ray in reflect2
					if (recurse == 2)
						reflect2.Add(reflectray);
					// are we within the recursion limit
					if (recursion++ < MaxRecursion)
						if(intersect.Primitive.Material.ReflectPercentage < 1)
						return intersect.Primitive.Material.Color * intersect.Primitive.Material.Color * Trace(reflectray, recursion) * intersect.Primitive.Material.ReflectPercentage + intersect.Primitive.Material.Color * intersect.Primitive.GetTexture(intersect) * Illumination(intersect, ray) * ( 1 - intersect.Primitive.Material.ReflectPercentage);
						else
							return intersect.Primitive.Material.Color* intersect.Primitive.Material.Color* Trace(reflectray, recursion);
					else
						return intersect.Primitive.Material.Color;
				}
				// regular ray with shadow
				else
				{
					// illumination of the point we hit
					Vector3 illumination = Illumination(intersect, ray, shadow);
					// no texture
					if (intersect.Primitive.Material.Texture == null)
					{
						return intersect.Primitive.Material.Color * illumination;
					}
					// with texture
					else
					{
						return intersect.Primitive.GetTexture(intersect) * illumination;
					}
				}
				
			}
			// we hit nothing, so we return the skybox color where the ray would hit
			return Skybox(ray);
		}

		// return the direction of a ray if it reflects
		private Vector3 Reflect(Vector3 direction, Vector3 normal)
		{
			return direction - 2 * Vector3.Dot(direction, normal) * normal;
		}

		// how much is the intersection illuminated (adaptation from the slides)
		private Vector3 Illumination(Intersection intersection, Ray ray, int shadow = 0)
		{
			// what we will return
			Vector3 shadows = new Vector3(0, 0, 0);
			// check for each lightsource what illumination it adds
			for (int i = 0; i < Scene.LightSources.Count; i++)
			{
				// intersection point
				Vector3 I = intersection.IntersectionPoint;
				// vector from intersection point to lightsource
				Vector3 L = Scene.LightSources[i].Position - I;
				// the normal of the intersection
				Vector3 N = intersection.IntersectionNormal;
				// distance lightsource - intersection point
				float dist =  L.Length;
				// normalize L
				L /= dist;
				float NormalDot = Vector3.Dot(N, L);
				// check if the ray is behind the primitive
				if (NormalDot > 0)
				{
					// setup the shadow ray
					Ray shadowRay = new Ray { Origin = I, Direction = L, Distance = dist };
					// send the shadowray
					Intersection result = Scene.FirstIntersect(shadowRay);
					// did we hit anything
					if (result == null)
					{
						Primitive primitive = intersection.Primitive;
						float specComponent = 0;
						// how much of the lightsource is used
						float attenuation = 1f / (dist * dist);

						if (primitive.Material.SpecularPercentage > 0f)
						{
							Vector3 V = -ray.Direction;
							Vector3 H = Vector3.Normalize(V + L);
							float specDot = Vector3.Dot(N, H);
							/*Vector3 R = Vector3.Normalize(L - 2 * NormalDot * N);
							float specDot = Vector3.Dot(V, R);*/

							specComponent = primitive.Material.SpecularPercentage * (float)Math.Pow(Math.Max(0, specDot), primitive.Material.Specularity) * attenuation;
						}

						// check for spotlights
						if (Scene.LightSources[i].GetType() == typeof(Spotlight))
						{
							Spotlight light = (Spotlight)Scene.LightSources[i];
							float dot = -Vector3.Dot(light.Direction, L);
							if (dot >= light.Dot && dot > 0)
								shadows += light.Intensity * NormalDot * attenuation * primitive.Material.DiffusePercentage + light.Intensity * specComponent;
						}
						// regular light
						else
							shadows += Scene.LightSources[i].Intensity * NormalDot * attenuation * primitive.Material.DiffusePercentage + Scene.LightSources[i].Intensity * specComponent;
					}
					// we hit something
					else
					{
						shadowRay.Distance = result.Distance;
					}
					// do we save the shadow rays
					if (shadow == 1)
						shadowrays1.Add(shadowRay);
					if (shadow == 2)
						shadowrays2.Add(shadowRay);
				}
			}
			// minimum light level of 0.3f
			if (shadows.X < 0.3f)
				shadows.X = 0.3f;
			if (shadows.Y < 0.3f)
				shadows.Y = 0.3f;
			if (shadows.Z < 0.3f)
				shadows.Z = 0.3f;
			return shadows;
		}

		// intersection with the skybox (calculations from http://www.pauldebevec.com/Probes/)
		private Vector3 Skybox(Ray ray)
		{
			// flipping the image
			Vector3 d = -ray.Direction;
			float r = (float) ( ( 1d / Math.PI ) * Math.Acos(d.Z) / Math.Sqrt(d.X * d.X + d.Y * d.Y) );
			// find the coordinates
			float u = r*d.X+1;
			float v = r*d.Y+1;
			// scale the coordinates to image size
			int iu = (int) ( u * Skydome.Texture.Image.GetLength(0)  / 2) ;
			int iv = (int) ( v * Skydome.Texture.Image.GetLength(1) / 2 );
			// fail safe to make sure we're inside of the image coordinates
			if (iu >= Skydome.Texture.Image.GetLength(0) || iu < 0)
				iu = 0;
			if (iv >= Skydome.Texture.Image.GetLength(1) || iv < 0)
				iv = 0;
			// return the color
			return Skydome.Texture.Image[iu, iv];
		}
	}

	public class Camera
	{
		public Vector3 Position { get; set; }
		public Vector3 Direction { get; set; }
		// screen for shooting rays
		public Screen Screen { get; set; }
		// rotation of the camera in degrees (x is for turning left and right, y for up and down)
		public float X = 0, Z = 90;
		// field of view, in degrees
		public float FOV;
		// distance from camera to screen (for fov)
		public float ScreenDistance;

		// initialize the camera
		public Camera(Vector3 position, Vector3 direction, float fov = 90)
		{
			FOV = fov;
			// calculate the proper screen distance for the given fov
			float distance = 1 / (float) Math.Tan(( fov * ( Math.PI / 180 ) ) / 2);
			Position = position;
			Direction = direction;
			ScreenDistance = distance;
			CreateScreen();
		}

		// recalculate the screen and direction every time this is called
		public void Update()
		{
			Directionchange(X, Z);
			CreateScreen();
		}

		// change the direction to the given angles
		void Directionchange(float x, float z)
		{
			Direction = new Vector3((float) Math.Sin(x * Math.PI / 180) * (float) Math.Sin(z * Math.PI / 180), (float) Math.Cos(z * Math.PI / 180), (float) Math.Cos(x * Math.PI / 180) * (float) Math.Sin(z * Math.PI / 180));
		}

		// set the coordinates of the screen
		void CreateScreen()
		{
			ScreenDistance = 1 / (float) Math.Tan(( FOV * ( Math.PI / 180 ) ) / 2);
			Screen = new Screen();
			// vector perpendicular to the up direction (for correct x and z rotation)
			Vector3 perp = Vector3.Normalize(Vector3.Cross(Direction, new Vector3(0, 1, 0)));
			// vector perpendicular to the previous vector (for correct y rotation)
			Vector3 perpz = -Vector3.Normalize(Vector3.Cross(Direction, perp));
			// make sure the screen is not upside down
			if (perpz.Y < 0)
				perpz = -perpz;
			// set the 4 points of the screen
			Screen.p0 = Position + Direction * ScreenDistance + perp + perpz;
			Screen.p1 = Position + Direction * ScreenDistance - perp + perpz;
			Screen.p2 = Position + Direction * ScreenDistance + perp - perpz;
			Screen.p3 = Position + Direction * ScreenDistance - perp - perpz;
		}

	}

	// screen used for shooting rays
	public class Screen
	{
		public Vector3 p0, p1, p2, p3;
	}

	// saves all info of a ray
	public class Ray
	{
		public Vector3 Direction { get; set; }
		public Vector3 Origin { get; set; }
		public float Distance { get; set; } = 1e34f;
	}

	// saves info of a lightsource
	public class LightSource
	{
		public Vector3 Position { get; set; }
		public Vector3 Intensity { get; set; }
	}

	// information for spotlights
	public class Spotlight : LightSource
	{
		public Vector3 Direction { get; set; }
		public float Angle;
		public float Dot;

		// initialize the spotlight
		public Spotlight(Vector3 position, Vector3 intensity, Vector3 direction, float angle)
		{
			Position = position;
			Intensity = intensity;
			Direction = Vector3.Normalize(direction);
			Angle = angle / 2;
			Dot = (float)Math.Cos(Angle / 180 * Math.PI);
		}
	}
	
	public class Scene
	{
		// list of primitives
		public List<Primitive> Primitives { get; set; }
		// list of lightsources
		public List<LightSource> LightSources { get; set; }

		// initialize all the primitives in the scene
		public Scene()
		{
			// add the lists for primitives and lightsources
			Primitives = new List<Primitive>();
			LightSources = new List<LightSource>();
			// add 2 standard lightsources and 1 spotlight
			LightSources.Add(new LightSource { Intensity = new Vector3(10f,10f,10f), Position = new Vector3( 0f, 0f, 5f) });
			LightSources.Add(new LightSource { Intensity = new Vector3(10f, 10f, 10f), Position = new Vector3(0.3f, 0f, -1f) });
			LightSources.Add(new Spotlight(new Vector3(0, 5, 4), new Vector3(20f, 20f, 15f), new Vector3(0f, -1, 0), 60));
			LightSources.Add(new LightSource { Intensity = new Vector3(50f, 50f, 45f), Position = new Vector3(-8f, 5f, 1f) });
			// add the bottom plane
			Plane bottom = new Plane(new Vector3(0f, -1.5f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1, 1, 1));
			bottom.Material.Texture = new Texture("../../assets/checkers.png");
			Primitives.Add(bottom);
			// add the left (red) sphere
			Primitives.Add(new Sphere(new Vector3(-3f, 0f,5f), 1.5f, new Vector3(1,0.1f,0.1f), 0.5f, 50));
			// add the middle (textured) sphere
			Sphere texturedSphere = new Sphere(new Vector3(0f, 0f, 3f), 1.5f, new Vector3(1f, 1, 1f), true);
			texturedSphere.Material.Texture = new Texture("../../assets/globe.jpg");
			texturedSphere.Material.ReflectPercentage = 0.1f;
			texturedSphere.Material.DiffusePercentage = 0.3f;
			texturedSphere.Material.SpecularPercentage = 0.7f;
			texturedSphere.Material.Specularity = 20;
			Primitives.Add(texturedSphere);
			// add the right (reflective) sphere
			Primitives.Add(new Sphere(new Vector3(3f, 0f, 5f), 1.5f, new Vector3(1f, 1f, 1f), true));
			// add the triangles ( a textured pyramid) (source of picture gizeh.jpg: http://www.geschichteinchronologie.com/welt/arch-Scott-Onstott-ENGL/ph01-protocol/008-012-great-pyramid-Giza-864-Heliopolis-d/007-interior-stones-great-pyramid.jpg)
			Triangle temp1 = new Triangle(new Vector3(1, 3, 3), new Vector3(-1, 3, 3), new Vector3(0, 4, 4), new Vector3(1, 1, 1));
			temp1.Material.Texture = new Texture("../../assets/gizeh.jpg");
			Primitives.Add(temp1);
			Triangle temp2 = new Triangle(new Vector3(-1, 3, 3), new Vector3(-1, 3, 5), new Vector3(0, 4, 4), new Vector3(1, 1, 1));
			temp2.Material.Texture = new Texture("../../assets/gizeh.jpg");
			Primitives.Add(temp2);
			Triangle temp3 = new Triangle(new Vector3(-1, 3, 5), new Vector3(1, 3, 5), new Vector3(0, 4, 4), new Vector3(1, 1, 1));
			temp3.Material.Texture = new Texture("../../assets/gizeh.jpg");
			Primitives.Add(temp3);
			Triangle temp4 = new Triangle(new Vector3(1, 3, 5), new Vector3(1, 3, 3), new Vector3(0, 4, 4), new Vector3(1, 1, 1));
			temp4.Material.Texture = new Texture("../../assets/gizeh.jpg");
			Primitives.Add(temp4);
			Triangle temp5 = new Triangle(new Vector3(1, 3, 3), new Vector3(-1, 3, 3), new Vector3(1, 3, 5), new Vector3(1, 1, 1));
			temp5.Material.Texture = new Texture("../../assets/gizeh.jpg");
			Primitives.Add(temp5);
			Triangle temp6 = new Triangle(new Vector3(-1, 3, 5), new Vector3(1, 3, 5), new Vector3(-1, 3, 3), new Vector3(1, 1, 1));
			temp6.Material.Texture = new Texture("../../assets/gizeh.jpg");
			Primitives.Add(temp6);
		}

		// find the nearest primitive to the origin of the ray
		public Intersection NearestIntersect(Ray ray)
		{
			// setup intersectino for returning
			Intersection result = null;
			// check for each primitive
			foreach(Primitive primitive in Primitives)
			{
				// intersect the primitive and the ray
				Intersection temp = primitive.Intersect(ray);
				// if we hit something
				if (temp != null)
				{
					// did we hit in front of the camera
					if (temp.Distance > 0)
					{
						// if this is the first primitive we hit
						if (result == null)
						{
							// setup result
							result = temp;
							ray.Distance = temp.Distance;
						}
						else
						{
							// is this intersection closer than the current closest
							if (temp.Distance < ray.Distance)
							{
								// change the result intersection to this one
								result = temp;
								ray.Distance = result.Distance;
							}
						}
					}
				}
			}
			return result;
		}

		// check if the ray intersects with a primitive
		public Intersection FirstIntersect(Ray ray)
		{
			// check for each primitive if we hit it
			foreach (Primitive primitive in Primitives)
			{
				// intersect the ray and the primitive
				Intersection temp = primitive.Intersect(ray);
				// make sure the primitive is between the light source and farther than 0.0001f
				if (temp != null && temp.Distance < ray.Distance && temp.Distance > 0.0001f)
				{
					// we hit something, cast a shadow
					return temp;
				}
			}
			// we hit nothing
			return null;
		}
	}

	// store intersection data
	public class Intersection
	{
		public Vector3 IntersectionPoint { get; set; }
		public Vector3 IntersectionNormal { get; set; }
		public float Distance { get; set; }
		public Primitive Primitive { get; set; }
	}
}
