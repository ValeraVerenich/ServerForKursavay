using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkPlayerSync : MonoBehaviour
{
    public float sendRate = 0.05f;
    private float timer;

    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        NetworkClient networkClient = FindObjectOfType<NetworkClient>();
        client = networkClient.GetComponent<TcpClient>();
        stream = client.GetStream();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= sendRate)
        {
            SendPosition();
            timer = 0;
        }
    }

    void SendPosition()
    {
        if (stream == null || !stream.CanWrite) return;

        string message = "POS|" + transform.position.y;
        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }
}
