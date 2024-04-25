using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{   
    public bool newQuestion = false;
    private Renderer rend;
    private AudioSource audioSource;
    
    public event EventHandler<QuestionParameters> addedClip;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rend = GetComponent<Renderer>();
        addedClip += ChangeColor;
        addedClip += SetNewQuestion;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && audioSource.clip != null)
        {
            audioSource.Play();
            ChangeColor(Color.blue);
            SetNewQuestion(false);
        }

        if (!(audioSource.isPlaying || newQuestion))
        {
            ChangeColor(Color.red);
        }
    }

    public void AddAudioClip(AudioClip clip)
    {
        audioSource.clip = clip;
        addedClip?.Invoke(this, new QuestionParameters{color = Color.green, newQuestion = true});
    }

    void ChangeColor(Color color)
    {   
        rend.material.color = color;
    }

    void ChangeColor(object sender, QuestionParameters e)
    {   
        Debug.Log("Color changed to " + e.color);
        ChangeColor(e.color);
    }

    void SetNewQuestion(bool newQuestion)
    {
        this.newQuestion = newQuestion;
    }

    void SetNewQuestion(object sender, QuestionParameters e)
    {
        SetNewQuestion(e.newQuestion);
    }

}

public class QuestionParameters : System.EventArgs
{
    public Color color { get; set; }
    public bool newQuestion { get; set; }
}
