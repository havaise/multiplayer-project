# Practice 2 (manual Editor setup)

Code is implemented. Scene/prefab wiring is done manually in Unity.

## New scripts

- `PlayerMovement`
- `PlayerCamera`
- `PlayerShooting`
- `Projectile`
- `PickupManager`
- `HealthPickup`

Also updated:

- `PlayerNetwork` (added `IsAlive`, death/respawn cycle, spawn points support)
- `ConnectionUI` (shoot button -> projectile shot, ammo and respawn text)
- `PlayerInput` (optional extra shoot hotkey `F`)

## Editor setup checklist

1. Player prefab:
- Keep existing: `NetworkObject`, `PlayerNetwork`, `PlayerView`, `PlayerInput`.
- Add: `NetworkTransform`, `CharacterController`, `PlayerMovement`, `PlayerCamera`, `PlayerShooting`.
- Optional: remove old `PlayerCombat` component.

2. Fire point:
- Create child `FirePoint` on player (for example at `0, 1.2, 0.8`).
- Assign `FirePoint` to `PlayerShooting -> Fire Point`.

3. Respawn points for players:
- Create 2-3 empty objects on scene (for example `PlayerSpawn_1..3`).
- Assign them to `PlayerNetwork -> Spawn Points` on player prefab.

4. Projectile prefab:
- Create sphere/capsule projectile prefab.
- Add: `NetworkObject`, `SphereCollider` (`Is Trigger = true`), `Projectile`.
- Add `Rigidbody` (`Is Kinematic = false`, `Use Gravity = false`).
- Assign projectile prefab to `PlayerShooting -> Projectile Prefab`.

5. Health pickup prefab:
- Create pickup prefab (cube/sphere).
- Add: `NetworkObject`, `SphereCollider` (`Is Trigger = true`), `HealthPickup`.
- Collider radius ~`1.0`.

6. Register network prefabs:
- On `NetworkManager`, open `Network Prefabs`.
- Add projectile prefab.
- Add health pickup prefab.
- Ensure player prefab is assigned in `Player Prefab`.

7. Pickup manager on scene:
- Create scene object `PickupManager`.
- Add script `PickupManager`.
- Assign `Health Pickup Prefab`.
- Create pickup spawn points (`PickupSpawn_1..N`) and assign to `Spawn Points`.

8. Canvas UI (`ConnectionUI`) fields:
- Existing fields: connect/gameplay panels, nickname/address inputs, host/client buttons, status/mode/nickname texts.
- New optional fields:
- `Ammo Text` (TMP text, ex: "Ammo: 10")
- `Respawn Text` (TMP text, empty by default)

## Runtime checks

1. Two clients move independently (WASD), remote movement is smooth.
2. Camera follows only local player in each window.
3. Shooting (`Space`, `F`, or UI button) works only when alive, with ammo and cooldown limits.
4. On HP = 0 player dies, cannot move/shoot, respawns after ~3 seconds with full HP.
5. Pickups spawn, heal only when needed, despawn on pickup, respawn after delay.
