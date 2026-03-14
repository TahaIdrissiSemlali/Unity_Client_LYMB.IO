using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly Vector3 CoinRotation = new Vector3(90, 0, -90);
    private const int BoardColumnNumbers = 8;
    private const int BoardRowNumbers = 5;
    private const int Player1Id = 1;
    private const int Player2Id = 2;

    [Header("Players")] 
    [SerializeField] 
    private GameObject player1;
    
    [SerializeField] 
    private GameObject player2;
    
    [Header("Spawn Positions")]
    [SerializeField]
    private GameObject[] spawnPositions;
    
    private bool player1Turn = true;

    private int[,] board;

    private void Start()
    {
        board = new int[BoardColumnNumbers, BoardRowNumbers];
    }

    public void SelectColumn(int index)
    {
        TakeTurn(index);
    }

    private void TakeTurn(int columnIndex)
    {
        GameObject currentPlayer = player1Turn ? player1 : player2;
        Vector3 rotation = CoinRotation;

        // Case column not full
        if (TryPlaceCoin(columnIndex))
        {
            SpawnCoin(currentPlayer, spawnPositions[columnIndex].transform.position, rotation);

            player1Turn = !player1Turn;
        }
    }

    private void SpawnCoin(GameObject player, Vector3 position, Vector3 rotation)
    {
        var coin = Instantiate(player, position, Quaternion.identity);
        coin.transform.Rotate(rotation);
    }

    private bool TryPlaceCoin(int columnIndex)
    {
        for (int row = 0; row < BoardRowNumbers; row++)
        {
            if (board[columnIndex, row] == 0)
            {
                int currentPlayerId = player1Turn ? Player1Id : Player2Id;
                PlaceCoin(columnIndex, row, currentPlayerId);

                Debug.Log($"Player {currentPlayerId} has placed a coin in column {columnIndex} and row {row}");
                return true;
            }
        }

        Debug.Log($"Column {columnIndex} is full");
        return false;
    }

    private void PlaceCoin(int columnIndex, int row, int playerId)
    {
        board[columnIndex, row] = playerId;
    }
}
