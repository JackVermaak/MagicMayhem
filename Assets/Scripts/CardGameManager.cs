using System.Collections;
using MagicMayhem.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runs a match in the scene: deals the player's hand, reacts when a card is placed on the
/// table, shows the computer's answer and the result, and restarts the scene at the end.
/// The rules themselves live in MagicMayhem.Rules (Match, CardRules), which is tested
/// without Unity. Field names are unchanged from the course version so the scene keeps its
/// references.
/// </summary>
public class CardGameManager : MonoBehaviour
{
    [Header("Cards")]
    public GameObject cardPrefab;
    [Tooltip("Cards in the player's hand. One slot in Card Pos is needed per card.")]
    public int numberOfCards = 5;
    [Tooltip("Where the player's cards are dealt.")]
    public Transform[] cardPos;
    [Tooltip("Where the computer's card appears.")]
    public Transform tablePos;
    [Tooltip("Card faces, indexed by value 0 to 9.")]
    public Material[] materialsFire;
    public Material[] materialsIce;
    public Material[] materialsWater;
    [Tooltip("The trigger on the table where a card is played.")]
    public GameObject placedCardPos;
    public GameObject inventoryCardPos;
    public GameObject attachpoint; // unused; kept so the scene's reference stays valid

    [Header("Match")]
    [Tooltip("Rounds in a match. If the score is level after them, extra rounds decide it.")]
    public int roundsToPlay = 3;
    [Tooltip("Seconds both cards stay on the table after a round.")]
    public float roundDisplayTime = 3f;
    [Tooltip("Seconds before a new match starts.")]
    public float restartDelay = 6f;
    [Tooltip("Layer for the player's cards.")]
    public int cardLayer = 7;

    [Header("Feedback")]
    public AudioSource winSound;
    public AudioSource loseSound;
    public Canvas winCanvas;
    public Canvas tableCanvas;
    [Tooltip("Seconds a message stays up.")]
    public float displayTime = 3f;
    [Tooltip("Seconds the instructions stay up at the start.")]
    public float introDisplayTime = 8f;

    private Match match;
    private bool roundInProgress;
    private Coroutine hideMessage;
    private TextMeshProUGUI messageText;
    private TextMeshProUGUI scoreText;

    private void Start()
    {
        messageText = winCanvas != null ? winCanvas.GetComponentInChildren<TextMeshProUGUI>() : null;
        scoreText = tableCanvas != null ? tableCanvas.GetComponentInChildren<TextMeshProUGUI>() : null;
        if (winCanvas != null) winCanvas.enabled = false;

        int handSize = Mathf.Clamp(numberOfCards, 1, cardPos != null ? cardPos.Length : 0);
        if (handSize < 1 || tablePos == null || cardPrefab == null)
        {
            Debug.LogError("CardGameManager needs a card prefab, a table position and at least one card position.", this);
            enabled = false;
            return;
        }
        int rounds = Mathf.Clamp(roundsToPlay, 1, handSize);
        match = new Match(new System.Random(), handSize, rounds);

        for (int i = 0; i < match.PlayerHand.Count; i++)
        {
            GameObject cardObject = SpawnCard(cardPos[i], match.PlayerHand[i]);
            cardObject.layer = cardLayer;
            var handler = cardObject.GetComponent<ObjectPlacementHandler>();
            handler.CardGameManager = this;
            handler.targetSpot = placedCardPos;
            handler.inventorySpot = inventoryCardPos;
        }

        UpdateScore();
        ShowMessage($"Play {rounds} of your {handSize} cards, one at a time, by placing them on the table. "
                    + "Fire beats Ice, Ice beats Water and Water beats Fire. The same element: higher number wins.",
                    introDisplayTime);
    }

    /// <summary>Called by a card when it is released on the table. Returns false if the card
    /// cannot be played right now (a round is still on the table, or the match is over).</summary>
    public bool PlaceCard(GameObject placedCard)
    {
        if (match == null || match.IsOver) return false;
        if (roundInProgress)
        {
            ShowMessage("Wait for the cards on the table to clear first.");
            return false;
        }
        var handler = placedCard.GetComponent<ObjectPlacementHandler>();
        if (handler == null || handler.played) return false;

        handler.played = true;
        roundInProgress = true;
        StartCoroutine(PlayRound(placedCard, handler.Card));
        return true;
    }

    private IEnumerator PlayRound(GameObject playerCardObject, Card playerCard)
    {
        RoundResult round = match.Play(playerCard);
        GameObject computerCardObject = SpawnCard(tablePos, round.ComputerCard);
        // The computer's card is only for show: it cannot be picked up or played.
        var computerHandler = computerCardObject.GetComponent<ObjectPlacementHandler>();
        if (computerHandler != null) computerHandler.played = true;

        string message = round.Describe();
        if (match.IsOver || match.InTieBreak) message += "\n" + match.ResultText;
        ShowMessage(message, match.IsOver ? restartDelay : displayTime);
        if (round.Outcome == Outcome.Win && winSound != null) winSound.Play();
        if (round.Outcome == Outcome.Lose && loseSound != null) loseSound.Play();
        UpdateScore();

        yield return new WaitForSeconds(roundDisplayTime);
        Destroy(playerCardObject);
        Destroy(computerCardObject);
        roundInProgress = false;

        if (match.IsOver)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, restartDelay - roundDisplayTime));
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // deal a fresh match
        }
    }

    private GameObject SpawnCard(Transform slot, Card card)
    {
        GameObject cardObject = Instantiate(cardPrefab, slot);
        cardObject.transform.localPosition = Vector3.zero;
        var handler = cardObject.GetComponent<ObjectPlacementHandler>();
        handler.cardType = (int)card.Element;
        handler.cardValue = card.Value;
        Material face = FaceFor(card);
        if (face != null) cardObject.GetComponent<Renderer>().material = face;
        return cardObject;
    }

    private Material FaceFor(Card card)
    {
        Material[] faces = card.Element == Element.Fire ? materialsFire
            : card.Element == Element.Ice ? materialsIce : materialsWater;
        if (faces == null || card.Value >= faces.Length)
        {
            Debug.LogWarning($"No card face for {card}.", this);
            return null;
        }
        return faces[card.Value];
    }

    private void UpdateScore()
    {
        if (scoreText != null) scoreText.text = match.ScoreText;
    }

    public void ShowMessage(string message) => ShowMessage(message, displayTime);

    public void ShowMessage(string message, float seconds)
    {
        if (messageText == null) return;
        messageText.text = message;
        winCanvas.enabled = true;
        // A new message replaces the old one, and the old timer must not hide it early.
        if (hideMessage != null) StopCoroutine(hideMessage);
        hideMessage = StartCoroutine(HideMessageAfter(seconds));
    }

    private IEnumerator HideMessageAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        winCanvas.enabled = false;
        hideMessage = null;
    }
}
