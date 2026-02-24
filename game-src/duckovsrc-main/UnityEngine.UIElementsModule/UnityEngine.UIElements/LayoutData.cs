using System;

namespace UnityEngine.UIElements;

internal struct LayoutData : IStyleDataGroup<LayoutData>, IEquatable<LayoutData>
{
	public Align alignContent;

	public Align alignItems;

	public Align alignSelf;

	public float borderBottomWidth;

	public float borderLeftWidth;

	public float borderRightWidth;

	public float borderTopWidth;

	public Length bottom;

	public DisplayStyle display;

	public Length flexBasis;

	public FlexDirection flexDirection;

	public float flexGrow;

	public float flexShrink;

	public Wrap flexWrap;

	public Length height;

	public Justify justifyContent;

	public Length left;

	public Length marginBottom;

	public Length marginLeft;

	public Length marginRight;

	public Length marginTop;

	public Length maxHeight;

	public Length maxWidth;

	public Length minHeight;

	public Length minWidth;

	public Length paddingBottom;

	public Length paddingLeft;

	public Length paddingRight;

	public Length paddingTop;

	public Position position;

	public Length right;

	public Length top;

	public Length width;

	public LayoutData Copy()
	{
		return this;
	}

	public void CopyFrom(ref LayoutData other)
	{
		this = other;
	}

	public static bool operator ==(LayoutData lhs, LayoutData rhs)
	{
		return lhs.alignContent == rhs.alignContent && lhs.alignItems == rhs.alignItems && lhs.alignSelf == rhs.alignSelf && lhs.borderBottomWidth == rhs.borderBottomWidth && lhs.borderLeftWidth == rhs.borderLeftWidth && lhs.borderRightWidth == rhs.borderRightWidth && lhs.borderTopWidth == rhs.borderTopWidth && lhs.bottom == rhs.bottom && lhs.display == rhs.display && lhs.flexBasis == rhs.flexBasis && lhs.flexDirection == rhs.flexDirection && lhs.flexGrow == rhs.flexGrow && lhs.flexShrink == rhs.flexShrink && lhs.flexWrap == rhs.flexWrap && lhs.height == rhs.height && lhs.justifyContent == rhs.justifyContent && lhs.left == rhs.left && lhs.marginBottom == rhs.marginBottom && lhs.marginLeft == rhs.marginLeft && lhs.marginRight == rhs.marginRight && lhs.marginTop == rhs.marginTop && lhs.maxHeight == rhs.maxHeight && lhs.maxWidth == rhs.maxWidth && lhs.minHeight == rhs.minHeight && lhs.minWidth == rhs.minWidth && lhs.paddingBottom == rhs.paddingBottom && lhs.paddingLeft == rhs.paddingLeft && lhs.paddingRight == rhs.paddingRight && lhs.paddingTop == rhs.paddingTop && lhs.position == rhs.position && lhs.right == rhs.right && lhs.top == rhs.top && lhs.width == rhs.width;
	}

	public static bool operator !=(LayoutData lhs, LayoutData rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(LayoutData other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is LayoutData && Equals((LayoutData)obj);
	}

	public override int GetHashCode()
	{
		int num = (int)alignContent;
		num = (num * 397) ^ (int)alignItems;
		num = (num * 397) ^ (int)alignSelf;
		num = (num * 397) ^ borderBottomWidth.GetHashCode();
		num = (num * 397) ^ borderLeftWidth.GetHashCode();
		num = (num * 397) ^ borderRightWidth.GetHashCode();
		num = (num * 397) ^ borderTopWidth.GetHashCode();
		num = (num * 397) ^ bottom.GetHashCode();
		num = (num * 397) ^ (int)display;
		num = (num * 397) ^ flexBasis.GetHashCode();
		num = (num * 397) ^ (int)flexDirection;
		num = (num * 397) ^ flexGrow.GetHashCode();
		num = (num * 397) ^ flexShrink.GetHashCode();
		num = (num * 397) ^ (int)flexWrap;
		num = (num * 397) ^ height.GetHashCode();
		num = (num * 397) ^ (int)justifyContent;
		num = (num * 397) ^ left.GetHashCode();
		num = (num * 397) ^ marginBottom.GetHashCode();
		num = (num * 397) ^ marginLeft.GetHashCode();
		num = (num * 397) ^ marginRight.GetHashCode();
		num = (num * 397) ^ marginTop.GetHashCode();
		num = (num * 397) ^ maxHeight.GetHashCode();
		num = (num * 397) ^ maxWidth.GetHashCode();
		num = (num * 397) ^ minHeight.GetHashCode();
		num = (num * 397) ^ minWidth.GetHashCode();
		num = (num * 397) ^ paddingBottom.GetHashCode();
		num = (num * 397) ^ paddingLeft.GetHashCode();
		num = (num * 397) ^ paddingRight.GetHashCode();
		num = (num * 397) ^ paddingTop.GetHashCode();
		num = (num * 397) ^ (int)position;
		num = (num * 397) ^ right.GetHashCode();
		num = (num * 397) ^ top.GetHashCode();
		return (num * 397) ^ width.GetHashCode();
	}
}
