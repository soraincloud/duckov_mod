namespace UnityEngine.UIElements;

internal class VisualElementFocusChangeTarget : FocusChangeDirection
{
	private static readonly ObjectPool<VisualElementFocusChangeTarget> Pool = new ObjectPool<VisualElementFocusChangeTarget>(() => new VisualElementFocusChangeTarget());

	public Focusable target { get; private set; }

	public static VisualElementFocusChangeTarget GetPooled(Focusable target)
	{
		VisualElementFocusChangeTarget visualElementFocusChangeTarget = Pool.Get();
		visualElementFocusChangeTarget.target = target;
		return visualElementFocusChangeTarget;
	}

	protected override void Dispose()
	{
		target = null;
		Pool.Release(this);
	}

	internal override void ApplyTo(FocusController focusController, Focusable f)
	{
		focusController.selectedTextElement = null;
		f.Focus();
	}

	public VisualElementFocusChangeTarget()
		: base(FocusChangeDirection.unspecified)
	{
	}
}
