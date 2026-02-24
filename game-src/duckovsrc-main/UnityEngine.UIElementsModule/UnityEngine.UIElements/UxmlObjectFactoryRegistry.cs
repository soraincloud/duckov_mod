using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.UIElements;

internal class UxmlObjectFactoryRegistry
{
	private static Dictionary<string, List<IBaseUxmlObjectFactory>> s_Factories;

	internal static Dictionary<string, List<IBaseUxmlObjectFactory>> factories
	{
		get
		{
			if (s_Factories == null)
			{
				s_Factories = new Dictionary<string, List<IBaseUxmlObjectFactory>>();
				RegisterEngineFactories();
				RegisterUserFactories();
			}
			return s_Factories;
		}
	}

	protected static void RegisterFactory(IBaseUxmlObjectFactory factory)
	{
		if (factories.TryGetValue(factory.uxmlQualifiedName, out var value))
		{
			foreach (IBaseUxmlObjectFactory item in value)
			{
				if (item.GetType() == factory.GetType())
				{
					throw new ArgumentException("A factory for the type " + factory.GetType().FullName + " was already registered");
				}
			}
			value.Add(factory);
		}
		else
		{
			value = new List<IBaseUxmlObjectFactory> { factory };
			factories.Add(factory.uxmlQualifiedName, value);
		}
	}

	internal static bool TryGetFactories(string fullTypeName, out List<IBaseUxmlObjectFactory> factoryList)
	{
		return factories.TryGetValue(fullTypeName, out factoryList);
	}

	private static void RegisterEngineFactories()
	{
		IBaseUxmlObjectFactory[] array = new IBaseUxmlObjectFactory[4]
		{
			new Columns.UxmlObjectFactory<Columns>(),
			new Column.UxmlObjectFactory<Column>(),
			new SortColumnDescriptions.UxmlObjectFactory<SortColumnDescriptions>(),
			new SortColumnDescription.UxmlObjectFactory<SortColumnDescription>()
		};
		IBaseUxmlObjectFactory[] array2 = array;
		foreach (IBaseUxmlObjectFactory factory in array2)
		{
			RegisterFactory(factory);
		}
	}

	private static void RegisterUserFactories()
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
				if (typeof(IBaseUxmlObjectFactory).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract && !type.IsGenericType)
				{
					IBaseUxmlObjectFactory factory = (IBaseUxmlObjectFactory)Activator.CreateInstance(type);
					RegisterFactory(factory);
				}
			}
		}
	}
}
