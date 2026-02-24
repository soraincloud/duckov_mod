using System;

namespace UnityEngine.ProBuilder.Shapes;

[Shape("Stairs")]
public class Stairs : Shape
{
	[SerializeField]
	private StepGenerationType m_StepGenerationType = StepGenerationType.Count;

	[Min(0.01f)]
	[SerializeField]
	private float m_StepsHeight = 0.2f;

	[Range(1f, 256f)]
	[SerializeField]
	private int m_StepsCount = 10;

	[SerializeField]
	private bool m_HomogeneousSteps = true;

	[Range(-360f, 360f)]
	[SerializeField]
	private float m_Circumference;

	[SerializeField]
	private bool m_Sides = true;

	[SerializeField]
	[Min(0f)]
	private float m_InnerRadius;

	public bool sides
	{
		get
		{
			return m_Sides;
		}
		set
		{
			m_Sides = value;
		}
	}

	public override void CopyShape(Shape shape)
	{
		if (shape is Stairs)
		{
			Stairs stairs = (Stairs)shape;
			m_StepGenerationType = stairs.m_StepGenerationType;
			m_StepsHeight = stairs.m_StepsHeight;
			m_StepsCount = stairs.m_StepsCount;
			m_HomogeneousSteps = stairs.m_HomogeneousSteps;
			m_Circumference = stairs.m_Circumference;
			m_Sides = stairs.m_Sides;
			m_InnerRadius = stairs.m_InnerRadius;
		}
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		if (Mathf.Abs(m_Circumference) > 0f)
		{
			return BuildCurvedStairs(mesh, size, rotation);
		}
		return BuildStairs(mesh, size, rotation);
	}

	public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
	{
		if (Mathf.Abs(m_Circumference) > 0f)
		{
			bounds.center = mesh.mesh.bounds.center;
			bounds.size = Vector3.Scale(size.Sign(), mesh.mesh.bounds.size);
		}
		else
		{
			bounds = mesh.mesh.bounds;
			bounds.size = size;
		}
		return bounds;
	}

	private Bounds BuildStairs(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		Vector3 vector = Vector3.Scale(rotation * Vector3.up, size);
		Vector3 vector2 = Vector3.Scale(rotation * Vector3.right, size);
		Vector3 vector3 = Vector3.Scale(rotation * Vector3.forward, size);
		Vector3 vector4 = new Vector3(vector2.magnitude, vector.magnitude, vector3.magnitude);
		bool flag = m_StepGenerationType == StepGenerationType.Height;
		float y = vector4.y;
		float num = Mathf.Min(m_StepsHeight, y);
		int num2 = m_StepsCount;
		if (flag)
		{
			if (y > 0f)
			{
				num2 = (int)(y / num);
				if (m_HomogeneousSteps)
				{
					num = y / (float)num2;
				}
				else
				{
					num2 += ((y / num - (float)num2 > 0.001f) ? 1 : 0);
				}
			}
			else
			{
				num2 = 1;
			}
		}
		if (num2 > 256)
		{
			num2 = 256;
			num = y / (float)num2;
		}
		Vector3[] array = new Vector3[4 * num2 * 2];
		Face[] array2 = new Face[num2 * 2];
		Vector3 vector5 = vector4 * 0.5f;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < num2; i++)
		{
			float num5 = (float)i * num;
			float num6 = ((i != num2 - 1) ? ((float)(i + 1) * num) : vector4.y);
			float num7 = (float)i / (float)num2;
			float num8 = (float)(i + 1) / (float)num2;
			float x = vector4.x - vector5.x;
			float x2 = 0f - vector5.x;
			float y2 = (flag ? num5 : (vector4.y * num7)) - vector5.y;
			float y3 = (flag ? num6 : (vector4.y * num8)) - vector5.y;
			float z = vector4.z * num7 - vector5.z;
			float z2 = vector4.z * num8 - vector5.z;
			array[num3] = new Vector3(x, y2, z);
			array[num3 + 1] = new Vector3(x2, y2, z);
			array[num3 + 2] = new Vector3(x, y3, z);
			array[num3 + 3] = new Vector3(x2, y3, z);
			array[num3 + 4] = new Vector3(x, y3, z);
			array[num3 + 5] = new Vector3(x2, y3, z);
			array[num3 + 6] = new Vector3(x, y3, z2);
			array[num3 + 7] = new Vector3(x2, y3, z2);
			array2[num4] = new Face(new int[6]
			{
				num3,
				num3 + 1,
				num3 + 2,
				num3 + 1,
				num3 + 3,
				num3 + 2
			});
			array2[num4 + 1] = new Face(new int[6]
			{
				num3 + 4,
				num3 + 5,
				num3 + 6,
				num3 + 5,
				num3 + 7,
				num3 + 6
			});
			num3 += 8;
			num4 += 2;
		}
		if (sides)
		{
			float num9 = 0f;
			for (int j = 0; j < 2; j++)
			{
				Vector3[] array3 = new Vector3[num2 * 4 + (num2 - 1) * 3];
				Face[] array4 = new Face[num2 + num2 - 1];
				int num10 = 0;
				int num11 = 0;
				for (int k = 0; k < num2; k++)
				{
					float num5 = (float)Mathf.Max(k, 1) * num;
					float num6 = ((k != num2 - 1) ? ((float)(k + 1) * num) : vector4.y);
					float num7 = (float)Mathf.Max(k, 1) / (float)num2;
					float num8 = (float)(k + 1) / (float)num2;
					float y2 = (flag ? num5 : (num7 * vector4.y));
					float y3 = (flag ? num6 : (num8 * vector4.y));
					num7 = (float)k / (float)num2;
					float z = num7 * vector4.z;
					float z2 = num8 * vector4.z;
					array3[num10] = new Vector3(num9, 0f, z) - vector5;
					array3[num10 + 1] = new Vector3(num9, 0f, z2) - vector5;
					array3[num10 + 2] = new Vector3(num9, y2, z) - vector5;
					array3[num10 + 3] = new Vector3(num9, y3, z2) - vector5;
					array4[num11++] = new Face((j % 2 != 0) ? new int[6]
					{
						num3 + 2,
						num3 + 1,
						num3,
						num3 + 2,
						num3 + 3,
						num3 + 1
					} : new int[6]
					{
						num3,
						num3 + 1,
						num3 + 2,
						num3 + 1,
						num3 + 3,
						num3 + 2
					});
					array4[num11 - 1].textureGroup = j + 1;
					num3 += 4;
					num10 += 4;
					if (k > 0)
					{
						array3[num10] = new Vector3(num9, y2, z) - vector5;
						array3[num10 + 1] = new Vector3(num9, y3, z) - vector5;
						array3[num10 + 2] = new Vector3(num9, y3, z2) - vector5;
						array4[num11++] = new Face((j % 2 != 0) ? new int[3]
						{
							num3,
							num3 + 1,
							num3 + 2
						} : new int[3]
						{
							num3 + 2,
							num3 + 1,
							num3
						});
						array4[num11 - 1].textureGroup = j + 1;
						num3 += 3;
						num10 += 3;
					}
				}
				array = array.Concat(array3);
				array2 = array2.Concat(array4);
				num9 += vector4.x;
			}
			array = array.Concat(new Vector3[4]
			{
				new Vector3(0f, 0f, vector4.z) - vector5,
				new Vector3(vector4.x, 0f, vector4.z) - vector5,
				new Vector3(0f, vector4.y, vector4.z) - vector5,
				new Vector3(vector4.x, vector4.y, vector4.z) - vector5
			});
			array2 = array2.Add(new Face(new int[6]
			{
				num3,
				num3 + 1,
				num3 + 2,
				num3 + 1,
				num3 + 3,
				num3 + 2
			}));
		}
		Vector3 scale = size.Sign();
		for (int l = 0; l < array.Length; l++)
		{
			array[l] = rotation * array[l];
			array[l].Scale(scale);
		}
		if (scale.x * scale.y * scale.z < 0f)
		{
			Face[] array5 = array2;
			for (int m = 0; m < array5.Length; m++)
			{
				array5[m].Reverse();
			}
		}
		mesh.RebuildWithPositionsAndFaces(array, array2);
		return UpdateBounds(mesh, size, rotation, default(Bounds));
	}

	private Bounds BuildCurvedStairs(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		Vector3 vector = size.Abs();
		bool flag = m_Sides;
		float num = Mathf.Min(vector.x, vector.z);
		float num2 = Mathf.Clamp(m_InnerRadius, 0f, num - float.Epsilon);
		float num3 = num - num2;
		float num4 = Mathf.Abs(vector.y);
		float circumference = m_Circumference;
		bool flag2 = num2 < Mathf.Epsilon;
		bool flag3 = m_StepGenerationType == StepGenerationType.Height;
		float num5 = Mathf.Min(m_StepsHeight, num4);
		int num6 = m_StepsCount;
		if (flag3 && num5 > 0.01f * m_StepsHeight)
		{
			if (num4 > 0f)
			{
				num6 = (int)(num4 / m_StepsHeight);
				if (m_HomogeneousSteps && num6 > 0)
				{
					num5 = num4 / (float)num6;
				}
				else
				{
					num6 += ((num4 / m_StepsHeight - (float)num6 > 0.001f) ? 1 : 0);
				}
			}
			else
			{
				num6 = 1;
			}
		}
		if (num6 > 256)
		{
			num6 = 256;
			num5 = num4 / (float)num6;
		}
		Vector3[] array = new Vector3[4 * num6 + (flag2 ? 3 : 4) * num6];
		Face[] array2 = new Face[num6 * 2];
		int num7 = 0;
		int num8 = 0;
		float num9 = Mathf.Abs(circumference) * (MathF.PI / 180f);
		float num10 = num2 + num3;
		for (int i = 0; i < num6; i++)
		{
			float num11 = (float)i / (float)num6 * num9;
			float num12 = (float)(i + 1) / (float)num6 * num9;
			float y = (flag3 ? ((float)i * num5) : ((float)i / (float)num6 * num4));
			float y2 = ((!flag3) ? ((float)(i + 1) / (float)num6 * num4) : ((i != num6 - 1) ? ((float)(i + 1) * num5) : num4));
			Vector3 vector2 = new Vector3(0f - Mathf.Cos(num11), 0f, Mathf.Sin(num11));
			Vector3 vector3 = new Vector3(0f - Mathf.Cos(num12), 0f, Mathf.Sin(num12));
			array[num7] = vector2 * num2;
			array[num7 + 1] = vector2 * num10;
			array[num7 + 2] = vector2 * num2;
			array[num7 + 3] = vector2 * num10;
			array[num7].y = y;
			array[num7 + 1].y = y;
			array[num7 + 2].y = y2;
			array[num7 + 3].y = y2;
			array[num7 + 4] = array[num7 + 2];
			array[num7 + 5] = array[num7 + 3];
			array[num7 + 6] = vector3 * num10;
			array[num7 + 6].y = y2;
			if (!flag2)
			{
				array[num7 + 7] = vector3 * num2;
				array[num7 + 7].y = y2;
			}
			array2[num8] = new Face(new int[6]
			{
				num7,
				num7 + 1,
				num7 + 2,
				num7 + 1,
				num7 + 3,
				num7 + 2
			});
			if (flag2)
			{
				array2[num8 + 1] = new Face(new int[3]
				{
					num7 + 4,
					num7 + 5,
					num7 + 6
				});
			}
			else
			{
				array2[num8 + 1] = new Face(new int[6]
				{
					num7 + 4,
					num7 + 5,
					num7 + 6,
					num7 + 4,
					num7 + 6,
					num7 + 7
				});
			}
			float num13 = (num12 + num11) * -0.5f * 57.29578f;
			num13 %= 360f;
			if (num13 < 0f)
			{
				num13 = 360f + num13;
			}
			AutoUnwrapSettings uv = array2[num8 + 1].uv;
			uv.rotation = num13;
			array2[num8 + 1].uv = uv;
			num7 += (flag2 ? 7 : 8);
			num8 += 2;
		}
		if (flag)
		{
			float num14 = (flag2 ? (num2 + num3) : num2);
			for (int j = (flag2 ? 1 : 0); j < 2; j++)
			{
				Vector3[] array3 = new Vector3[num6 * 4 + (num6 - 1) * 3];
				Face[] array4 = new Face[num6 + num6 - 1];
				int num15 = 0;
				int num16 = 0;
				for (int k = 0; k < num6; k++)
				{
					float f = (float)k / (float)num6 * num9;
					float f2 = (float)(k + 1) / (float)num6 * num9;
					float y3 = (flag3 ? ((float)Mathf.Max(k, 1) * num5) : ((float)Mathf.Max(k, 1) / (float)num6 * num4));
					float y4 = ((!flag3) ? ((float)(k + 1) / (float)num6 * num4) : ((k != num6 - 1) ? ((float)(k + 1) * num5) : vector.y));
					Vector3 vector4 = new Vector3(0f - Mathf.Cos(f), 0f, Mathf.Sin(f)) * num14;
					Vector3 vector5 = new Vector3(0f - Mathf.Cos(f2), 0f, Mathf.Sin(f2)) * num14;
					array3[num15] = vector4;
					array3[num15 + 1] = vector5;
					array3[num15 + 2] = vector4;
					array3[num15 + 3] = vector5;
					array3[num15].y = 0f;
					array3[num15 + 1].y = 0f;
					array3[num15 + 2].y = y3;
					array3[num15 + 3].y = y4;
					array4[num16++] = new Face((j % 2 != 0) ? new int[6]
					{
						num7,
						num7 + 1,
						num7 + 2,
						num7 + 1,
						num7 + 3,
						num7 + 2
					} : new int[6]
					{
						num7 + 2,
						num7 + 1,
						num7,
						num7 + 2,
						num7 + 3,
						num7 + 1
					});
					array4[num16 - 1].smoothingGroup = j + 1;
					num7 += 4;
					num15 += 4;
					if (k > 0)
					{
						array4[num16 - 1].textureGroup = j * num6 + k;
						array3[num15] = vector4;
						array3[num15 + 1] = vector5;
						array3[num15 + 2] = vector4;
						array3[num15].y = y3;
						array3[num15 + 1].y = y4;
						array3[num15 + 2].y = y4;
						array4[num16++] = new Face((j % 2 != 0) ? new int[3]
						{
							num7,
							num7 + 1,
							num7 + 2
						} : new int[3]
						{
							num7 + 2,
							num7 + 1,
							num7
						});
						array4[num16 - 1].textureGroup = j * num6 + k;
						array4[num16 - 1].smoothingGroup = j + 1;
						num7 += 3;
						num15 += 3;
					}
				}
				array = array.Concat(array3);
				array2 = array2.Concat(array4);
				num14 += num3;
			}
			float num17 = 0f - Mathf.Cos(num9);
			float num18 = Mathf.Sin(num9);
			array = array.Concat(new Vector3[4]
			{
				new Vector3(num17, 0f, num18) * num2,
				new Vector3(num17, 0f, num18) * num10,
				new Vector3(num17 * num2, num4, num18 * num2),
				new Vector3(num17 * num10, num4, num18 * num10)
			});
			array2 = array2.Add(new Face(new int[6]
			{
				num7 + 2,
				num7 + 1,
				num7,
				num7 + 2,
				num7 + 3,
				num7 + 1
			}));
		}
		if (circumference < 0f)
		{
			Vector3 scale = new Vector3(-1f, 1f, 1f);
			for (int l = 0; l < array.Length; l++)
			{
				array[l].Scale(scale);
			}
			Face[] array5 = array2;
			for (int m = 0; m < array5.Length; m++)
			{
				array5[m].Reverse();
			}
		}
		Vector3 scale2 = size.Sign();
		for (int n = 0; n < array.Length; n++)
		{
			array[n] = rotation * array[n];
			array[n].Scale(scale2);
		}
		if (scale2.x * scale2.y * scale2.z < 0f)
		{
			Face[] array5 = array2;
			for (int m = 0; m < array5.Length; m++)
			{
				array5[m].Reverse();
			}
		}
		mesh.RebuildWithPositionsAndFaces(array, array2);
		mesh.TranslateVerticesInWorldSpace(mesh.mesh.triangles, mesh.transform.TransformDirection(-mesh.mesh.bounds.center));
		mesh.Refresh();
		return UpdateBounds(mesh, size, rotation, default(Bounds));
	}
}
