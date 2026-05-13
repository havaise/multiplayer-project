using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

namespace Practice1
{
    [RequireComponent(typeof(NetworkObject))]
    public class MatchManager : NetworkBehaviour
    {
        public enum GameState
        {
            WaitingForPlayers,
            InProgress,
            ShowingResults
        }

        public readonly SyncVar<GameState> CurrentState = new(GameState.WaitingForPlayers);
        public readonly SyncVar<int> ConnectedPlayers = new(0);
        public readonly SyncVar<float> MatchTimer = new(0f);
        public readonly SyncVar<string> ResultsText = new(string.Empty);

        [Header("Settings")]
        [SerializeField] private int _requiredPlayers = 2;
        [SerializeField] private float _matchDuration = 60f;
        [SerializeField] private float _resultsDuration = 5f;
        [SerializeField] private int _scoreLimit = 3;
        [SerializeField] private float _autoRestartDelay = 1f;

        private Coroutine _resultsCoroutine;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            CurrentState.OnChange += OnGameStateChanged;
            ConnectedPlayers.OnChange += OnConnectedPlayersChanged;
            MatchTimer.OnChange += OnMatchTimerChanged;
            ResultsText.OnChange += OnResultsTextChanged;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            CurrentState.OnChange -= OnGameStateChanged;
            ConnectedPlayers.OnChange -= OnConnectedPlayersChanged;
            MatchTimer.OnChange -= OnMatchTimerChanged;
            ResultsText.OnChange -= OnResultsTextChanged;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            MatchTimer.Value = _matchDuration;
            CurrentState.Value = GameState.WaitingForPlayers;

            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
                RecountConnectedPlayers();
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            }

            if (_resultsCoroutine != null)
            {
                StopCoroutine(_resultsCoroutine);
                _resultsCoroutine = null;
            }
        }

        private void Update()
        {
            if (!base.IsServerInitialized)
            {
                return;
            }

            if (CurrentState.Value != GameState.InProgress)
            {
                return;
            }

            MatchTimer.Value = Mathf.Max(0f, MatchTimer.Value - Time.deltaTime);
            if (MatchTimer.Value <= 0f)
            {
                EndMatch();
                return;
            }

            if (_scoreLimit > 0)
            {
                foreach (PlayerNetwork player in PlayerNetwork.ActivePlayers)
                {
                    if (player != null && player.Score.Value >= _scoreLimit)
                    {
                        EndMatch();
                        return;
                    }
                }
            }
        }

        private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            RecountConnectedPlayers();
        }

        private void RecountConnectedPlayers()
        {
            if (!base.IsServerInitialized || InstanceFinder.ServerManager == null)
            {
                return;
            }

            ConnectedPlayers.Value = InstanceFinder.ServerManager.Clients.Count;

            if (CurrentState.Value == GameState.WaitingForPlayers && ConnectedPlayers.Value >= _requiredPlayers)
            {
                StartMatch();
            }
        }

        [Server]
        private void StartMatch()
        {
            if (CurrentState.Value != GameState.WaitingForPlayers)
            {
                return;
            }

            if (_resultsCoroutine != null)
            {
                StopCoroutine(_resultsCoroutine);
                _resultsCoroutine = null;
            }

            foreach (PlayerNetwork player in PlayerNetwork.ActivePlayers)
            {
                player?.ServerResetForMatch();
            }

            ResultsText.Value = string.Empty;
            MatchTimer.Value = _matchDuration;
            CurrentState.Value = GameState.InProgress;
        }

        [Server]
        private void EndMatch()
        {
            if (CurrentState.Value != GameState.InProgress)
            {
                return;
            }

            CurrentState.Value = GameState.ShowingResults;
            ResultsText.Value = BuildResultsText();
            PushResultsObserversRpc(ResultsText.Value);

            if (_resultsCoroutine != null)
            {
                StopCoroutine(_resultsCoroutine);
            }

            _resultsCoroutine = StartCoroutine(ResetToLobbyRoutine());
        }

        [Server]
        private IEnumerator ResetToLobbyRoutine()
        {
            yield return new WaitForSeconds(_resultsDuration);
            ResetToLobby();
        }

        [Server]
        private void ResetToLobby()
        {
            foreach (PlayerNetwork player in PlayerNetwork.ActivePlayers)
            {
                player?.ServerResetForLobby(resetScore: true);
            }

            MatchTimer.Value = _matchDuration;
            CurrentState.Value = GameState.WaitingForPlayers;
            ResultsText.Value = string.Empty;

            if (ConnectedPlayers.Value >= _requiredPlayers)
            {
                StartCoroutine(AutoStartAfterLobbyDelay());
            }
        }

        [Server]
        private IEnumerator AutoStartAfterLobbyDelay()
        {
            yield return new WaitForSeconds(_autoRestartDelay);
            if (CurrentState.Value == GameState.WaitingForPlayers && ConnectedPlayers.Value >= _requiredPlayers)
            {
                StartMatch();
            }
        }

        [ObserversRpc(BufferLast = true)]
        private void PushResultsObserversRpc(string text)
        {
            // Intentionally left lightweight: results are also synchronized via SyncVar.
        }

        private static string BuildResultsText()
        {
            List<PlayerNetwork> players = PlayerNetwork.ActivePlayers.Where(p => p != null).ToList();
            players.Sort((a, b) => b.Score.Value.CompareTo(a.Score.Value));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < players.Count; i++)
            {
                PlayerNetwork p = players[i];
                sb.Append(i + 1)
                    .Append(". ")
                    .Append(string.IsNullOrWhiteSpace(p.Nickname.Value) ? $"Player_{p.OwnerId}" : p.Nickname.Value)
                    .Append(" | Score: ")
                    .Append(p.Score.Value)
                    .Append(" | K/D: ")
                    .Append(p.Kills.Value)
                    .Append("/")
                    .Append(p.Deaths.Value)
                    .AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private void OnGameStateChanged(GameState previous, GameState next, bool asServer) { }
        private void OnConnectedPlayersChanged(int previous, int next, bool asServer) { }
        private void OnMatchTimerChanged(float previous, float next, bool asServer) { }
        private void OnResultsTextChanged(string previous, string next, bool asServer) { }

        public int RequiredPlayers => _requiredPlayers;
    }
}
