using System;

namespace UnityEngine.UIElements;

internal static class MeshGenerationContextUtils
{
	public struct BorderParams
	{
		public Rect rect;

		public Color playmodeTintColor;

		public Color leftColor;

		public Color topColor;

		public Color rightColor;

		public Color bottomColor;

		public float leftWidth;

		public float topWidth;

		public float rightWidth;

		public float bottomWidth;

		public Vector2 topLeftRadius;

		public Vector2 topRightRadius;

		public Vector2 bottomRightRadius;

		public Vector2 bottomLeftRadius;

		public Material material;

		internal ColorPage leftColorPage;

		internal ColorPage topColorPage;

		internal ColorPage rightColorPage;

		internal ColorPage bottomColorPage;

		internal MeshBuilderNative.NativeBorderParams ToNativeParams()
		{
			return new MeshBuilderNative.NativeBorderParams
			{
				rect = rect,
				leftColor = leftColor,
				topColor = topColor,
				rightColor = rightColor,
				bottomColor = bottomColor,
				leftWidth = leftWidth,
				topWidth = topWidth,
				rightWidth = rightWidth,
				bottomWidth = bottomWidth,
				topLeftRadius = topLeftRadius,
				topRightRadius = topRightRadius,
				bottomRightRadius = bottomRightRadius,
				bottomLeftRadius = bottomLeftRadius,
				leftColorPage = leftColorPage.ToNativeColorPage(),
				topColorPage = topColorPage.ToNativeColorPage(),
				rightColorPage = rightColorPage.ToNativeColorPage(),
				bottomColorPage = bottomColorPage.ToNativeColorPage()
			};
		}
	}

	public struct RectangleParams
	{
		public Rect rect;

		public Rect uv;

		public Color color;

		public Rect subRect;

		public Rect backgroundRepeatRect;

		public BackgroundPosition backgroundPositionX;

		public BackgroundPosition backgroundPositionY;

		public BackgroundRepeat backgroundRepeat;

		public BackgroundSize backgroundSize;

		public Texture texture;

		public Sprite sprite;

		public VectorImage vectorImage;

		public Material material;

		public ScaleMode scaleMode;

		public Color playmodeTintColor;

		public Vector2 topLeftRadius;

		public Vector2 topRightRadius;

		public Vector2 bottomRightRadius;

		public Vector2 bottomLeftRadius;

		public Vector2 contentSize;

		public Vector2 textureSize;

		public int leftSlice;

		public int topSlice;

		public int rightSlice;

		public int bottomSlice;

		public float sliceScale;

		internal Rect spriteGeomRect;

		public Vector4 rectInset;

		internal ColorPage colorPage;

		internal MeshGenerationContext.MeshFlags meshFlags;

		public static RectangleParams MakeSolid(Rect rect, Color color, ContextType panelContext)
		{
			Color color2 = ((panelContext == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white);
			return new RectangleParams
			{
				rect = rect,
				color = color,
				uv = new Rect(0f, 0f, 1f, 1f),
				playmodeTintColor = color2
			};
		}

		private static void AdjustUVsForScaleMode(Rect rect, Rect uv, Texture texture, ScaleMode scaleMode, out Rect rectOut, out Rect uvOut)
		{
			float num = Mathf.Abs((float)texture.width * uv.width / ((float)texture.height * uv.height));
			float num2 = rect.width / rect.height;
			switch (scaleMode)
			{
			case ScaleMode.ScaleAndCrop:
				if (num2 > num)
				{
					float num5 = uv.height * (num / num2);
					float num6 = (uv.height - num5) * 0.5f;
					uv = new Rect(uv.x, uv.y + num6, uv.width, num5);
				}
				else
				{
					float num7 = uv.width * (num2 / num);
					float num8 = (uv.width - num7) * 0.5f;
					uv = new Rect(uv.x + num8, uv.y, num7, uv.height);
				}
				break;
			case ScaleMode.ScaleToFit:
				if (num2 > num)
				{
					float num3 = num / num2;
					rect = new Rect(rect.xMin + rect.width * (1f - num3) * 0.5f, rect.yMin, num3 * rect.width, rect.height);
				}
				else
				{
					float num4 = num2 / num;
					rect = new Rect(rect.xMin, rect.yMin + rect.height * (1f - num4) * 0.5f, rect.width, num4 * rect.height);
				}
				break;
			default:
				throw new NotImplementedException();
			case ScaleMode.StretchToFill:
				break;
			}
			rectOut = rect;
			uvOut = uv;
		}

		private static void AdjustSpriteUVsForScaleMode(Rect containerRect, Rect srcRect, Rect spriteGeomRect, Sprite sprite, ScaleMode scaleMode, out Rect rectOut, out Rect uvOut)
		{
			float num = sprite.rect.width / sprite.rect.height;
			float num2 = containerRect.width / containerRect.height;
			Rect rect = spriteGeomRect;
			rect.position -= (Vector2)sprite.bounds.min;
			rect.position /= (Vector2)sprite.bounds.size;
			rect.size /= (Vector2)sprite.bounds.size;
			Vector2 position = rect.position;
			position.y = 1f - rect.size.y - position.y;
			rect.position = position;
			switch (scaleMode)
			{
			case ScaleMode.StretchToFill:
			{
				Vector2 size2 = containerRect.size;
				containerRect.position = rect.position * size2;
				containerRect.size = rect.size * size2;
				break;
			}
			case ScaleMode.ScaleAndCrop:
			{
				Rect b = containerRect;
				if (num2 > num)
				{
					b.height = b.width / num;
					b.position = new Vector2(b.position.x, (0f - (b.height - containerRect.height)) / 2f);
				}
				else
				{
					b.width = b.height * num;
					b.position = new Vector2((0f - (b.width - containerRect.width)) / 2f, b.position.y);
				}
				Vector2 size = b.size;
				b.position += rect.position * size;
				b.size = rect.size * size;
				Rect rect2 = RectIntersection(containerRect, b);
				if (rect2.width < 1E-30f || rect2.height < 1E-30f)
				{
					rect2 = Rect.zero;
				}
				else
				{
					Rect rect3 = rect2;
					rect3.position -= b.position;
					rect3.position /= b.size;
					rect3.size /= b.size;
					Vector2 position2 = rect3.position;
					position2.y = 1f - rect3.size.y - position2.y;
					rect3.position = position2;
					srcRect.position += rect3.position * srcRect.size;
					srcRect.size *= rect3.size;
				}
				containerRect = rect2;
				break;
			}
			case ScaleMode.ScaleToFit:
				if (num2 > num)
				{
					float num3 = num / num2;
					containerRect = new Rect(containerRect.xMin + containerRect.width * (1f - num3) * 0.5f, containerRect.yMin, num3 * containerRect.width, containerRect.height);
				}
				else
				{
					float num4 = num2 / num;
					containerRect = new Rect(containerRect.xMin, containerRect.yMin + containerRect.height * (1f - num4) * 0.5f, containerRect.width, num4 * containerRect.height);
				}
				containerRect.position += rect.position * containerRect.size;
				containerRect.size *= rect.size;
				break;
			default:
				throw new NotImplementedException();
			}
			rectOut = containerRect;
			uvOut = srcRect;
		}

		internal static Rect RectIntersection(Rect a, Rect b)
		{
			Rect zero = Rect.zero;
			zero.min = Vector2.Max(a.min, b.min);
			zero.max = Vector2.Min(a.max, b.max);
			zero.size = Vector2.Max(zero.size, Vector2.zero);
			return zero;
		}

		private static Rect ComputeGeomRect(Sprite sprite)
		{
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			Vector2[] vertices = sprite.vertices;
			foreach (Vector2 rhs in vertices)
			{
				vector = Vector2.Min(vector, rhs);
				vector2 = Vector2.Max(vector2, rhs);
			}
			return new Rect(vector, vector2 - vector);
		}

		private static Rect ComputeUVRect(Sprite sprite)
		{
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			Vector2[] array = sprite.uv;
			foreach (Vector2 rhs in array)
			{
				vector = Vector2.Min(vector, rhs);
				vector2 = Vector2.Max(vector2, rhs);
			}
			return new Rect(vector, vector2 - vector);
		}

		private static Rect ApplyPackingRotation(Rect uv, SpritePackingRotation rotation)
		{
			switch (rotation)
			{
			case SpritePackingRotation.FlipHorizontal:
			{
				uv.position += new Vector2(uv.size.x, 0f);
				Vector2 size2 = uv.size;
				size2.x = 0f - size2.x;
				uv.size = size2;
				break;
			}
			case SpritePackingRotation.FlipVertical:
			{
				uv.position += new Vector2(0f, uv.size.y);
				Vector2 size = uv.size;
				size.y = 0f - size.y;
				uv.size = size;
				break;
			}
			case SpritePackingRotation.Rotate180:
				uv.position += uv.size;
				uv.size = -uv.size;
				break;
			}
			return uv;
		}

		public static RectangleParams MakeTextured(Rect rect, Rect uv, Texture texture, ScaleMode scaleMode, ContextType panelContext)
		{
			Color color = ((panelContext == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white);
			AdjustUVsForScaleMode(rect, uv, texture, scaleMode, out rect, out uv);
			Vector2 vector = new Vector2(texture.width, texture.height);
			return new RectangleParams
			{
				rect = rect,
				subRect = new Rect(0f, 0f, 1f, 1f),
				uv = uv,
				color = Color.white,
				texture = texture,
				contentSize = vector,
				textureSize = vector,
				scaleMode = scaleMode,
				playmodeTintColor = color
			};
		}

		public static RectangleParams MakeSprite(Rect containerRect, Rect subRect, Sprite sprite, ScaleMode scaleMode, ContextType panelContext, bool hasRadius, ref Vector4 slices, bool useForRepeat = false)
		{
			if (sprite == null || sprite.bounds.size.x < 1E-30f || sprite.bounds.size.y < 1E-30f)
			{
				return default(RectangleParams);
			}
			if (sprite.texture == null)
			{
				Debug.LogWarning("Ignoring textureless sprite named \"" + sprite.name + "\", please import as a VectorImage instead");
				return default(RectangleParams);
			}
			Color color = ((panelContext == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white);
			Rect rect = ComputeGeomRect(sprite);
			Rect rect2 = ComputeUVRect(sprite);
			Vector4 border = sprite.border;
			bool flag = border != Vector4.zero || slices != Vector4.zero;
			bool flag2 = subRect != new Rect(0f, 0f, 1f, 1f);
			bool flag3 = scaleMode == ScaleMode.ScaleAndCrop || flag || hasRadius || useForRepeat || flag2;
			if (flag3 && sprite.packed && sprite.packingRotation != SpritePackingRotation.None)
			{
				rect2 = ApplyPackingRotation(rect2, sprite.packingRotation);
			}
			Rect srcRect;
			if (flag2)
			{
				srcRect = subRect;
				srcRect.position *= rect2.size;
				srcRect.position += rect2.position;
				srcRect.size *= rect2.size;
			}
			else
			{
				srcRect = rect2;
			}
			AdjustSpriteUVsForScaleMode(containerRect, srcRect, rect, sprite, scaleMode, out var rectOut, out var uvOut);
			Rect rect3 = rect;
			rect3.size /= (Vector2)sprite.bounds.size;
			rect3.position -= (Vector2)sprite.bounds.min;
			rect3.position /= (Vector2)sprite.bounds.size;
			rect3.position = new Vector2(rect3.position.x, 1f - (rect3.position.y + rect3.height));
			RectangleParams result = new RectangleParams
			{
				rect = rectOut,
				uv = uvOut,
				subRect = rect3,
				color = Color.white,
				texture = (flag3 ? sprite.texture : null),
				sprite = (flag3 ? null : sprite),
				contentSize = sprite.rect.size,
				textureSize = new Vector2(sprite.texture.width, sprite.texture.height),
				spriteGeomRect = rect,
				scaleMode = scaleMode,
				playmodeTintColor = color,
				meshFlags = (sprite.packed ? MeshGenerationContext.MeshFlags.SkipDynamicAtlas : MeshGenerationContext.MeshFlags.None)
			};
			Vector4 vector = new Vector4(border.x, border.w, border.z, border.y);
			if (slices != Vector4.zero && vector != Vector4.zero && vector != slices)
			{
				Debug.LogWarning($"Sprite \"{sprite.name}\" borders {vector} are overridden by style slices {slices}");
			}
			else if (slices == Vector4.zero)
			{
				slices = vector;
			}
			return result;
		}

		public static RectangleParams MakeVectorTextured(Rect rect, Rect uv, VectorImage vectorImage, ScaleMode scaleMode, ContextType panelContext)
		{
			Color color = ((panelContext == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white);
			return new RectangleParams
			{
				rect = rect,
				subRect = new Rect(0f, 0f, 1f, 1f),
				uv = uv,
				color = Color.white,
				vectorImage = vectorImage,
				contentSize = new Vector2(vectorImage.width, vectorImage.height),
				scaleMode = scaleMode,
				playmodeTintColor = color
			};
		}

		internal bool HasRadius(float epsilon)
		{
			return (topLeftRadius.x > epsilon && topLeftRadius.y > epsilon) || (topRightRadius.x > epsilon && topRightRadius.y > epsilon) || (bottomRightRadius.x > epsilon && bottomRightRadius.y > epsilon) || (bottomLeftRadius.x > epsilon && bottomLeftRadius.y > epsilon);
		}

		internal bool HasSlices(float epsilon)
		{
			return (float)leftSlice > epsilon || (float)topSlice > epsilon || (float)rightSlice > epsilon || (float)bottomSlice > epsilon;
		}

		internal MeshBuilderNative.NativeRectParams ToNativeParams(Rect uvRegion)
		{
			return new MeshBuilderNative.NativeRectParams
			{
				rect = rect,
				subRect = subRect,
				backgroundRepeatRect = backgroundRepeatRect,
				uv = uv,
				uvRegion = uvRegion,
				color = color,
				scaleMode = scaleMode,
				topLeftRadius = topLeftRadius,
				topRightRadius = topRightRadius,
				bottomRightRadius = bottomRightRadius,
				bottomLeftRadius = bottomLeftRadius,
				contentSize = contentSize,
				textureSize = textureSize,
				texturePixelsPerPoint = 1f,
				leftSlice = leftSlice,
				topSlice = topSlice,
				rightSlice = rightSlice,
				bottomSlice = bottomSlice,
				sliceScale = sliceScale,
				rectInset = rectInset,
				colorPage = colorPage.ToNativeColorPage()
			};
		}
	}

	public static void Rectangle(this MeshGenerationContext mgc, RectangleParams rectParams)
	{
		mgc.painter.DrawRectangle(rectParams);
	}

	public static void Border(this MeshGenerationContext mgc, BorderParams borderParams)
	{
		mgc.painter.DrawBorder(borderParams);
	}

	public static void Text(this MeshGenerationContext mgc, TextElement te)
	{
		if (TextUtilities.IsFontAssigned(te))
		{
			mgc.painter.DrawText(te);
		}
	}

	private static Vector2 ConvertBorderRadiusPercentToPoints(Vector2 borderRectSize, Length length)
	{
		float a = length.value;
		float a2 = length.value;
		if (length.unit == LengthUnit.Percent)
		{
			a = borderRectSize.x * length.value / 100f;
			a2 = borderRectSize.y * length.value / 100f;
		}
		a = Mathf.Max(a, 0f);
		a2 = Mathf.Max(a2, 0f);
		return new Vector2(a, a2);
	}

	public static void GetVisualElementRadii(VisualElement ve, out Vector2 topLeft, out Vector2 bottomLeft, out Vector2 topRight, out Vector2 bottomRight)
	{
		IResolvedStyle resolvedStyle = ve.resolvedStyle;
		Vector2 borderRectSize = new Vector2(resolvedStyle.width, resolvedStyle.height);
		ComputedStyle computedStyle = ve.computedStyle;
		topLeft = ConvertBorderRadiusPercentToPoints(borderRectSize, computedStyle.borderTopLeftRadius);
		bottomLeft = ConvertBorderRadiusPercentToPoints(borderRectSize, computedStyle.borderBottomLeftRadius);
		topRight = ConvertBorderRadiusPercentToPoints(borderRectSize, computedStyle.borderTopRightRadius);
		bottomRight = ConvertBorderRadiusPercentToPoints(borderRectSize, computedStyle.borderBottomRightRadius);
	}

	public static void AdjustBackgroundSizeForBorders(VisualElement visualElement, ref RectangleParams rectParams)
	{
		IResolvedStyle resolvedStyle = visualElement.resolvedStyle;
		Vector4 zero = Vector4.zero;
		if (resolvedStyle.borderLeftWidth >= 1f && resolvedStyle.borderLeftColor.a >= 1f)
		{
			zero.x = 0.5f;
		}
		if (resolvedStyle.borderTopWidth >= 1f && resolvedStyle.borderTopColor.a >= 1f)
		{
			zero.y = 0.5f;
		}
		if (resolvedStyle.borderRightWidth >= 1f && resolvedStyle.borderRightColor.a >= 1f)
		{
			zero.z = 0.5f;
		}
		if (resolvedStyle.borderBottomWidth >= 1f && resolvedStyle.borderBottomColor.a >= 1f)
		{
			zero.w = 0.5f;
		}
		rectParams.rectInset = zero;
	}
}
