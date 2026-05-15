using UnityEngine.Events;

public interface ICorrectAnswerNotifier
{
    UnityEvent OnCorrectAnswer { get; }
}