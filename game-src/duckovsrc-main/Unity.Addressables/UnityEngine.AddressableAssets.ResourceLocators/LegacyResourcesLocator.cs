using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UnityEngine.AddressableAssets.ResourceLocators;

public class LegacyResourcesLocator : IResourceLocator
{
	public IEnumerable<object> Keys => null;

	public string LocatorId => "LegacyResourcesLocator";

	public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
	{
		locations = null;
		if (!(key is string id))
		{
			return false;
		}
		locations = new List<IResourceLocation>();
		locations.Add(new ResourceLocationBase("LegacyResourceLocation", id, typeof(LegacyResourcesProvider).FullName, typeof(Object)));
		return true;
	}
}
