using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UnityEngine.Rendering;

public class DebugUI
{
	public class Container : Widget, IContainer
	{
		private const string k_IDToken = "#";

		internal bool hideDisplayName
		{
			get
			{
				if (!string.IsNullOrEmpty(base.displayName))
				{
					return base.displayName.StartsWith("#");
				}
				return true;
			}
		}

		public ObservableList<Widget> children { get; private set; }

		public override Panel panel
		{
			get
			{
				return m_Panel;
			}
			internal set
			{
				if (value == null || !value.flags.HasFlag(Flags.FrequentlyUsed))
				{
					m_Panel = value;
					int count = children.Count;
					for (int i = 0; i < count; i++)
					{
						children[i].panel = value;
					}
				}
			}
		}

		public Container()
			: this(string.Empty, new ObservableList<Widget>())
		{
		}

		public Container(string id)
			: this("#" + id, new ObservableList<Widget>())
		{
		}

		public Container(string displayName, ObservableList<Widget> children)
		{
			base.displayName = displayName;
			this.children = children;
			children.ItemAdded += OnItemAdded;
			children.ItemRemoved += OnItemRemoved;
			for (int i = 0; i < this.children.Count; i++)
			{
				OnItemAdded(this.children, new ListChangedEventArgs<Widget>(i, this.children[i]));
			}
		}

		internal override void GenerateQueryPath()
		{
			base.GenerateQueryPath();
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				children[i].GenerateQueryPath();
			}
		}

		protected virtual void OnItemAdded(ObservableList<Widget> sender, ListChangedEventArgs<Widget> e)
		{
			if (e.item != null)
			{
				e.item.panel = m_Panel;
				e.item.parent = this;
			}
			if (m_Panel != null)
			{
				m_Panel.SetDirty();
			}
		}

		protected virtual void OnItemRemoved(ObservableList<Widget> sender, ListChangedEventArgs<Widget> e)
		{
			if (e.item != null)
			{
				e.item.panel = null;
				e.item.parent = null;
			}
			if (m_Panel != null)
			{
				m_Panel.SetDirty();
			}
		}

		public override int GetHashCode()
		{
			int num = 17;
			num = num * 23 + base.queryPath.GetHashCode();
			num = num * 23 + base.isHidden.GetHashCode();
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				num = num * 23 + children[i].GetHashCode();
			}
			return num;
		}
	}

	public class Foldout : Container, IValueField
	{
		public struct ContextMenuItem
		{
			public string displayName;

			public Action action;
		}

		public bool opened;

		public bool isHeader;

		public List<ContextMenuItem> contextMenuItems;

		public bool isReadOnly => false;

		public string[] columnLabels { get; set; }

		public string[] columnTooltips { get; set; }

		public Foldout()
		{
		}

		public Foldout(string displayName, ObservableList<Widget> children, string[] columnLabels = null, string[] columnTooltips = null)
			: base(displayName, children)
		{
			this.columnLabels = columnLabels;
			this.columnTooltips = columnTooltips;
		}

		public bool GetValue()
		{
			return opened;
		}

		object IValueField.GetValue()
		{
			return GetValue();
		}

		public void SetValue(object value)
		{
			SetValue((bool)value);
		}

		public object ValidateValue(object value)
		{
			return value;
		}

		public void SetValue(bool value)
		{
			opened = value;
		}
	}

	public class HBox : Container
	{
		public HBox()
		{
			base.displayName = "HBox";
		}
	}

	public class VBox : Container
	{
		public VBox()
		{
			base.displayName = "VBox";
		}
	}

	public class Table : Container
	{
		public class Row : Foldout
		{
			public Row()
			{
				base.displayName = "Row";
			}
		}

		public bool isReadOnly;

		private bool[] m_Header;

		public bool[] VisibleColumns
		{
			get
			{
				if (m_Header != null)
				{
					return m_Header;
				}
				int num = 0;
				if (base.children.Count != 0)
				{
					num = ((Container)base.children[0]).children.Count;
					for (int i = 1; i < base.children.Count; i++)
					{
						if (((Container)base.children[i]).children.Count != num)
						{
							Debug.LogError("All rows must have the same number of children.");
							return null;
						}
					}
				}
				m_Header = new bool[num];
				for (int j = 0; j < num; j++)
				{
					m_Header[j] = true;
				}
				return m_Header;
			}
		}

		public Table()
		{
			base.displayName = "Array";
		}

		public void SetColumnVisibility(int index, bool visible)
		{
			bool[] visibleColumns = VisibleColumns;
			if (index >= 0 && index <= visibleColumns.Length)
			{
				visibleColumns[index] = visible;
			}
		}

		public bool GetColumnVisibility(int index)
		{
			bool[] visibleColumns = VisibleColumns;
			if (index < 0 || index > visibleColumns.Length)
			{
				return false;
			}
			return visibleColumns[index];
		}

		protected override void OnItemAdded(ObservableList<Widget> sender, ListChangedEventArgs<Widget> e)
		{
			base.OnItemAdded(sender, e);
			m_Header = null;
		}

		protected override void OnItemRemoved(ObservableList<Widget> sender, ListChangedEventArgs<Widget> e)
		{
			base.OnItemRemoved(sender, e);
			m_Header = null;
		}
	}

	[Flags]
	public enum Flags
	{
		None = 0,
		EditorOnly = 2,
		RuntimeOnly = 4,
		EditorForceUpdate = 8,
		FrequentlyUsed = 0x10
	}

	public abstract class Widget
	{
		public struct NameAndTooltip
		{
			public string name;

			public string tooltip;
		}

		protected Panel m_Panel;

		protected IContainer m_Parent;

		public Func<bool> isHiddenCallback;

		public virtual Panel panel
		{
			get
			{
				return m_Panel;
			}
			internal set
			{
				m_Panel = value;
			}
		}

		public virtual IContainer parent
		{
			get
			{
				return m_Parent;
			}
			internal set
			{
				m_Parent = value;
			}
		}

		public Flags flags { get; set; }

		public string displayName { get; set; }

		public string tooltip { get; set; }

		public string queryPath { get; private set; }

		public bool isEditorOnly => flags.HasFlag(Flags.EditorOnly);

		public bool isRuntimeOnly => flags.HasFlag(Flags.RuntimeOnly);

		public bool isInactiveInEditor
		{
			get
			{
				if (isRuntimeOnly)
				{
					return !Application.isPlaying;
				}
				return false;
			}
		}

		public bool isHidden => isHiddenCallback?.Invoke() ?? false;

		public NameAndTooltip nameAndTooltip
		{
			set
			{
				displayName = value.name;
				tooltip = value.tooltip;
			}
		}

		internal virtual void GenerateQueryPath()
		{
			queryPath = displayName.Trim();
			if (m_Parent != null)
			{
				queryPath = m_Parent.queryPath + " -> " + queryPath;
			}
		}

		public override int GetHashCode()
		{
			return queryPath.GetHashCode() ^ isHidden.GetHashCode();
		}
	}

	public interface IContainer
	{
		ObservableList<Widget> children { get; }

		string displayName { get; set; }

		string queryPath { get; }
	}

	public interface IValueField
	{
		object GetValue();

		void SetValue(object value);

		object ValidateValue(object value);
	}

	public class Button : Widget
	{
		public Action action { get; set; }
	}

	public class Value : Widget
	{
		public float refreshRate = 0.1f;

		public string formatString;

		public Func<object> getter { get; set; }

		public Value()
		{
			base.displayName = "";
		}

		public virtual object GetValue()
		{
			return getter();
		}

		public virtual string FormatString(object value)
		{
			if (!string.IsNullOrEmpty(formatString))
			{
				return string.Format(formatString, value);
			}
			return $"{value}";
		}
	}

	public class ProgressBarValue : Value
	{
		public float min;

		public float max = 1f;

		public override string FormatString(object value)
		{
			float num = Remap(Mathf.Clamp((float)value, min, max), min, max);
			return $"{num:P1}";
			static float Remap(float v, float x0, float y0)
			{
				return (v - x0) / (y0 - x0);
			}
		}
	}

	public class ValueTuple : Widget
	{
		public Value[] values;

		public int pinnedElementIndex = -1;

		public int numElements => values.Length;

		public float refreshRate => values.FirstOrDefault()?.refreshRate ?? 0.1f;
	}

	public abstract class Field<T> : Widget, IValueField
	{
		public Action<Field<T>, T> onValueChanged;

		public Func<T> getter { get; set; }

		public Action<T> setter { get; set; }

		object IValueField.ValidateValue(object value)
		{
			return ValidateValue((T)value);
		}

		public virtual T ValidateValue(T value)
		{
			return value;
		}

		object IValueField.GetValue()
		{
			return GetValue();
		}

		public T GetValue()
		{
			return getter();
		}

		public void SetValue(object value)
		{
			SetValue((T)value);
		}

		public virtual void SetValue(T value)
		{
			T val = ValidateValue(value);
			if (val == null || !val.Equals(getter()))
			{
				setter(val);
				onValueChanged?.Invoke(this, val);
			}
		}
	}

	public class BoolField : Field<bool>
	{
	}

	public class HistoryBoolField : BoolField
	{
		public Func<bool>[] historyGetter { get; set; }

		public int historyDepth
		{
			get
			{
				Func<bool>[] array = historyGetter;
				if (array == null)
				{
					return 0;
				}
				return array.Length;
			}
		}

		public bool GetHistoryValue(int historyIndex)
		{
			return historyGetter[historyIndex]();
		}
	}

	public class IntField : Field<int>
	{
		public Func<int> min;

		public Func<int> max;

		public int incStep = 1;

		public int intStepMult = 10;

		public override int ValidateValue(int value)
		{
			if (min != null)
			{
				value = Mathf.Max(value, min());
			}
			if (max != null)
			{
				value = Mathf.Min(value, max());
			}
			return value;
		}
	}

	public class UIntField : Field<uint>
	{
		public Func<uint> min;

		public Func<uint> max;

		public uint incStep = 1u;

		public uint intStepMult = 10u;

		public override uint ValidateValue(uint value)
		{
			if (min != null)
			{
				value = (uint)Mathf.Max((int)value, (int)min());
			}
			if (max != null)
			{
				value = (uint)Mathf.Min((int)value, (int)max());
			}
			return value;
		}
	}

	public class FloatField : Field<float>
	{
		public Func<float> min;

		public Func<float> max;

		public float incStep = 0.1f;

		public float incStepMult = 10f;

		public int decimals = 3;

		public override float ValidateValue(float value)
		{
			if (min != null)
			{
				value = Mathf.Max(value, min());
			}
			if (max != null)
			{
				value = Mathf.Min(value, max());
			}
			return value;
		}
	}

	public abstract class EnumField<T> : Field<T>
	{
		public GUIContent[] enumNames;

		private int[] m_EnumValues;

		private static Regex s_NicifyRegEx = new Regex("([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", RegexOptions.Compiled);

		public int[] enumValues
		{
			get
			{
				return m_EnumValues;
			}
			set
			{
				if (value?.Distinct().Count() != value?.Count())
				{
					Debug.LogWarning(base.displayName + " - The values of the enum are duplicated, this might lead to a errors displaying the enum");
				}
				m_EnumValues = value;
			}
		}

		protected void AutoFillFromType(Type enumType)
		{
			if (enumType == null || !enumType.IsEnum)
			{
				throw new ArgumentException("enumType must not be null and it must be an Enum type");
			}
			List<GUIContent> value;
			using (ListPool<GUIContent>.Get(out value))
			{
				List<int> value2;
				using (ListPool<int>.Get(out value2))
				{
					foreach (FieldInfo item2 in from fieldInfo in enumType.GetFields(BindingFlags.Static | BindingFlags.Public)
						where !fieldInfo.IsDefined(typeof(ObsoleteAttribute)) && !fieldInfo.IsDefined(typeof(HideInInspector))
						select fieldInfo)
					{
						InspectorNameAttribute customAttribute = item2.GetCustomAttribute<InspectorNameAttribute>();
						GUIContent item = new GUIContent((customAttribute == null) ? s_NicifyRegEx.Replace(item2.Name, "$1 ") : customAttribute.displayName);
						value.Add(item);
						value2.Add((int)Enum.Parse(enumType, item2.Name));
					}
					enumNames = value.ToArray();
					enumValues = value2.ToArray();
				}
			}
		}
	}

	public class EnumField : EnumField<int>
	{
		internal int[] quickSeparators;

		private int[] m_Indexes;

		internal int[] indexes
		{
			get
			{
				int[] array = m_Indexes;
				if (array == null)
				{
					GUIContent[] array2 = enumNames;
					array = (m_Indexes = Enumerable.Range(0, (array2 != null) ? array2.Length : 0).ToArray());
				}
				return array;
			}
		}

		public Func<int> getIndex { get; set; }

		public Action<int> setIndex { get; set; }

		public int currentIndex
		{
			get
			{
				return getIndex();
			}
			set
			{
				setIndex(value);
			}
		}

		public Type autoEnum
		{
			set
			{
				AutoFillFromType(value);
				InitQuickSeparators();
			}
		}

		internal void InitQuickSeparators()
		{
			IEnumerable<string> source = enumNames.Select(delegate(GUIContent x)
			{
				string[] array = x.text.Split('/');
				return (array.Length == 1) ? "" : array[0];
			});
			quickSeparators = new int[source.Distinct().Count()];
			string text = null;
			int num = 0;
			int num2 = 0;
			for (; num < quickSeparators.Length; num++)
			{
				string text2 = source.ElementAt(num2);
				while (text == text2)
				{
					text2 = source.ElementAt(++num2);
				}
				text = text2;
				quickSeparators[num] = num2++;
			}
		}

		public override void SetValue(int value)
		{
			int num = ValidateValue(value);
			int num2 = Array.IndexOf(base.enumValues, num);
			if (currentIndex != num2 && !num.Equals(base.getter()))
			{
				base.setter(num);
				onValueChanged?.Invoke(this, num);
				if (num2 > -1)
				{
					currentIndex = num2;
				}
			}
		}
	}

	public class ObjectPopupField : Field<Object>
	{
		public Func<IEnumerable<Object>> getObjects { get; set; }
	}

	public class HistoryEnumField : EnumField
	{
		public Func<int>[] historyIndexGetter { get; set; }

		public int historyDepth
		{
			get
			{
				Func<int>[] array = historyIndexGetter;
				if (array == null)
				{
					return 0;
				}
				return array.Length;
			}
		}

		public int GetHistoryValue(int historyIndex)
		{
			return historyIndexGetter[historyIndex]();
		}
	}

	public class BitField : EnumField<Enum>
	{
		private Type m_EnumType;

		public Type enumType
		{
			get
			{
				return m_EnumType;
			}
			set
			{
				m_EnumType = value;
				AutoFillFromType(value);
			}
		}
	}

	public class ColorField : Field<Color>
	{
		public bool hdr;

		public bool showAlpha = true;

		public bool showPicker = true;

		public float incStep = 0.025f;

		public float incStepMult = 5f;

		public int decimals = 3;

		public override Color ValidateValue(Color value)
		{
			if (!hdr)
			{
				value.r = Mathf.Clamp01(value.r);
				value.g = Mathf.Clamp01(value.g);
				value.b = Mathf.Clamp01(value.b);
				value.a = Mathf.Clamp01(value.a);
			}
			return value;
		}
	}

	public class Vector2Field : Field<Vector2>
	{
		public float incStep = 0.025f;

		public float incStepMult = 10f;

		public int decimals = 3;
	}

	public class Vector3Field : Field<Vector3>
	{
		public float incStep = 0.025f;

		public float incStepMult = 10f;

		public int decimals = 3;
	}

	public class Vector4Field : Field<Vector4>
	{
		public float incStep = 0.025f;

		public float incStepMult = 10f;

		public int decimals = 3;
	}

	public class ObjectField : Field<Object>
	{
		public Type type = typeof(Object);
	}

	public class ObjectListField : Field<Object[]>
	{
		public Type type = typeof(Object);
	}

	public class MessageBox : Widget
	{
		public enum Style
		{
			Info,
			Warning,
			Error
		}

		public Style style;
	}

	public class Panel : IContainer, IComparable<Panel>
	{
		public Flags flags { get; set; }

		public string displayName { get; set; }

		public int groupIndex { get; set; }

		public string queryPath => displayName;

		public bool isEditorOnly => (flags & Flags.EditorOnly) != 0;

		public bool isRuntimeOnly => (flags & Flags.RuntimeOnly) != 0;

		public bool isInactiveInEditor
		{
			get
			{
				if (isRuntimeOnly)
				{
					return !Application.isPlaying;
				}
				return false;
			}
		}

		public bool editorForceUpdate => (flags & Flags.EditorForceUpdate) != 0;

		public ObservableList<Widget> children { get; private set; }

		public event Action<Panel> onSetDirty = delegate
		{
		};

		public Panel()
		{
			children = new ObservableList<Widget>();
			children.ItemAdded += OnItemAdded;
			children.ItemRemoved += OnItemRemoved;
		}

		protected virtual void OnItemAdded(ObservableList<Widget> sender, ListChangedEventArgs<Widget> e)
		{
			if (e.item != null)
			{
				e.item.panel = this;
				e.item.parent = this;
			}
			SetDirty();
		}

		protected virtual void OnItemRemoved(ObservableList<Widget> sender, ListChangedEventArgs<Widget> e)
		{
			if (e.item != null)
			{
				e.item.panel = null;
				e.item.parent = null;
			}
			SetDirty();
		}

		public void SetDirty()
		{
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				children[i].GenerateQueryPath();
			}
			this.onSetDirty(this);
		}

		public override int GetHashCode()
		{
			int num = 17;
			num = num * 23 + displayName.GetHashCode();
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				num = num * 23 + children[i].GetHashCode();
			}
			return num;
		}

		int IComparable<Panel>.CompareTo(Panel other)
		{
			if (other != null)
			{
				return groupIndex.CompareTo(other.groupIndex);
			}
			return 1;
		}
	}
}
