namespace Coherence.MatchmakingDialogSample
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Cloud;
    using Cloud.Coroutines;
    using Connection;
    using Runtime;
    using Toolkit;
    using UI;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class MatchmakingController : MonoBehaviour
    {
        private const string LobbyAttributeKey = "password";

        [SerializeField]
        private UIDocument matchmakingDialogRoot;

        [SerializeField]
        private UIDocument disconnectButtonUI;

        [SerializeField]
        private UIDocument chatPrefab;

        [SerializeField]
        private bool useChat = true;

        [SerializeField]
        private List<KeyCode> focusChatKeys;

        private IAuthClient authClient;

        private CoherenceBridge bridge;
        private CloudRooms cloudRooms;

        private Coroutine findMatchRoutine;
        private LobbiesService lobbyService;
        private LobbySession lobbySession;

        private ParentUI parentUi;
        private ConnectingUI connectingUi;
        private LoginUI loginUi;
        private MatchmakingUI matchmakingUi;
        private LobbyUI lobbyUi;
        private ChatUI chatUi;

        private DisconnectButton disconnectButton;

        private Coroutine waitForPlayersRoutine;

        private readonly List<string> chatMessage = new();
        private bool exitChatOnFocusKeyPress;

        #region Initialization

        private void Awake()
        {
            var root = matchmakingDialogRoot.rootVisualElement;
            parentUi = root.Q<ParentUI>();
            connectingUi = parentUi.ConnectingUI;
            loginUi = parentUi.LoginUI;
            matchmakingUi = parentUi.MatchmakingUI;
            lobbyUi = parentUi.LobbyUI;

            disconnectButton = disconnectButtonUI.rootVisualElement.Q<DisconnectButton>();
            disconnectButton.visible = false;

            if (useChat && chatPrefab != null)
            {
                var chatVisualElement = Instantiate(chatPrefab).rootVisualElement;
                chatUi = chatVisualElement.Q<ChatUI>();
                chatUi.visible = false;
                parentUi.ChatUI = chatUi;
            }

            if (connectingUi == null || loginUi == null || matchmakingUi == null || lobbyUi == null ||
                disconnectButton == null)
            {
                Debug.LogError("Can't find the MatchMaking dialog Elements");
            }
        }

        private IEnumerator Start()
        {
            _ = CoherenceBridgeStore.TryGetBridge(gameObject.scene, out bridge);

            parentUi.State = SampleState.Loading;

            if (!MeetSampleRequirements())
            {
                yield break;
            }

            // Dependencies
            authClient = bridge.CloudService.GameServices.AuthService;
            cloudRooms = bridge.CloudService.Rooms;
            lobbyService = cloudRooms.LobbyService;

            // Events Listeners
            RegisterEventsListeners();

            // UI Buttons Actions
            RegisterUiButtonsActions();

            // We wait for a websocket connection with the coherence Cloud to show the Login or Matchmaking UIs
            yield return RefreshCloudConnection();
        }

        // Check the requirements to use this Sample successfully
        // 1. Scene must have a CoherenceBridge Component.
        // 2. CoherenceBridge must have the Auto Login as Guest option unchecked.
        // 3. You must be logged in to the coherence Cloud via the Coherence Hub Window.
        private bool MeetSampleRequirements()
        {
            if (bridge == null)
            {
                connectingUi.SetContent("Missing coherence Bridge Component in the Scene.", false);
                return false;
            }

            if (bridge.AutoLoginAsGuest)
            {
                connectingUi.SetContent(
                    "coherence Bridge Auto-Login as guest option should be disabled to use this Sample.",
                    false);
                return false;
            }

            if (string.IsNullOrEmpty(RuntimeSettings.Instance.ProjectID))
            {
                connectingUi.SetContent("coherence Cloud Lobbies are not available because you are not " +
                                        "logged in. You can log in via the Cloud section in the " +
                                        "coherence Hub window.", false);
                return false;
            }

            return true;
        }

        // see region Events Listeners for the implementations
        private void RegisterEventsListeners()
        {
            lobbyService.OnPlaySessionStarted += JoinRoom;
            authClient.OnLogin += OnLoginToCoherenceAccount;
            authClient.OnLogout += OnLogoutFromCoherenceAccount;
            bridge.onConnected.AddListener(OnConnectedToGame);
            bridge.onDisconnected.AddListener(OnDisconnectedFromGame);
            bridge.onConnectionError.AddListener(OnConnectionError);
        }

        // see region UI Buttons Actions for the implementations
        private void RegisterUiButtonsActions()
        {
            loginUi.LoginButton.RegisterCallback<ClickEvent>(_ => Login());
            matchmakingUi.LogoutButton.RegisterCallback<ClickEvent>(_ => LeaveLobbyAndLogout());
            matchmakingUi.FindMatchButton.RegisterCallback<ClickEvent>(_ =>
                findMatchRoutine = StartCoroutine(FindMatchRoutine()));
            lobbyUi.LogoutButton.RegisterCallback<ClickEvent>(_ => LeaveLobbyAndLogout());
            lobbyUi.StartButton.RegisterCallback<ClickEvent>(_ => StartGameSession());
            lobbyUi.LeaveButton.RegisterCallback<ClickEvent>(_ => LeaveLobby());
            lobbyUi.ResumeButton.RegisterCallback<ClickEvent>(ResumeGameSession);
            disconnectButton.Button.RegisterCallback<ClickEvent>(_ => bridge.Disconnect());
            parentUi.MessageBox.OnMessageDismissed += ResetUiState;

            chatUi?.MessageBox.RegisterCallback<KeyDownEvent>(OnChatFocusKeyPress);
        }

        // Initialize connection with coherence and the show the initial UI
        private IEnumerator RefreshCloudConnection()
        {
            // We wait for a websocket connection with the coherence Cloud to show the Login or Matchmaking UIs

            connectingUi.SetContent("Waiting for connection to coherence Cloud...", true);

            yield return cloudRooms.WaitForCloudConnection();

            // Waiting for the session token refresh result will allow us to remember the last account we have logged in with
            // If there is no session token present, or the current one has expired, we will be forced to log in manually
            while (!authClient.SessionTokenRefreshResult.HasValue)
            {
                yield return null;
            }

            var sessionRefreshResult = authClient.SessionTokenRefreshResult.Value;

            if (sessionRefreshResult == Result.Success)
            {
                // The stored session token was valid, we transition to the Matchmaking UI
                InitMatchmakingUI();
            }
            else
            {
                // The stored session token was not present or was expired, we force to log in again
                parentUi.State = SampleState.Login;
            }
        }

        private void InitMatchmakingUI()
        {
            parentUi.State = SampleState.MatchMaking;

            matchmakingUi.Options.PlayerName = authClient.UserName;
            cloudRooms.RefreshRegions(OnRegionsRefreshed);
        }

        private void OnRegionsRefreshed(RequestResponse<IReadOnlyList<string>> response)
        {
            if (!IsSuccessfulRequest(response))
            {
                return;
            }

            matchmakingUi.Options.AvailableRegions = response.Result;
        }

        private void OnDisable()
        {
            StopRoutines();
        }

        #endregion

        #region Event Listeners

        // Update Sample UI state when we connect to a game session
        private void OnConnectedToGame(CoherenceBridge bridge)
        {
            parentUi.visible = false;
            disconnectButton.visible = true;
        }

        // Update Sample UI state when we disconnect from an ongoing game session
        private void OnDisconnectedFromGame(CoherenceBridge bridge, ConnectionCloseReason reason)
        {
            ResetUiState();
            parentUi.visible = true;
            disconnectButton.visible = false;
        }

        // Show Error Message when there is a connection error in the middle of a Game Session
        private void OnConnectionError(CoherenceBridge bridge, ConnectionException exception)
        {
            parentUi.MessageBox.Show(exception.Message);
            Debug.LogException(exception);
        }

        // Go back to the Login UI when we logout
        private void OnLogoutFromCoherenceAccount()
        {
            parentUi.State = SampleState.Login;
        }

        // Upon logging in, we check if we are currently part of any active Lobbies
        private void OnLoginToCoherenceAccount(LoginResponse response)
        {
            if (response.LobbyIds != null && response.LobbyIds.Count > 0)
            {
                TransitionUiOnSuccessfulLogin(response.LobbyIds[0]);
            }
        }

        // Join an active Game Session via a coherence Room
        private void JoinRoom(string lobbyId, RoomData room)
        {
            // The Lobby we were a part of has started a Game Session and we join the provided Room
            StopRoutines();
            parentUi.State = SampleState.Loading;
            connectingUi.SetContent("Starting game...", true);
            bridge.JoinRoom(room);
        }

        #endregion

        #region UI Button Actions

        // Hooked to the Login button of the Login UI
        private async void Login()
        {
            var username = loginUi.Username;
            var password = loginUi.Password;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                parentUi.MessageBox.Show(
                    "You must specify a username and a password. If the account does not exist, one will be created automatically for you.");
                return;
            }

            try
            {
                parentUi.State = SampleState.Loading;
                connectingUi.SetContent("Logging in...", true);
                var result = await authClient.LoginWithPassword(username, password, true);

                HandleLoginResult(result);
            }
            catch (Exception e)
            {
                parentUi.MessageBox.Show(e.Message);
                Debug.LogException(e);
            }
        }

        // Hooked to the Logout buttons of the Matchmaking and Lobby Session UIs
        private void LeaveLobbyAndLogout()
        {
            StopRoutines();

            if (lobbySession?.IsDisposed is false)
            {
                parentUi.State = SampleState.Loading;
                connectingUi.SetContent("Leaving Lobby...", true);
                lobbySession.LeaveLobby(response => OnLeftLobby(response, true));
            }
            else
            {
                authClient.Logout();
            }
        }

        // We start a Game Session for our current Lobby. This action can be started manually by pressing the Start button,
        // or it will be started automatically if the Lobby is full of players.
        private void StartGameSession()
        {
            if (lobbySession is { IsDisposed: true, LobbyOwnerActions: null, })
            {
                return;
            }

            StopRoutines();

            parentUi.State = SampleState.Loading;
            connectingUi.SetContent("Waiting for game to start...", true);

            Debug.Log($"[SAMPLE] Game Owner is sending /play request for Lobby {lobbySession.LobbyData.Id}");

            lobbySession.LobbyOwnerActions.StartGameSession(OnGameSessionStarted, unlistLobby: true, closeLobby: true);
        }

        // If the Lobby already has an ongoing Game Session, we can manually join the coherence Room.
        private void ResumeGameSession(ClickEvent evt)
        {
            if (lobbySession == null)
            {
                return;
            }

            RefreshLobby(() =>
            {
                lobbyUi.RefreshUI(lobbySession);

                if (lobbySession.LobbyData.RoomData.HasValue)
                {
                    JoinRoom(lobbySession.LobbyData.Id, lobbySession.LobbyData.RoomData.Value);
                }
                else
                {
                    parentUi.MessageBox.Show("Room has expired.");
                }
            });
        }

        // Hooked to the Leave button of the Lobby UI. This action will abandon the current Lobby without logging out,
        // and it will go back to the Matchmaking UI.
        private void LeaveLobby()
        {
            StopRoutines();

            if (!lobbySession?.IsDisposed ?? false)
            {
                parentUi.State = SampleState.Loading;
                connectingUi.SetContent("Leaving Lobby...", true);
                lobbySession.LeaveLobby(response => OnLeftLobby(response, false));
            }
            else
            {
                parentUi.State = SampleState.MatchMaking;
            }
        }

        // Hooked to the Find Match button of the Matchmaking UI. This action will start the matchmaking process given
        // the parameters selected.
        private IEnumerator FindMatchRoutine()
        {
            // Fetch selected regions from the UI
            var selectedRegions =
                matchmakingUi.Options.SelectedRegions.Select(r => r.ToLowerInvariant()).ToList();

            // Build filter for the selected regions, Lobby Passowrd and Max Players
            var lobbyFilter = new LobbyFilter()
                .WithAnd()
                .WithRegion(FilterOperator.Any, selectedRegions)
                .WithAnd()
                .WithStringAttribute(FilterOperator.Equals, StringAttributeIndex.s1, matchmakingUi.Options.LobbyID)
                .WithAnd()
                .WithMaxPlayers(FilterOperator.Equals, matchmakingUi.Options.MaxPlayers)
                .End()
                .End();

            CloudAttribute lobbyAttribute =
                new(LobbyAttributeKey, matchmakingUi.Options.LobbyID, StringAttributeIndex.s1,
                    StringAggregator.None, true);

            // We create the matchmaking options, we sort the results by the current number of players so lobbies that have players
            // are prioritized when considering which one to join
            FindLobbyOptions findOptions = new()
            {
                Limit = 20,
                LobbyFilters = new List<LobbyFilter>
                {
                    lobbyFilter,
                },
                Sort = new Dictionary<SortOptions, bool>
                {
                    { SortOptions.numPlayers, true },
                },
            };

            var regionToCreate = selectedRegions.FirstOrDefault() ?? cloudRooms.Regions.FirstOrDefault();

            // We provide a Create Options object to know how to create a new Lobby. This happens when no suitable Lobby
            // is found during the Find step.
            CreateLobbyOptions createOptions = new()
            {
                LobbyAttributes = new List<CloudAttribute>
                {
                    lobbyAttribute,
                },
                MaxPlayers = matchmakingUi.Options.MaxPlayers,
                Region = regionToCreate,
            };

            // Used filters can be easily debugged by printing them with the ToString function
            Debug.Log($"[SAMPLE] Starting Matchmaking with filter {lobbyFilter.ToString()}.");

            parentUi.State = SampleState.Loading;
            connectingUi.SetContent("Finding a match...", true);

            // We make the backend call to coherence to start the matchmaking process
            var task = lobbyService.FindOrCreateLobbyAsync(findOptions, createOptions);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted && task.Exception != null)
            {
                if (task.Exception.GetBaseException() is RequestException requestException)
                {
                    parentUi.MessageBox.Show($"{requestException.StatusCode}: {requestException.ErrorCode}");
                }
                else
                {
                    parentUi.MessageBox.Show(task.Exception.Message);
                }

                Debug.LogException(task.Exception);
                findMatchRoutine = null;

                yield break;
            }

            var newLobbySession = task.Result;

            // We transition the UI to the Lobby phase and we wait for the Lobby to be full to start the Game Session automatically
            yield return WaitingForPlayersAndGameStartPhase(newLobbySession);

            findMatchRoutine = null;
        }

        #endregion

        # region Chat Actions

        // Event callback fired when we receive chat messages from other players
        private void OnMessageReceived(LobbySession session, MessagesReceived messages)
        {
            // Ignore messages from ourselves since we print them instantly when they are input in the chat
            if (session.MyPlayer.HasValue && messages.PlayerSenderId == session.MyPlayer.Value.UserId)
            {
                return;
            }

            var senderName = session.LobbyData.Players
                .FirstOrDefault(p => p.UserId == messages.PlayerSenderId).Username;

            if (string.IsNullOrEmpty(senderName))
            {
                return;
            }

            foreach (var message in messages.Messages)
            {
                ProcessMessage(messages.Time, senderName, message);
            }
        }

        // We use the Update loop to focus the chat message box
        private void Update()
        {
            if (chatUi == null)
            {
                return;
            }

            if (IsFocusKeyPressed())
            {
                chatUi.FocusChat();
            }
        }

        private bool IsFocusKeyPressed()
        {
            return focusChatKeys.Any(Input.GetKeyDown);
        }

        private bool IsFocusKeyPressed(KeyCode key)
        {
            return focusChatKeys.Any(focusKey => focusKey == key);
        }

        // UI Key Down Action that we use within the chat message box to send the written message to other players
        private void OnChatFocusKeyPress(KeyDownEvent evt)
        {
            var isFocusKeyPressed = IsFocusKeyPressed(evt.keyCode);

            if (!isFocusKeyPressed || string.IsNullOrEmpty(chatUi.MessageBox.value) ||
                chatUi.MessageBox.value.Equals(ChatUI.PlaceholderText))
            {
                // We do this to avoid blurring the chat box when pressing enter with an empty message
                if (exitChatOnFocusKeyPress && evt.keyCode == KeyCode.None)
                {
                    exitChatOnFocusKeyPress = false;
                    evt.StopImmediatePropagation();
#if UNITY_2023_2_OR_NEWER
                    chatUi?.MessageBox.focusController?.IgnoreEvent(evt);
#else
                    evt.PreventDefault();
#endif
                }

                exitChatOnFocusKeyPress = isFocusKeyPressed;

                return;
            }

            evt.StopPropagation();

            OnMessageEntered(chatUi.MessageBox.value);
        }

        // We process the message we've just written in the chat message box
        private void OnMessageEntered(string text)
        {
            chatMessage.Clear();
            chatMessage.Add(text);
            ProcessMessage((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(), authClient.UserName, text);
            lobbySession.SendMessage(chatMessage, null);
            chatUi.MessageBox.SetValueWithoutNotify(string.Empty);
            chatUi.FocusChat();
        }

        private void ProcessMessage(int time, string senderName, string message)
        {
            var timeStamp = DateTimeOffset.FromUnixTimeSeconds(time).LocalDateTime;
            var builtMessage =
                $"[{timeStamp.Hour}:{timeStamp.Minute:00}] [{senderName}]: {message}";
            chatUi.AddMessage(builtMessage);
        }

        #endregion

        private IEnumerator WaitingForPlayersAndGameStartPhase(LobbySession activeLobbySession)
        {
            Debug.Log(
                $"[SAMPLE] Starting Waiting For Players matchmaking phase for Lobby: {activeLobbySession.LobbyData.Id}");

            TransitionUiToLobby(activeLobbySession);

            StringBuilder logBuilder = new();

            // We wait for the Lobby to be full of players to start the Game Session automatically
            while (lobbySession.LobbyData.Players.Count < lobbySession.LobbyData.MaxPlayers)
            {
                logBuilder.Clear();
                logBuilder.Append(
                    $"[SAMPLE] Waiting For Lobby {lobbySession.LobbyData.Id} to be full to start the Game ({lobbySession.LobbyData.Players.Count}/{lobbySession.LobbyData.MaxPlayers}). Owner is {lobbySession.OwnerPlayer.Username}. PLAYERS: ");

                foreach (var player in lobbySession.LobbyData.Players)
                {
                    logBuilder.Append($" ({player.Username}) ");
                }

                Debug.Log(logBuilder.ToString());

                yield return new WaitForSeconds(2f);
            }

            // Lobby is full, Game will be launched by the owner

            Debug.Log(
                $"[SAMPLE] Lobby is FULL. Waiting for the Owner {lobbySession.OwnerPlayer.Username} to start the game of Lobby {lobbySession.LobbyData.Id}");

            StartGameSession();

            waitForPlayersRoutine = null;
        }

        private void TransitionUiToLobby(LobbySession activeLobbySession)
        {
            // Transition UI to the Lobby phase and we refresh it with the current Lobby
            parentUi.State = SampleState.InLobby;
            lobbyUi.RefreshUI(activeLobbySession);

            if (lobbySession != null)
            {
                lobbySession.OnLobbyUpdated -= OnLobbyUpdated;
                lobbySession.OnMessageReceived -= OnMessageReceived;
                lobbySession.OnPlayerJoined -= OnPlayerJoined;
                lobbySession.OnPlayerLeft -= OnPlayerLeft;
                lobbySession.OnLobbyDisposed -= OnLobbyDisposed;
            }

            lobbySession = activeLobbySession;

            // We hook a UI refresher to the OnLobbyUpdated event
            lobbySession.OnLobbyUpdated += OnLobbyUpdated;
            lobbySession.OnMessageReceived += OnMessageReceived;
            lobbySession.OnLobbyDisposed += OnLobbyDisposed;

            if (chatUi != null)
            {
                lobbySession.OnPlayerJoined += OnPlayerJoined;
                lobbySession.OnPlayerLeft += OnPlayerLeft;
            }
        }

        private void OnPlayerLeft(LobbySession session, Player player, string reason)
        {
            var message =
                $"{player.Username} has left the lobby! ({session.LobbyData.Players.Count}/{session.LobbyData.MaxPlayers})";
            chatUi.AddMessage(message);
        }

        private void OnPlayerJoined(LobbySession session, Player player)
        {
            var message =
                $"{player.Username} has joined the lobby! ({session.LobbyData.Players.Count}/{session.LobbyData.MaxPlayers})";
            chatUi.AddMessage(message);
        }

        // Upon logging in, if we are part of an active Lobby, we check the status of the Lobby and we transition the UI accordingly
        private async void TransitionUiOnSuccessfulLogin(string lobbyId)
        {
            Debug.Log($"[SAMPLE] LoginResponse returned Lobby: {lobbyId}. Moving to next matchmaking phase.");

            try
            {
                var sessionForLobbyId = await lobbyService.GetActiveLobbySessionForLobbyId(lobbyId);

                if (sessionForLobbyId != null)
                {
                    // If the active Lobby has no active Game Session, we transition the UI to the Lobby screen
                    Debug.Log(
                        $"[SAMPLE] LoginResponse returned Lobby: {lobbyId}. Moving to Waiting For Players matchmaking phase.");

                    waitForPlayersRoutine = StartCoroutine(WaitingForPlayersAndGameStartPhase(sessionForLobbyId));
                }
                else
                {
                    parentUi.State = SampleState.MatchMaking;
                }
            }
            catch (Exception e)
            {
                parentUi.MessageBox.Show(e.Message);
                Debug.LogException(e);
            }
        }

        private void OnLobbyUpdated(LobbySession activeLobbySession)
        {
            lobbyUi.RefreshUI(activeLobbySession);
        }

        private void HandleLoginResult(Result result)
        {
            if (result != Result.Success)
            {
                parentUi.MessageBox.Show(
                    $"{(result == Result.InvalidCredentials ? "Invalid username or password" : result)}");
                return;
            }

            InitMatchmakingUI();
        }

        private void OnLeftLobby(RequestResponse<bool> response, bool logout)
        {
            if (!IsSuccessfulRequest(response))
            {
                return;
            }

            if (!logout)
            {
                return;
            }

            authClient.Logout();
        }

        private void OnLobbyDisposed(LobbySession lobby)
        {
            if (lobbySession != lobby)
            {
                return;
            }

            lobbySession.OnLobbyUpdated -= OnLobbyUpdated;
            lobbySession.OnMessageReceived -= OnMessageReceived;
            lobbySession.OnPlayerJoined -= OnPlayerJoined;
            lobbySession.OnPlayerLeft -= OnPlayerLeft;
            lobbySession.OnLobbyDisposed -= OnLobbyDisposed;
            lobbySession = null;
            parentUi.State = SampleState.MatchMaking;
        }

        private void OnGameSessionStarted(RequestResponse<bool> response)
        {
            IsSuccessfulRequest(response);

            // We don't have to do anything else at this point, coherence will start the game and join the room automatically.
            // If we want to override this behaviour and handle it manually, we have to register a callback through LobbyService.OnPlaySessionStarted
        }

        private void StopRoutines()
        {
            if (findMatchRoutine != null)
            {
                StopCoroutine(findMatchRoutine);
                findMatchRoutine = null;
            }

            if (waitForPlayersRoutine != null)
            {
                StopCoroutine(waitForPlayersRoutine);
                waitForPlayersRoutine = null;
            }
        }

        private void ResetUiState()
        {
            if (!authClient.LoggedIn)
            {
                parentUi.State = SampleState.Login;
            }
            else if (lobbySession?.IsDisposed is false)
            {
                RefreshLobby(() =>
                {
                    lobbyUi.RefreshUI(lobbySession);
                    parentUi.State = SampleState.InLobby;
                });
            }
            else
            {
                parentUi.State = SampleState.MatchMaking;
            }
        }

        private void RefreshLobby(Action onRefreshed)
        {
            parentUi.State = SampleState.Loading;
            connectingUi.SetContent("Refreshing Lobby Data", true);
            lobbySession.RefreshLobby(onRefreshed);
        }

        private bool IsSuccessfulRequest<T>(RequestResponse<T> response)
        {
            if (response.Status == RequestStatus.Success)
            {
                return true;
            }

            parentUi.MessageBox.Show(response.Exception.Message);
            return false;
        }
    }
}
