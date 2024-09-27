namespace Coherence.MatchmakingDialogSample.UI
{
    using UnityEngine.UIElements;

    public class ChatUI : VisualElement
    {
        private const string ClassName = "chat-ui";
        private const string FocusedClassName = ClassName + "__focused";
        private const string TextFieldClassName = ClassName + "__text-field";
        private const string TextFieldBoxClassName = ClassName + "__text-field-box";
        private const string TextClassName = ClassName + "__text";
        private const string ScrollClassName = ClassName + "__scroll";
        
#if UNITY_2022_3_OR_NEWER
        private const string TextFieldBoxFocusClassName = ClassName + "__text-field-box-focus";
#endif

        public const string PlaceholderText = "Press Enter to start typing...";

        private ScrollView chatScroll;
        private VisualElement messageBoxInput;

        public TextField MessageBox { get; }

        public ChatUI()
        {
            AddToClassList(ClassName);

            MessageBox = new TextField(string.Empty);
            MessageBox.AddToClassList(TextFieldClassName);
            MessageBox.SetPlaceholderText(PlaceholderText, false);
            messageBoxInput = MessageBox.Q("unity-text-input");
            messageBoxInput.AddToClassList(CommonUI.TextInputClassName);
            messageBoxInput.AddToClassList(TextFieldBoxClassName);
            messageBoxInput.AddToClassList(TextClassName);
            Add(MessageBox);

#if UNITY_2022_3_OR_NEWER
            messageBoxInput.RegisterCallback<FocusInEvent>(_ => OnChatFocused());
            messageBoxInput.RegisterCallback<FocusOutEvent>(_ => OnChatUnfocused());
#else
            messageBoxInput.RegisterCallback<FocusEvent>(_ => OnChatFocused());
            messageBoxInput.RegisterCallback<BlurEvent>(_ => OnChatUnfocused());
#endif
           
            chatScroll = new ScrollView
            {
                mode = ScrollViewMode.Vertical,
            };
            chatScroll.AddToClassList(ScrollClassName);
            Add(chatScroll);
        }

        public void AddMessage(string message)
        {
            var messageLabel = new Label(message);
            messageLabel.AddToClassList(TextClassName);
            messageLabel.enableRichText = false;
            chatScroll.Insert(0, messageLabel);
            schedule.Execute(() => chatScroll.ScrollTo(messageLabel))
                .StartingIn(100);
        }

        public void FocusChat()
        {
            schedule.Execute(() =>
            {
                if (IsMessageBoxFocused())
                {
                    return;
                }

                messageBoxInput.Focus();
            }).StartingIn(1);
        }

        private bool IsMessageBoxFocused()
        {
            return messageBoxInput.panel.focusController.focusedElement == messageBoxInput;
        }

        public void ClearChat()
        {
            chatScroll.Clear();
        }

        private void OnChatFocused()
        {
            RemoveFromClassList(ClassName);
            AddToClassList(FocusedClassName);
#if UNITY_2022_3_OR_NEWER
            messageBoxInput.AddToClassList(TextFieldBoxFocusClassName);
#endif
        }

        private void OnChatUnfocused()
        {
            RemoveFromClassList(FocusedClassName);
            AddToClassList(ClassName);
#if UNITY_2022_3_OR_NEWER
            messageBoxInput.RemoveFromClassList(TextFieldBoxFocusClassName);
#endif
        }

        public new class UxmlFactory : UxmlFactory<ChatUI, UxmlTraits>
        {
        }
    }
}
