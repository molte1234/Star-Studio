using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// Core game manager singleton - handles all game state
/// Fixed version with all necessary fields and proper access modifiers
/// </summary>
public class GameManager : MonoBehaviour
{
    // ============================================
    // SINGLETON PATTERN
    // ============================================

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ GameManager singleton created");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ============================================
    // GAME RULES (Required by TimeManager/CircularTimer)
    // ============================================

    [Title("Game Configuration")]
    [Required]
    [Tooltip("Game balance configuration (ScriptableObject)")]
    public GameRules rules;

    // ============================================
    // CHARACTER & BAND STATE
    // ============================================

    [Title("Character System")]
    [Tooltip("Array of character states (public for other scripts)")]
    public CharacterSlotState[] characterStates = new CharacterSlotState[6];

    [Header("Band Info")]
    public string bandName = "The Band";

    // ============================================
    // ROOM SYSTEM
    // ============================================

    [Title("Room System")]
    [Required]
    [Tooltip("Default room where characters spawn (usually Lobby)")]
    public RoomData defaultRoom;

    [Tooltip("All room definitions in the game")]
    public RoomData[] allRooms;

    [Tooltip("Room controllers in the scene (auto-found if not set)")]
    public RoomController[] roomControllers;

    [Header("Character Spawning")]
    [Required]
    [Tooltip("Prefab for spawning character visuals (must have CharacterObject component)")]
    public GameObject characterPrefab;

    [Tooltip("Delay before spawning characters (gives UI time to initialize)")]
    public float spawnDelay = 0.5f;

    // ============================================
    // GAME RESOURCES
    // ============================================

    [Title("Resources")]
    [ShowInInspector] public int money = 50000;
    [ShowInInspector] public int fans = 100;
    [ShowInInspector] public float unity = 0.8f;

    // ============================================
    // TIME PROGRESSION
    // ============================================

    [Title("Time System")]
    [ShowInInspector] public int currentQuarter = 0;
    [ShowInInspector] public int currentYear = 1;

    // ============================================
    // STATS (8-STAT SYSTEM)
    // ============================================

    [Title("Band Stats (Sum of all members)")]
    [ShowInInspector, ReadOnly] public int charisma = 0;
    [ShowInInspector, ReadOnly] public int stagePerformance = 0;
    [ShowInInspector, ReadOnly] public int vocal = 0;
    [ShowInInspector, ReadOnly] public int instrument = 0;
    [ShowInInspector, ReadOnly] public int songwriting = 0;
    [ShowInInspector, ReadOnly] public int production = 0;
    [ShowInInspector, ReadOnly] public int management = 0;
    [ShowInInspector, ReadOnly] public int practical = 0;

    // ============================================
    // MANAGER REFERENCES (Public for access)
    // ============================================

    [Title("Manager References")]
    [Tooltip("EventManager reference (public for UIController_GameOver)")]
    public EventManager eventManager;

    [SerializeField] private UIController_Game uiController;

    // Note: AudioManager uses singleton pattern (AudioManager.Instance)
    // so we don't need to store a reference here

    // ============================================
    // DEBUG DISPLAY
    // ============================================

    [Title("Debug - Band Members")]
    [ShowInInspector, ReadOnly] private string slot0_Name = "EMPTY";
    [ShowInInspector, ReadOnly] private string slot1_Name = "EMPTY";
    [ShowInInspector, ReadOnly] private string slot2_Name = "EMPTY";
    [ShowInInspector, ReadOnly] private string slot3_Name = "EMPTY";
    [ShowInInspector, ReadOnly] private string slot4_Name = "EMPTY";
    [ShowInInspector, ReadOnly] private string slot5_Name = "EMPTY";

    [Header("Testing")]
    public bool skipWelcomeScreen = false;
    public bool testingMode = false;

    // ============================================
    // FLAGS & STATE (Public for UIController_GameOver)
    // ============================================

    [HideInInspector] public List<string> flags = new List<string>();
    public bool isNewGame = true;  // Made public for UIController_GameOver
    private bool hasSpawnedCharacters = false;
    private bool isWaitingToSpawn = false;

    // ============================================
    // INITIALIZATION
    // ============================================

    void Start()
    {
        // Validate game rules
        if (rules == null)
        {
            Debug.LogError("❌ GameManager: No GameRules assigned! TimeManager won't work properly.");
        }

        // Find room controllers will happen when we're ready to spawn
        Debug.Log("🎮 GameManager initialized - waiting for band setup...");

        // Validate setup
        if (characterPrefab == null)
        {
            Debug.LogError("❌ GameManager: No character prefab assigned! Characters won't spawn visually.");
        }
        else
        {
            var charObj = characterPrefab.GetComponent<CharacterObject>();
            if (charObj == null)
            {
                Debug.LogError("❌ GameManager: Character prefab doesn't have CharacterObject component!");
            }
        }

        if (defaultRoom == null)
        {
            Debug.LogWarning("⚠️ GameManager: No default room assigned! Please assign Lobby or BreakRoom.");
        }
    }

    void Update()
    {
        UpdateDebugDisplay();

        // Check if we should spawn characters
        if (isWaitingToSpawn && !hasSpawnedCharacters)
        {
            CheckIfReadyToSpawn();
        }
    }

    // ============================================
    // DELAYED SPAWNING SYSTEM
    // ============================================

    /// <summary>
    /// Check if we're ready to spawn (all systems initialized)
    /// </summary>
    private void CheckIfReadyToSpawn()
    {
        // Make sure we have characters to spawn
        bool hasCharacters = false;
        for (int i = 0; i < characterStates.Length; i++)
        {
            if (characterStates[i] != null && characterStates[i].slotData != null)
            {
                hasCharacters = true;
                break;
            }
        }

        if (!hasCharacters)
        {
            Debug.Log("⏳ Waiting for characters to be set up...");
            return;
        }

        // Make sure UI is ready
        if (uiController == null)
        {
            Debug.Log("⏳ Waiting for UIController to register...");
            return;
        }

        // All systems ready - spawn!
        StartCoroutine(DelayedSpawn());
    }

    /// <summary>
    /// Spawn with a small delay to ensure everything is ready
    /// </summary>
    private IEnumerator DelayedSpawn()
    {
        isWaitingToSpawn = false;
        hasSpawnedCharacters = true;

        Debug.Log($"⏰ Spawning characters in {spawnDelay} seconds...");
        yield return new WaitForSeconds(spawnDelay);

        // Find room controllers now
        if (roomControllers == null || roomControllers.Length == 0)
        {
            roomControllers = FindObjectsOfType<RoomController>();
            Debug.Log($"🏠 Found {roomControllers.Length} room controllers in scene");

            foreach (var controller in roomControllers)
            {
                if (controller.roomData != null)
                {
                    Debug.Log($"   - {controller.roomData.roomName} controller ready");
                }
            }
        }

        // Now spawn
        if (defaultRoom != null)
        {
            SpawnCharactersInDefaultRoom();
        }
        else
        {
            Debug.LogError("❌ Cannot spawn characters - no default room set!");
        }
    }

    // ============================================
    // GAME SETUP
    // ============================================

    /// <summary>
    /// Called by BandSetupManager when starting new game
    /// OR by TestBandHelper for debug testing
    /// </summary>
    public void SetupNewGame(SlotData[] selectedBand, string bandName)
    {
        this.bandName = bandName;

        Debug.Log("========================================");
        Debug.Log($"🎸 SETTING UP NEW GAME: {bandName}");
        Debug.Log("========================================");

        // Clear any existing characters first
        ClearAllCharacters();

        // Create character states from selected band
        int validCharacterCount = 0;
        for (int i = 0; i < selectedBand.Length && i < characterStates.Length; i++)
        {
            if (selectedBand[i] != null)
            {
                characterStates[i] = new CharacterSlotState(selectedBand[i]);

                // Set default room
                if (defaultRoom != null)
                {
                    characterStates[i].currentRoom = defaultRoom;
                    // Note: Don't add to room's charactersPresent yet - do that when spawning
                }

                validCharacterCount++;
                Debug.Log($"   ✅ Slot {i}: {selectedBand[i].displayName}");
            }
            else
            {
                characterStates[i] = null;
                Debug.Log($"   ⬜ Slot {i}: Empty");
            }
        }

        Debug.Log($"   Total Characters: {validCharacterCount}");
        Debug.Log($"   Starting Room: {(defaultRoom != null ? defaultRoom.roomName : "NOT SET")}");

        // Recalculate band stats
        RecalculateStats();

        // Mark that we're ready to spawn (but wait for other systems)
        isWaitingToSpawn = true;
        hasSpawnedCharacters = false;

        Debug.Log("========================================");
        Debug.Log("📍 Band setup complete - will spawn when all systems ready");
    }

    // ============================================
    // CHARACTER SPAWNING
    // ============================================

    /// <summary>
    /// Spawns all band members in the default room (usually Lobby)
    /// </summary>
    private void SpawnCharactersInDefaultRoom()
    {
        if (defaultRoom == null)
        {
            Debug.LogError("❌ No default room set!");
            return;
        }

        // Clear room's character list first
        defaultRoom.charactersPresent.Clear();

        // Find the room controller for default room
        RoomController roomController = FindRoomController(defaultRoom);
        if (roomController == null)
        {
            Debug.LogError($"❌ No RoomController found for {defaultRoom.roomName}! " +
                          "Make sure there's a RoomController in the scene with this room assigned.");
            return;
        }

        Debug.Log($"🎬 Spawning characters in {defaultRoom.roomName}...");

        // Spawn each character at an available socket
        int spawnedCount = 0;
        for (int i = 0; i < characterStates.Length; i++)
        {
            if (characterStates[i] != null && characterStates[i].slotData != null)
            {
                // Add to room's character list
                defaultRoom.charactersPresent.Add(characterStates[i]);

                // Find socket and spawn visual
                int emptySocket = roomController.FindEmptySocket();
                if (emptySocket >= 0)
                {
                    roomController.AssignCharacterToSocket(emptySocket, characterStates[i]);
                    spawnedCount++;
                    Debug.Log($"   ✅ Spawned {characterStates[i].slotData.displayName} at socket {emptySocket}");
                }
                else
                {
                    Debug.LogWarning($"   ⚠️ No empty socket for {characterStates[i].slotData.displayName}!");
                }
            }
        }

        Debug.Log($"🎭 Successfully spawned {spawnedCount} characters in {defaultRoom.roomName}");

        // Refresh UI after spawning
        ForceRefreshUI();
    }

    /// <summary>
    /// Find the RoomController for a specific room
    /// </summary>
    private RoomController FindRoomController(RoomData room)
    {
        if (roomControllers == null || room == null) return null;

        foreach (var controller in roomControllers)
        {
            if (controller != null && controller.roomData == room)
            {
                return controller;
            }
        }

        return null;
    }

    /// <summary>
    /// Clear all characters from all rooms
    /// </summary>
    private void ClearAllCharacters()
    {
        // Clear character states
        for (int i = 0; i < characterStates.Length; i++)
        {
            if (characterStates[i] != null && characterStates[i].currentRoom != null)
            {
                characterStates[i].currentRoom.RemoveCharacter(characterStates[i]);
            }
            characterStates[i] = null;
        }

        // Clear all room controllers
        if (roomControllers != null)
        {
            foreach (var controller in roomControllers)
            {
                if (controller != null)
                {
                    controller.ClearAllSockets();
                }
            }
        }

        // Clear room data
        if (allRooms != null)
        {
            foreach (var room in allRooms)
            {
                if (room != null)
                {
                    room.charactersPresent.Clear();
                }
            }
        }

        hasSpawnedCharacters = false;
    }

    // ============================================
    // ROOM MANAGEMENT
    // ============================================

    /// <summary>
    /// Move a character to a different room
    /// </summary>
    public void MoveCharacterToRoom(int characterIndex, RoomData targetRoom)
    {
        if (characterIndex < 0 || characterIndex >= characterStates.Length)
        {
            Debug.LogError($"❌ Invalid character index: {characterIndex}");
            return;
        }

        CharacterSlotState character = characterStates[characterIndex];
        if (character == null || character.slotData == null)
        {
            Debug.LogError($"❌ No character at slot {characterIndex}");
            return;
        }

        if (targetRoom == null)
        {
            Debug.LogError("❌ Target room is null!");
            return;
        }

        // Check if room can be entered
        if (!targetRoom.CanEnter())
        {
            Debug.LogWarning($"⚠️ Cannot enter {targetRoom.roomName} - locked or full!");
            return;
        }

        // Find controllers
        RoomController currentController = FindRoomController(character.currentRoom);
        RoomController targetController = FindRoomController(targetRoom);

        if (targetController == null)
        {
            Debug.LogError($"❌ No controller found for {targetRoom.roomName}!");
            return;
        }

        // Find empty socket in target room
        int targetSocket = targetController.FindEmptySocket();
        if (targetSocket < 0)
        {
            Debug.LogWarning($"⚠️ No empty sockets in {targetRoom.roomName}!");
            return;
        }

        // Remove from current room
        if (character.currentRoom != null)
        {
            character.currentRoom.RemoveCharacter(character);

            if (currentController != null)
            {
                int currentSocket = currentController.FindCharacterSocket(character);
                if (currentSocket >= 0)
                {
                    currentController.ClearSocket(currentSocket);
                }
            }
        }

        // Add to target room
        targetRoom.AddCharacter(character);
        character.currentRoom = targetRoom;
        targetController.AssignCharacterToSocket(targetSocket, character);

        Debug.Log($"🚶 {character.slotData.displayName} moved to {targetRoom.roomName}");

        // Refresh UI
        ForceRefreshUI();
    }

    // ============================================
    // STATS CALCULATION
    // ============================================

    public void RecalculateStats()
    {
        // Reset all stats
        charisma = 0;
        stagePerformance = 0;
        vocal = 0;
        instrument = 0;
        songwriting = 0;
        production = 0;
        management = 0;
        practical = 0;

        // Sum up stats from all characters
        for (int i = 0; i < characterStates.Length; i++)
        {
            if (characterStates[i] != null && characterStates[i].slotData != null)
            {
                SlotData data = characterStates[i].slotData;
                charisma += data.charisma;
                stagePerformance += data.stagePerformance;
                vocal += data.vocal;
                instrument += data.instrument;
                songwriting += data.songwriting;
                production += data.production;
                management += data.management;
                practical += data.practical;
            }
        }

        Debug.Log($"📊 Stats recalculated - Total Charisma: {charisma}, Vocal: {vocal}, etc.");
    }

    // ============================================
    // ACTION SYSTEM
    // ============================================

    public void StartAction(ActionData action, List<int> memberIndices)
    {
        Debug.Log($"🎬 Starting action: {action.actionName} with {memberIndices.Count} members");

        // Check if we have enough money
        int totalCost = action.baseCost + (action.costPerCharacter * memberIndices.Count);
        if (money < totalCost)
        {
            Debug.LogWarning($"❌ Not enough money for {action.actionName}! Need ${totalCost}, have ${money}");
            return;
        }

        // Deduct cost
        money -= totalCost;
        Debug.Log($"💰 Paid ${totalCost} for action");

        // Start action for each selected character
        foreach (int index in memberIndices)
        {
            if (index >= 0 && index < characterStates.Length && characterStates[index] != null)
            {
                CharacterSlotState character = characterStates[index];
                character.isBusy = true;
                character.currentAction = action;
                character.actionTimeRemaining = action.baseTime;
                character.actionTotalDuration = action.baseTime;

                Debug.Log($"   ▶️ {character.slotData.displayName} started {action.actionName}");
            }
        }

        ForceRefreshUI();
    }

    public void CancelAction(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characterStates.Length) return;

        CharacterSlotState character = characterStates[characterIndex];
        if (character == null || !character.isBusy) return;

        Debug.Log($"🛑 Canceling action for {character.slotData.displayName}");

        character.isBusy = false;
        character.currentAction = null;
        character.actionTimeRemaining = 0f;
        character.actionTotalDuration = 0f;

        ForceRefreshUI();
    }

    public void CompleteAction(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characterStates.Length) return;

        CharacterSlotState character = characterStates[characterIndex];
        if (character == null || !character.isBusy) return;

        ActionData action = character.currentAction;
        if (action != null)
        {
            // Add rewards (using correct field names)
            money += action.rewardMoney;
            fans += action.rewardFans;

            Debug.Log($"✅ {character.slotData.displayName} completed {action.actionName}");
            Debug.Log($"   💰 +${action.rewardMoney} | 👥 +{action.rewardFans} fans");
        }

        // Clear action state
        character.isBusy = false;
        character.currentAction = null;
        character.actionTimeRemaining = 0f;
        character.actionTotalDuration = 0f;

        ForceRefreshUI();
    }

    // ============================================
    // TIME PROGRESSION
    // ============================================

    public void AdvanceQuarter()
    {
        currentQuarter++;

        // Check for year advancement
        if (currentQuarter >= 4)
        {
            currentQuarter = 0;
            currentYear++;
            Debug.Log($"📅 Advanced to Year {currentYear}");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayYearAdvance();
            }
        }
        else
        {
            Debug.Log($"📅 Advanced to Quarter {currentQuarter + 1}, Year {currentYear}");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayQuarterAdvance();
            }
        }

        ForceRefreshUI();
    }

    // ============================================
    // UI REFRESH
    // ============================================

    public void ForceRefreshUI()
    {
        if (uiController != null)
        {
            uiController.RefreshUI();
        }
    }

    // ============================================
    // MANAGER REGISTRATION
    // ============================================

    public void RegisterUIController(UIController_Game controller)
    {
        uiController = controller;
        Debug.Log("✅ UIController registered with GameManager");

        // Check if we should spawn now
        if (isWaitingToSpawn && !hasSpawnedCharacters)
        {
            CheckIfReadyToSpawn();
        }
    }

    public void RegisterEventManager(EventManager manager)
    {
        eventManager = manager;
        Debug.Log("✅ EventManager registered with GameManager");

        // Check for welcome screen
        CheckForWelcomeScreen();
    }

    private void CheckForWelcomeScreen()
    {
        if (isNewGame && eventManager != null && !skipWelcomeScreen)
        {
            Debug.Log("🎮 NEW GAME START - SHOWING WELCOME SCREEN");
            eventManager.ShowWelcomeScreen();

            if (!testingMode)
            {
                isNewGame = false;
            }
        }
    }

    // ============================================
    // DEBUG DISPLAY
    // ============================================

    private void UpdateDebugDisplay()
    {
        slot0_Name = GetSlotDebugName(0);
        slot1_Name = GetSlotDebugName(1);
        slot2_Name = GetSlotDebugName(2);
        slot3_Name = GetSlotDebugName(3);
        slot4_Name = GetSlotDebugName(4);
        slot5_Name = GetSlotDebugName(5);
    }

    private string GetSlotDebugName(int index)
    {
        if (index < 0 || index >= characterStates.Length) return "ERROR";

        if (characterStates[index] != null && characterStates[index].slotData != null)
        {
            string name = characterStates[index].slotData.displayName;
            string busy = characterStates[index].isBusy ? " [BUSY]" : "";
            string room = characterStates[index].currentRoom != null
                ? $" ({characterStates[index].currentRoom.roomName})"
                : " (NO ROOM)";
            return name + busy + room;
        }
        return "EMPTY";
    }

    // ============================================
    // GETTERS
    // ============================================

    public CharacterSlotState[] GetCharacterStates()
    {
        return characterStates;
    }

    public CharacterSlotState GetCharacterState(int index)
    {
        if (index >= 0 && index < characterStates.Length)
        {
            return characterStates[index];
        }
        return null;
    }

    public bool HasCharacters()
    {
        for (int i = 0; i < characterStates.Length; i++)
        {
            if (characterStates[i] != null && characterStates[i].slotData != null)
            {
                return true;
            }
        }
        return false;
    }
}