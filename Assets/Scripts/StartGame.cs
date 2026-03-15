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

       private void Start()
       {
           startButton.onClick.AddListener(OnStartClicked);
           
           player1Input.onValueChanged.AddListener((value) => HandleStartConditions());
           player2Input.onValueChanged.AddListener((value) => HandleStartConditions());
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