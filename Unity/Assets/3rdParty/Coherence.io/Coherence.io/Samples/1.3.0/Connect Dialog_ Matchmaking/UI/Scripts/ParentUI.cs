namespace Coherence.MatchmakingDialogSample.UI
{
    using System;
    using UnityEngine.UIElements;

    public enum SampleState
    {
        Loading,
        Login,
        MatchMaking,
        InLobby,
    }

    public class ParentUI : CommonUI
    {
        private const string ClassName = "matchmaking-dialog";

        private SampleState state;

        public ConnectingUI ConnectingUI { get; }
        public LoginUI LoginUI { get; }
        public MatchmakingUI MatchmakingUI { get; }
        public LobbyUI LobbyUI { get; }
        public MessageBox MessageBox { get; }

        public ChatUI ChatUI { get; set; }

        private readonly VisualElement formContainer;

        private VisualElement currentForm;

        public SampleState State
        {
            get => state;
            set
            {
                if (value != state)
                {
                    SetState(value);
                }
            }
        }

        public ParentUI()
        {
            AddToClassList(ClassName);
            formContainer = new VisualElement();
            {
                LoginUI = new LoginUI();
                MatchmakingUI = new MatchmakingUI();
                ConnectingUI = new ConnectingUI();
                LobbyUI = new LobbyUI();
            }
            Add(formContainer);
            MessageBox = new MessageBox();
            MessageBox.dialogParent = this;
            SetState(SampleState.Loading);
        }

        private void SetState(SampleState newState)
        {
            state = newState;
            var newForm = GetFormForState(newState);

            if (newForm == currentForm)
            {
                return;
            }

            formContainer.Clear();
            currentForm = newForm;
            formContainer.Add(newForm);

            if (ChatUI == null || newState == SampleState.Loading)
            {
                return;
            }

            bool alreadyVisible = ChatUI.visible;

            ChatUI.visible = newState == SampleState.InLobby;

            if (alreadyVisible && ChatUI.visible)
            {
                return;
            }

            ChatUI.ClearChat();
        }

        private VisualElement GetFormForState(SampleState newState)
        {
            return newState switch
            {
                SampleState.Loading => ConnectingUI,
                SampleState.Login => LoginUI,
                SampleState.MatchMaking => MatchmakingUI,
                SampleState.InLobby => LobbyUI,
                _ => throw new ArgumentOutOfRangeException(nameof(newState), newState, null),
            };
        }

        public new class UxmlFactory : UxmlFactory<ParentUI, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlEnumAttributeDescription<SampleState> loginState = new()
            {
                name = "state",
                defaultValue = SampleState.MatchMaking,
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var dialog = (ParentUI)ve;
                dialog.State = loginState.GetValueFromBag(bag, cc);
            }
        }
    }
}
