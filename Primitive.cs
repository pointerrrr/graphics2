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

		public virtual Vector3 GetNormal(Vector3 IntersectionPoint)
		{
			return new Vector3(0,0,0);
		}
	} // class Primitive

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

			if (psqr > radiussqr)
				return null;

			xDist -= (float) Math.Sqrt(radiussqr - psqr);			
			if (xDist > 0)
			{
				result.Distance = xDist - 0.0001f;
				result.IntersectionPoint = result.Distance * ray.Direction + ray.Origin;
				result.IntersectionNormal = Vector3.Normalize(result.IntersectionPoint - Position);
				return result;
			}
			return null;			
		}

		public override Vector3 GetNormal(Vector3 IntersectionPoint)
		{
			Vector3 normal;
			normal = new Vector3(Vector3.Normalize(IntersectionPoint - Position));
			return normal;
		}
	} // class Sphere : Primitive

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
			float denominator;
			denominator = Vector3.Dot(direction, Normal);

			if (Math.Abs(denominator) < 0.0001f)
				return null;

			float distance = Vector3.Dot(Position - ray.Origin, Normal) / denominator;
			if (distance < 0)
				return null;

			result.Distance = distance - 0.0001f;
			result.IntersectionNormal = Normal;
			result.IntersectionPoint = result.Distance * ray.Direction + ray.Origin - 0.0001f * ray.Direction;
			return result;
		}

		public override Vector3 GetNormal(Vector3 IntersectionPoint)
		{
			return Normal;
		}
	} // class Plane : Primitive
} // namespace Template
