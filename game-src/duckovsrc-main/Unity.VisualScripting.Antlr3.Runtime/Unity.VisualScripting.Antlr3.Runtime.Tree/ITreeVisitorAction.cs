namespace Unity.VisualScripting.Antlr3.Runtime.Tree;

public interface ITreeVisitorAction
{
	object Pre(object t);

	object Post(object t);
}
