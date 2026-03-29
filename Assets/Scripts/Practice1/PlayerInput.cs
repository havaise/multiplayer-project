using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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

            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                _combat.TryAttackNearest();
            }
        }
    }
}
