namespace Coherence.MatchmakingDialogSample.UI
{
    using UnityEngine.UIElements;

    public class MatchmakingUI : VisualElement
    {
        private const string FindMatchText = "Find Match";

        private const string ClassName = "matchmaking-lobby";

        private const string FindMatchContentLabelClassName = ClassName + "__findmatch-content";
        private const string ButtonsRowContainerClassName = ClassName + "__buttons";

        public MatchmakingOptionsUI Options { get; }

        public Button LogoutButton => Options.LogoutButton;
        public Button FindMatchButton { get; }

        private readonly VisualElement findMatchContent;
        private readonly VisualElement findingMatchButtons;

        public MatchmakingUI()
        {
            AddToClassList(ClassName);

            Options = new MatchmakingOptionsUI();
            Add(Options);

            findMatchContent = new VisualElement();
            findMatchContent.AddToClassList(FindMatchContentLabelClassName);
            {
                findingMatchButtons = new VisualElement();
                findingMatchButtons.AddToClassList(ButtonsRowContainerClassName);
                {
                    FindMatchButton = new Button();
                    FindMatchButton.text = FindMatchText;
                    findingMatchButtons.Add(FindMatchButton);
                }
                findMatchContent.Add(findingMatchButtons);
            }
            Add(findMatchContent);
        }

        public new class UxmlFactory : UxmlFactory<MatchmakingUI, UxmlTraits>
        {
        }
    }
}
