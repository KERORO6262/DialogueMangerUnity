using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCAttribute : MonoBehaviour, IAttributeChanger
{
    public float happinessChange;

    public float happinessValue;
    public float hungerValue;
    public float excretionValue;
    public float fatigueValue;
    public float fearValue;
    public float distance = 0.0f;
    private System.Random rand = new System.Random();

    private Dictionary<string, Dictionary<int, int>> characterExcretionBonuses = new Dictionary<string, Dictionary<int, int>>//排泄值增長隨機數
    {
        { "彥佑", new Dictionary<int, int> { {1, 3}, {2, 5}, {3, 7} } },
        { "阿威", new Dictionary<int, int> { {1, 2}, {2, 4}, {3, 7} } },
        { "阿昌", new Dictionary<int, int> { {1, 3}, {2, 4}, {3, 5} } },
        { "小凱", new Dictionary<int, int> { {1, 2}, {2, 4}, {3, 6} } },
        { "小明", new Dictionary<int, int> { {1, 2}, {2, 3}, {3, 4} } },
    };
    private Dictionary<string, Dictionary<int, int>> characterHungerBonuses = new Dictionary<string, Dictionary<int, int>> //飢餓值增長隨機數
    {
        { "彥佑", new Dictionary<int, int> { {1, 3}, {2, 5}, {3, 6} } },
        { "阿威", new Dictionary<int, int> { {1, 3}, {2, 4}, {3, 5} } },
        { "阿昌", new Dictionary<int, int> { {1, 3}, { 2, 4 }, { 3, 6 } } },
        { "小凱", new Dictionary<int, int> { {1, 2}, {2, 5}, {3, 6} } },
        { "小明", new Dictionary<int, int> { {1, 2}, {2, 4}, {3, 5} } },
    };

    private Dictionary<string, Dictionary<int, int>> characterFatigueBonuses = new Dictionary<string, Dictionary<int, int>>//疲勞值增長隨機數
    {
        { "彥佑", new Dictionary<int, int> { {1, 3}, {2, 6}, {3, 4} } },
        { "阿威", new Dictionary<int, int> { {1, 1}, {2, 2}, {3, 3} } },
        { "阿昌", new Dictionary<int, int> { {1, 2}, {2, 3}, {3, 4} } },
        { "小凱", new Dictionary<int, int> { {1, 1}, {2, 2}, {3, 3} } },
        { "小明", new Dictionary<int, int> { {1, 3}, {2, 5}, {3, 6} } },
    };
    private Dictionary<string, Dictionary<int, (int min, int max)>> characterFearBonuses = new Dictionary<string, Dictionary<int, (int min, int max)>>//恐懼值增長隨機數
    {
        { "彥佑", new Dictionary<int, (int, int)> { { 1, (2, 2) }, { 2, (3, 3) }, { 3, (4, 4) } } },
        { "阿威", new Dictionary<int, (int, int)> { { 1, (2, 3) }, { 2, (3, 4) }, { 3, (4, 5) } } },
        { "阿昌", new Dictionary<int, (int, int)> { { 1, (2, 3) }, { 2, (3, 4) }, { 3, (4, 5) } } },
        { "小凱", new Dictionary<int, (int, int)> { { 1, (2, 3) }, { 2, (3, 4) }, { 3, (4, 5) } } },
        { "小明", new Dictionary<int, (int, int)> { { 1, (2, 3) }, { 2, (3, 4) }, { 3, (4, 5) } } },
    };

    private Dictionary<string, Dictionary<int, int>> characterDistanceBonuses = new Dictionary<string, Dictionary<int, int>>
    {
        { "彥佑", new Dictionary<int, int> { {1, 1}, {2, 2}, {3, 3} } },
        { "阿威", new Dictionary<int, int> { {1, 2}, {2, 4}, {3, 4} } },
        { "阿昌", new Dictionary<int, int> { {1, 2}, {2, 4}, {3, 5} } },
        { "小凱", new Dictionary<int, int> { {1, 2}, {2, 3}, {3, 4} } },
        { "小明", new Dictionary<int, int> { {1, 2}, {2, 3}, {3, 4} } },
    };
    void Start()
    {
        // Initialize NPC attributes here, if necessary
    }
    public void UpdateExcretionValue(TimeSpan eventDuration, string characterName, TimeSpan currentTime)
    {
        Debug.Log($"NPC {characterName} updated excretion value: {excretionValue}");

        float durationInMinutes = (float)eventDuration.TotalMinutes;

        // Determine current time range
        bool isMorning = currentTime.Hours < 17;

        // Define intervals based on current time range
        int interval = isMorning ? (durationInMinutes <= 7 ? 1 :
                                    durationInMinutes <= 10 ? 2 :
                                    durationInMinutes <= 14 ? 3 : 0) :
                                   (durationInMinutes <= 4 ? 1 :
                                    durationInMinutes <= 6 ? 2 :
                                    durationInMinutes <= 9 ? 3 : 0);

        // Get the character-specific bonus
        int bonus = 0;
        if (characterExcretionBonuses.ContainsKey(characterName) && interval > 0)
        {
            bonus = characterExcretionBonuses[characterName][interval];
        }

        // Ensure there is always an increment
        if (bonus == 0)
        {
            bonus = 1; // Minimum increment if no specific bonus is found
        }

        // Update the excretion value with the bonus
        excretionValue += bonus;

        // Ensuring excretionValue does not go negative
        excretionValue = Mathf.Max(0, excretionValue);

        // Update UI to reflect the changes
    }

    public void UpdateHungerValue(TimeSpan eventDuration, string characterName, TimeSpan currentTime)
    {
        Debug.Log($"NPC {characterName} updated hunger value: {hungerValue}");

        float durationInMinutes = (float)eventDuration.TotalMinutes;
        bool isMorning = currentTime.Hours < 17;
        int interval = isMorning ? (durationInMinutes <= 7 ? 1 :
                                    durationInMinutes <= 10 ? 2 :
                                    durationInMinutes <= 14 ? 3 : 0) :
                                   (durationInMinutes <= 4 ? 1 :
                                    durationInMinutes <= 6 ? 2 :
                                    durationInMinutes <= 9 ? 3 : 0);

        int bonus = 0;
        if (characterHungerBonuses.ContainsKey(characterName))
        {
            if (interval > 0 && characterHungerBonuses[characterName].ContainsKey(interval))
            {
                bonus = characterHungerBonuses[characterName][interval];
            }
        }
        else
        {
            Debug.LogWarning($"Character name '{characterName}' not found in hunger bonuses dictionary.");
        }

        hungerValue += bonus;
        hungerValue = Mathf.Max(0, hungerValue);
    }

    private int DetermineInterval(bool isMorning, float durationInMinutes)
    {
        return isMorning ? (durationInMinutes <= 7 ? 1 :
                                    durationInMinutes <= 10 ? 2 :
                                    durationInMinutes <= 14 ? 3 : 0) :
                                   (durationInMinutes <= 4 ? 1 :
                                    durationInMinutes <= 6 ? 2 :
                                    durationInMinutes <= 9 ? 3 : 0);
    }
    public void UpdateFatigueValue(float eventDuration, float distance, string characterName, TimeSpan currentTime)
    {
        Debug.Log($"NPC {characterName} updated fatigue value: {fatigueValue}");

        float durationInMinutes = eventDuration; // Assuming timeSpent is in minutes
        bool isMorning = currentTime.Hours < 17;
        int interval = DetermineInterval(isMorning, durationInMinutes);

        int bonus = 0;
        if (characterFatigueBonuses.ContainsKey(characterName) && interval > 0)
        {
            bonus = characterFatigueBonuses[characterName][interval];
        }

        fatigueValue += bonus;
        fatigueValue = Mathf.Max(0, fatigueValue);
    }
    public void UpdateFearValue(TimeSpan currentTime, string characterName)
    {
        Debug.Log($"NPC {characterName} updated fear value: {fearValue}");

        if (currentTime.Hours >= 17)
        {
            int interval = currentTime.Minutes <= 1 ? 1 :
                           currentTime.Minutes <= 3 ? 2 :
                           currentTime.Minutes <= 7 ? 3 : 0;

            int bonus = 0;

            if (interval > 0 && characterFearBonuses.ContainsKey(characterName) && characterFearBonuses[characterName].ContainsKey(interval))
            {
                var bonusRange = characterFearBonuses[characterName][interval];
                bonus = rand.Next(bonusRange.min, bonusRange.max + 1); // Random bonus in the specified range
            }

            fearValue += bonus;
            fearValue = Mathf.Max(0, fearValue);
        }

    }

    public float GetDistance()
    {
        return distance;
    }
    public void CalculateDistance(TimeSpan currentTime, string characterName)
    {
        Debug.Log($"NPC {characterName} calculated distance: {distance}");

        // Determine current time range
        bool isMorning = currentTime.Hours < 17;

        // Define intervals based on current time range
        int interval = isMorning ? (currentTime.Minutes <= 7 ? 1 :
                                    currentTime.Minutes <= 10 ? 2 :
                                    currentTime.Minutes <= 14 ? 3 : 0) :
                                   (currentTime.Minutes <= 4 ? 1 :
                                    currentTime.Minutes <= 6 ? 2 :
                                    currentTime.Minutes <= 9 ? 3 : 0);

        // Get the character-specific distance bonus or penalty
        int distanceChange = 0;
        if (characterDistanceBonuses.ContainsKey(characterName) && interval > 0)
        {
            distanceChange = characterDistanceBonuses[characterName][interval];
        }

        // Update the distance value with the change
        distance += distanceChange;

        // Ensuring distance does not go negative
        distance = Mathf.Max(0, distance);

        // Update UI or other logic as needed
        Debug.Log($"Distance calculated: {distance}");
    }
    public void ChangeAttribute(string attribute, float changeValue)
    {
        switch (attribute)
        {
            case "happinessValue": happinessValue += changeValue; break;
            case "hungerValue": hungerValue += changeValue; break;
            case "excretionValue": excretionValue += changeValue; break;
            case "fatigueValue": fatigueValue += changeValue; break;
            case "fearValue": fearValue += changeValue; break;
            case "distance": distance += changeValue; break;

        }

        // Optionally, update the NPC's UI or state here
    }



    // You can add methods here to modify the attributes, similar to CharacterAttribute
}