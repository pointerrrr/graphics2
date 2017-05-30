using System;
using OpenTK;
using System.Drawing;

namespace Template
{
	// base class of all primitives, contains material, intersect function and a function to get what part of the texture should be drawn
	public abstract class Primitive
	{
		// properties all primitives have
		public Material Material { get; set; }

		// intersect function for override
		public abstract Intersection Intersect(Ray ray);

		// texture function for override
		public abstract Vector3 GetTexture(Intersection intersect);
	}

	// sphere primitive
	public class Sphere : Primitive
	{
		// properties unique to sphere
		public Vector3 Position { get; set; }
		public float Radius { get; set; }

		// initialize the sphere
		public Sphere(Vector3 position, float radius, Vector3 color, bool reflect = false)
		{
			Material = new Material();
			Position = position;
			Radius = radius;
			Material.Color = color;
			Material.Reflect = reflect;
		}

		// adaptation of the fast ray - sphere intersect from the slides
		public override Intersection Intersect(Ray ray)
		{
			// setup the result for returning
			Intersection result = new Intersection();
			result.Primitive = this;
			Vector3 centerVector = Position - ray.Origin;
			float xDist = ( Vector3.Dot(centerVector, ray.Direction));
			Vector3 yVector = centerVector - xDist * ray.Direction;
			float psqr = Vector3.Dot(yVector, yVector);
			float radiussqr = Radius * Radius;
			// ray does not hit the sphere
			if (psqr > radiussqr)
			{
				return null;
			}
			xDist -= (float) Math.Sqrt(radiussqr - psqr);
			// check if sphere is behind camera
			if (xDist > 0)
			{
				// distance with correction
				result.Distance = xDist - 0.0001f;
				result.IntersectionPoint = result.Distance * ray.Direction + ray.Origin;
				result.IntersectionNormal = Vector3.Normalize(result.IntersectionPoint - Position);
				return result;
			}
			return null;			
		}

		// finding the texture of the sphere ( calculations found on http://www.pauldebevec.com/Probes/)
		public override Vector3 GetTexture(Intersection intersect)
		{
			// direction of vector for finding coordinates
			Vector3 d = Vector3.Normalize( Position - intersect.IntersectionPoint);
			float r = (float) ( ( 1d / Math.PI ) * Math.Acos(d.Z) / Math.Sqrt(d.X * d.X + d.Y * d.Y) );
			// finding the coordinates
			float u = r * d.X + 1;
			float v = r * d.Y + 1;
			// scaling the coordinates to image size
			int iu = (int) ( u * intersect.Primitive.Material.Texture.Image.GetLength(0) / 2) ;
			int iv = (int) ( v * intersect.Primitive.Material.Texture.Image.GetLength(1) / 2 );
			// fail-safe to make sure the returned value is always within the image
			if (iu >= intersect.Primitive.Material.Texture.Image.GetLength(0) || iu < 0)
				iu = 0;
			if (iv >= intersect.Primitive.Material.Texture.Image.GetLength(1) || iv < 0)
				iv = 0;
			return intersect.Primitive.Material.Texture.Image[iu, iv];
		}
	}

	public class Plane : Primitive
	{
		// properties unique to plane
		public Vector3 Position { get; set; }
		public Vector3 Normal { get; set; }

		// initialize the plane
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
			float denominator = Vector3.Dot(direction, Normal);
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

		public override Vector3 GetTexture(Intersection intersect)
		{
			Vector3[,] image = intersect.Primitive.Material.Texture.Image;
			Vector3 temp = intersect.IntersectionPoint - intersect.IntersectionNormal * intersect.IntersectionPoint.Y;
			float x, y;
			x = temp.X % 1;
			if (x < 0)
				x = 1 + x;
			y = temp.Z % 1;
			if (y < 0)
				y = 1 + y;
			// fail safe to make sure the pixel is on the image
			if (x >= 1 || x < 0)
				x = 0;
			if (y >= 1 || y < 0)
				y = 0;
			return image[(int) ( x * image.GetLength(0) ), (int) ( y * image.GetLength(1) )];
		}
	}

	public class Triangle : Primitive
	{
		// properties unique to triangle
		public Vector3 p0, p1, p2;
		public Vector3 Normal;

		// initialize the triangle
		public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 color, bool reflect = false)
		{
			Material = new Material();
			Material.Color = color;
			Material.Reflect = reflect;
			p0 = v0;
			p1 = v1;
			p2 = v2;
			Normal = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p0));
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
				// checking how long the ray has to travel
				float denominator = Vector3.Dot(ray.Direction, Normal);
				float distance = Vector3.Dot(p0 - ray.Origin, Normal) / denominator;
				result.Distance = distance - 0.0001f;
				if (Vector3.Dot(Normal, ray.Direction) > 0)
					result.IntersectionNormal = -Normal;
				else
					result.IntersectionNormal = Normal;
				result.IntersectionPoint = result.Distance * ray.Direction + ray.Origin;
				return result;
			}
			// No hit, no win
			return null;
		}

		// essentially the same as plane texture method
		public override Vector3 GetTexture(Intersection intersect)
		{
			
			Vector3[,] image = intersect.Primitive.Material.Texture.Image;
			Vector3 temp = intersect.IntersectionPoint - intersect.IntersectionNormal * intersect.IntersectionPoint.Y;
			float x, y;
			// the texture will be drawn 
			x = temp.X % 1;
			if (x < 0)
				x = 1 + x;
			y = temp.Z % 1;
			if (y < 0)
				y = 1 + y;
			// fail safe to make sure the pixel is on the image
			if (x >= 1 || x < 0)
				x = 0;
			if (y >= 1 || y < 0)
				y = 0;
			return image[(int) ( x * image.GetLength(0) ), (int) ( y * image.GetLength(1) )];
		}
	}

	// material class for all primitives
	public class Material
	{
		public Vector3 Color { get; set; }
		public bool Reflect { get; set; }
		public float ReflectPercentage { get; set; } = 1f;
		public Texture Texture { get; set; }

		public Material()
		{
			Color = new Vector3(1,1,1);
			Reflect = false;
		}
	}

	// skybox for when rays hit nothing
	public class Skybox
	{
		public Texture Texture { get; set; }

		// initialize texture via string
		public Skybox(string path)
		{
			Texture = new Texture(path);
		}
	}

	// texture for all primitives
	public class Texture
	{
		// 2d-array of vector3's to make the image accesible in multiple threads
		public Vector3[,] Image { get; set; }
		// bitmap used for final texture (changes the Image array when the bitmap is changed as well)
		public Bitmap Bitmap
		{
			get { return Bitmap; }
			set
			{
				Image = new Vector3[value.Width, value.Height];
				for (int i = 0; i < value.Width; i++)
					for (int j = 0; j < value.Height; j++)
					{
						Color color = value.GetPixel(i, j);
						Image[i, j] = new Vector3((float) color.R / 255, (float) color.G / 255, (float) color.B / 255);
					}
			}
		}

		// initialize texture via string
		public Texture(string path)
		{
			Bitmap image = new Bitmap(path);
			Bitmap = image;
		}
	}
}
