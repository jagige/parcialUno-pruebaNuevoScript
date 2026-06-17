using Unity.Netcode;
using UnityEngine;

public class playerItem : NetworkBehaviour
{
    // Variable para saber si este jugador lleva el objeto
    public bool hasItem = false;
    private bool tieneRegalo;
    

    //Referencia visual del objeto que lleva (para activarlo o desactivarlo)
    //[SerializeField] private GameObject visualItem;
    [SerializeField] private GameObject duende2;
    [SerializeField] private GameObject duendeConRegalo;

    public void PickUpItem()
    {
        if (!IsOwner) return; // Solo el dueño del personaje puede recogerlo activamente

        hasItem = true;
        if (duende2 != null)
        {
            duende2.SetActive(false);
            duendeConRegalo.SetActive(true);
        }
    }

    public void DeliverItem()
    {
        if (!IsOwner || !hasItem) return;

        hasItem = false;
        if (duendeConRegalo != null)
        {
            duende2.SetActive(true);
            duendeConRegalo.SetActive(false);
        }

        // Le avisamos al servidor que entregamos el objeto con éxito
        SubmitScoreServerRpc();
    }

    // Llamado al reiniciar la partida para que nadie arranque la ronda nueva
    // ya cargando un regalo de la ronda anterior.
    public void ResetItemState()
    {
        hasItem = false;
        if (duende2 != null) duende2.SetActive(true);
        if (duendeConRegalo != null) duendeConRegalo.SetActive(false);
    }

    // Un ServerRpc se ejecuta en el servidor, solicitado por el cliente
    [ServerRpc]
    private void SubmitScoreServerRpc(ServerRpcParams rpcParams = default)
    {
        // El servidor sabe qué cliente llamó este método gracias a OwnerClientId
        gameManager.Instance.AddPoint(OwnerClientId);
    }
}
