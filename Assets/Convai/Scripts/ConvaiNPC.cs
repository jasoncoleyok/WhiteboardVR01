using System;
using System.Collections;

using Convai.gRPCAPI;

using UnityEngine;
using UnityEngine.SceneManagement;

using Grpc.Core;
using Service;

using TMPro;

using System.Collections.Generic;
using ReadyPlayerMe;
using static ConvaiNPC;
using UnityEngine.Android;

// The main class for controlling the NPC
[RequireComponent(typeof(Animator), typeof(AudioSource))]
public class ConvaiNPC : MonoBehaviour
{
    // A list of responses from the Convai API
    [HideInInspector]
    public List<GetResponseResponse> getResponseResponses = new List<GetResponseResponse>();

    // The ID for the current session
    public string sessionID = "-1";

    // A list of audio responses from the Convai API
    List<ResponseAudio> ResponseAudios = new List<ResponseAudio>();

    // Inner class for holding audio response data
    public class ResponseAudio
    {
        public AudioClip audioClip;
        public string audioTranscript;
    };

    // Add a new public delegate and event
    public delegate void ButtonPressedAction();
    public static event ButtonPressedAction OnButtonPressed;
    public static event ButtonPressedAction OnButtonReleased;

    // The ID and name for the character
    [SerializeField] public string CharacterID;
    [SerializeField] public string CharacterName;
    [SerializeField] public AudioClip testClip;  // Drag your test sound file here in the Inspector

    // The AudioSource for playing back audio
    private AudioSource audioSource;

    // The Animator for controlling character animations
    private Animator characterAnimator;

    // The VoiceHandler for handling voice data
    private VoiceHandler voiceHandler;

    // LipSync and UI Handler references
    private ConvaiRPMLipSync convaiRPMLipSync;
    private ConvaiChatUIHandler convaiChatUIHandler;

    // Boolean for checking if an animation is playing
    bool animationPlaying = false;

    // Boolean for controlling the audio playback loop
    bool playingStopLoop = false;

    // gRPC channel and client for communicating with the Convai API
    private Channel channel;
    private ConvaiService.ConvaiServiceClient client;

    // Constants for the audio sample rate and gRPC API endpoint
    private const int AUDIO_SAMPLE_RATE = 44100;
    private const string GRPC_API_ENDPOINT = "stream.convai.com";

    // The frequency and length of audio recording
    private int recordingFrequency = AUDIO_SAMPLE_RATE;
    private int recordingLength = 30;

    // The gRPC API object for communicating with the Convai API
    private ConvaiGRPCAPI grpcAPI;

    // Boolean to check if the character is active
    [SerializeField] public bool isCharacterActive;

    // Boolean to enable test mode
    [SerializeField] bool enableTestMode;

    // The test user query for the Convai API
    [SerializeField] string testUserQuery;

    // Boolean to check if the character is listening
    public bool isCharacterListening = false;

    // Method that runs when the object is first instantiated
    private void Awake()
    {
        // Get references to the necessary components and objects
        grpcAPI = FindObjectOfType<ConvaiGRPCAPI>();
        convaiChatUIHandler = FindObjectOfType<ConvaiChatUIHandler>();

        audioSource = GetComponent<AudioSource>();
        characterAnimator = GetComponent<Animator>();

        if (GetComponent<VoiceHandler>())
        {
            voiceHandler = GetComponent<VoiceHandler>();
        }

        if (GetComponent<ConvaiRPMLipSync>() != null)
        {
            convaiRPMLipSync = GetComponent<ConvaiRPMLipSync>();
        }
    }

    // Method that runs when the scene first starts
    private void Start()
    {
        // Start the audio playback coroutine
        StartCoroutine(playAudioInOrder());

        // Check for microphone permissions on Android
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif

        // Set up the gRPC client
        SslCredentials credentials = new SslCredentials();

        // The IP Address could be down
        channel = new Channel(GRPC_API_ENDPOINT, credentials);

        client = new ConvaiService.ConvaiServiceClient(channel);
    }

    // Method for starting audio recording
    public void StartListening()
    {
        Debug.Log("Started Listening...");
        grpcAPI.StartRecordAudio(client, recordingFrequency, recordingLength, CharacterID, enableTestMode, testUserQuery);
    }

    // Method for stopping audio recording
    public void StopListening()
    {
        Debug.Log("Stopped Listening...");
        grpcAPI.StopRecordAudio();
    }

    // Method that runs every frame
    private void Update()
    {
        // Start and stop audio recording based on input
        if (isCharacterActive)
        {
            // Start recording when the space bar is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Space bar pressed. Starting recording...");
                StartListening();
            }

            // Stop recording when the space bar is released
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Debug.Log("Space bar released. Stopping recording...");
                StopListening();
            }
        }

        // Reload the scene if R and Equals keys are pressed
        if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.Equals))
        {
            Debug.Log("Reloading scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Quit the application if Escape and Equals keys are pressed
        if (Input.GetKey(KeyCode.Escape) && Input.GetKey(KeyCode.Equals))
        {
            Debug.Log("Quitting application...");
            Application.Quit();
        }

        // Process response audio if any is available
        if (getResponseResponses.Count > 0)
        {
            Debug.Log("Processing response audio...");
            ProcessResponseAudio(getResponseResponses[0]);
            getResponseResponses.Remove(getResponseResponses[0]);
        }

        // Add this block to play the test sound when the T key is pressed
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T key pressed. Playing test sound...");
            audioSource.PlayOneShot(testClip);
        }

        // Play the talking animation if there is audio to play
        if (ResponseAudios.Count > 0)
        {
            if (animationPlaying == false)
            {
                // Enable animation according to response
                animationPlaying = true;

                characterAnimator.SetBool("Talk", true);
            }
        }
        else
        {
            // Deactivate animations if there is no audio to play
            if (animationPlaying == true)
            {
                animationPlaying = false;

                characterAnimator.SetBool("Talk", false);
            }
        }
    }

    // Method that runs when the script is enabled
    private void OnEnable()
    {
        // Subscribe to the OnButtonPressed and OnButtonReleased events
        OnButtonPressed += StartListening;
        OnButtonReleased += StopListening;

        Debug.Log("Subscribed to VR button events.");
    }

    // Method that runs when the script is disabled
    private void OnDisable()
    {
        // Unsubscribe from the OnButtonPressed and OnButtonReleased events
        OnButtonPressed -= StartListening;
        OnButtonReleased -= StopListening;

        Debug.Log("Unsubscribed from VR button events.");
    }

    // Method for processing response audio
    void ProcessResponseAudio(GetResponseResponse getResponseResponse)
    {
        Debug.Log("In ProcessResponseAudio...");
        if (isCharacterActive)
        {
            string tempString = "";

            if (getResponseResponse.AudioResponse.TextData != null)
                tempString = getResponseResponse.AudioResponse.TextData;

            byte[] byteAudio = getResponseResponse.AudioResponse.AudioData.ToByteArray();

            AudioClip clip = grpcAPI.ProcessByteAudioDataToAudioClip(byteAudio, getResponseResponse.AudioResponse.AudioConfig.SampleRateHertz.ToString());

            ResponseAudios.Add(new ResponseAudio
            {
                audioClip = clip,
                audioTranscript = tempString
            });
        }
    }

    // Coroutine for playing audio in order
    IEnumerator playAudioInOrder()
    {
        Debug.Log("In playAudioInOrder...");
        // Plays audio as soon as there is audio to play
        while (!playingStopLoop)
        {
            if (ResponseAudios.Count > 0)
            {
                if (GetComponent<OVRLipSync>())
                {
                    Debug.Log("Playing audio with OVRLipSync...");
                    audioSource.clip = ResponseAudios[0].audioClip;
                    audioSource.Play();
                }
                else if (voiceHandler)
                {
                    Debug.Log("Playing audio with VoiceHandler...");
                    voiceHandler.PlayAudioClip(ResponseAudios[0].audioClip);
                }
                else
                {
                    Debug.Log("Playing audio with AudioSource...");
                    audioSource.clip = ResponseAudios[0].audioClip;
                    audioSource.Play();
                }

                if (convaiChatUIHandler != null)
                    convaiChatUIHandler.isCharacterTalking = true;

                if (convaiChatUIHandler != null)
                {
                    convaiChatUIHandler.characterText = ResponseAudios[0].audioTranscript;
                    convaiChatUIHandler.characterName = CharacterName;
                }

                yield return new WaitForSeconds(ResponseAudios[0].audioClip.length);

                if (convaiChatUIHandler != null)
                    convaiChatUIHandler.isCharacterTalking = false;

                audioSource.Stop();

                if (ResponseAudios.Count > 0)
                    ResponseAudios.RemoveAt(0);
            }
            else
                yield return null;
        }
    }

    // New method for invoking the OnButtonPressed event
    public void InvokeOnButtonPressed()
    {
        Debug.Log("Invoking OnButtonPressed event...");
        OnButtonPressed?.Invoke();
    }

    // New method for invoking the OnButtonReleased event
    public void InvokeOnButtonReleased()
    {
        Debug.Log("Invoking OnButtonReleased event...");
        OnButtonReleased?.Invoke();
    }

    // Method that runs when the application quits
    void OnApplicationQuit()
    {
        Debug.Log("Application quitting...");
        playingStopLoop = true;
    }
}