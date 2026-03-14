using System;
using UnityEngine;

public class FieldEntry : MonoBehaviour
{
    [SerializeField]
    private int columIndex;
    
    [SerializeField]
    private GameManager gameManager;

    private void OnMouseDown()
    {
        gameManager.SelectColumn(columIndex);
    }

    private void OnMouseOver()
    {
        gameManager.OnHoverColumn(columIndex);
    }
}
