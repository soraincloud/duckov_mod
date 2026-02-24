using Unity.Properties.Internal;

namespace Unity.Properties;

public readonly struct VisitContext<TContainer, TValue>
{
	private readonly ReadOnlyAdapterCollection.Enumerator m_Enumerator;

	private readonly PropertyVisitor m_Visitor;

	public Property<TContainer, TValue> Property { get; }

	internal static VisitContext<TContainer, TValue> FromProperty(PropertyVisitor visitor, ReadOnlyAdapterCollection.Enumerator enumerator, Property<TContainer, TValue> property)
	{
		return new VisitContext<TContainer, TValue>(visitor, enumerator, property);
	}

	private VisitContext(PropertyVisitor visitor, ReadOnlyAdapterCollection.Enumerator enumerator, Property<TContainer, TValue> property)
	{
		m_Visitor = visitor;
		m_Enumerator = enumerator;
		Property = property;
	}

	public void ContinueVisitation(ref TContainer container, ref TValue value)
	{
		m_Visitor.ContinueVisitation(Property, m_Enumerator, ref container, ref value);
	}

	public void ContinueVisitationWithoutAdapters(ref TContainer container, ref TValue value)
	{
		m_Visitor.ContinueVisitationWithoutAdapters(Property, m_Enumerator, ref container, ref value);
	}
}
public readonly struct VisitContext<TContainer>
{
	private delegate void VisitDelegate(PropertyVisitor visitor, ReadOnlyAdapterCollection.Enumerator enumerator, IProperty<TContainer> property, ref TContainer container);

	private delegate void VisitWithoutAdaptersDelegate(PropertyVisitor visitor, IProperty<TContainer> property, ref TContainer container);

	private readonly ReadOnlyAdapterCollection.Enumerator m_Enumerator;

	private readonly PropertyVisitor m_Visitor;

	private readonly VisitDelegate m_Continue;

	private readonly VisitWithoutAdaptersDelegate m_ContinueWithoutAdapters;

	public IProperty<TContainer> Property { get; }

	internal static VisitContext<TContainer> FromProperty<TValue>(PropertyVisitor visitor, ReadOnlyAdapterCollection.Enumerator enumerator, Property<TContainer, TValue> property)
	{
		return new VisitContext<TContainer>(visitor, enumerator, property, delegate(PropertyVisitor v, ReadOnlyAdapterCollection.Enumerator e, IProperty<TContainer> p, ref TContainer c)
		{
			Property<TContainer, TValue> property2 = (Property<TContainer, TValue>)p;
			TValue value = property2.GetValue(ref c);
			v.ContinueVisitation(property2, e, ref c, ref value);
		}, delegate(PropertyVisitor v, IProperty<TContainer> p, ref TContainer c)
		{
			Property<TContainer, TValue> property2 = (Property<TContainer, TValue>)p;
			TValue value = property2.GetValue(ref c);
			v.ContinueVisitation(property2, ref c, ref value);
		});
	}

	private VisitContext(PropertyVisitor visitor, ReadOnlyAdapterCollection.Enumerator enumerator, IProperty<TContainer> property, VisitDelegate continueVisitation, VisitWithoutAdaptersDelegate continueVisitationWithoutAdapters)
	{
		m_Visitor = visitor;
		m_Enumerator = enumerator;
		Property = property;
		m_Continue = continueVisitation;
		m_ContinueWithoutAdapters = continueVisitationWithoutAdapters;
	}

	public void ContinueVisitation(ref TContainer container)
	{
		m_Continue(m_Visitor, m_Enumerator, Property, ref container);
	}

	public void ContinueVisitationWithoutAdapters(ref TContainer container)
	{
		m_ContinueWithoutAdapters(m_Visitor, Property, ref container);
	}
}
