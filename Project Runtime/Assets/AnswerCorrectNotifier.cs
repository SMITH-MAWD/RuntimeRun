using UnityEngine.Events;

public interface IAnswerCorrectNotifier
{
    UnityEvent OnCorrectAnswer { get; }
}