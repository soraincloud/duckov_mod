using System;

namespace UnityEngine.UIElements;

public interface ITextEdition
{
	internal bool multiline { get; set; }

	bool isReadOnly { get; set; }

	int maxLength { get; set; }

	bool isDelayed { get; set; }

	internal Func<char, bool> AcceptCharacter { get; set; }

	internal Action<bool> UpdateScrollOffset { get; set; }

	internal Action UpdateValueFromText { get; set; }

	internal Action UpdateTextFromValue { get; set; }

	internal Action MoveFocusToCompositeRoot { get; set; }

	char maskChar { get; set; }

	bool isPassword { get; set; }

	bool autoCorrection
	{
		get
		{
			Debug.Log("Type " + GetType().Name + " implementing interface ITextEdition is missing the implementation for autoCorrection. Calling ITextEdition.autoCorrection of this type will always return false.");
			return false;
		}
		set
		{
			Debug.Log("Type " + GetType().Name + " implementing interface ITextEdition is missing the implementation for autoCorrection. Assigning a value to ITextEdition.autoCorrection will not update its value.");
		}
	}

	bool hideMobileInput
	{
		get
		{
			Debug.Log("Type " + GetType().Name + " implementing interface ITextEdition is missing the implementation for hideMobileInput. Calling ITextEdition.hideMobileInput of this type will always return false.");
			return false;
		}
		set
		{
			Debug.Log("Type " + GetType().Name + " implementing interface ITextEdition is missing the implementation for hideMobileInput. Assigning a value to ITextEdition.hideMobileInput will not update its value.");
		}
	}

	TouchScreenKeyboard touchScreenKeyboard
	{
		get
		{
			Debug.Log("Type " + GetType().Name + " implementing interface ITextEdition is missing the implementation for touchScreenKeyboard. Calling ITextEdition.touchScreenKeyboard of this type will always return null.");
			return null;
		}
	}

	TouchScreenKeyboardType keyboardType
	{
		get
		{
			Debug.Log("Type " + GetType().Name + " implementing interface ITextEdition is missing the implementation for keyboardType. Calling ITextEdition.keyboardType of this type will always return Default.");
			return TouchScreenKeyboardType.Default;
		}
		set
		{
			Debug.Log("Type " + GetType().Name + " implementing interface ITextEdition is missing the implementation for keyboardType. Assigning a value to ITextEdition.keyboardType will not update its value.");
		}
	}

	internal void ResetValueAndText();

	internal void SaveValueAndText();

	internal void RestoreValueAndText();

	internal void UpdateText(string value);

	internal string CullString(string s);
}
