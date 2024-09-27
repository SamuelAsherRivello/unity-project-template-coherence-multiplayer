using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Coherence.MatchmakingDialogSample.UI
{
    using System;

    public class MatchmakingOptionsUI : VisualElement
    {
        private const string MaxPlayersLabelText = "Max Players";
        private const string LobbyIDLabelText = "Lobby Password";

        private const int MaxPlayersDefault = 8;
        private const int MinPlayersAllowed = 1;
        private const int MaxPlayersAllowed = 20;

        private const string ClassName = "matchmaking-options";

        public string PlayerName
        {
            set => activePlayerUI.PlayerName = value;
        }

        public IEnumerable<string> SelectedRegions => regionsInput.SelectedRegions;

        public IEnumerable<string> AvailableRegions
        {
            set => regionsInput.AvailableRegions = value;
        }

        public int MaxPlayers
        {
            get => int.Parse(maxPlayersField.value);
            set => maxPlayersField.value = value.ToString();
        }

        public Button LogoutButton => activePlayerUI.LogoutButton;

        public string LobbyID => lobbyIdField.text;

        private ActivePlayerUI activePlayerUI;
        private RegionsInput regionsInput;
        private TextField maxPlayersField;
        private TextField lobbyIdField;

        public MatchmakingOptionsUI()
        {
            AddToClassList(ClassName);

            activePlayerUI = new ActivePlayerUI();
            Add(activePlayerUI);
            
            regionsInput = new RegionsInput();
            Add(regionsInput);

            maxPlayersField = new TextField(MaxPlayersLabelText) { value = MaxPlayersDefault.ToString() };
            maxPlayersField.Q<Label>().AddToClassList(CommonUI.InputLabelClassName);
            maxPlayersField.RegisterValueChangedCallback(OnMaxPlayersChanged);
            var inputMaxPlayers = maxPlayersField.Q("unity-text-input");
            inputMaxPlayers.AddToClassList(CommonUI.TextInputClassName);
            Add(maxPlayersField);

            lobbyIdField = new TextField(LobbyIDLabelText);
            lobbyIdField.Q<Label>().AddToClassList(CommonUI.InputLabelClassName);
            var input = lobbyIdField.Q("unity-text-input");
            input.AddToClassList(CommonUI.InputFieldClassName);
            input.AddToClassList(CommonUI.TextInputClassName);
            Add(lobbyIdField);
        }

        private void OnMaxPlayersChanged(ChangeEvent<string> evt)
        {
            if (!int.TryParse(evt.newValue, out int result))
            {
                maxPlayersField.value = evt.previousValue;
            }
            
            maxPlayersField.value = Math.Clamp(result, MinPlayersAllowed, MaxPlayersAllowed).ToString();
        }

        public new class UxmlFactory : UxmlFactory<MatchmakingOptionsUI, UxmlTraits>
        {
        }
    }
}
