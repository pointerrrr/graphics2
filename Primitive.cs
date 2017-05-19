using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Template
{

	public class Primitive
	{
		public Vector3 Color { get; set; } = new Vector3(1, 1, 1);

		public virtual Intersection Intersect(Ray ray)
		{
			return null;
		}
	}

	public class Sphere : Primitive
	{
		
		public Vector3 Position { get; set; }
		public float Radius { get; set; }

		public Sphere()
		{

		}
		
		public Sphere(Vector3 position, float radius)
		{
			Position = position;
			Radius = radius;
		}

		public Sphere(Vector3 position, float radius, Vector3 color)
		{
			Position = position;
			Radius = radius;
			Color = color;
		}

		public override Intersection Intersect(Ray ray)
		{
			Intersection result = new Intersection();
			result.Primitive = this;
			Vector3 centerVector = Position - ray.Origin;
			float xDist = ( Vector3.Dot(centerVector, ray.Direction));
			Vector3 yVector = centerVector - xDist * ray.Direction;
			float psqr = Vector3.Dot(yVector, yVector);
			float radiussqr = Radius * Radius;
			if (psqr > radiussqr) return null;
			xDist -= (float) Math.Sqrt(radiussqr - psqr);
			
			if (xDist > 0)
			{
				result.Distance = xDist;
			}
			return result;
		}
	}

	public class Plane : Primitive
	{
		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }

		public Plane()
		{

		}

		public Plane(Vector3 position, Vector3 normal)
		{
			Position = position;
			Normal = normal;
		}

		public Plane(Vector3 position, Vector3 normal, Vector3 color)
		{
			Position = position;
			Normal = normal;
			Color = color;
		}

		// based off: http://cmichel.io/howto-raytracer-ray-plane-intersection-theory/
		public override Intersection Intersect(Ray ray)
		{
			Intersection result = new Intersection();
			result.Primitive = this;
			Vector3 direction = ray.Direction;
			float denominator = Vector3.Dot(direction, Normal);
			if (Math.Abs(denominator) < 0.0001f)
				return null;
			float distance = Vector3.Dot(Position - ray.Origin, Normal) / denominator;
			if (distance < 0)
				return null;
			result.Distance = distance;
			return result;
		}

	}
}
