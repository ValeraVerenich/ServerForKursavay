using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    public static bool IsHost { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("NetworkManager ������, IsHost: " + IsHost);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void SetHost(bool host)
    {
        IsHost = host;
        Debug.Log("����������� ����: IsHost = " + IsHost);
    }

    // �������� �����������
    public static bool IsConnected()
    {
        return NetworkClient.Instance != null && NetworkClient.Instance.IsConnected();
    }
}