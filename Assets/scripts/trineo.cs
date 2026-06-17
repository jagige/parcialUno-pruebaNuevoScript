using UnityEngine;

public class trineo : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        playerItem player = other.GetComponent<playerItem>();

        // Si es un jugador y tiene un objeto, hace la entrega
        if (player != null && player.hasItem)
        {
            player.DeliverItem();

            // Opcional: Aquí podrías reactivar el objeto 'PickableItem' en el mapa si deseas
        }
    }
}
