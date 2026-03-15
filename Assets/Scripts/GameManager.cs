using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The GameManager class is responsible for handling the core gameplay logic and interactions within the game.
/// It manages player turns, column selection, and UI updates. This class interacts with other Unity components,
/// such as Camera and TextMeshProUGUI, to provide a functional and user-friendly game experience.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static readonly Vector3 CoinRotation = new(90, 0, -90);

    private const int BoardColumnNumbers = 8;
    private const int BoardRowNumbers = 5;

    private const int Player1Id = 1;
    private const int Player2Id = 2;

    private readonly Vector3 TargetPosition = new Vector3(13.8f, 1.9f, -24f);
    private readonly Vector3 TargetRotation = new Vector3(-3, -90, 0);

    private readonly Vector3 StartPosition = new Vector3(13.8f, 1.9f, -16f);
    private readonly Vector3 StartRotation = new Vector3(-5.62f, -114.32f, 0);

    [Header("Players Coins")] [SerializeField]
    private GameObject player1;

    [SerializeField] private GameObject player2;

    [Header("Players Coins Preview")] [SerializeField]
    private GameObject player1Preview;

    [SerializeField] private GameObject player2Preview;

    [Header("Spawn Positions")] [SerializeField]
    private GameObject[] spawnPositions;

    [Header("Kamera-Settings")] public Camera mainCamera;

    [Header("Players UI")] [SerializeField]
    public TextMeshProUGUI playerNameTurnText;

    [SerializeField] public TextMeshProUGUI winningPlayerText;

    [SerializeField] private CanvasGroup winningTextCanvasGroup;

    [SerializeField] private Button retryButton;

    private bool gameStopped = false;

    private bool isCameraMoving = false;

    private bool isPlayer1Turn = true;

    private int[,] board;

    private GameObject fallingCoin;

    private string player1Name;

    private string player2Name;

    private List<Vector3> player1Positions = new List<Vector3>();
    private List<Vector3> player2Positions = new List<Vector3>();

    private List<GameObject> winningCoins = new List<GameObject>();

    private void Start()
    {
        board = new int[BoardColumnNumbers, BoardRowNumbers];

        player1Preview.SetActive(false);
        player2Preview.SetActive(false);

        SetCamera();

        player1Name = GameData.Player1Name;
        player2Name = GameData.Player2Name;

        SetPlayerTurnText(true);
        SetPlayerNameColor(true);

        winningPlayerText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the UI text to display the current player's turn.
    /// </summary>
    /// <param name="isPlayer1Turn">
    /// A boolean value indicating whether it's Player 1's turn.
    /// If true, Player 1's turn text is displayed; otherwise, Player 2's turn text is displayed.
    /// </param>
    private void SetPlayerTurnText(bool isPlayer1Turn)
    {
        playerNameTurnText.text = isPlayer1Turn ? $"Player {player1Name} turn" : $"Player {player2Name} turn";
    }

    /// <summary>
    /// Sets the color of the player name indicator text depending on the current player's turn.
    /// </summary>
    /// <param name="isPlayer1Turn">Indicates whether it is Player 1's turn. If true, the color will be set to red; otherwise, blue.</param>
    private void SetPlayerNameColor(bool isPlayer1Turn)
    {
        if (playerNameTurnText == null) return;

        Color targetColor = isPlayer1Turn ? Color.red : Color.blue;
        playerNameTurnText.DOColor(targetColor, 0.5f);
    }

    /// <summary>
    /// Animates the player's turn name text element with a combination of rotation, scaling, and fading effects.
    /// </summary>
    /// <remarks>
    /// This method applies the following animations to the <c>playerNameTurnText</c>:
    /// 1. A 360-degree rotation with a bounce easing effect.
    /// 2. A scale increase and decrease with a "YoYo" loop and smooth easing.
    /// 3. A fade in and out "YoYo" loop with sine wave easing.
    /// If <c>playerNameTurnText</c> is not assigned, the method returns without performing any operations.
    /// </remarks>
    private void SetPlayerTurnNameAnimation()
    {
        if (playerNameTurnText == null) return;

        playerNameTurnText.transform
            .DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutBounce);

        playerNameTurnText.transform
            .DOScale(Vector3.one * 1.2f, 0.5f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);


        playerNameTurnText.DOFade(0, 0.3f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// Initializes and animates the camera transition from the start position and rotation
    /// to the target position and rotation, creating a smooth and interpolated movement
    /// using DOMove and DORotate methods from the DOTween library.
    /// </summary>
    /// <remarks>
    /// While the camera is transitioning, the <c>isCameraMoving</c> flag is set to true to
    /// prevent any player interactions that depend on the camera's state. Once the movement
    /// and rotation are complete, the flag is set back to false.
    /// </remarks>
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

    /// Handles the hover logic for a specific column in the game.
    /// Activates the appropriate player preview object and updates its position if the column is not full and no coin is in motion.
    /// <param name="columnIndex">The index of the column that is being hovered over.</param>
    public void OnHoverColumn(int columnIndex)
    {
        if (isCameraMoving || gameStopped) return;

        if (IsColumnNotFull(columnIndex) && IsCoinStationary())
        {
            var previewToActivate = isPlayer1Turn ? player1Preview : player2Preview;
            ActivatePreview(previewToActivate, columnIndex);
        }
    }

    /// <summary>
    /// Checks if the specified column is not full in the game board.
    /// </summary>
    /// <param name="columnIndex">The index of the column to check.</param>
    /// <returns>Returns true if the column is not full; otherwise, returns false.</returns>
    private bool IsColumnNotFull(int columnIndex)
    {
        return board[columnIndex, BoardRowNumbers - 1] == 0;
    }

    /// Checks if the falling coin is stationary or if there is no falling coin currently active.
    /// <returns>
    /// Returns true if there is no falling coin or if the coin's linear velocity is zero,
    /// indicating that the coin is stationary. Otherwise, returns false.
    /// </returns>
    private bool IsCoinStationary()
    {
        return fallingCoin == null || fallingCoin.GetComponent<Rigidbody>().linearVelocity == Vector3.zero;
    }

    /// Activates the specified preview object and positions it at the corresponding spawn position.
    /// <param name="preview">The preview GameObject to be activated.</param>
    /// <param name="columnIndex">The index of the column where the preview object should be positioned.</param>
    private void ActivatePreview(GameObject preview, int columnIndex)
    {
        preview.SetActive(true);
        preview.transform.position = spawnPositions[columnIndex].transform.position;
    }

    /// <summary>
    /// Handles the selection of a column for gameplay actions such as placing a coin.
    /// </summary>
    /// <param name="columnIndex">The index of the column that the player selected.</param>
    public void SelectColumn(int columnIndex)
    {
        if (isCameraMoving || gameStopped) return;

        if (IsCoinStationary())
        {
            TakeTurn(columnIndex);
        }
    }

    /// <summary>
    /// Handles the actions required to take a turn in the game. Places a coin for the current player
    /// in the designated column if valid, updates the UI, and checks for win or draw conditions.
    /// </summary>
    /// <param name="columnIndex">The index of the column where the current player wants to place their coin.</param>
    private void TakeTurn(int columnIndex)
    {
        GameObject currentPlayer = isPlayer1Turn ? player1 : player2;
        Vector3 rotation = CoinRotation;

        // Case column not full
        if (TryPlaceCoin(columnIndex))
        {
            player1Preview.SetActive(false);
            player2Preview.SetActive(false);

            SpawnCoin(currentPlayer, spawnPositions[columnIndex].transform.position, rotation);

            isPlayer1Turn = !isPlayer1Turn;
            SetPlayerTurnText(isPlayer1Turn);
            SetPlayerNameColor(isPlayer1Turn);
            SetPlayerTurnNameAnimation();
        }

        if (IsDraw())
        {
            SeTDrawText();
        }
    }

    /// <summary>
    /// Spawns a coin for the specified player at a given position and rotation.
    /// </summary>
    /// <param name="player">The game object representing the player's coin to spawn.</param>
    /// <param name="position">The position where the coin will be spawned.</param>
    /// <param name="rotation">The rotation of the coin when it is spawned.</param>
    private void SpawnCoin(GameObject player, Vector3 position, Vector3 rotation)
    {
        fallingCoin = Instantiate(player, position, Quaternion.identity);
        fallingCoin.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0.1f, 0);
        fallingCoin.transform.Rotate(rotation);

        int playerId = isPlayer1Turn ? Player1Id : Player2Id;

        StartCoroutine(WaitForStationaryCoin(fallingCoin, playerId));

        if (IsWin(playerId))
        {
            Debug.Log($"{playerId} has won!");
            SeWinningPlayerText(playerId);
            AnimateWinningCoins(playerId);
        }
    }

    private IEnumerator WaitForStationaryCoin(GameObject coin, int playerId)
    {
        Rigidbody rb = coin.GetComponent<Rigidbody>();
        while (rb != null && rb.linearVelocity.magnitude > 0.01f)
        {
            yield return null;
        }

        if (playerId == Player1Id)
        {
            player1Positions.Add(coin.transform.position);
        }
        else if (playerId == Player2Id)
        {
            player2Positions.Add(coin.transform.position);
        }

        Debug.Log($"Coin position saved for Player {playerId}: {coin.transform.position}");
    }

    private void SeWinningPlayerText(int playerId)
    {
        winningPlayerText.gameObject.SetActive(true);
        winningPlayerText.color = Color.green;

        if (playerId == Player1Id)
        {
            winningPlayerText.text = $"{player1Name} has won!";
        }
        else
        {
            winningPlayerText.text = $"{player2Name} has won!";
        }

        winningTextCanvasGroup.alpha = 0;
        winningTextCanvasGroup.DOFade(1, 1f).SetEase(Ease.OutBounce);

        winningPlayerText.transform.localScale = Vector3.zero;
        winningPlayerText.transform.DOScale(Vector3.one, 1f)
            .SetEase(Ease.OutElastic)
            .OnComplete(() =>
            {
                winningPlayerText.transform.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.5f)
                    .SetEase(Ease.InOutBounce)
                    .SetLoops(-1, LoopType.Yoyo);
            });

        retryButton.gameObject.SetActive(true);

        playerNameTurnText.gameObject.SetActive(false);
        gameStopped = true;
    }

    private void SeTDrawText()
    {
        winningPlayerText.gameObject.SetActive(true);
        winningPlayerText.color = Color.red;
        winningPlayerText.text = $"Is a Draw";


        winningTextCanvasGroup.alpha = 0;
        winningTextCanvasGroup.DOFade(1, 1f).SetEase(Ease.OutBounce);

        winningPlayerText.transform.localScale = Vector3.zero;
        winningPlayerText.transform.DOScale(Vector3.one, 1f)
            .SetEase(Ease.OutElastic)
            .OnComplete(() =>
            {
                winningPlayerText.transform.DOScale(new Vector3(1.1f, 1.1f, 1f), 0.5f)
                    .SetEase(Ease.InOutBounce)
                    .SetLoops(-1, LoopType.Yoyo);
            });

        retryButton.gameObject.SetActive(true);

        playerNameTurnText.gameObject.SetActive(false);
        gameStopped = true;
    }

    public void ResumeGame()
    {
        DOTween.Kill(winningPlayerText, true);
        winningPlayerText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        playerNameTurnText.gameObject.SetActive(true);
        gameStopped = false;
        SceneManager.LoadScene("StartScene");
    }

    /// Attempts to place a coin in the specified column for the current player.
    /// If the column is not full, the coin is placed and the method returns true.
    /// If the column is full, no coin is placed and the method returns false.
    /// <param name="columnIndex">The index of the column where the coin should be placed.</param>
    /// <returns>True if a coin was successfully placed; otherwise, false.</returns>
    private bool TryPlaceCoin(int columnIndex)
    {
        for (int row = 0; row < BoardRowNumbers; row++)
        {
            if (board[columnIndex, row] == 0)
            {
                int currentPlayerId = isPlayer1Turn ? Player1Id : Player2Id;
                PlaceCoin(columnIndex, row, currentPlayerId);

                Debug.Log($"Player {currentPlayerId} has placed a coin in column {columnIndex} and row {row}");
                return true;
            }
        }

        Debug.Log($"Column {columnIndex} is full");
        return false;
    }

    /// <summary>
    /// Places a coin on the game board at the specified column and row for the given player's ID.
    /// </summary>
    /// <param name="columnIndex">The column index on the game board where the coin is to be placed.</param>
    /// <param name="row">The row index on the game board where the coin is to be placed.</param>
    /// <param name="playerId">The ID of the player placing the coin, used to mark the board.</param>
    private void PlaceCoin(int columnIndex, int row, int playerId)
    {
        board[columnIndex, row] = playerId;
    }

    /// <summary>
    /// Determines whether the given player has won the game by checking for a sequence of connected marks on the board.
    /// </summary>
    /// <param name="playerId">The ID of the player to check for a winning condition.</param>
    /// <returns>True if the player has achieved a winning condition, otherwise false.</returns>
    private bool IsWin(int playerId)
    {
        winningCoins.Clear();

        // Horizontal
        for (int i = 0; i < BoardColumnNumbers - 3; i++)
        {
            for (int j = 0; j < BoardRowNumbers; j++)
            {
                if (board[i, j] == playerId && board[i + 1, j] == playerId
                                            && board[i + 2, j] == playerId && board[i + 3, j] == playerId)
                {
                    AddWinningCoins(i, j, 1, 0);
                    return true;
                }
            }
        }

        // Vertikal
        for (int i = 0; i < BoardColumnNumbers; i++)
        {
            for (int j = 0; j < BoardRowNumbers - 3; j++)
            {
                if (board[i, j] == playerId && board[i, j + 1] == playerId
                                            && board[i, j + 2] == playerId && board[i, j + 3] == playerId)
                {
                    AddWinningCoins(i, j, 0, 1);
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
                    AddWinningCoins(i, j, 1, 1);
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
                    AddWinningCoins(i, j + 3, 1, -1);
                    return true;
                }
            }
        }

        return false;
    }

    private void AddWinningCoins(int startX, int startY, int stepX, int stepY)
    {
        for (int k = 0; k < 4; k++)
        {
            var coin = FindCoinAt(startX + k * stepX, startY + k * stepY);
            if (coin != null)
            {
                winningCoins.Add(coin);
            }
        }
    }

    private GameObject FindCoinAt(int columnIndex, int rowIndex)
    {
        foreach (var coin in FindObjectsOfType<Rigidbody>())
        {
            if (Vector3.Distance(coin.transform.position,
                    spawnPositions[columnIndex].transform.position + Vector3.up * rowIndex) < 0.1f)
            {
                return coin.gameObject;
            }
        }

        return null;
    }

    private void AnimateWinningCoins(int playerId)
    {
        if (winningCoins == null || winningCoins.Count == 0)
        {
            Debug.LogWarning($"Winning coins list is null or empty for Player {playerId}.");
            return;
        }

        foreach (var coin in winningCoins)
        {
            if (coin == null)
            {
                Debug.LogWarning("One of the winning coins is null or has been destroyed.");
                continue;
            }

            if (!coin.activeSelf)
            {
                Debug.LogWarning($"Coin '{coin.name}' is inactive and cannot be animated.");
                continue;
            }

            var renderer = coin.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogWarning($"Coin '{coin.name}' does not have a Renderer component!");
                continue;
            }

            Material mat = renderer.material;
            if (mat == null)
            {
                Debug.LogWarning($"Renderer on coin '{coin.name}' has no valid Material assigned!");
                continue;
            }
            
            bool hasCoinMaterial1Color = mat.HasProperty("CoinMaterial1Color");
            bool hasCoinMaterial2Color = mat.HasProperty("CoinMaterial2Color");

            if (!hasCoinMaterial1Color && !hasCoinMaterial2Color)
            {
                Debug.LogWarning(
                    $"Coin '{coin.name}' uses a Material without 'CoinMaterial1Color' or 'CoinMaterial2Color' properties!");
                continue;
            }


            string colorProperty = hasCoinMaterial1Color ? "CoinMaterial1Color" : "CoinMaterial2Color";
            Color color = mat.GetColor(colorProperty);

            DOTween.To(() => color.a, alpha =>
                {
                    color.a = alpha;
                    mat.SetColor(colorProperty, color);
                }, 0f, 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(true)
                .OnKill(() =>
                    Debug.Log($"Tween killed for material transparency on coin '{coin.name}' using {colorProperty}."));

            Debug.Log(
                $"Animating material transparency for coin '{coin.name}' using '{colorProperty}' for Player {playerId}.");
        }
    }

    /// <summary>
    /// Checks if the game has reached a draw condition.
    /// </summary>
    /// <returns>
    /// True if no more moves can be made (all columns are full), otherwise false.
    /// </returns>
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