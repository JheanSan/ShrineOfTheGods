using System.Collections.Generic;
using UnityEngine;

namespace Shrine.Data
{
    /// <summary>
    /// Canonical content tables. The editor builder writes these into ScriptableObject
    /// assets; the runtime reads those assets through <see cref="ShrineDatabase"/>.
    /// </summary>
    public static class ContentFactory
    {
        public static readonly CardData[] Cards =
        {
            new CardData("dragon", "Dragon", ElementKind.Fire, 3, 2, 3,
                "A fearsome dragon that breathes fire. Its scales glisten like gold in the sunlight.", "CardArt/dragon"),
            new CardData("warrior", "Warrior", ElementKind.Earth, 4, 2, 4,
                "A strong warrior with unbreakable will, bearing a crest older than the shrine.", "CardArt/warrior"),
            new CardData("mage", "Mage", ElementKind.Water, 2, 3, 4,
                "A wise mage whose staff holds the memory of drowned temples.", "CardArt/mage"),
            new CardData("elf", "Elf", ElementKind.Air, 3, 2, 3,
                "An agile elf whose arrows never forget the shape of a promise.", string.Empty),
            new CardData("dwarf", "Dwarf", ElementKind.Earth, 5, 2, 3,
                "A sturdy dwarf with an axe carved from the roots of the first mountain.", string.Empty),
            new CardData("troll", "Troll", ElementKind.Fire, 5, 3, 4,
                "A troll that mistakes the shrine's braziers for a hearth.", string.Empty),
            new CardData("goblin", "Goblin", ElementKind.Air, 3, 3, 2,
                "A cunning goblin with quick hands and a love of gilded things.", string.Empty),
            new CardData("wizard", "Wizard", ElementKind.Water, 2, 4, 6,
                "A wizard whose beard holds the names of seven forgotten seas.", string.Empty),
            new CardData("cyclops", "Cyclops", ElementKind.Earth, 4, 3, 5,
                "A cyclops that searches the countryside for its next meal.", string.Empty),
            new CardData("mermaid", "Mermaid", ElementKind.Water, 4, 2, 4,
                "A mermaid whose song draws sailors toward the drowning dark.", string.Empty),
            new CardData("gryphon", "Gryphon", ElementKind.Air, 4, 3, 5,
                "A gryphon that circles the ruins, hunting what the gods abandoned.", string.Empty),
            new CardData("minotaur", "Minotaur", ElementKind.Earth, 5, 3, 6,
                "A minotaur whose charge splits stone and undermines faith.", string.Empty),
            new CardData("unicorn", "Unicorn", ElementKind.Air, 4, 4, 7,
                "A unicorn whose horn shines with the last unbroken vow.", string.Empty),
            new CardData("yeti", "Yeti", ElementKind.Earth, 4, 3, 6,
                "A yeti wrapped in white fur and the silence of high places.", string.Empty),
            new CardData("phoenix", "Phoenix", ElementKind.Fire, 5, 3, 2,
                "A phoenix that remembers every fire it has ever been reborn from.", string.Empty),
            new CardData("chimera", "Chimera", ElementKind.Fire, 4, 4, 8,
                "A chimera breathing fire and poison in the same breath.", string.Empty),
            new CardData("hydra", "Hydra", ElementKind.Water, 5, 4, 7,
                "A hydra that grows a new answer for every wound.", string.Empty),
            new CardData("harpy", "Harpy", ElementKind.Air, 5, 3, 8,
                "A harpy that carries the prayers of the lost to a silent heaven.", string.Empty),
            new CardData("gorgon", "Gorgon", ElementKind.Earth, 5, 4, 8,
                "A gorgon whose gaze turns belief into stone.", string.Empty)
        };

        public static readonly PatronData[] Patrons =
        {
            new PatronData(PatronKind.Flame, "Valen, the Ember Saint", "Crown of Cinders",
                "Deal 4 damage to every opponent card and 2 to the opponent.",
                "FotosMenuInicial/ValenFire", ElementKind.Fire),
            new PatronData(PatronKind.Tide, "The Drowned Oracle", "Mercy of Tides",
                "Restore 5 health and draw a card.",
                "FotosMenuInicial/ShrineGays", ElementKind.Water),
            new PatronData(PatronKind.Stone, "Ale, the Ash Warden", "Bulwark of Ash",
                "Give every friendly card +2 defence and shield 3 hero damage.",
                "FotosMenuInicial/AleGod", ElementKind.Earth),
            new PatronData(PatronKind.Gale, "Jean, the Skywright", "Gale's Rebuke",
                "Refund 3 mana and let every friendly card attack again.",
                "FotosMenuInicial/JeanGenius", ElementKind.Air)
        };

        public static readonly OmenData[] Omens =
        {
            new OmenData("emberfall", "Emberfall", "Fire cards strike with +1 attack this round.", attackBonus: 1),
            new OmenData("tidal_vigil", "Tidal Vigil", "Water cards mend 1 defence when played.", defenseBonus: 1),
            new OmenData("stone_mass", "Stone Mass", "All cards gain +1 defence on entering the field.", defenseBonus: 1),
            new OmenData("gale_whisper", "Gale Whisper", "Air cards cost 1 less this round (minimum 0).", refreshMana: true),
            new OmenData("blood_moon", "Blood Moon", "Direct attacks to a hero deal +1 damage.", directDamageBonus: 1),
            new OmenData("divine_harvest", "Divine Harvest", "Favoured cards grant double favour this round.", favourBonus: true)
        };
    }

    /// <summary>Loads and caches all design data from Resources.</summary>
    public static class ShrineDatabase
    {
        private static CardDefinition[] _cards;
        private static PatronDefinition[] _patrons;
        private static OmenDefinition[] _omens;
        private static BalanceConfig _balance;

        public static IReadOnlyList<CardDefinition> Cards =>
            _cards ??= Resources.LoadAll<CardDefinition>("Shrine/Cards");

        public static IReadOnlyList<PatronDefinition> Patrons =>
            _patrons ??= Resources.LoadAll<PatronDefinition>("Shrine/Patrons");

        public static IReadOnlyList<OmenDefinition> Omens =>
            _omens ??= Resources.LoadAll<OmenDefinition>("Shrine/Omens");

        public static BalanceConfig Balance =>
            _balance ??= Resources.Load<BalanceConfig>("Shrine/BalanceConfig");

        public static void ClearCache()
        {
            _cards = null;
            _patrons = null;
            _omens = null;
            _balance = null;
        }
    }
}
