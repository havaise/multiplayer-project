using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Practice1
{
    public class PlayerNetwork : NetworkBehaviour
    {
        private static readonly HashSet<PlayerNetwork> Players = new HashSet<PlayerNetwork>();

        public static IEnumerable<PlayerNetwork> ActivePlayers => Players;

        public NetworkVariable<FixedString32Bytes> Nickname = new NetworkVariable<FixedString32Bytes>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<int> HP = new NetworkVariable<int>(
            100,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public override void OnNetworkSpawn()
        {
            Players.Add(this);

            if (IsServer)
            {
                int slot = (int)(OwnerClientId % 8);
                transform.position = new Vector3(-7f + slot * 2f, 1f, 0f);
            }

            if (IsOwner)
            {
                SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
            }
        }

        public override void OnNetworkDespawn()
        {
            Players.Remove(this);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SubmitNicknameServerRpc(string nickname)
        {
            string safeValue = string.IsNullOrWhiteSpace(nickname)
                ? $"Player_{OwnerClientId}"
                : nickname.Trim();

            Nickname.Value = safeValue;
            HP.Value = Mathf.Max(0, HP.Value);
        }
    }
}
