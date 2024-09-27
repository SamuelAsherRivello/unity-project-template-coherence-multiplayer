namespace Coherence.MatchmakingDialogSample.UI
{
    using UnityEngine.UIElements;

    public class ActivePlayerUI : VisualElement
    {
        private const string LogoutText = "Logout";
        private const string PlayerLabelText = "Player";

        private const string ClassName = "matchmaking-options";
        private const string LogoutButtonClassName = ClassName + "__logout-button";
        private const string AccountContainerClassName = ClassName + "__account";
        private const string PlayerTextFieldClassName = ClassName + "__player-name";

        public Button LogoutButton { get; }

        public string PlayerName
        {
            set => playerNameField.value = value;
        }

        private readonly TextField playerNameField;

        public ActivePlayerUI()
        {
            AddToClassList(AccountContainerClassName);

            playerNameField = new TextField(PlayerLabelText)
            {
                isReadOnly = true,
            };
            playerNameField.AddToClassList(PlayerTextFieldClassName);
            playerNameField.Q<Label>().AddToClassList(CommonUI.InputLabelClassName);
            var playerNameInput = playerNameField.Q("unity-text-input");
            playerNameInput.AddToClassList(CommonUI.TextInputClassName);
            playerNameInput.AddToClassList(CommonUI.NoBoxInputFieldClassName);
            Add(playerNameField);

            LogoutButton = new Button
            {
                text = LogoutText,
            };
            LogoutButton.AddToClassList(LogoutButtonClassName);
            LogoutButton.AddToClassList(CommonUI.SecondaryButtonClassName);
            Add(LogoutButton);
        }
    }
}
