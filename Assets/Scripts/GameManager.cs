using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Players")] 
    [SerializeField] 
    private GameObject player1;
    
    [SerializeField] 
    private GameObject player2;

    public void SelectColumn(int index)
    {
        Debug.Log("Selected column: " + index);
    }
}
