namespace Coherence.MatchmakingDialogSample.UI
{
    using Cloud;
    using UnityEngine.UIElements;

    public class LobbyUI : VisualElement
    {
        private const string RegionLabelText = "Region";
        private const string MaxPlayersLabelText = "Max Players";
        private const string LobbyPwLabelText = "Password";
        private const string OpenToPlayersText = "Opened";
        private const int MinPlayerRows = 8;

        private const string ClassName = "lobby-session";
        private const string PlayersContainerClassName = ClassName + "__players-container";
        private const string PlayersHeaderClassName = ClassName + "__players-header";
        private const string PlayerCellClassName = ClassName + "__player-cell";
        private const string EvenBgClassName = ClassName + "__player-background-even";
        private const string OddBgClassName = ClassName + "__player-background-odd";
        private const string PlayersScrollClassName = ClassName + "__players-scroll";
        private const string ButtonsContainerClassName = ClassName + "__buttons-container";
        private const string UserStarClassName = ClassName + "__user-star";
        private const string PlayerOwnerClassName = ClassName + "__player-owner";

        private const float ScrollMouseWheelSpeed = 150f;

        private readonly ActivePlayerUI activePlayerUI;
        private readonly VisualElement buttonsContainer;
        private readonly TextField lobbyPw;
        private readonly TextField maxPlayers;
        private readonly TextField isLobbyOpened;
        private readonly Label playersHeader;
        private readonly ScrollView playersScroll;
        private readonly TextField region;

        public Button LogoutButton => activePlayerUI.LogoutButton;
        public Button LeaveButton { get; }
        public Button StartButton { get; }
        public Button ResumeButton { get; }

        public LobbyUI()
        {
            activePlayerUI = new ActivePlayerUI();
            Add(activePlayerUI);

            region = AddInfoTextField(RegionLabelText);
            maxPlayers = AddInfoTextField(MaxPlayersLabelText);
            lobbyPw = AddInfoTextField(LobbyPwLabelText);
            isLobbyOpened = AddInfoTextField(OpenToPlayersText);

            var playersInfoContainer = new VisualElement();
            playersInfoContainer.AddToClassList(PlayersContainerClassName);
            {
                var headerContainer = new VisualElement();
                headerContainer.AddToClassList(PlayersHeaderClassName);
                playersHeader = new Label();
                headerContainer.Add(playersHeader);
                playersInfoContainer.Add(headerContainer);

                playersScroll = new ScrollView
                {
                    mode = ScrollViewMode.Vertical,
                };
                playersScroll.AddToClassList(PlayersScrollClassName);
                playersInfoContainer.Add(playersScroll);

                buttonsContainer = new VisualElement();
                buttonsContainer.AddToClassList(ButtonsContainerClassName);
                playersInfoContainer.Add(buttonsContainer);

                LeaveButton = new Button
                {
                    text = "Leave",
                };
                LeaveButton.AddToClassList(CommonUI.SecondaryButtonClassName);

                StartButton = new Button
                {
                    text = "Start",
                };

                ResumeButton = new Button
                {
                    text = "Resume",
                };
            }

            Add(playersInfoContainer);
        }

        public void RefreshUI(LobbySession lobbySession)
        {
            activePlayerUI.PlayerName = lobbySession.MyPlayer.Value.Username;
            region.value = lobbySession.LobbyData.Region.ToUpperInvariant();
            maxPlayers.value = lobbySession.LobbyData.MaxPlayers.ToString();
            isLobbyOpened.value = lobbySession.LobbyData.Closed ? "Not Accepting New Players" : "Accepting New Players";

            var pwAttribute = lobbySession.LobbyData.GetAttribute("password");

            lobbyPw.value = pwAttribute.HasValue ? pwAttribute.Value.GetStringValue() : string.Empty;
            playersHeader.text =
                $"Players ({lobbySession.LobbyData.Players.Count}/{lobbySession.LobbyData.MaxPlayers})";

            BuildPlayersScroll(lobbySession);

            buttonsContainer.Clear();

            buttonsContainer.Add(LeaveButton);

            if (lobbySession.LobbyData.RoomData.HasValue)
            {
                buttonsContainer.Add(ResumeButton);
                return;
            }

            if (lobbySession.OwnerPlayer != lobbySession.MyPlayer)
            {
                return;
            }

            buttonsContainer.Add(StartButton);
        }

        private void BuildPlayersScroll(LobbySession lobbySession)
        {
            playersScroll.Clear();

            for (var i = 0; i < lobbySession.LobbyData.Players.Count; i++)
            {
                var isEven = i % 2 == 0;

                var player = lobbySession.LobbyData.Players[i];

                var playerContainer = new VisualElement();
                playerContainer.AddToClassList(PlayerCellClassName);
                playerContainer.AddToClassList(isEven ? EvenBgClassName : OddBgClassName);

                if (player == lobbySession.OwnerPlayer)
                {
                    var userStar = new Image();
                    userStar.AddToClassList(UserStarClassName);
                    playerContainer.Add(userStar);

                    var playerLabel = new Label(player.Username);
                    playerLabel.AddToClassList(PlayerOwnerClassName);
                    playerContainer.Add(playerLabel);
                }
                else
                {
                    var playerLabel = new Label(player.Username);
                    playerContainer.Add(playerLabel);
                }

                playersScroll.Add(playerContainer);
            }

            if (lobbySession.LobbyData.Players.Count >= MinPlayerRows)
            {
                return;
            }

            for (var i = lobbySession.LobbyData.Players.Count; i <= MinPlayerRows; i++)
            {
                var isEven = i % 2 == 0;

                var playerContainer = new VisualElement();
                playerContainer.AddToClassList(isEven ? EvenBgClassName : OddBgClassName);
                var emptyLabel = new Label();
                playerContainer.Add(emptyLabel);
                playersScroll.Add(playerContainer);
            }
        }

        private TextField AddInfoTextField(string title)
        {
            var newTextField = new TextField(title)
            {
                isReadOnly = true,
            };
            newTextField.Q<Label>().AddToClassList(CommonUI.InputLabelClassName);
            var textFieldInput = newTextField.Q("unity-text-input");
            textFieldInput.AddToClassList(CommonUI.InputFieldClassName);
            textFieldInput.AddToClassList(CommonUI.TextInputClassName);
            textFieldInput.AddToClassList(CommonUI.NoBoxInputFieldClassName);
            Add(newTextField);

            return newTextField;
        }
    }
}
