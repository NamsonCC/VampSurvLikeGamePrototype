using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(CounselCharacters))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CounselCharacters item = (CounselCharacters)target;

        // General Information
        GUILayout.Label("General Information", EditorStyles.boldLabel);
        item.Name = EditorGUILayout.TextField("Name", item.Name);
        item.Description = EditorGUILayout.TextField("Description", item.Description);
        item.Rarity = (GlobalData.RarityDegree)EditorGUILayout.EnumPopup("Rarity", item.Rarity);
        item.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", item.Icon, typeof(Sprite), false);

        GUILayout.Space(10);
        GUILayout.Label("Economy", EditorStyles.boldLabel);
        item.RenownPrice = EditorGUILayout.IntField("Renown Price", item.RenownPrice);

        // Skill & Training Group
        item.ShowSkillGroup = EditorGUILayout.Foldout(item.ShowSkillGroup, "Skill & Training");
        if (item.ShowSkillGroup)
        {
            EditorGUI.indentLevel++;
            item.MaxSkillLevel = EditorGUILayout.IntField("Max Skill Level", item.MaxSkillLevel);
            item.TrainingBonusPercantage = EditorGUILayout.IntField("Training Bonus (%)", item.TrainingBonusPercantage);
            item.PossibleLootPercantage = EditorGUILayout.IntField("Possible Loot Chance (%)", item.PossibleLootPercantage);
            item.PossibleLootQuality = EditorGUILayout.IntField("Possible Loot Quality (%)", item.PossibleLootQuality);
            EditorGUI.indentLevel--;
        }

        // Enemy Weakness Group
        item.ShowWeaknessGroup = EditorGUILayout.Foldout(item.ShowWeaknessGroup, "Enemy Weakness");
        if (item.ShowWeaknessGroup)
        {
            EditorGUI.indentLevel++;
            item.ReduceEnemyResistance = EditorGUILayout.IntField("Reduce Enemy Resistance (%)", item.ReduceEnemyResistance);
            item.ReduceEnemyMagicResistence = EditorGUILayout.IntField("Reduce Enemy Magic Resistance (%)", item.ReduceEnemyMagicResistence);
            item.ReduceEnemyPhysicalResistence = EditorGUILayout.IntField("Reduce Enemy Physical Resistance (%)", item.ReduceEnemyPhysicalResistence);
            item.CriticalWeakness = EditorGUILayout.IntField("Critical_Index Weakness (%)", item.CriticalWeakness);
            item.FireWeakness = EditorGUILayout.IntField("Fire Weakness (%)", item.FireWeakness);
            item.FrostWeakness = EditorGUILayout.IntField("Frost Weakness (%)", item.FrostWeakness);
            item.ShockWeakness = EditorGUILayout.IntField("Shock_Index Weakness (%)", item.ShockWeakness);
            EditorGUI.indentLevel--;
        }

        // Effect Strength Group
        item.ShowEffectGroup = EditorGUILayout.Foldout(item.ShowEffectGroup, "Effect Strength");
        if (item.ShowEffectGroup)
        {
            EditorGUI.indentLevel++;
            item.IncreaseDamageEffectStrenght = EditorGUILayout.IntField("Increase Damage Effect (%)", item.IncreaseDamageEffectStrenght);
            item.IncreaseBleedEffectStrenght = EditorGUILayout.IntField("Increase Bleed_Index Effect (%)", item.IncreaseBleedEffectStrenght);
            item.IncreasePoisonEffectStrenght = EditorGUILayout.IntField("Increase Poison_Index Effect (%)", item.IncreasePoisonEffectStrenght);
            item.IncreaseFrostEffectStrenght = EditorGUILayout.IntField("Increase Frost Effect (%)", item.IncreaseFrostEffectStrenght);
            item.IncreaseFireEffectStrenght = EditorGUILayout.IntField("Increase Fire Effect (%)", item.IncreaseFireEffectStrenght);
            item.IncreaseShockEffectStrenght = EditorGUILayout.IntField("Increase Shock_Index Effect (%)", item.IncreaseShockEffectStrenght);
            EditorGUI.indentLevel--;
        }

        // Enemy Modifiers Group
        item.ShowModifierGroup = EditorGUILayout.Foldout(item.ShowModifierGroup, "Enemy Modifiers");
        if (item.ShowModifierGroup)
        {
            EditorGUI.indentLevel++;
            item.ReduceEnemyHp = EditorGUILayout.IntField("Reduce Enemy HP (%)", item.ReduceEnemyHp);
            item.ReduceEnemySpeed = EditorGUILayout.IntField("Reduce Enemy Speed (%)", item.ReduceEnemySpeed);
            EditorGUI.indentLevel--;
        }

        // Other Modifiers
        item.ShowOtherGroup = EditorGUILayout.Foldout(item.ShowOtherGroup, "Other Modifiers");
        if (item.ShowOtherGroup)
        {
            EditorGUI.indentLevel++;
            item.ReduceEnemyRegeneration = EditorGUILayout.IntField("Reduce Enemy Regeneration (%)", item.ReduceEnemyRegeneration);
            item.IncreaseEnemyCooldown = EditorGUILayout.IntField("Increase Enemy Cooldown (%)", item.IncreaseEnemyCooldown);
            item.ReviveAgain = EditorGUILayout.IntField("Revive Again", item.ReviveAgain);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(10);
        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);
        }
    }
}

