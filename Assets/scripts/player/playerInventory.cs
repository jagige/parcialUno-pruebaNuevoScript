using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private bool tieneRegalo;
    [SerializeField] private  GameObject duende2;
    [SerializeField] private GameObject duendeConRegalo;
    private void OnTriggerEnter(Collider other)
    {
       
        if (other.gameObject.CompareTag("regalo")) 
        {
            tieneRegalo = true;
        }

        if (other.gameObject.CompareTag("trineo"))
        {
            tieneRegalo = false;
        }
    }

    private void Update()
    {
        if (tieneRegalo == true)
        {
            duende2.SetActive(false);
            duendeConRegalo.SetActive(true);
        }
        else
        {
            duende2.SetActive(true);
            duendeConRegalo.SetActive(false);
        }

        
    }


}