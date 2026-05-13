using FishNet;
using TMPro;
using UnityEngine;

namespace Practice1
{
    public class MatchUI : MonoBehaviour
    {
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private TMP_Text _lobbyText;
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _localScoreText;
        [SerializeField] private GameObject _resultsPanel;
        [SerializeField] private TMP_Text _resultsTitleText;
        [SerializeField] private TMP_Text _resultsBodyText;
        [SerializeField] private TMP_Text _resultsFooterText;

        private MatchManager _matchManager;

        private void Awake()
        {
            if (Application.isBatchMode)
            {
                gameObject.SetActive(false);
                return;
            }

            SetDefaultResultsText();
        }

        private void Update()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            if (_matchManager == null)
            {
                _matchManager = FindFirstObjectByType<MatchManager>();
            }

            if (_matchManager == null)
            {
                SetPanels(lobby: false, hud: false, results: false);
                return;
            }

            switch (_matchManager.CurrentState.Value)
            {
                case MatchManager.GameState.WaitingForPlayers:
                    SetPanels(lobby: true, hud: false, results: false);
                    if (_lobbyText != null)
                    {
                        _lobbyText.text = $"Ожидание игроков: {_matchManager.ConnectedPlayers.Value}/{_matchManager.RequiredPlayers}";
                    }
                    break;
                case MatchManager.GameState.InProgress:
                    SetPanels(lobby: false, hud: true, results: false);
                    if (_timerText != null)
                    {
                        _timerText.text = $"Time: {_matchManager.MatchTimer.Value:0.0}s";
                    }

                    PlayerNetwork localPlayer = TryGetLocalPlayer();
                    if (_localScoreText != null)
                    {
                        _localScoreText.text = localPlayer == null
                            ? "Score: -"
                            : $"Score: {localPlayer.Score.Value} (K/D: {localPlayer.Kills.Value}/{localPlayer.Deaths.Value})";
                    }
                    break;
                case MatchManager.GameState.ShowingResults:
                    SetPanels(lobby: false, hud: false, results: true);
                    if (_resultsBodyText != null)
                    {
                        _resultsBodyText.text = string.IsNullOrWhiteSpace(_matchManager.ResultsText.Value)
                            ? "No results yet"
                            : _matchManager.ResultsText.Value;
                    }
                    break;
            }
        }

        private void SetPanels(bool lobby, bool hud, bool results)
        {
            if (_lobbyPanel != null)
            {
                _lobbyPanel.SetActive(lobby);
            }

            if (_hudPanel != null)
            {
                _hudPanel.SetActive(hud);
            }

            if (_resultsPanel != null)
            {
                _resultsPanel.SetActive(results);
            }
        }

        private void SetDefaultResultsText()
        {
            if (_resultsTitleText != null)
            {
                _resultsTitleText.text = "Результаты";
            }

            if (_resultsFooterText != null)
            {
                _resultsFooterText.text = "Возврат в лобби...";
            }
        }

        private static PlayerNetwork TryGetLocalPlayer()
        {
            foreach (PlayerNetwork player in PlayerNetwork.ActivePlayers)
            {
                if (player != null && player.IsOwner)
                {
                    return player;
                }
            }

            return null;
        }
    }
}
