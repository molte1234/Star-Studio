# Character Menu System - Unity Setup Guide

## Overview
The Character Menu System allows players to click on characters to open an expandable menu with action buttons. When clicked, the character moves to a "focus socket" and 4 action buttons expand outward with a close button to return the character to their original position.

## Components

### 1. CharacterMenuController
**Purpose:** Manages the expanded character menu state, button animations, and character movement.

**Location:** Should be placed on a GameObject in your scene (e.g., on the UI Canvas or as a separate MenuManager GameObject)

### 2. CharacterObject (Modified)
**Purpose:** Handles character clicks and communicates with CharacterMenuController.

**Changes Made:**
- Added `SetRoomController()` method to store room reference
- Added `GetCharacter()` method to retrieve character state
- Modified `CharacterClicked()` to open menu via CharacterMenuController
- Added check to prevent menu opening when character is busy

### 3. RoomController (Modified)
**Purpose:** Now includes a focus socket for the expanded character view.

**Changes Made:**
- Added `focusSocket` field - a Transform where the character moves when clicked
- Modified `SpawnCharacterVisual()` to call `SetRoomController()` on spawned characters

## Unity Scene Setup

### Step 1: Create the CharacterMenuController GameObject

1. In your scene hierarchy, create a new empty GameObject
2. Name it "CharacterMenuController"
3. Add the `CharacterMenuController` component to it
4. **Important:** This should be a single instance in your scene (uses singleton pattern)

### Step 2: Create the Menu Button Container

1. Create a new UI GameObject under your Canvas
   - Right-click Canvas → UI → Empty (or use an existing panel)
   - Name it "MenuButtonsContainer"

2. Add a Canvas Group component to it (for potential fade effects)

3. Position it where you want the buttons to appear (usually near the center or where characters are displayed)

### Step 3: Create the Action Buttons

1. Create 4 UI Buttons as children of MenuButtonsContainer
   - Name them: "ActionButton1", "ActionButton2", "ActionButton3", "ActionButton4"

2. Add the `UIButton` component to each button (if not already present)

3. Layout options:
   - **Horizontal Layout:** Space them horizontally with even spacing
   - **Radial Layout:** Arrange them in a semi-circle
   - **Grid Layout:** 2x2 grid arrangement

4. Set their positions to (0,0,0) in local space initially - the CharacterMenuController will animate them

### Step 4: Create the Close Button

1. Create a UI Button as a child of MenuButtonsContainer
2. Name it "CloseButton"
3. Add the `UIButton` component to it
4. Position it above or in the center of the action buttons
5. Set appropriate sprite/text (e.g., "X" or "Close")

### Step 5: Connect CharacterMenuController References

Select the CharacterMenuController GameObject and in the Inspector:

1. **Current Room Controller:** Drag the active RoomController from your scene
   - This can be set dynamically in code if you switch rooms

2. **Menu Buttons Container:** Drag the MenuButtonsContainer GameObject

3. **Action Buttons (List):**
   - Set size to 4
   - Drag each ActionButton (1-4) into the array elements

4. **Close Button:** Drag the CloseButton GameObject

5. **Animation Settings:**
   - Button Animation Duration: `0.3` (default)
   - Button Spacing: `100` (adjust based on your button size)
   - Button Ease: `OutBack` (default)
   - Character Move Duration: `0.5` (default)
   - Character Move Ease: `OutCubic` (default)

### Step 6: Setup Focus Socket in RoomController

For each RoomController in your scene:

1. Select the RoomController GameObject

2. In the scene view, create a new empty GameObject
   - Position it where you want characters to move when clicked
   - This is typically in the center or foreground of your room view
   - Name it "FocusSocket"

3. Make the FocusSocket a child of the RoomController (or keep it separate if you prefer)

4. In the RoomController Inspector, find the new "Focus Socket" field

5. Drag the FocusSocket GameObject into this field

### Step 7: Configure Action Buttons (Optional)

For each action button, you can:

1. Set the button's sprite/appearance
2. Add text labels
3. Connect onClick events to specific actions
4. Add button hover sounds via the UIButton component

Example button functions to connect:
- Start Training Action
- Move to Different Room
- View Character Stats
- Assign to Action

## Animation Flow

### Opening the Menu

1. Player clicks on a character
2. `CharacterObject.CharacterClicked()` is called
3. Checks if character is busy (if yes, shows "busy" feedback)
4. Calls `CharacterMenuController.Instance.OpenCharacterMenu()`
5. CharacterMenuController stores the original socket position
6. Character animates to the focus socket (moves + scales up to 1.1x)
7. Menu buttons animate from scale 0 → 1 with staggered timing
8. Buttons spread out horizontally based on buttonSpacing
9. Close button appears last

### Closing the Menu

1. Player clicks the close button
2. `CharacterMenuController.CloseMenu()` is called
3. Buttons animate to scale 0 (reverse animation)
4. Character animates back to original socket position
5. Character scales back to 1.0x
6. Menu buttons container is hidden
7. Menu state is reset

## Testing Checklist

- [ ] CharacterMenuController is in the scene as a singleton
- [ ] Focus socket is assigned in each RoomController
- [ ] Menu buttons container has Canvas Group
- [ ] All 4 action buttons are assigned in CharacterMenuController
- [ ] Close button is assigned in CharacterMenuController
- [ ] Click on character opens menu
- [ ] Buttons animate in with stagger effect
- [ ] Close button returns character to original position
- [ ] Can't open menu when character is busy
- [ ] Only one menu can be open at a time

## Common Issues & Solutions

### "Menu is already open" warning
**Cause:** Trying to open a second menu before closing the first
**Solution:** Close the current menu before opening a new one, or add auto-close behavior

### Character doesn't move to focus socket
**Cause:** Focus socket not assigned in RoomController
**Solution:** Create a FocusSocket GameObject and assign it in the Inspector

### Buttons don't animate
**Cause:** Menu buttons container or buttons not assigned
**Solution:** Check all references are assigned in CharacterMenuController Inspector

### Character returns to wrong position
**Cause:** Character socket assignment changed while menu was open
**Solution:** Store socket position (not index) or prevent socket changes while menu is open

### Character can't be clicked
**Cause:** Missing collider on CharacterObject's Image component
**Solution:** Ensure the Image component has a collider (BoxCollider2D or use AlphaRaycast.cs)

## Future Enhancements

- Add action button functionality (connect to GameManager.StartAction)
- Add "Move to Room" buttons that show available rooms
- Add character stats display in the menu
- Add animations for different button layouts (radial, grid)
- Add sound effects for menu open/close
- Add particle effects when character moves
- Add blur/dim effect on background when menu is open
- Support for dynamic button count (not just 4)

## Code Integration Example

### Connecting Action Buttons to Game Actions

```csharp
// In CharacterMenuController or a separate ActionButtonHandler:

public void OnActionButton1Clicked()
{
    if (currentExpandedCharacter != null)
    {
        // Example: Start a training action
        CharacterSlotState character = currentExpandedCharacter.GetCharacter();
        ActionData trainingAction = GameManager.Instance.GetActionByName("Train");

        if (trainingAction != null)
        {
            GameManager.Instance.StartAction(trainingAction, new int[] { character.slotIndex });
            CloseMenu(); // Close the menu after starting action
        }
    }
}
```

### Dynamic Room Selection

```csharp
// Show buttons for each available room
public void ShowRoomButtons()
{
    List<RoomData> availableRooms = GameManager.Instance.GetAvailableRooms();

    for (int i = 0; i < actionButtons.Count && i < availableRooms.Count; i++)
    {
        int roomIndex = i; // Capture for lambda
        actionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = availableRooms[roomIndex].roomName;
        actionButtons[i].onClick.RemoveAllListeners();
        actionButtons[i].onClick.AddListener(() => MoveCharacterToRoom(availableRooms[roomIndex]));
    }
}

private void MoveCharacterToRoom(RoomData targetRoom)
{
    if (currentExpandedCharacter != null)
    {
        CharacterSlotState character = currentExpandedCharacter.GetCharacter();
        GameManager.Instance.MoveCharacterToRoom(character, targetRoom);
        CloseMenu();
    }
}
```

## Notes

- The system uses DOTween for all animations - ensure DOTween is imported in your project
- The singleton pattern is used for CharacterMenuController - only one instance should exist
- Characters can only open menu when not busy (isBusy = false)
- Menu automatically prevents opening multiple menus simultaneously
- All animation durations and easing can be customized in the Inspector
