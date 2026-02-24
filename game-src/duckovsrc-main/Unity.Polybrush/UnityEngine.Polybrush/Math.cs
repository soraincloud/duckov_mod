using System;
using System.Collections.Generic;

namespace UnityEngine.Polybrush;

public static class Math
{
	public const float phi = 1.618034f;

	private const float k_FltEpsilon = float.Epsilon;

	private const float k_FltCompareEpsilon = 0.0001f;

	internal const float handleEpsilon = 0.0001f;

	private static Vector3 tv1;

	private static Vector3 tv2;

	private static Vector3 tv3;

	private static Vector3 tv4;

	internal static Vector2 PointInCircumference(float radius, float angleInDegrees, Vector2 origin)
	{
		float x = radius * Mathf.Cos(MathF.PI / 180f * angleInDegrees) + origin.x;
		float y = radius * Mathf.Sin(MathF.PI / 180f * angleInDegrees) + origin.y;
		return new Vector2(x, y);
	}

	internal static Vector3 PointInSphere(float radius, float latitudeAngle, float longitudeAngle)
	{
		float x = radius * Mathf.Cos(MathF.PI / 180f * latitudeAngle) * Mathf.Sin(MathF.PI / 180f * longitudeAngle);
		float y = radius * Mathf.Sin(MathF.PI / 180f * latitudeAngle) * Mathf.Sin(MathF.PI / 180f * longitudeAngle);
		float z = radius * Mathf.Cos(MathF.PI / 180f * longitudeAngle);
		return new Vector3(x, y, z);
	}

	internal static float SignedAngle(Vector2 a, Vector2 b)
	{
		float num = Vector2.Angle(a, b);
		if (b.x - a.x < 0f)
		{
			num = 360f - num;
		}
		return num;
	}

	public static float SqrDistance(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.y - a.y;
		float num3 = b.z - a.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public static float TriangleArea(Vector3 x, Vector3 y, Vector3 z)
	{
		float num = SqrDistance(x, y);
		float num2 = SqrDistance(y, z);
		float num3 = SqrDistance(z, x);
		return Mathf.Sqrt((2f * num * num2 + 2f * num2 * num3 + 2f * num3 * num - num * num - num2 * num2 - num3 * num3) / 16f);
	}

	internal static float PolygonArea(Vector3[] vertices, int[] indexes)
	{
		float num = 0f;
		for (int i = 0; i < indexes.Length; i += 3)
		{
			num += TriangleArea(vertices[indexes[i]], vertices[indexes[i + 1]], vertices[indexes[i + 2]]);
		}
		return num;
	}

	internal static Vector2 RotateAroundPoint(this Vector2 v, Vector2 origin, float theta)
	{
		float x = origin.x;
		float y = origin.y;
		float x2 = v.x;
		float y2 = v.y;
		float num = Mathf.Sin(theta * (MathF.PI / 180f));
		float num2 = Mathf.Cos(theta * (MathF.PI / 180f));
		float num3 = x2 - x;
		y2 -= y;
		float num4 = num3 * num2 + y2 * num;
		float num5 = (0f - num3) * num + y2 * num2;
		float x3 = num4 + x;
		y2 = num5 + y;
		return new Vector2(x3, y2);
	}

	public static Vector2 ScaleAroundPoint(this Vector2 v, Vector2 origin, Vector2 scale)
	{
		return Vector2.Scale(v - origin, scale) + origin;
	}

	public static Vector2 ReflectPoint(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
	{
		Vector2 vector = lineEnd - lineStart;
		Vector2 vector2 = new Vector2(0f - vector.y, vector.x);
		float num = Mathf.Sin(Vector2.Angle(vector, point - lineStart) * (MathF.PI / 180f)) * Vector2.Distance(point, lineStart);
		return point + vector2 * (num * 2f) * ((Vector2.Dot(point - lineStart, vector2) > 0f) ? (-1f) : 1f);
	}

	internal static float SqrDistanceRayPoint(Ray ray, Vector3 point)
	{
		return Vector3.Cross(ray.direction, point - ray.origin).sqrMagnitude;
	}

	public static float DistancePointLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
	{
		float num = (lineStart.x - lineEnd.x) * (lineStart.x - lineEnd.x) + (lineStart.y - lineEnd.y) * (lineStart.y - lineEnd.y);
		if (num == 0f)
		{
			return Vector2.Distance(point, lineStart);
		}
		float num2 = Vector2.Dot(point - lineStart, lineEnd - lineStart) / num;
		if ((double)num2 < 0.0)
		{
			return Vector2.Distance(point, lineStart);
		}
		if ((double)num2 > 1.0)
		{
			return Vector2.Distance(point, lineEnd);
		}
		Vector2 b = lineStart + num2 * (lineEnd - lineStart);
		return Vector2.Distance(point, b);
	}

	public static float DistancePointLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{
		float num = (lineStart.x - lineEnd.x) * (lineStart.x - lineEnd.x) + (lineStart.y - lineEnd.y) * (lineStart.y - lineEnd.y) + (lineStart.z - lineEnd.z) * (lineStart.z - lineEnd.z);
		if (num == 0f)
		{
			return Vector3.Distance(point, lineStart);
		}
		float num2 = Vector3.Dot(point - lineStart, lineEnd - lineStart) / num;
		if ((double)num2 < 0.0)
		{
			return Vector3.Distance(point, lineStart);
		}
		if ((double)num2 > 1.0)
		{
			return Vector3.Distance(point, lineEnd);
		}
		Vector3 b = lineStart + num2 * (lineEnd - lineStart);
		return Vector3.Distance(point, b);
	}

	public static Vector3 GetNearestPointRayRay(Ray a, Ray b)
	{
		return GetNearestPointRayRay(a.origin, a.direction, b.origin, b.direction);
	}

	internal static Vector3 GetNearestPointRayRay(Vector3 ao, Vector3 ad, Vector3 bo, Vector3 bd)
	{
		float num = Vector3.Dot(ad, bd);
		float num2 = Mathf.Abs(num);
		if (num2 - 1f > Mathf.Epsilon || num2 < Mathf.Epsilon)
		{
			return ao;
		}
		Vector3 rhs = bo - ao;
		float num3 = (0f - num) * Vector3.Dot(bd, rhs) + Vector3.Dot(ad, rhs) * Vector3.Dot(bd, bd);
		float num4 = Vector3.Dot(ad, ad) * Vector3.Dot(bd, bd) - num * num;
		return ao + ad * (num3 / num4);
	}

	internal static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 intersect)
	{
		intersect = Vector2.zero;
		Vector2 vector = default(Vector2);
		vector.x = p1.x - p0.x;
		vector.y = p1.y - p0.y;
		Vector2 vector2 = default(Vector2);
		vector2.x = p3.x - p2.x;
		vector2.y = p3.y - p2.y;
		float num = ((0f - vector.y) * (p0.x - p2.x) + vector.x * (p0.y - p2.y)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
		float num2 = (vector2.x * (p0.y - p2.y) - vector2.y * (p0.x - p2.x)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
		if (num >= 0f && num <= 1f && num2 >= 0f && num2 <= 1f)
		{
			intersect.x = p0.x + num2 * vector.x;
			intersect.y = p0.y + num2 * vector.y;
			return true;
		}
		return false;
	}

	internal static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		Vector2 vector = default(Vector2);
		vector.x = p1.x - p0.x;
		vector.y = p1.y - p0.y;
		Vector2 vector2 = default(Vector2);
		vector2.x = p3.x - p2.x;
		vector2.y = p3.y - p2.y;
		float num = ((0f - vector.y) * (p0.x - p2.x) + vector.x * (p0.y - p2.y)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
		float num2 = (vector2.x * (p0.y - p2.y) - vector2.y * (p0.x - p2.x)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
		if (num >= 0f && num <= 1f && num2 >= 0f)
		{
			return num2 <= 1f;
		}
		return false;
	}

	public static bool RayIntersectsTriangle(Ray InRay, Vector3 InTriangleA, Vector3 InTriangleB, Vector3 InTriangleC, out float OutDistance, out Vector3 OutPoint)
	{
		OutDistance = 0f;
		OutPoint = Vector3.zero;
		Vector3 vector = InTriangleB - InTriangleA;
		Vector3 vector2 = InTriangleC - InTriangleA;
		Vector3 rhs = Vector3.Cross(InRay.direction, vector2);
		float num = Vector3.Dot(vector, rhs);
		if (num > 0f - Mathf.Epsilon && num < Mathf.Epsilon)
		{
			return false;
		}
		float num2 = 1f / num;
		Vector3 lhs = InRay.origin - InTriangleA;
		float num3 = Vector3.Dot(lhs, rhs) * num2;
		if (num3 < 0f || num3 > 1f)
		{
			return false;
		}
		Vector3 rhs2 = Vector3.Cross(lhs, vector);
		float num4 = Vector3.Dot(InRay.direction, rhs2) * num2;
		if (num4 < 0f || num3 + num4 > 1f)
		{
			return false;
		}
		float num5 = Vector3.Dot(vector2, rhs2) * num2;
		if (num5 > Mathf.Epsilon)
		{
			OutDistance = num5;
			OutPoint.x = num3 * InTriangleB.x + num4 * InTriangleC.x + (1f - (num3 + num4)) * InTriangleA.x;
			OutPoint.y = num3 * InTriangleB.y + num4 * InTriangleC.y + (1f - (num3 + num4)) * InTriangleA.y;
			OutPoint.z = num3 * InTriangleB.z + num4 * InTriangleC.z + (1f - (num3 + num4)) * InTriangleA.z;
			return true;
		}
		return false;
	}

	internal static bool RayIntersectsTriangle2(Vector3 origin, Vector3 dir, Vector3 vert0, Vector3 vert1, Vector3 vert2, out float distance, out Vector3 normal)
	{
		Subtract(vert0, vert1, ref tv1);
		Subtract(vert0, vert2, ref tv2);
		normal = Vector3.Cross(tv1, tv2);
		distance = 0f;
		if (Vector3.Dot(dir, normal) > 0f)
		{
			return false;
		}
		Cross(dir, tv2, ref tv4);
		float num = Vector3.Dot(tv1, tv4);
		if (num < Mathf.Epsilon)
		{
			return false;
		}
		Subtract(vert0, origin, ref tv3);
		float num2 = Vector3.Dot(tv3, tv4);
		if (num2 < 0f || num2 > num)
		{
			return false;
		}
		Cross(tv3, tv1, ref tv4);
		float num3 = Vector3.Dot(dir, tv4);
		if (num3 < 0f || num2 + num3 > num)
		{
			return false;
		}
		distance = Vector3.Dot(tv2, tv4) * (1f / num);
		return distance > 0f;
	}

	public static float Secant(float x)
	{
		return 1f / Mathf.Cos(x);
	}

	public static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		float ax = p1.x - p0.x;
		float ay = p1.y - p0.y;
		float az = p1.z - p0.z;
		float bx = p2.x - p0.x;
		float num = p2.y - p0.y;
		float bz = p2.z - p0.z;
		Vector3 zero = Vector3.zero;
		Cross(ax, ay, az, bx, num, bz, ref zero.x, ref zero.y, ref zero.z);
		if (zero.magnitude < Mathf.Epsilon)
		{
			return new Vector3(0f, 0f, 0f);
		}
		zero.Normalize();
		return zero;
	}

	internal static Vector3 Normal(IList<Vector3> p)
	{
		if (p == null || p.Count < 3)
		{
			return Vector3.zero;
		}
		int count = p.Count;
		if (count % 3 == 0)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < count; i += 3)
			{
				zero += Normal(p[i], p[i + 1], p[i + 2]);
			}
			zero /= (float)count / 3f;
			zero.Normalize();
			return zero;
		}
		Vector3 vector = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
		if (vector.magnitude < Mathf.Epsilon)
		{
			return new Vector3(0f, 0f, 0f);
		}
		return vector.normalized;
	}

	public static Vector2 Average(IList<Vector2> array, IList<int> indexes = null)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Vector2 zero = Vector2.zero;
		float num = indexes?.Count ?? array.Count;
		if (indexes == null)
		{
			for (int i = 0; (float)i < num; i++)
			{
				zero += array[i];
			}
		}
		else
		{
			for (int j = 0; (float)j < num; j++)
			{
				zero += array[indexes[j]];
			}
		}
		return zero / num;
	}

	public static Vector3 Average(IList<Vector3> array, IList<int> indexes = null)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Vector3 zero = Vector3.zero;
		float num = indexes?.Count ?? array.Count;
		if (indexes == null)
		{
			for (int i = 0; (float)i < num; i++)
			{
				zero.x += array[i].x;
				zero.y += array[i].y;
				zero.z += array[i].z;
			}
		}
		else
		{
			for (int j = 0; (float)j < num; j++)
			{
				zero.x += array[indexes[j]].x;
				zero.y += array[indexes[j]].y;
				zero.z += array[indexes[j]].z;
			}
		}
		return zero / num;
	}

	internal static Vector3 Average<T>(this IList<T> list, Func<T, Vector3> selector, IList<int> indexes = null)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		Vector3 zero = Vector3.zero;
		float num = indexes?.Count ?? list.Count;
		if (indexes == null)
		{
			for (int i = 0; (float)i < num; i++)
			{
				zero += selector(list[i]);
			}
		}
		else
		{
			for (int j = 0; (float)j < num; j++)
			{
				zero += selector(list[indexes[j]]);
			}
		}
		return zero / num;
	}

	public static Vector4 Average(IList<Vector4> v, IList<int> indexes = null)
	{
		if (v == null)
		{
			throw new ArgumentNullException("v");
		}
		Vector4 zero = Vector4.zero;
		float num = indexes?.Count ?? v.Count;
		if (indexes == null)
		{
			for (int i = 0; (float)i < num; i++)
			{
				zero += v[i];
			}
		}
		else
		{
			for (int j = 0; (float)j < num; j++)
			{
				zero += v[indexes[j]];
			}
		}
		return zero / num;
	}

	internal static Color Average(IList<Color> c, IList<int> indexes = null)
	{
		if (c == null)
		{
			throw new ArgumentNullException("c");
		}
		Color color = c[0];
		float num = indexes?.Count ?? c.Count;
		if (indexes == null)
		{
			for (int i = 1; (float)i < num; i++)
			{
				color += c[i];
			}
		}
		else
		{
			for (int j = 1; (float)j < num; j++)
			{
				color += c[indexes[j]];
			}
		}
		return color / num;
	}

	internal static bool Approx2(this Vector2 a, Vector2 b, float delta = 0.0001f)
	{
		if (Mathf.Abs(a.x - b.x) < delta)
		{
			return Mathf.Abs(a.y - b.y) < delta;
		}
		return false;
	}

	internal static bool Approx3(this Vector3 a, Vector3 b, float delta = 0.0001f)
	{
		if (Mathf.Abs(a.x - b.x) < delta && Mathf.Abs(a.y - b.y) < delta)
		{
			return Mathf.Abs(a.z - b.z) < delta;
		}
		return false;
	}

	internal static bool Approx4(this Vector4 a, Vector4 b, float delta = 0.0001f)
	{
		if (Mathf.Abs(a.x - b.x) < delta && Mathf.Abs(a.y - b.y) < delta && Mathf.Abs(a.z - b.z) < delta)
		{
			return Mathf.Abs(a.w - b.w) < delta;
		}
		return false;
	}

	internal static bool ApproxC(this Color a, Color b, float delta = 0.0001f)
	{
		if (Mathf.Abs(a.r - b.r) < delta && Mathf.Abs(a.g - b.g) < delta && Mathf.Abs(a.b - b.b) < delta)
		{
			return Mathf.Abs(a.a - b.a) < delta;
		}
		return false;
	}

	internal static bool Approx(this float a, float b, float delta = 0.0001f)
	{
		return Mathf.Abs(b - a) < Mathf.Abs(delta);
	}

	internal static int Wrap(int value, int lowerBound, int upperBound)
	{
		int num = upperBound - lowerBound + 1;
		if (value < lowerBound)
		{
			value += num * ((lowerBound - value) / num + 1);
		}
		return lowerBound + (value - lowerBound) % num;
	}

	public static int Clamp(int value, int lowerBound, int upperBound)
	{
		if (value >= lowerBound)
		{
			if (value <= upperBound)
			{
				return value;
			}
			return upperBound;
		}
		return lowerBound;
	}

	internal static Vector3 ToSignedMask(this Vector3 vec, float delta = float.Epsilon)
	{
		return new Vector3((Mathf.Abs(vec.x) > delta) ? (vec.x / Mathf.Abs(vec.x)) : 0f, (Mathf.Abs(vec.y) > delta) ? (vec.y / Mathf.Abs(vec.y)) : 0f, (Mathf.Abs(vec.z) > delta) ? (vec.z / Mathf.Abs(vec.z)) : 0f);
	}

	internal static Vector3 Abs(this Vector3 v)
	{
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	internal static int IntSum(this Vector3 mask)
	{
		return (int)Mathf.Abs(mask.x) + (int)Mathf.Abs(mask.y) + (int)Mathf.Abs(mask.z);
	}

	internal static void Cross(Vector3 a, Vector3 b, ref float x, ref float y, ref float z)
	{
		x = a.y * b.z - a.z * b.y;
		y = a.z * b.x - a.x * b.z;
		z = a.x * b.y - a.y * b.x;
	}

	internal static void Cross(Vector3 a, Vector3 b, ref Vector3 res)
	{
		res.x = a.y * b.z - a.z * b.y;
		res.y = a.z * b.x - a.x * b.z;
		res.z = a.x * b.y - a.y * b.x;
	}

	internal static void Cross(float ax, float ay, float az, float bx, float by, float bz, ref float x, ref float y, ref float z)
	{
		x = ay * bz - az * by;
		y = az * bx - ax * bz;
		z = ax * by - ay * bx;
	}

	internal static void Add(Vector3 a, Vector3 b, ref Vector3 res)
	{
		res.x = a.x + b.x;
		res.y = a.y + b.y;
		res.z = a.z + b.z;
	}

	internal static void Subtract(Vector3 a, Vector3 b, ref Vector3 res)
	{
		res.x = b.x - a.x;
		res.y = b.y - a.y;
		res.z = b.z - a.z;
	}

	internal static void Divide(Vector3 vector, float value, ref Vector3 res)
	{
		res.x = vector.x / value;
		res.y = vector.y / value;
		res.z = vector.z / value;
	}

	internal static void Multiply(Vector3 vector, float value, ref Vector3 res)
	{
		res.x = vector.x * value;
		res.y = vector.y * value;
		res.z = vector.z * value;
	}

	internal static int Min(int a, int b)
	{
		if (a >= b)
		{
			return b;
		}
		return a;
	}

	internal static int Max(int a, int b)
	{
		if (a <= b)
		{
			return b;
		}
		return a;
	}

	internal static bool IsNumber(float value)
	{
		if (!float.IsInfinity(value))
		{
			return !float.IsNaN(value);
		}
		return false;
	}

	internal static bool IsNumber(Vector2 value)
	{
		if (IsNumber(value.x))
		{
			return IsNumber(value.y);
		}
		return false;
	}

	internal static bool IsNumber(Vector3 value)
	{
		if (IsNumber(value.x) && IsNumber(value.y))
		{
			return IsNumber(value.z);
		}
		return false;
	}

	internal static bool IsNumber(Vector4 value)
	{
		if (IsNumber(value.x) && IsNumber(value.y) && IsNumber(value.z))
		{
			return IsNumber(value.w);
		}
		return false;
	}

	internal static float MakeNonZero(float value, float min = 0.0001f)
	{
		if (float.IsNaN(value) || float.IsInfinity(value) || Mathf.Abs(value) < min)
		{
			return min * Mathf.Sign(value);
		}
		return value;
	}

	internal static bool VectorIsUniform(Vector3 vector)
	{
		if (Mathf.Abs(vector.x - vector.y) < Mathf.Epsilon)
		{
			return Mathf.Abs(vector.x - vector.z) < Mathf.Epsilon;
		}
		return false;
	}

	internal static Vector3 WeightedAverage(Vector3[] array, IList<int> indices, float[] weightLookup)
	{
		if (array == null || indices == null || weightLookup == null)
		{
			return Vector3.zero;
		}
		float num = 0f;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < indices.Count; i++)
		{
			float num2 = weightLookup[indices[i]];
			zero.x += array[indices[i]].x * num2;
			zero.y += array[indices[i]].y * num2;
			zero.z += array[indices[i]].z * num2;
			num += num2;
		}
		if (!(num > Mathf.Epsilon))
		{
			return Vector3.zero;
		}
		return zero /= num;
	}
}
