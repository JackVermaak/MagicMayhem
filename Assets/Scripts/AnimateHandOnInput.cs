using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Drives the hand model's pinch and grip animations from the controller's trigger
/// and grip buttons.</summary>
public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty pinchAnimationAction;
    public InputActionProperty gripAnimationAction;
    public Animator handAnimator;

    private static readonly int Trigger = Animator.StringToHash("Trigger");
    private static readonly int Grip = Animator.StringToHash("Grip");

    private void Update()
    {
        if (handAnimator == null) return;
        if (pinchAnimationAction.action != null)
            handAnimator.SetFloat(Trigger, pinchAnimationAction.action.ReadValue<float>());
        if (gripAnimationAction.action != null)
            handAnimator.SetFloat(Grip, gripAnimationAction.action.ReadValue<float>());
    }
}
