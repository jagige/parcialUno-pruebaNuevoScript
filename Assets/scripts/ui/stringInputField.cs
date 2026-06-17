using UnityEngine;

public class stringInputField : MonoBehaviour
{
    string numeroIp;
    public void ReadStringName (string name)
    {
        if (name.Length <= 0)
        {
            Debug.Log("nombre no válido");
        }
        else
        {
            Debug.Log(name);
        }
        numeroIp = name;
        Debug.Log(numeroIp);
    }

}
