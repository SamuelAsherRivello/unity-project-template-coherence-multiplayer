namespace Coherence.MatchmakingDialogSample.UI
{
    using UnityEngine.UIElements;

    public class DisconnectButton : VisualElement
    {
        private const string BtnLabel = "Disconnect";

        private const string ClassName = "disconnect-button";
        private const string ButtonContainerClassName = ClassName + "__button-container";
        private const string ButtonClassName = ClassName + "__button";
        public Button Button { get; }

        public DisconnectButton()
        {
            AddToClassList(ClassName);

            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList(ButtonContainerClassName);
            {
                Button = new Button
                {
                    text = BtnLabel,
                };
                Button.AddToClassList(ButtonClassName);
                buttonContainer.Add(Button);
            }

            Add(buttonContainer);
        }

        public new class UxmlFactory : UxmlFactory<DisconnectButton, UxmlTraits>
        {
        }
    }
}
