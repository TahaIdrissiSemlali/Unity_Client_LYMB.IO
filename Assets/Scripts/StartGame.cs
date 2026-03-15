   using TMPro;
   using UnityEngine;
   using UnityEngine.SceneManagement;
   using UnityEngine.UI;

   public class StartGame : MonoBehaviour
   {
       [Header("UI Elemente")]
       public TMP_InputField player1Input;
       public TMP_InputField player2Input;
       public Button startButton;
       
       [Header("Background Music")]
       [SerializeField]
       private AudioClip backgroundMusic;
       
       private AudioSource backgroundMusicSource;

       private void Start()
       {
           RegisterUIEventListeners();
           InitializeBackgroundMusic();
       }

       private void RegisterUIEventListeners()
       {
           startButton.onClick.AddListener(OnStartClicked);
           player1Input.onValueChanged.AddListener((value) => UpdateStartButtonState());
           player2Input.onValueChanged.AddListener((value) => UpdateStartButtonState());
       }

       private void InitializeBackgroundMusic()
       {
           backgroundMusicSource = gameObject.AddComponent<AudioSource>();
           backgroundMusicSource.clip = backgroundMusic;
           backgroundMusicSource.loop = true;
           backgroundMusicSource.Play();
       }

       private void UpdateStartButtonState()
       {
           startButton.interactable = IsStartButtonInteractable();
       }

       
       private void HandleStartConditions()
       {
           startButton.interactable = IsStartButtonInteractable();
       }

       private bool IsStartButtonInteractable()
       {
           return !string.IsNullOrEmpty(player1Input.text) && !string.IsNullOrEmpty(player2Input.text);
       }
       
       private void OnStartClicked()
       {
           GameData.Player1Name = player1Input.text;
           GameData.Player2Name = player2Input.text;
           
           backgroundMusicSource.Stop();
           SceneManager.LoadScene("GameScene");
       }

       private void Update()
       {
           if (Input.GetKeyDown(KeyCode.Return) && startButton.interactable)
           {
               OnStartClicked();
           }
       }
   }