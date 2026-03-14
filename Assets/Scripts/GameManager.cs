using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Players")] 
    [SerializeField] 
    private GameObject player1;
    
    [SerializeField] 
    private GameObject player2;
    
    [Header("Spawn Positions")]
    [SerializeField]
    private GameObject[] spawnPositions;

    public void SelectColumn(int index)
    {
        Debug.Log("Selected column: " + index);
        TakeTurn(index);
    }

    private void TakeTurn(int columnIndex)
    {
        var coin = Instantiate(player1, spawnPositions[columnIndex].transform.position, Quaternion.identity);
        coin.transform.Rotate(new Vector3(90, 0, -90));
    }
}
