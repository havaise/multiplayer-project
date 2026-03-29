# Practice 1 (Canvas UI, manual setup)

Only scripts are provided. Scene and prefabs are configured manually.

## Implemented in code

- Host/Client start with nickname input.
- Nickname sync via `NetworkVariable<FixedString32Bytes>`.
- HP sync via `NetworkVariable<int>`.
- UI refresh via `OnValueChanged`.
- Server-side damage via `ServerRpc`.
- No self-hit.
- HP is clamped to `>= 0`.
- No `OnGUI`: UI is now Canvas-based.

## Manual setup

1. Open your scene (for example: `Assets/Scenes/SampleScene.unity`).
2. Create `NetworkManager` object and add:
   - `NetworkManager`
   - `UnityTransport`
3. Create player prefab and add:
   - `NetworkObject`
   - `PlayerNetwork`
   - `PlayerCombat`
   - `PlayerInput`
   - `PlayerView`
4. Assign the player prefab into `NetworkManager -> Player Prefab`.
5. Create `Canvas` (Screen Space - Overlay).
6. Create object `UIRoot` under Canvas and add `ConnectionUI`.
7. Under Canvas create two panels:
   - `ConnectPanel` with:
   - `NicknameInput` (`TMP_InputField`)
   - `AddressInput` (`TMP_InputField`)
   - `HostButton` (`Button`)
   - `ClientButton` (`Button`)
   - `StatusText` (`TMP_Text`)
   - `GameplayPanel` with:
   - `AttackButton` (`Button`)
   - `ModeText` (`TMP_Text`)
   - `NicknameText` (`TMP_Text`)
8. Assign all these references in `ConnectionUI` inspector fields.
9. Run two game instances:
   - first presses `Start Host`
   - second presses `Start Client`
10. Check results:
   - connection UI switches from `ConnectPanel` to `GameplayPanel`
   - both players show nickname and HP labels above characters
   - `F` key and `AttackButton` damage another player
   - HP does not go below zero
