using System;
using System.Collections.Generic;

namespace MagicMayhem.Rules
{
    public enum MatchResult
    {
        InProgress,
        PlayerWon,
        ComputerWon,
        Draw,
    }

    public readonly struct RoundResult
    {
        public Card PlayerCard { get; }
        public Card ComputerCard { get; }
        public Outcome Outcome { get; }

        public RoundResult(Card player, Card computer, Outcome outcome)
        {
            PlayerCard = player;
            ComputerCard = computer;
            Outcome = outcome;
        }

        public string Describe() => CardRules.Describe(PlayerCard, ComputerCard);
    }

    /// <summary>A match against the computer. Both sides are dealt a hand from the same deck.
    /// Each round the player plays a card and the computer answers with a random card from its
    /// own hand, so it never sees more than the player does. The match is over after the set
    /// number of rounds, unless the score is level: then extra rounds are played while both
    /// players still have cards, and it is a draw if they run out level.</summary>
    public class Match
    {
        private readonly Random random;
        private readonly List<Card> playerHand;
        private readonly List<Card> computerHand;

        public int RoundsToPlay { get; }
        public int RoundsPlayed { get; private set; }
        public int PlayerScore { get; private set; }
        public int ComputerScore { get; private set; }

        public IReadOnlyList<Card> PlayerHand => playerHand;
        public int ComputerCardsLeft => computerHand.Count;

        public Match(Random random, int handSize = 5, int roundsToPlay = 3)
        {
            if (handSize < 1 || handSize > 15)
                throw new ArgumentOutOfRangeException(nameof(handSize), "Each hand has 1 to 15 cards.");
            if (roundsToPlay < 1 || roundsToPlay > handSize)
                throw new ArgumentOutOfRangeException(nameof(roundsToPlay), "There must be enough cards for every round.");
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            RoundsToPlay = roundsToPlay;
            var deck = new Deck(random);
            playerHand = deck.Deal(handSize);
            computerHand = deck.Deal(handSize);
        }

        public MatchResult Result
        {
            get
            {
                bool roundsDone = RoundsPlayed >= RoundsToPlay;
                bool outOfCards = playerHand.Count == 0 || computerHand.Count == 0;
                if (!roundsDone && !outOfCards) return MatchResult.InProgress;
                if (PlayerScore > ComputerScore) return MatchResult.PlayerWon;
                if (ComputerScore > PlayerScore) return MatchResult.ComputerWon;
                return outOfCards ? MatchResult.Draw : MatchResult.InProgress; // level: play on
            }
        }

        public bool IsOver => Result != MatchResult.InProgress;

        /// <summary>True when the set rounds are done and the score is level, so the next
        /// round decides the match.</summary>
        public bool InTieBreak => RoundsPlayed >= RoundsToPlay && !IsOver;

        public RoundResult Play(Card playerCard)
        {
            if (IsOver)
                throw new InvalidOperationException("The match is over.");
            if (!playerHand.Remove(playerCard))
                throw new InvalidOperationException($"{playerCard} is not in the player's hand.");

            int i = random.Next(computerHand.Count);
            Card computerCard = computerHand[i];
            computerHand.RemoveAt(i);

            Outcome outcome = CardRules.Compare(playerCard, computerCard);
            if (outcome == Outcome.Win) PlayerScore++;
            else if (outcome == Outcome.Lose) ComputerScore++;
            RoundsPlayed++;
            return new RoundResult(playerCard, computerCard, outcome);
        }

        public string ScoreText => $"Your score: {PlayerScore}\nComputer score: {ComputerScore}";

        public string ResultText
        {
            get
            {
                switch (Result)
                {
                    case MatchResult.PlayerWon: return $"You won the match {PlayerScore} to {ComputerScore}!";
                    case MatchResult.ComputerWon: return $"The computer won the match {ComputerScore} to {PlayerScore}.";
                    case MatchResult.Draw: return $"The match is a draw, {PlayerScore} all.";
                    default: return InTieBreak ? "The score is level: the next round decides it." : "";
                }
            }
        }
    }
}
