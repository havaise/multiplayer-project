using FishNet;
using UnityEngine;

namespace Practice1
{
    public class ServerAutoStart : MonoBehaviour
    {
        private void Start()
        {
            if (!Application.isBatchMode)
            {
                return;
            }

            Debug.Log("[Server] Headless mode detected. Starting server...");

            ConnectionUI[] connectionUis = FindObjectsByType<ConnectionUI>(FindObjectsSortMode.None);
            for (int i = 0; i < connectionUis.Length; i++)
            {
                if (connectionUis[i] != null)
                {
                    connectionUis[i].gameObject.SetActive(false);
                }
            }

            if (InstanceFinder.ServerManager != null && !InstanceFinder.ServerManager.Started)
            {
                InstanceFinder.ServerManager.StartConnection();
            }
        }
    }
}
