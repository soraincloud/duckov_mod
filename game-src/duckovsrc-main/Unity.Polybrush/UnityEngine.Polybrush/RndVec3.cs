using System;

namespace UnityEngine.Polybrush;

public struct RndVec3 : IEquatable<RndVec3>
{
	internal float x;

	internal float y;

	internal float z;

	private const float resolution = 0.0001f;

	internal RndVec3(Vector3 vector)
	{
		x = vector.x;
		y = vector.y;
		z = vector.z;
	}

	public bool Equals(RndVec3 p)
	{
		if (Mathf.Abs(x - p.x) < 0.0001f && Mathf.Abs(y - p.y) < 0.0001f)
		{
			return Mathf.Abs(z - p.z) < 0.0001f;
		}
		return false;
	}

	public bool Equals(Vector3 p)
	{
		if (Mathf.Abs(x - p.x) < 0.0001f && Mathf.Abs(y - p.y) < 0.0001f)
		{
			return Mathf.Abs(z - p.z) < 0.0001f;
		}
		return false;
	}

	public override bool Equals(object b)
	{
		if (!(b is RndVec3) || !Equals((RndVec3)b))
		{
			if (b is Vector3)
			{
				return Equals((Vector3)b);
			}
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return ((27 * 29 + round(x)) * 29 + round(y)) * 29 + round(z);
	}

	public override string ToString()
	{
		return $"{{{x:F2}, {y:F2}, {z:F2}}}";
	}

	private int round(float v)
	{
		return (int)(v / 0.0001f);
	}

	public static implicit operator Vector3(RndVec3 p)
	{
		return new Vector3(p.x, p.y, p.z);
	}

	public static implicit operator RndVec3(Vector3 p)
	{
		return new RndVec3(p);
	}
}
