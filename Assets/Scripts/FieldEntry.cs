using System;
using UnityEngine;

public class FieldEntry : MonoBehaviour
{
    [SerializeField]
    private int columIndex;
    
    [SerializeField]
    private GameManager gameManager;
    
    [Header("Drop Sound")]
    [SerializeField]
    private AudioClip dropSound;
       
    private AudioSource dropSoundSource;

    private void Start()
    {
        dropSoundSource = gameObject.AddComponent<AudioSource>();
        dropSoundSource.clip = dropSound;
    }

    private void OnMouseDown()
    {
        gameManager.SelectColumn(columIndex);
        dropSoundSource.Play();
    }

    private void OnMouseOver()
    {
        gameManager.OnHoverColumn(columIndex);
    }
}
