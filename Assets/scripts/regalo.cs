using UnityEngine;

public class regalo : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si lo que entró es un jugador
        playerItem player = other.GetComponent<playerItem>();

        if (player != null && !player.hasItem)
        {
            player.PickUpItem();
            // Aquí podrías destruir el objeto o moverlo a otra posición para que reaparezca
            gameObject.SetActive(false);
        }
    }
}
