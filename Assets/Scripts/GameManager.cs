using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly Vector3 CoinRotation = new Vector3(90, 0, -90);

    [Header("Players")] 
    [SerializeField] 
    private GameObject player1;
    
    [SerializeField] 
    private GameObject player2;
    
    [Header("Spawn Positions")]
    [SerializeField]
    private GameObject[] spawnPositions;
    
    private bool player1Turn = true;

    public void SelectColumn(int index)
    {
        Debug.Log("Selected column: " + index);
        TakeTurn(index);
    }

    private void TakeTurn(int columnIndex)
    {
        GameObject currentPlayer = player1Turn ? player1 : player2;
        Vector3 rotation = CoinRotation;

        SpawnCoin(currentPlayer, spawnPositions[columnIndex].transform.position, rotation);

        player1Turn = !player1Turn;
    }

    private void SpawnCoin(GameObject player, Vector3 position, Vector3 rotation)
    {
        var coin = Instantiate(player, position, Quaternion.identity);
        coin.transform.Rotate(rotation);
    }
}
