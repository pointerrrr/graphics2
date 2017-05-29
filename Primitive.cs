using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;
using System.Threading;

namespace Template
{

	public class Primitive
	{
		public Material Material { get; set; }

		public virtual Intersection Intersect(Ray ray)
		{
			return null;
		}

		public virtual Vector3 GetNormal(Vector3 IntersectionPoint)
		{
			return new Vector3(0,0,0);
		}
	}

	public class Sphere : Primitive
	{
		
		public Vector3 Position { get; set; }
		public float Radius { get; set; }

		public Sphere()
		{
			Material = new Material();
		}
		
		public Sphere(Vector3 position, float radius)
		{
			Material = new Material();
			Position = position;
			Radius = radius;
		}

		public Sphere(Vector3 position, float radius, Vector3 color, bool reflect = false)
		{
			Material = new Material();
			Position = position;
			Radius = radius;
			Material.Color = color;
			Material.Reflect = reflect;
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
			{
				return null;
			}
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
	}

	public class Plane : Primitive
	{
		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }

		public Plane()
		{
			Material = new Material();
		}

		public Plane(Vector3 position, Vector3 normal)
		{
			Material = new Material();
			Position = position;
			Normal = normal;
		}

		public Plane(Vector3 position, Vector3 normal, Vector3 color, bool reflect = false)
		{
			Material = new Material();
			Position = position;
			Normal = normal;
			Material.Color = color;
			Material.Reflect = reflect;
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
			{
				return null;
			}
			result.Distance = distance - 0.0001f;
			if (Vector3.Dot(Normal, ray.Direction) > 0)
				result.IntersectionNormal = -Normal;
			else
				result.IntersectionNormal = Normal;
			result.IntersectionPoint = result.Distance * ray.Direction + ray.Origin;
			return result;
		}

		public override Vector3 GetNormal(Vector3 IntersectionPoint)
		{
			return Normal;
		}
	}

	public class Triangle : Primitive
	{
		public Vector3 p0, p1, p2;
		public Vector3 Normal;
		private Plane plane;

		public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
		{
			Material = new Material();
			Material.Color = new Vector3(1,1,1);
			p0 = v0;
			p1 = v1;
			p2 = v2;
			Normal = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p0));
			plane = new Plane(p0, Normal);
		}

		// modified version of https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
		public override Intersection Intersect(Ray ray)
		{
			Intersection result = new Intersection();
			result.Primitive = this;
			Vector3 e1, e2;  //Edge1, Edge2
			Vector3 P, Q, T;
			float det, inv_det, u, v;
			float t;

			//Find vectors for two edges sharing V1
			e1 = p1 - p0;
			e2 = p2 - p0;
			//Begin calculating determinant - also used to calculate u parameter
			P = Vector3.Cross(ray.Direction, e2);
			//if determinant is near zero, ray lies in plane of triangle or ray is parallel to plane of triangle
			det = Vector3.Dot(e1, P);
			//NOT CULLING
			if (det > -0.0001f && det < 0.0001f) return null;

			inv_det = 1f / det;

			//calculate distance from V1 to ray origin
			T = Vector3.Subtract(ray.Origin, p0);

			//Calculate u parameter and test bound
			u = Vector3.Dot(T, P) * inv_det;
			//The intersection lies outside of the triangle
			if (u < 0f || u > 1f) return null;

			//Prepare to test v parameter
			Q = Vector3.Cross(T, e1);

			//Calculate V parameter and test bound
			v = Vector3.Dot(ray.Direction, Q) * inv_det;
			//The intersection lies outside of the triangle
			if (v < 0f || u + v > 1f) return null;

			t = Vector3.Dot(e2, Q) * inv_det;

			if (t > 0.0001f)
			{ //ray intersection
				result = plane.Intersect(ray);
				return result;
			}

			// No hit, no win
			return null;
		}
	}

	public class Material
	{
		public Vector3 Color { get; set; }
		public bool Reflect { get; set; }
		public float ReflectPercentage { get; set; }
		public bool Refract { get; set; }
		public float RefractPercentage { get; set; }
		public float RefractionIndex { get; set; }
		public Texture Texture { get; set; }

		public Material()
		{
			Color = new Vector3(1,1,1);
			Reflect = false;
		}
	}

	public class Texture
	{
		public Vector3[,] Image { get; set; }
		public Bitmap bitmap { get { return bitmap; } set {
				Image = new Vector3[value.Width, value.Height];
				for (int i = 0; i < value.Width; i++)
					for (int j = 0; j < value.Height; j++)
					{
						Color color = value.GetPixel(i, j);
						Image[i, j] = new Vector3((float) color.R / 255, (float) color.G / 255, (float) color.B / 255);
					}
			} }

		public Texture(Bitmap image)
		{
			bitmap = image;
		}

		public Texture(string path)
		{
			Bitmap image = new Bitmap(path);
			bitmap = image;
		}
	}
}
