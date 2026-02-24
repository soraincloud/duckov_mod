using System;
using System.Collections.Generic;

namespace UnityEngine.TextCore.Text;

[Serializable]
public class UnicodeLineBreakingRules
{
	[SerializeField]
	private UnityEngine.TextAsset m_UnicodeLineBreakingRules;

	[SerializeField]
	private UnityEngine.TextAsset m_LeadingCharacters;

	[SerializeField]
	private UnityEngine.TextAsset m_FollowingCharacters;

	[SerializeField]
	private bool m_UseModernHangulLineBreakingRules;

	private HashSet<uint> m_LeadingCharactersLookup;

	private HashSet<uint> m_FollowingCharactersLookup;

	public UnityEngine.TextAsset lineBreakingRules => m_UnicodeLineBreakingRules;

	public UnityEngine.TextAsset leadingCharacters => m_LeadingCharacters;

	public UnityEngine.TextAsset followingCharacters => m_FollowingCharacters;

	internal HashSet<uint> leadingCharactersLookup
	{
		get
		{
			if (m_LeadingCharactersLookup == null)
			{
				LoadLineBreakingRules(leadingCharacters, followingCharacters);
			}
			return m_LeadingCharactersLookup;
		}
		set
		{
			m_LeadingCharactersLookup = value;
		}
	}

	internal HashSet<uint> followingCharactersLookup
	{
		get
		{
			if (m_LeadingCharactersLookup == null)
			{
				LoadLineBreakingRules(leadingCharacters, followingCharacters);
			}
			return m_FollowingCharactersLookup;
		}
		set
		{
			m_FollowingCharactersLookup = value;
		}
	}

	public bool useModernHangulLineBreakingRules
	{
		get
		{
			return m_UseModernHangulLineBreakingRules;
		}
		set
		{
			m_UseModernHangulLineBreakingRules = value;
		}
	}

	internal void LoadLineBreakingRules()
	{
		if (m_LeadingCharactersLookup == null)
		{
			if (m_LeadingCharacters == null)
			{
				m_LeadingCharacters = Resources.Load<UnityEngine.TextAsset>("LineBreaking Leading Characters");
			}
			m_LeadingCharactersLookup = ((m_LeadingCharacters != null) ? GetCharacters(m_LeadingCharacters) : new HashSet<uint>());
			if (m_FollowingCharacters == null)
			{
				m_FollowingCharacters = Resources.Load<UnityEngine.TextAsset>("LineBreaking Following Characters");
			}
			m_FollowingCharactersLookup = ((m_FollowingCharacters != null) ? GetCharacters(m_FollowingCharacters) : new HashSet<uint>());
		}
	}

	internal void LoadLineBreakingRules(UnityEngine.TextAsset leadingRules, UnityEngine.TextAsset followingRules)
	{
		if (m_LeadingCharactersLookup == null)
		{
			if (leadingRules == null)
			{
				leadingRules = Resources.Load<UnityEngine.TextAsset>("LineBreaking Leading Characters");
			}
			m_LeadingCharactersLookup = ((leadingRules != null) ? GetCharacters(leadingRules) : new HashSet<uint>());
			if (followingRules == null)
			{
				followingRules = Resources.Load<UnityEngine.TextAsset>("LineBreaking Following Characters");
			}
			m_FollowingCharactersLookup = ((followingRules != null) ? GetCharacters(followingRules) : new HashSet<uint>());
		}
	}

	private static HashSet<uint> GetCharacters(UnityEngine.TextAsset file)
	{
		HashSet<uint> hashSet = new HashSet<uint>();
		string text = file.text;
		for (int i = 0; i < text.Length; i++)
		{
			hashSet.Add(text[i]);
		}
		return hashSet;
	}
}
