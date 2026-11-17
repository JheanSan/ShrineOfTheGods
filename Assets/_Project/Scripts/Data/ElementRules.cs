namespace Shrine.Data
{
    /// <summary>
    /// The elemental cycle, in one place. Every rule, tooltip and legend reads from
    /// here, so combat, AI scoring and the interface can never disagree.
    ///
    /// FIRE beats AIR, AIR beats EARTH, EARTH beats WATER, WATER beats FIRE.
    /// A card that beats its target deals <see cref="AdvantageMultiplier"/> damage.
    /// </summary>
    public static class ElementRules
    {
        public const int AdvantageMultiplier = 2;

        /// <summary>The element this one deals double damage to.</summary>
        public static ElementKind Beats(ElementKind element)
        {
            switch (element)
            {
                case ElementKind.Fire: return ElementKind.Air;
                case ElementKind.Air: return ElementKind.Earth;
                case ElementKind.Earth: return ElementKind.Water;
                default: return ElementKind.Fire;
            }
        }

        /// <summary>True when <paramref name="attacker"/> holds the elemental advantage.</summary>
        public static bool HasAdvantage(ElementKind attacker, ElementKind defender)
        {
            return Beats(attacker) == defender;
        }

        public static bool IsWeakTo(ElementKind element, ElementKind other)
        {
            return Beats(other) == element;
        }

        /// <summary>Damage multiplier applied when <paramref name="attacker"/> strikes.</summary>
        public static int Multiplier(ElementKind attacker, ElementKind defender)
        {
            return HasAdvantage(attacker, defender) ? AdvantageMultiplier : 1;
        }

        /// <summary>The element that deals double damage to this one.</summary>
        public static ElementKind WeaknessOf(ElementKind element)
        {
            switch (element)
            {
                case ElementKind.Fire: return ElementKind.Water;
                case ElementKind.Water: return ElementKind.Earth;
                case ElementKind.Earth: return ElementKind.Air;
                default: return ElementKind.Fire;
            }
        }

        /// <summary>Player-facing name in the shrine's voice.</summary>
        public static string Title(ElementKind element)
        {
            switch (element)
            {
                case ElementKind.Fire: return "Ember";
                case ElementKind.Water: return "Tide";
                case ElementKind.Earth: return "Stone";
                default: return "Gale";
            }
        }

        /// <summary>One sentence shown in the card tooltip and the battle legend.</summary>
        public static string Description(ElementKind element)
        {
            switch (element)
            {
                case ElementKind.Fire:
                    return "Sears Air for double damage.";
                case ElementKind.Water:
                    return "Drowns Fire for double damage. Gains 2 defence the first time it strikes.";
                case ElementKind.Earth:
                    return "Grinds Water for double damage. Ignores the first damage it would take.";
                default:
                    return "Scatters Earth for double damage. The first gale omened air card costs 1 less.";
            }
        }

        /// <summary>"FIRE > AIR" style label used by cards and the legend.</summary>
        public static string AdvantageLabel(ElementKind element)
        {
            return element.ToString().ToUpperInvariant() + " > " + Beats(element).ToString().ToUpperInvariant();
        }
    }
}
