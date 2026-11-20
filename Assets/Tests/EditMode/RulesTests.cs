using System;
using System.Linq;
using MagicMayhem.Rules;
using NUnit.Framework;

namespace MagicMayhem.Tests
{
    public class CardRulesTests
    {
        [TestCase(Element.Fire, Element.Ice)]
        [TestCase(Element.Ice, Element.Water)]
        [TestCase(Element.Water, Element.Fire)]
        public void EachElementBeatsOneAndLosesToTheOther(Element winner, Element loser)
        {
            // Element beats element whatever the values: a 0 still beats a 9.
            Assert.AreEqual(Outcome.Win, CardRules.Compare(new Card(winner, 0), new Card(loser, 9)));
            Assert.AreEqual(Outcome.Lose, CardRules.Compare(new Card(loser, 9), new Card(winner, 0)));
        }

        [Test]
        public void TheSameElementComparesByValue()
        {
            Assert.AreEqual(Outcome.Win, CardRules.Compare(new Card(Element.Ice, 7), new Card(Element.Ice, 3)));
            Assert.AreEqual(Outcome.Lose, CardRules.Compare(new Card(Element.Ice, 2), new Card(Element.Ice, 3)));
            Assert.AreEqual(Outcome.Draw, CardRules.Compare(new Card(Element.Ice, 3), new Card(Element.Ice, 3)));
        }

        [Test]
        public void EveryPairingHasExactlyOneOutcomeAndTheyMirror()
        {
            foreach (Element a in Enum.GetValues(typeof(Element)))
            foreach (Element b in Enum.GetValues(typeof(Element)))
            for (int x = 0; x <= 9; x++)
            for (int y = 0; y <= 9; y++)
            {
                Outcome ab = CardRules.Compare(new Card(a, x), new Card(b, y));
                Outcome ba = CardRules.Compare(new Card(b, y), new Card(a, x));
                Outcome mirrored = ab == Outcome.Win ? Outcome.Lose : ab == Outcome.Lose ? Outcome.Win : Outcome.Draw;
                Assert.AreEqual(mirrored, ba, $"{a} {x} vs {b} {y}");
            }
        }

        [Test]
        public void DescriptionsSayWhyAndWhoWon()
        {
            Assert.AreEqual("Water beats Fire. You win the round!",
                CardRules.Describe(new Card(Element.Water, 1), new Card(Element.Fire, 8)));
            Assert.AreEqual("Fire beats Ice. The computer wins the round.",
                CardRules.Describe(new Card(Element.Ice, 9), new Card(Element.Fire, 0)));
            Assert.AreEqual("Both played Ice 4. This round is a draw.",
                CardRules.Describe(new Card(Element.Ice, 4), new Card(Element.Ice, 4)));
        }

        [Test]
        public void CardsOutsideTheRangeAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Card(Element.Fire, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Card(Element.Fire, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Card((Element)3, 1));
        }
    }

    public class DeckTests
    {
        [Test]
        public void TheDeckHoldsThirtyDifferentCardsAndNeverDealsOneTwice()
        {
            var deck = new Deck(new Random(1));
            Assert.AreEqual(30, deck.Count);
            var all = deck.Deal(30);
            Assert.AreEqual(30, all.Distinct().Count());
            Assert.Throws<InvalidOperationException>(() => deck.Draw());
        }

        [Test]
        public void TheSameSeedDealsTheSameCards()
        {
            CollectionAssert.AreEqual(new Deck(new Random(7)).Deal(5), new Deck(new Random(7)).Deal(5));
        }
    }

    public class MatchTests
    {
        [Test]
        public void BothSidesGetTheirOwnHandFromOneDeck()
        {
            var match = new Match(new Random(3));
            Assert.AreEqual(5, match.PlayerHand.Count);
            Assert.AreEqual(5, match.ComputerCardsLeft);
        }

        [Test]
        public void APlayedCardLeavesTheHandAndCannotBePlayedAgain()
        {
            var match = new Match(new Random(3));
            Card card = match.PlayerHand[0];
            match.Play(card);
            Assert.AreEqual(4, match.PlayerHand.Count);
            Assert.AreEqual(4, match.ComputerCardsLeft);
            Assert.Throws<InvalidOperationException>(() => match.Play(card));
        }

        [Test]
        public void ScoresFollowTheRoundOutcomes()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var match = new Match(new Random(seed));
                int wins = 0, losses = 0;
                while (!match.IsOver)
                {
                    RoundResult r = match.Play(match.PlayerHand[0]);
                    Assert.AreEqual(CardRules.Compare(r.PlayerCard, r.ComputerCard), r.Outcome);
                    if (r.Outcome == Outcome.Win) wins++;
                    if (r.Outcome == Outcome.Lose) losses++;
                }
                Assert.AreEqual(wins, match.PlayerScore);
                Assert.AreEqual(losses, match.ComputerScore);
            }
        }

        [Test]
        public void AMatchEndsAfterThreeRoundsUnlessLevelThenPlaysOn()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                var match = new Match(new Random(seed));
                while (!match.IsOver)
                {
                    if (match.RoundsPlayed >= 3)
                        Assert.IsTrue(match.InTieBreak, "only a level score goes past three rounds");
                    match.Play(match.PlayerHand[0]);
                }
                Assert.That(match.RoundsPlayed, Is.InRange(3, 5));
                if (match.Result == MatchResult.Draw)
                    Assert.AreEqual(0, match.PlayerHand.Count, "a draw only when the cards run out level");
                else if (match.RoundsPlayed > 3)
                    Assert.AreNotEqual(match.PlayerScore, match.ComputerScore);
            }
        }

        [Test]
        public void TheResultMatchesTheScore()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                var match = new Match(new Random(seed));
                while (!match.IsOver) match.Play(match.PlayerHand[0]);
                MatchResult expected = match.PlayerScore > match.ComputerScore ? MatchResult.PlayerWon
                    : match.ComputerScore > match.PlayerScore ? MatchResult.ComputerWon : MatchResult.Draw;
                Assert.AreEqual(expected, match.Result);
                StringAssert.DoesNotContain("ERROR", match.ResultText);
                Assert.IsNotEmpty(match.ResultText);
                Assert.Throws<InvalidOperationException>(() => match.Play(match.PlayerHand.Count > 0 ? match.PlayerHand[0] : new Card(Element.Fire, 0)));
            }
        }

        [Test]
        public void InvalidSetupsAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Match(new Random(), handSize: 2, roundsToPlay: 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Match(new Random(), handSize: 16));
            Assert.Throws<ArgumentNullException>(() => new Match(null));
        }
    }
}
