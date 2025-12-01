using UnityEngine;

public class StartAnimationTrigger : MonoBehaviour
{

    // Reference to the character's Animator component
    public Animator characterAnimator;

    // The name of the Trigger parameter defined in the Animator Controller
    public string triggerParameterName = "StartTrigger";

    // Assign this method to the Button's OnClick() event
    public void TriggerJumpAnimation()
    {
        if (characterAnimator != null)
        {
            // Fires the animation transition
            characterAnimator.SetTrigger(triggerParameterName);
        }
        else
        {
            Debug.LogWarning("Animator is not assigned in the inspector!");
        }
    }
}