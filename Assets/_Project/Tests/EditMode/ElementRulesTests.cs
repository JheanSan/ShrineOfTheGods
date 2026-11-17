using NUnit.Framework;
using Shrine.Data;
using Shrine.Gameplay;

namespace Shrine.Tests
{
    /// <summary>
    /// Pins the elemental cycle. These four arrows decide every trade in the game, so a
    /// silent change to <see cref="ElementRules"/> would corrupt combat, the AI and the
    /// combat forecast at once. See docs/AI_GUIDE.md and docs/STYLE_BIBLE.md.
    /// </summary>
    public sealed class ElementRulesTests
    {
        private static readonly ElementKind[] Elements =
        {
            ElementKind.Fire, ElementKind.Air, ElementKind.Earth, ElementKind.Water
        };

        [Test]
        public void CycleIsClosedAndIrreflexive()
        {
            foreach (ElementKind element in Elements)
            {
                Assert.AreNotEqual(element, ElementRules.Beats(element),
                    $"{element} must not beat itself.");
                Assert.AreEqual(element, ElementRules.Beats(ElementRules.Beats(
                        ElementRules.Beats(ElementRules.Beats(element)))),
                    "Four steps through the cycle must return to the start.");
            }
        }

        [Test]
        public void EveryElementBeatsExactlyOneAndIsWeakToExactlyOne()
        {
            foreach (ElementKind element in Elements)
            {
                int beaten = 0;
                int weak = 0;
                foreach (ElementKind other in Elements)
                {
                    if (ElementRules.HasAdvantage(element, other)) beaten++;
                    if (ElementRules.IsWeakTo(element, other)) weak++;
                }
                Assert.AreEqual(1, beaten, $"{element} must beat exactly one element.");
                Assert.AreEqual(1, weak, $"{element} must be weak to exactly one element.");
            }
        }

        [Test]
        public void MultiplierIsDoubleOnlyAgainstTheBeatenElement()
        {
            foreach (ElementKind attacker in Elements)
            foreach (ElementKind defender in Elements)
            {
                int expected = ElementRules.HasAdvantage(attacker, defender)
                    ? ElementRules.AdvantageMultiplier
                    : 1;
                Assert.AreEqual(expected, ElementRules.Multiplier(attacker, defender),
                    $"{attacker} vs {defender}");
            }
            Assert.AreEqual(2, ElementRules.AdvantageMultiplier);
        }

        [Test]
        public void AdvantageLabelMatchesTheRule()
        {
            foreach (ElementKind element in Elements)
            {
                string label = ElementRules.AdvantageLabel(element);
                StringAssert.Contains(element.ToString().ToUpperInvariant(), label);
                StringAssert.Contains(ElementRules.Beats(element).ToString().ToUpperInvariant(), label);
            }
        }

        [Test]
        public void MultipliersFollowTheDocumentedOrder()
        {
            Assert.AreEqual(2, ElementRules.Multiplier(ElementKind.Fire, ElementKind.Air));
            Assert.AreEqual(2, ElementRules.Multiplier(ElementKind.Air, ElementKind.Earth));
            Assert.AreEqual(2, ElementRules.Multiplier(ElementKind.Earth, ElementKind.Water));
            Assert.AreEqual(2, ElementRules.Multiplier(ElementKind.Water, ElementKind.Fire));

            // Advantage is one-way: the defender never doubles just because it is being hit.
            Assert.AreEqual(1, ElementRules.Multiplier(ElementKind.Air, ElementKind.Fire));
            Assert.AreEqual(1, ElementRules.Multiplier(ElementKind.Earth, ElementKind.Air));
        }
    }
}
