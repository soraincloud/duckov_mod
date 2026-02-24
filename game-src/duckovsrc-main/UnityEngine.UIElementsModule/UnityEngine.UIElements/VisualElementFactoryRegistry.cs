using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

internal class VisualElementFactoryRegistry
{
	private static Dictionary<string, List<IUxmlFactory>> s_Factories;

	private static Dictionary<string, List<IUxmlFactory>> s_MovedTypesFactories;

	internal static Dictionary<string, List<IUxmlFactory>> factories
	{
		get
		{
			if (s_Factories == null)
			{
				s_Factories = new Dictionary<string, List<IUxmlFactory>>();
				s_MovedTypesFactories = new Dictionary<string, List<IUxmlFactory>>(50);
				RegisterEngineFactories();
				RegisterUserFactories();
			}
			return s_Factories;
		}
	}

	private static string GetMovedUIControlTypeName(Type type, MovedFromAttribute attr)
	{
		if (type == null)
		{
			return string.Empty;
		}
		MovedFromAttributeData data = attr.data;
		string text = (data.nameSpaceHasChanged ? data.nameSpace : VisualElement.GetOrCreateTypeData(type).typeNamespace);
		string text2 = (data.classHasChanged ? data.className : VisualElement.GetOrCreateTypeData(type).typeName);
		return text + "." + text2;
	}

	protected static void RegisterFactory(IUxmlFactory factory)
	{
		if (factories.TryGetValue(factory.uxmlQualifiedName, out var value))
		{
			foreach (IUxmlFactory item in value)
			{
				if (item.GetType() == factory.GetType())
				{
					throw new ArgumentException("A factory for the type " + factory.GetType().FullName + " was already registered");
				}
			}
			value.Add(factory);
			return;
		}
		value = new List<IUxmlFactory>();
		value.Add(factory);
		factories.Add(factory.uxmlQualifiedName, value);
		Type uxmlType = factory.uxmlType;
		MovedFromAttribute movedFromAttribute = uxmlType?.GetCustomAttribute<MovedFromAttribute>(inherit: false);
		if (movedFromAttribute != null && typeof(VisualElement).IsAssignableFrom(uxmlType))
		{
			string movedUIControlTypeName = GetMovedUIControlTypeName(uxmlType, movedFromAttribute);
			if (!string.IsNullOrEmpty(movedUIControlTypeName))
			{
				s_MovedTypesFactories.Add(movedUIControlTypeName, value);
			}
		}
	}

	internal static bool TryGetValue(string fullTypeName, out List<IUxmlFactory> factoryList)
	{
		bool flag = factories.TryGetValue(fullTypeName, out factoryList);
		if (!flag)
		{
			flag = s_MovedTypesFactories.TryGetValue(fullTypeName, out factoryList);
		}
		return flag;
	}

	internal static bool TryGetValue(Type type, out List<IUxmlFactory> factoryList)
	{
		foreach (List<IUxmlFactory> value in factories.Values)
		{
			if (value[0].uxmlType == type)
			{
				factoryList = value;
				return true;
			}
		}
		factoryList = null;
		return false;
	}

	private static void RegisterEngineFactories()
	{
		IUxmlFactory[] array = new IUxmlFactory[52]
		{
			new UxmlRootElementFactory(),
			new UxmlTemplateFactory(),
			new UxmlStyleFactory(),
			new UxmlAttributeOverridesFactory(),
			new Button.UxmlFactory(),
			new VisualElement.UxmlFactory(),
			new IMGUIContainer.UxmlFactory(),
			new Image.UxmlFactory(),
			new Label.UxmlFactory(),
			new RepeatButton.UxmlFactory(),
			new ScrollView.UxmlFactory(),
			new Scroller.UxmlFactory(),
			new Slider.UxmlFactory(),
			new SliderInt.UxmlFactory(),
			new MinMaxSlider.UxmlFactory(),
			new GroupBox.UxmlFactory(),
			new RadioButton.UxmlFactory(),
			new RadioButtonGroup.UxmlFactory(),
			new Toggle.UxmlFactory(),
			new TextField.UxmlFactory(),
			new TemplateContainer.UxmlFactory(),
			new Box.UxmlFactory(),
			new EnumField.UxmlFactory(),
			new DropdownField.UxmlFactory(),
			new HelpBox.UxmlFactory(),
			new PopupWindow.UxmlFactory(),
			new ProgressBar.UxmlFactory(),
			new ListView.UxmlFactory(),
			new TwoPaneSplitView.UxmlFactory(),
			new TreeView.UxmlFactory(),
			new Foldout.UxmlFactory(),
			new MultiColumnListView.UxmlFactory(),
			new MultiColumnTreeView.UxmlFactory(),
			new BindableElement.UxmlFactory(),
			new TextElement.UxmlFactory(),
			new ButtonStripField.UxmlFactory(),
			new FloatField.UxmlFactory(),
			new DoubleField.UxmlFactory(),
			new Hash128Field.UxmlFactory(),
			new IntegerField.UxmlFactory(),
			new LongField.UxmlFactory(),
			new UnsignedIntegerField.UxmlFactory(),
			new UnsignedLongField.UxmlFactory(),
			new RectField.UxmlFactory(),
			new Vector2Field.UxmlFactory(),
			new RectIntField.UxmlFactory(),
			new Vector3Field.UxmlFactory(),
			new Vector4Field.UxmlFactory(),
			new Vector2IntField.UxmlFactory(),
			new Vector3IntField.UxmlFactory(),
			new BoundsField.UxmlFactory(),
			new BoundsIntField.UxmlFactory()
		};
		IUxmlFactory[] array2 = array;
		foreach (IUxmlFactory factory in array2)
		{
			RegisterFactory(factory);
		}
	}

	internal static void RegisterUserFactories()
	{
		HashSet<string> hashSet = new HashSet<string>(ScriptingRuntime.GetAllUserAssemblies());
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			if (!hashSet.Contains(assembly.GetName().Name + ".dll") || assembly.GetName().Name == "UnityEngine.UIElementsModule")
			{
				continue;
			}
			Type[] types = assembly.GetTypes();
			Type[] array2 = types;
			foreach (Type type in array2)
			{
				if (typeof(IUxmlFactory).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract && !type.IsGenericType)
				{
					IUxmlFactory factory = (IUxmlFactory)Activator.CreateInstance(type);
					RegisterFactory(factory);
				}
			}
		}
	}
}
