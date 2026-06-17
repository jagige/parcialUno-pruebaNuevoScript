using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerConfig : NetworkBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private GameObject _camera;

    public Material verde;
    public Material rojo;
    public MeshRenderer meshRenderer;
    public MeshRenderer meshRendererR;

    public Transform posisionSpawn;
    public Transform posisionSpawn2;

    private void Awake()
    {
        //PlayerInput
        _playerInput.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        _playerInput.enabled = IsOwner;
        _camera.SetActive(IsOwner);

        if (!IsOwner) return;

        AplicarConfigPorRol();
    }

    // OwnerClientId es el ID estable asignado por Netcode a este objeto,
    // a diferencia de NetworkManager.LocalClientId que depende de la
    // instancia local y puede inducir a confusión.
    private void AplicarConfigPorRol()
    {
        Transform destino = OwnerClientId == 0 ? posisionSpawn : posisionSpawn2;

        if (OwnerClientId == 0)
        {
            meshRenderer.material = verde;
        }
        else
        {
            meshRenderer.material = rojo;
            meshRendererR.material = rojo;
        }

        // Si hay un CharacterController, hay que desactivarlo antes de
        // teletransportar el transform a mano, o puede ignorar el movimiento
        // o generar colisiones falsas (esto importa sobre todo en el reinicio,
        // donde el jugador ya tiene un CharacterController activo en juego).
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        transform.position = destino.position;

        if (controller != null) controller.enabled = true;
    }

    // Llamado por el servidor al reiniciar la partida: vuelve a ubicar al
    // jugador en su punto de spawn sin necesidad de recargar la escena.
    [ClientRpc]
    public void ReiniciarJugadorClientRpc()
    {
        if (!IsOwner) return;

        AplicarConfigPorRol();

        playerItem item = GetComponent<playerItem>();
        if (item != null) item.ResetItemState();
    }

    public override void OnNetworkDespawn()
    {
        _playerInput.enabled = false;
        _camera.SetActive(false);
    }
}