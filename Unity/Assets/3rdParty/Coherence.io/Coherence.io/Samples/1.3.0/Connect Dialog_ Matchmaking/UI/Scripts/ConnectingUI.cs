namespace Coherence.MatchmakingDialogSample.UI
{
    using UnityEngine.UIElements;

    public class ConnectingUI : VisualElement
    {
        private const string ClassName = "connecting-ui";
        private const string ContainerClassName = ClassName + "__content";
        private const string LoadingContentClassName = ClassName + "__loading-content";
        private const string LabelClassName = ClassName + "__label";

        private readonly Label messageLabel;
        private readonly VisualElement content;
        private readonly VisualElement loadingContainer;
        private readonly Spinner spinner;

        public ConnectingUI()
        {
            AddToClassList(ClassName);
            content = new VisualElement();
            content.AddToClassList(ContainerClassName);
            {
                loadingContainer = new VisualElement();
                loadingContainer.AddToClassList(LoadingContentClassName);
                {
                    messageLabel = new Label();
                    messageLabel.AddToClassList(LabelClassName);
                    loadingContainer.Add(messageLabel);
                    spinner = new Spinner();
                    loadingContainer.Add(spinner);
                }
                content.Add(loadingContainer);
            }
            Add(content);
        }

        public void SetContent(string text, bool showSpinner)
        {
            messageLabel.text = text;

            switch (showSpinner)
            {
                case true when !loadingContainer.Contains(spinner):
                    loadingContainer.Add(spinner);
                    break;
                case false when loadingContainer.Contains(spinner):
                    loadingContainer.Remove(spinner);
                    break;
            }
        }
    }
}
