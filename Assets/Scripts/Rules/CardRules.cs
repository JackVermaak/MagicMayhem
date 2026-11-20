namespace MagicMayhem.Rules
{
    public enum Outcome
    {
        Win,
        Lose,
        Draw,
    }

    /// <summary>Who wins a round. Each element beats one other and loses to the third:
    /// Fire melts Ice, Ice freezes Water, Water puts out Fire. Two cards of the same element
    /// compare by value.</summary>
    public static class CardRules
    {
        public static Element Beats(Element element)
        {
            switch (element)
            {
                case Element.Fire: return Element.Ice;
                case Element.Ice: return Element.Water;
                default: return Element.Fire;
            }
        }

        /// <summary>The round from the player's side.</summary>
        public static Outcome Compare(Card player, Card opponent)
        {
            if (player.Element == opponent.Element)
            {
                if (player.Value == opponent.Value) return Outcome.Draw;
                return player.Value > opponent.Value ? Outcome.Win : Outcome.Lose;
            }
            return Beats(player.Element) == opponent.Element ? Outcome.Win : Outcome.Lose;
        }

        /// <summary>A one-line explanation for the table, e.g. "Water beats Fire. You win the round!"</summary>
        public static string Describe(Card player, Card opponent)
        {
            Outcome outcome = Compare(player, opponent);
            string why;
            if (player.Element == opponent.Element)
                why = outcome == Outcome.Draw
                    ? $"Both played {player}."
                    : $"{player} against {opponent}.";
            else if (outcome == Outcome.Win)
                why = $"{player.Element} beats {opponent.Element}.";
            else
                why = $"{opponent.Element} beats {player.Element}.";

            string result = outcome == Outcome.Win ? "You win the round!"
                : outcome == Outcome.Lose ? "The computer wins the round."
                : "This round is a draw.";
            return $"{why} {result}";
        }
    }
}
