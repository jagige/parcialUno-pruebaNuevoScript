using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [Header("Spawn variables and prefab")]
    [SerializeField] GameObject _pickUp;            //spawned obj
    [SerializeField] int _currentPickUps;           //how many spawned obj are on scene
    [SerializeField] int _maxPickUps;               //max amount of spawned obj
    [SerializeField] float _spawnTime;
    [SerializeField] float _spawnCooldown;
    [SerializeField] float _spawnRange;             //area to spawn

    // Referencia a los pickups instanciados en esta ronda para poder
    // despawnearlos limpiamente cuando se reinicia la partida.
    private readonly List<NetworkObject> _activePickUps = new List<NetworkObject>();

    private void Update()
    {
        if (!IsServer || !IsSpawned) return;
        SpawnPickUpObject();
    }
    void SpawnPickUpObject()
    {
        if (_currentPickUps >= _maxPickUps) return;     //ignore if there are enough spawned objects

        //Cooldown to spawn pickups
        _spawnTime -= Time.deltaTime;
        if (_spawnTime > 0) return;
        _spawnTime = _spawnCooldown;

        //instantiate pick up obj on network
        GameObject clone = Instantiate(_pickUp, GenerateSpawnPosition(), _pickUp.transform.rotation);
        NetworkObject netObj = clone.GetComponent<NetworkObject>();
        netObj.Spawn();

        _activePickUps.Add(netObj);
        _currentPickUps++;      //add to variable to check how many spawned obj are on scene
    }

    //generates a random position within the spawn range
    private Vector3 GenerateSpawnPosition()
    {
        //random X and Z coordinates within the spawn range
        float spawnPosX = Random.Range(-_spawnRange, _spawnRange);
        float spawnPosZ = Random.Range(-_spawnRange, _spawnRange);

        //return a new position vector at ground level (Y = 0)
        Vector3 randomPos = new Vector3(spawnPosX, 0.1f, spawnPosZ);

        return randomPos;
    }

    // Llamado por gameManager al reiniciar la partida: limpia todos los
    // regalos que quedaron en el mapa y reinicia el contador y el cooldown
    // para que la nueva ronda empiece de cero.
    public void ReiniciarPickUps()
    {
        if (!IsServer) return;

        foreach (NetworkObject pickUp in _activePickUps)
        {
            if (pickUp != null && pickUp.IsSpawned)
            {
                pickUp.Despawn(true); // true = también destruye el GameObject
            }
        }

        _activePickUps.Clear();
        _currentPickUps = 0;
        _spawnTime = 0f; // para que el primer pickup de la ronda nueva no tarde un cooldown completo
    }
}
