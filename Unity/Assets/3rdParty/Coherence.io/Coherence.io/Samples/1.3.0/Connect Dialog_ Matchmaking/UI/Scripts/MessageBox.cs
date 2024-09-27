namespace Coherence.MatchmakingDialogSample.UI
{
    using System;
    using UnityEngine.UIElements;

    public class MessageBox : VisualElement
    {
        private const string ClassName = "message-box";
        private const string MessageContainerClassName = ClassName + "__message-container";
        private const string IconClassName = ClassName + "__icon";
        private const string WindowClassName = ClassName + "__window";
        private const string MessageClassName = ClassName + "__label";
        private const string TitleClassName = ClassName + "__title";
        private const string DismissClassName = ClassName + "__dismiss";

        private readonly VisualElement window;
        private readonly Label messageLabel;
        private readonly Label titleLabel;
        private readonly Image icon;

        public VisualElement dialogParent;

        public Action OnMessageDismissed;

        public string Text
        {
            get => messageLabel.text;
            set => messageLabel.text = value;
        }

        public MessageBox()
        {
            AddToClassList(ClassName);

            window = new VisualElement();
            window.AddToClassList(WindowClassName);
            {
                var messageContainer = new VisualElement();
                messageContainer.AddToClassList(MessageContainerClassName);
                {
                    icon = new Image();
                    icon.AddToClassList(IconClassName);
                    messageContainer.Add(icon);
                    titleLabel = new Label();
                    titleLabel.text = "Error";
                    titleLabel.AddToClassList(TitleClassName);
                    messageContainer.Add(titleLabel);
                }
                window.Add(messageContainer);
                messageLabel = new Label();
                messageLabel.AddToClassList(MessageClassName);
                window.Add(messageLabel);

                var buttonContainer = new VisualElement();
                buttonContainer.AddToClassList(DismissClassName);
                {
                    var dismissButton = new Button(Hide);
                    dismissButton.text = "Dismiss";
                    dismissButton.AddToClassList(CommonUI.SecondaryButtonClassName);
                    buttonContainer.Add(dismissButton);
                }
                window.Add(buttonContainer);
            }
            Add(window);
        }

        public void Show(string message)
        {
            Text = message;
            dialogParent.Add(this);
            visible = true;
        }

        private void Hide()
        {
            visible = false;
            dialogParent.Remove(this);
            OnMessageDismissed?.Invoke();
        }

        public new class UxmlFactory : UxmlFactory<MessageBox, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription message = new()
            {
                name = "message",
                defaultValue = "",
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var box = (MessageBox)ve;
                box.Text = message.GetValueFromBag(bag, cc);
            }
        }
    }
}
