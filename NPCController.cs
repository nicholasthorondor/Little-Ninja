using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour {

    [SerializeField] string[] dialogEntries;
    [SerializeField] string dialogExhausted;

    bool npcIsTalking = false; // Use to track whether npc is talking to the player.
    int dialogCount = 0;
    int maxDialogCount = 0;
    bool isDialogExhausted = false;

    // Returns npc talking status.
    public bool IsNpcTalking {
        get {
            return npcIsTalking;
        }
    }

    // Handles the delivery of the dialog assigned to the NPC.
    public void Talk () {
        Flip ();
        maxDialogCount = dialogEntries.Length; // Keeps track of the maximum dialog entries associated with the npc.

        if (dialogCount < maxDialogCount) { // Each time the player presses next the dialog will carry forward onto the next.
            npcIsTalking = true;
            print (dialogEntries [dialogCount]);
            dialogCount++;
        } else if (dialogCount == maxDialogCount && !isDialogExhausted) { // Once dialog is exhausted, set bool to trigger general response.
            print ("fade out and restore player control"); // TODO - fade out text, restore player control as main dialog is done.
            isDialogExhausted = true;
            npcIsTalking = false;
        } else if (isDialogExhausted && !npcIsTalking) { // Print the npcs general response.
            npcIsTalking = true;
            print (dialogExhausted);
        } else if (isDialogExhausted && npcIsTalking) { // Toggles npc talking bool so player is able to move after talking to npc during general response.
            npcIsTalking = false;
        }
    }

    // Flips the NPCs x axis to face the player when spoken to.
    void Flip () {
        Vector3 scale = GameManager.instance.Player.transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
