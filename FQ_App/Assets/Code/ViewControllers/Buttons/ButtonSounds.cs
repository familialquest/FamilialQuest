using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

class ButtonSounds : MonoBehaviour
{
    public AudioClip sound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = sound;
        audioSource.playOnAwake = false;

        //gameObject.GetComponent<Button>().onClick.AddListener(() => OnClick_PlaySound());
    }

    public void OnClick_PlaySound()
    {
        //Handheld.Vibrate();

        //try
        //{            
        //    audioSource.PlayOneShot(sound);
        //}
        //catch(Exception ex)
        //{
        //    var a = ex;
        //}
    }
}

