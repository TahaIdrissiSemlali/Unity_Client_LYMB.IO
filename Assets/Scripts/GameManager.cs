using System;
using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly Vector3 CoinRotation = new(90, 0, -90);
    
    private const int BoardColumnNumbers = 8;
    private const int BoardRowNumbers = 5;
    
    private const int Player1Id = 1;
    private const int Player2Id = 2;

    private readonly Vector3 TargetPosition = new Vector3(13.8f, 1.9f, -22.19f);
    private readonly Vector3 TargetRotation = new Vector3(-3, -90, 0);
    
    private readonly Vector3 StartPosition = new Vector3(13.8f, 1.9f, -16f);
    private readonly Vector3 StartRotation = new Vector3(-5.62f, -114.32f, 0);

    [Header("Players Coins")] 
    [SerializeField]
    private GameObject player1;

    [SerializeField] 
    private GameObject player2;

    [Header("Players Coins Preview")] 
    [SerializeField]
    private GameObject player1Preview;

    [SerializeField] 
    private GameObject player2Preview;

    [Header("Spawn Positions")] 
    [SerializeField]
    private GameObject[] spawnPositions;
    
    [Header("Kamera-Settings")]
    public Camera mainCamera;
    
    private bool isCameraMoving = false;

    private bool player1Turn = true;

    private int[,] board;

    private GameObject fallingCoin;

    private void Start()
    {
        board = new int[BoardColumnNumbers, BoardRowNumbers];

        player1Preview.SetActive(false);
        player2Preview.SetActive(false);

        SetCamera();
    }
    
    private void SetCamera()
    {
        isCameraMoving = true;
        
        mainCamera.transform.position = StartPosition;
        mainCamera.transform.rotation = Quaternion.Euler(StartRotation);
        
        mainCamera.transform.DOMove(TargetPosition, 1f).SetEase(Ease.InOutQuad);
        mainCamera.transform.DORotate(TargetRotation, 1f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            isCameraMoving = false;
        });
    }

    public void OnHoverColumn(int columnIndex)
    {
        if (isCameraMoving) return;
        
        if (IsColumnNotFull(columnIndex) && IsCoinStationary())
        {
            var previewToActivate = player1Turn ? player1Preview : player2Preview;
            ActivatePreview(previewToActivate, columnIndex);
        }
    }

    private bool IsColumnNotFull(int columnIndex)
    {
        return board[columnIndex, BoardRowNumbers - 1] == 0;
    }

    private bool IsCoinStationary()
    {
        return fallingCoin == null || fallingCoin.GetComponent<Rigidbody>().linearVelocity == Vector3.zero;
    }

    private void ActivatePreview(GameObject preview, int columnIndex)
    {
        preview.SetActive(true);
        preview.transform.position = spawnPositions[columnIndex].transform.position;
    }

    public void SelectColumn(int columnIndex)
    {
        if (isCameraMoving) return;
        
        if (IsCoinStationary())
        {
            TakeTurn(columnIndex);
        }
    }

    private void TakeTurn(int columnIndex)
    {
        GameObject currentPlayer = player1Turn ? player1 : player2;
        Vector3 rotation = CoinRotation;

        // Case column not full
        if (TryPlaceCoin(columnIndex))
        {
            player1Preview.SetActive(false);
            player2Preview.SetActive(false);

            SpawnCoin(currentPlayer, spawnPositions[columnIndex].transform.position, rotation);

            player1Turn = !player1Turn;
        }

        if (IsDraw())
        {
            Debug.Log("Draw!");
        }
    }

    private void SpawnCoin(GameObject player, Vector3 position, Vector3 rotation)
    {
        fallingCoin = Instantiate(player, position, Quaternion.identity);
        fallingCoin.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0.1f, 0);
        fallingCoin.transform.Rotate(rotation);

        int playerId = player1Turn ? Player1Id : Player2Id;

        if (IsWin(playerId))
        {
            Debug.Log($"{playerId} has won!");
        }
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

    private bool IsWin(int playerId)
    {
        // Horizontal
        for (int i = 0; i < BoardColumnNumbers - 3; i++)
        {
            for (int j = 0; j < BoardRowNumbers; j++)
            {
                if (board[i, j] == playerId && board[i + 1, j] == playerId
                                            && board[i + 2, j] == playerId && board[i + 3, j] == playerId)
                {
                    return true;
                }
            }
        }

        // Vertical
        for (int i = 0; i < BoardColumnNumbers; i++)
        {
            for (int j = 0; j < BoardRowNumbers - 3; j++)
            {
                if (board[i, j] == playerId && board[i, j + 1] == playerId
                                            && board[i, j + 2] == playerId && board[i, j + 3] == playerId)
                {
                    return true;
                }
            }
        }

        // i = j
        for (int i = 0; i < BoardColumnNumbers - 3; i++)
        {
            for (int j = 0; j < BoardRowNumbers - 3; j++)
            {
                if (board[i, j] == playerId && board[i + 1, j + 1] == playerId
                                            && board[i + 2, j + 2] == playerId && board[i + 3, j + 3] == playerId)
                {
                    return true;
                }
            }
        }

        // i = -j
        for (int i = 0; i < BoardColumnNumbers - 3; i++)
        {
            for (int j = 0; j < BoardRowNumbers - 3; j++)
            {
                if (board[i, j + 3] == playerId && board[i + 1, j + 2] == playerId
                                            && board[i + 2, j + 1] == playerId && board[i + 3, j] == playerId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsDraw()
    {
        for (int i = 0; i < BoardColumnNumbers; i++)
        {
            if (board[i, BoardRowNumbers - 1] == 0)
            {
                return false;
            }
        }
        return true;
    }
}