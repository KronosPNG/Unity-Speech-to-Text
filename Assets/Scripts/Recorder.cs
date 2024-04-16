using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Recorder : MonoBehaviour
{
    private AudioSource audioIn;
    public UnityWebRequest www;

    void Start()
    {
        audioIn = GetComponent<AudioSource>();
        // StartRecording();       
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            StopRecording();
        }

        if(Microphone.IsRecording(null))
        {
            Debug.Log("Recording in progress");
        }
        
        if (Input.GetKey(KeyCode.P))
        {
            PlayAudio();
        }
    }

    void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.Log("No microphone found");
            return;
        }
        
        audioIn.clip = Microphone.Start(null, false, 3, 44100);
        // audioIn.Play();
        Debug.Log("Recording started");
    }

    void StopRecording()
    {
        Microphone.End(null);
        Debug.Log("Recording stopped");

    }

    void PlayAudio()
    {
        Debug.Log("Playing audio");
        audioIn.Play();
    }

    private IEnumerator SendAudioToServer(string url = "https://127.0.0.1:5000")
    {
        float[] audioData = new float[audioIn.clip.samples * audioIn.clip.channels];
        audioIn.clip.GetData(audioData, 0);
        byte[] bytes = new byte[audioData.Length * 4];
        Buffer.BlockCopy(audioData, 0, bytes, 0, bytes.Length);
        
        www = new UnityWebRequest(url, "POST");

        www.uploadHandler = new UploadHandlerRaw(bytes);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "audio/wav");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Audio recording sent!");
            Debug.Log("Response: " + www.downloadHandler.text);
        }
    }
}
