using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

public class Recorder : MonoBehaviour
{
    private AudioSource audioIn,
                        audioOut;
    public UnityWebRequest www;

    [Serializable]
    private class TestData{
        public string test;
    }

    void Start()
    {
        audioIn = GetComponent<AudioSource>();
        audioOut = GetComponent<AudioSource>();
        StartCoroutine(SendTextToServer());  
        StartRecording();    
    }

    void Update()
    {   
        // if (Input.GetKey(KeyCode.Space))
        // {
        //     StopRecording();
        // }

        if (Input.GetKeyDown(KeyCode.P) && audioIn.clip != null)
        {
            PlayAudio(audioIn);
        }

        if (Input.GetKeyDown(KeyCode.O) && audioOut.clip != null)
        {
            PlayAudio(audioOut);
        }

        if (Input.GetKeyDown(KeyCode.Space) && audioIn.clip != null)
        {
            StartCoroutine(SendAudioToServer());
        }
    }

    void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.Log("No microphone found");
            return;
        }
        
        audioIn.clip = Microphone.Start(null, false, 5, 44100);
        Debug.Log("Recording started");
    }

    void StopRecording()
    {
        Microphone.End(null);
        Debug.Log("Recording stopped");

    }

    void PlayAudio(AudioSource audio)
    {
        Debug.Log("Playing audio");
        audio.Play();
    }

    private IEnumerator SendTextToServer(string url = "http://127.0.0.1:5000/test")
    {  

        var data = new TestData {
            test = "TEST MESSAGE!"
        };

        string json = JsonUtility.ToJson(data);

        Debug.Log("JSON: " + json);

        // Convert the JSON string to a byte array
        byte[] bytes = Encoding.UTF8.GetBytes(json);


        www = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bytes),
            downloadHandler = new DownloadHandlerBuffer()
        };
        
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Text sent!");
            Debug.Log("Response: " + www.downloadHandler.text);
        }
    }

    private IEnumerator SendAudioToServer(string url = "http://127.0.0.1:5000/generate_question")
    {
        byte[] bytes = ConvertAudioClipToWav(audioIn.clip);

        www = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bytes),
            downloadHandler = new DownloadHandlerBuffer()
        };

        www.SetRequestHeader("Content-Type", "audio/wav");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Audio recording sent!");
            Debug.Log("Response: " + www.downloadHandler);

            // Get the audio data from the response
            byte[] audioBytes = www.downloadHandler.data;

            // Convert the byte array to a float array
            float[] audioDataResponse = new float[audioBytes.Length / 2];

            for (int i = 0; i < audioBytes.Length; i += 2)
            {
                short sample = BitConverter.ToInt16(audioBytes, i);
                audioDataResponse[i / 2] = sample / 32768.0f;
            }

            // Create a new AudioClip and set the audio data
            AudioClip audioClip = AudioClip.Create("ReceivedAudio", audioDataResponse.Length, 1, 24000, false);
            audioClip.SetData(audioDataResponse, 0);

            audioOut.clip = audioClip;
            audioOut.Play();
        }
    }

    private byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(44 + samples * channels * 2); // File size
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // Sub chunk size
        writer.Write((short)1); // Audio format (1 = PCM)
        writer.Write((short)channels);
        writer.Write(hz);
        writer.Write(hz * channels * 2);
        writer.Write((short)(channels * 2));
        writer.Write((short)16); // Bits per sample
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(samples * channels * 2);

        float[] audioData = new float[samples * channels];
        clip.GetData(audioData, 0);

        for (int i = 0; i < audioData.Length; i++)
        {
            writer.Write((short)(audioData[i] * short.MaxValue));
        }

        byte[] bytes = stream.ToArray();

        writer.Close();
        stream.Close();

        return bytes;
    }
}
