using MagicMayhem.Rules;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// One card in the scene. When the player lets go of it while it is over the table's play
/// spot, it snaps onto the spot and is played. Field names are unchanged from the course
/// version so the card prefab keeps its values.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class ObjectPlacementHandler : MonoBehaviour
{
    [Tooltip("The trigger on the table where a card is played. Set by CardGameManager.")]
    public GameObject targetSpot;
    public GameObject inventorySpot;
    public CardGameManager CardGameManager;
    public string pickedUpLayerName = "PickedUpObject";

    [Tooltip("0 = Fire, 1 = Ice, 2 = Water. Set by CardGameManager.")]
    public int cardType;
    public bool inInventory;
    [Tooltip("0 to 9. Set by CardGameManager.")]
    public int cardValue;

    /// <summary>True once the card has been played (or for the computer's card), after which
    /// it cannot be picked up again.</summary>
    [System.NonSerialized] public bool played;

    public Card Card => new Card((Element)cardType, cardValue);

    private XRGrabInteractable grabInteractable;
    private AudioSource grabSound;
    private bool overTarget;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabSound = GetComponent<AudioSource>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDestroy()
    {
        if (grabInteractable == null) return;
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetSpot) overTarget = true;
    }

    private void OnTriggerExit(Collider other)
    {
        // Without this, touching the play spot once meant a card released anywhere was played.
        if (other.gameObject == targetSpot) overTarget = false;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (grabSound != null) grabSound.Play();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (played || !overTarget || targetSpot == null || CardGameManager == null) return;
        if (!CardGameManager.PlaceCard(gameObject)) return;

        transform.SetPositionAndRotation(targetSpot.transform.position, targetSpot.transform.rotation);
        var body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true; // stay on the spot while the round is shown
        }
        grabInteractable.enabled = false; // a played card cannot be picked up and played again
    }
}
