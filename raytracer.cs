using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;

namespace template
{
	public class Application
	{

	}

	public class Raytracer
	{
		public Scene scene;
		public Camera camera;


		public void Render()
		{

		}
	}

	public class Camera
	{
		public Vector3 position;
		public Vector3 direction;
		public Vector3 screenx1, screenc2, screenx3, screenx4, screeny1, screeny2, screeny3, screeny4;
	}

	public class Primitive
	{
		public Vector3 color;
		public virtual void Intersect(Vector3 ray)
		{

		}
	}

	public class Sphere : Primitive
	{
		public Vector3 position;
		public float radius;
	}

	public class Plane : Primitive
	{
		public Vector3 position;
		public Vector3 normal;
		
	}

	public class LightSource
	{
		public Vector3 position;
		public Vector3 intensity;
	}

	public class Scene
	{
		public List<Primitive> primitives;
		public List<LightSource> lightSources;

		public Intersection Intersect()
		{

			return null;
		}
	}

	public class Intersection
	{
		public Vector3 intersection;
		public Vector3 intersectionNormal;
		public float distance;
		public Primitive nearest;		
	}
}
