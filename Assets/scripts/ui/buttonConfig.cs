using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class buttonConfig : MonoBehaviour
{
    [SerializeField] private Button _botonServidor;
    [SerializeField] private Button _botonCliente;
    [SerializeField] private GameObject CanvasInicial;
    [SerializeField] TextMeshProUGUI _textoVariable;
    public void conectarCliente()
    {
        CanvasInicial.SetActive(false);
        NetworkManager.Singleton.StartClient();
    }

    public void conectarServidor()
    {
        CanvasInicial.SetActive(false);
        NetworkManager.Singleton.StartHost();
    }

    private void OnEnable()
    {
        _botonServidor.onClick.AddListener(conectarServidor);
        _botonCliente.onClick.AddListener(conectarCliente);
    }
    private void OnDisable()
    {
        _botonServidor.onClick.RemoveListener(conectarServidor);
        _botonCliente.onClick.RemoveListener(conectarCliente);
    }

}
