using Unity.Netcode;
using UnityEngine;

namespace Practice1
{
    [RequireComponent(typeof(PlayerCombat))]
    public class PlayerInput : NetworkBehaviour
    {
        private PlayerCombat _combat;

        private void Awake()
        {
            _combat = GetComponent<PlayerCombat>();
        }

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                _combat.TryAttackNearest();
            }
        }
    }
}
