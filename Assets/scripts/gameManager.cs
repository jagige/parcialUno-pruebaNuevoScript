using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class gameManager : NetworkBehaviour
{
    public static gameManager Instance;

    // NetworkVariables para sincronizar los puntajes entre servidor y clientes
    public NetworkVariable<int> scorePlayer1 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> scorePlayer2 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // El tiempo también tiene que viajar por red: si cada cliente lo calculara
    // por su cuenta, solo el servidor (host) lo vería bajar, porque es el único
    // que tiene autoridad para modificar el estado del juego.
    public NetworkVariable<float> tiempoRestanteSync = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI txtScorePlayer1;
    [SerializeField] private TextMeshProUGUI txtScorePlayer2;
    //pantalla final
    [SerializeField] private TextMeshProUGUI txtoGanador;
    [SerializeField] private Button _botonVolveraJugar;
    [SerializeField] private Button _botonSalir;
    //reloj
    [SerializeField] float duracionPartida; // duración fija de cada ronda, en segundos
    [SerializeField] private GameObject _CanvasFinal;
    [SerializeField] TextMeshProUGUI tiempoText;

    // El servidor controla esto para no llamar gameOver() en bucle ni en los clientes
    private bool _partidaTerminada;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Solo el servidor decide cuándo termina la partida; los clientes solo muestran el reloj
        if (IsServer && !_partidaTerminada)
        {
            tiempoRestanteSync.Value -= Time.deltaTime;

            if (tiempoRestanteSync.Value <= 0f)
            {
                tiempoRestanteSync.Value = 0f;
                GameOverClientRpc(scorePlayer1.Value, scorePlayer2.Value);
                _partidaTerminada = true;
            }
        }

        // Tanto host como cliente leen el mismo valor sincronizado por red
        tiempoText.text = Mathf.Max(tiempoRestanteSync.Value, 0f).ToString("F0");
    }


    public override void OnNetworkSpawn()
    {
        // Nos suscribimos al evento de cambio de valor para actualizar la UI
        scorePlayer1.OnValueChanged += OnScoreChanged;
        scorePlayer2.OnValueChanged += OnScoreChanged;

        UpdateUI(scorePlayer1.Value, scorePlayer2.Value); // Inicializar UI con el valor real

        // El reloj solo se inicializa una vez, al arrancar la primera partida.
        // En los reinicios posteriores ya lo hace ReiniciarPartida().
        if (IsServer)
        {
            tiempoRestanteSync.Value = duracionPartida;
        }
    }

    public override void OnNetworkDespawn()
    {
        scorePlayer1.OnValueChanged -= OnScoreChanged;
        scorePlayer2.OnValueChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(int previousValue, int newValue)
    {
        UpdateUI(scorePlayer1.Value, scorePlayer2.Value);
    }

    private void UpdateUI(int p1Score, int p2Score)
    {
        txtScorePlayer1.text = $"Jugador 1: {p1Score}";
        txtScorePlayer2.text = $"Jugador 2: {p2Score}";
    }

    // Método que solo el servidor puede ejecutar para sumar puntos
    public void AddPoint(ulong clientId)
    {
        if (!IsServer || _partidaTerminada) return;

        // OwnerClientId del primer jugador conectado (el host) siempre es 0,
        // así que esto es válido para identificar a J1 vs J2.
        if (clientId == 0)
        {
            scorePlayer1.Value++;
        }
        else
        {
            scorePlayer2.Value++;
        }
    }

    // Se ejecuta en host y clientes para mostrar la pantalla final con el resultado ya decidido por el servidor
    [ClientRpc]
    private void GameOverClientRpc(int p1Score, int p2Score)
    {
        _CanvasFinal.SetActive(true);

        if (p1Score > p2Score)
        {
            txtoGanador.text = "GANÓ EL JUGADOR VERDE";
        }
        else if (p2Score > p1Score)
        {
            txtoGanador.text = "GANÓ EL JUGADOR ROJO";
        }
        else
        {
            txtoGanador.text = "EMPATARON";
        }
    }

    private void OnEnable()
    {
        _botonVolveraJugar.onClick.AddListener(volverAJugar);
        _botonSalir.onClick.AddListener(Application.Quit);
    }
    private void OnDisable()
    {
        _botonVolveraJugar.onClick.RemoveListener(volverAJugar);
    }

    // Cualquiera de los dos jugadores puede tocar el botón. Si quien lo toca
    // es el servidor, reinicia directo; si es un cliente, se lo pide al
    // servidor por RPC, porque solo el servidor puede modificar el estado del juego.
    private void volverAJugar()
    {
        if (IsServer)
        {
            ReiniciarPartida();
        }
        else
        {
            SolicitarReinicioServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SolicitarReinicioServerRpc(ServerRpcParams rpcParams = default)
    {
        ReiniciarPartida();
    }

    private void ReiniciarPartida()
    {
        scorePlayer1.Value = 0;
        scorePlayer2.Value = 0;
        tiempoRestanteSync.Value = duracionPartida;
        _partidaTerminada = false;

        SpawnManager spawnManager = FindFirstObjectByType<SpawnManager>();
        if (spawnManager != null)
        {
            spawnManager.ReiniciarPickUps();
        }

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            NetworkPlayerConfig playerConfig = client.PlayerObject.GetComponent<NetworkPlayerConfig>();
            if (playerConfig != null)
            {
                playerConfig.ReiniciarJugadorClientRpc();
            }
        }

        ReiniciarUIClientRpc();
    }

    [ClientRpc]
    private void ReiniciarUIClientRpc()
    {
        _CanvasFinal.SetActive(false);
    }
}
