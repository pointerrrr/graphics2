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
				ray.Distance = xDist;
			}
			return result;
			/*
			Intersection result = new Intersection();
			result.Primitive = this;
			Vector3 eMinusS = ray.Origin - Position;
			Vector3 d = ray.Direction;
			double discriminant = Math.Pow(2 * Vector3.Dot(d, eMinusS), 2) - 4 * Vector3.Dot(d, d) *
						( Vector3.Dot(eMinusS, eMinusS) - Math.Pow(Radius, 2.0f) );

			if (discriminant < 0.0001f)
			{   // 0 hits
				return null;
			}
			else
			{      // there will be one or two hits
				float front = -2.0f * Vector3.Dot(d, eMinusS);
				float denominator = 2.0f * Vector3.Dot(d, d);
				if (discriminant <= 0.0001f)
				{   // 1 hit
					result.Distance = (float) ( front + Math.Sqrt(discriminant) ) / denominator;  // does not matter if +- discriminant
				}
				else
				{  // 2 hits
					float t1 = -(float) ( front - Math.Sqrt(discriminant) ) / denominator;  // smaller t value
					float t2 = -(float) ( front + Math.Sqrt(discriminant) ) / denominator;  // larger t value
					if (t2 < 0) // sphere is "behind" start of ray
					{
						return null;    // no hit
					}
					else
					{  // one of them is in front
						if (t1 <= 0) result.Distance = t1; // return first intersection with sphere (usual case, smaller t)
						else result.Distance = t2;        // return second hit (ray's origin is inside the sphere)
					}
				}
			}

			// if we are here, info.time has been set, otherwise the function would have returned
			//result.IntersectionPoint = ray.GetPoint(info.time);
			//info.normal = ( info.hitPoint - center ).normalized;
			ray.Distance = result.Distance;
			return result;*/
			
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
			float distance = - Vector3.Dot(Position - ray.Origin, Normal) / denominator;
			if (distance < 0)
				return null;

			result.Distance = distance;
			ray.Distance = distance;

			return result;
		}

	}
}
