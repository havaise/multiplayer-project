# PRACTICE 4 RUNBOOK

## 1) Linux Dedicated Server build in Unity
1. Open **File -> Build Settings**.
2. Ensure your gameplay scene (for example `Assets/Scenes/MainScene.unity`) is added to **Scenes In Build**.
3. Set platform to **Linux**.
4. Enable **Dedicated Server** (headless build profile).
5. Build output, for example `Builds/LinuxServer/`.

## 2) Run server in WSL
```bash
cd /path/to/Builds/LinuxServer
chmod +x ./GameServer.x86_64
./GameServer.x86_64 -batchmode -nographics
```

Expected log contains:
`[Server] Headless mode detected. Starting server...`

## 3) Get WSL IP
```bash
hostname -I
```
Use the IPv4 value shown there.

## 4) Connect Windows client
1. Launch Windows client build (or Play in Editor).
2. In connection UI, enter server IP from `hostname -I`.
3. Enter Tugboat port used by your project (current scene setup uses `7777`; if changed, use your configured value).
4. Click **Connect**.

## 5) What to demonstrate for submission
1. WSL console with server startup logs in headless mode.
2. Client connection by IP (not only localhost).
3. Lobby status `1/2` then `2/2`.
4. Auto-start match when required players join.
5. Match loop with timer and score updates.
6. Results screen after match end.
7. Automatic return to lobby and next round without server restart.
