
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Audio Source
    [Header("----------Audio Source---------------")]
    [SerializeField] AudioSource musicSource; 
    [SerializeField] AudioSource SFXSource; 
    //Audio Clip
    [Header("----------Audio Clip---------------")]
    public AudioClip background;
    public AudioClip death;
    public AudioClip checkpoint;
    public AudioClip run;
    public AudioClip jump;
    public AudioClip dash;
    public AudioClip TpIn;
    public AudioClip TpOut;



    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
