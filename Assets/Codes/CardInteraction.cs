using UnityEngine;

public class CardInteraction : MonoBehaviour
{
    public void InteractCards(CardDisplay attackerCard, CardDisplay defenderCard)
    {
        Card attacker = attackerCard.card;
        Card defender = defenderCard.card;

        // Calculate base damage
        int damageToDefender = attacker.Attack;
        int damageToAttacker = defender.Attack;

        // Element interactions
        if (attacker.Element == "Fire" && defender.Element == "Air")
        {
            damageToDefender *= 2; // Fire does double damage to Air
            Debug.Log($"Fire card {attacker.Name} deals double damage to Air card!");
        }
        
        if (attacker.Element == "Water" && !attacker.HasUsedElementalPower)
        {
            // Water heals itself on first attack
            attacker.Heal(2);
            attacker.HasUsedElementalPower = true;
            Debug.Log($"Water card {attacker.Name} healed itself!");
        }

        // Apply damage
        defender.TakeDamage(damageToDefender);
        attacker.TakeDamage(damageToAttacker);

        // Update displays
        UpdateCardAfterCombat(attackerCard);
        UpdateCardAfterCombat(defenderCard);
        
        // Reset position
        attackerCard.ResetToStartPosition();
    }

    private void UpdateCardAfterCombat(CardDisplay cardDisplay)
    {
        if (cardDisplay.card.CurrentDefense <= 0)
        {
            cardDisplay.RemoveFromPlay();
        }
        else
        {
            cardDisplay.UpdateCardDisplay();
        }
    }
}