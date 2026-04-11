using UnityEngine;

public class StayInPlace : MonoBehaviour
{
    [Header("----------AudioSource---------")]
   [SerializeField] AudioSource musicSource;
   [SerializeField] AudioSource SFXSource;

   
    [Header("----------AudioClip---------")]
    public AudioClip Background;
    public AudioClip Death;
   public AudioClip Jump;
   public AudioClip Hit;
   public AudioClip Checkpoint;
   public AudioClip ConsoleWin;
   public AudioClip ConsoleLose;
   public AudioClip Clock;
   public AudioClip Trampoline;
    
    private void Start()
    {
        musicSource.clip = Background;
        musicSource.Play();
    }
}