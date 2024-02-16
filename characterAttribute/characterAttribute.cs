using UnityEngine;

public class characterAttribute : MonoBehaviour, IAttributeChanger
{
    // 定義屬性值
    public float happinessValue;
    public float hungerValue;
    public float excretionValue;
    public float fatigueValue;
    public float fearValue;
    public float distance;
    public float Snickers;
    public float EnergyDrink;
    public float SupermarketHandRoll;
    public float PrayerValue;
    void Start()
    {
        // 初始化屬性值
        happinessValue = 0f;
        hungerValue = 0f;
        excretionValue = 0f;
        fatigueValue = 0f;
        fearValue = 0f;
        distance = 0f;
        EnergyDrink = 0f;
        Snickers = 0f;
        SupermarketHandRoll = 0f;
        PrayerValue = 3f;
    }

    public void ChangeAttribute(string attribute, float changeValue)
    {
        // 根據屬性名稱更改屬性值
        switch (attribute)
        {
            case "happinessValue":
                happinessValue += changeValue;
                break;
            case "hungerValue":
                hungerValue += changeValue;
                break;
            case "excretion":
                excretionValue += changeValue;
                break;
            case "fatigueValue":
                fatigueValue += changeValue;
                break;
            case "fearValue":
                fearValue += changeValue;
                break;
            case "distance":
                distance += changeValue;
                break;
            case "Snickers":
                Snickers += changeValue;
                break;
            case "EnergyDrink":
                EnergyDrink += changeValue;
                break;
            case "SupermarketHandRoll":
                SupermarketHandRoll += changeValue;
                break;
            case "PrayerValue":
                PrayerValue += changeValue;
                break;
            default:
                Debug.LogError("Unrecognized attribute: " + attribute);
                break;
        }

        // 確保屬性值不會低於零
        ClampAttributes();
    }

    private void ClampAttributes()
    {
        // 限制屬性值範圍
        happinessValue = Mathf.Clamp(happinessValue, 0, 100);
        hungerValue = Mathf.Clamp(hungerValue, 0, 100);
        excretionValue = Mathf.Clamp(excretionValue, 0, 100);
        fatigueValue = Mathf.Clamp(fatigueValue, 0, 100);
        fearValue = Mathf.Clamp(fearValue, 0, 100);
        distance = Mathf.Clamp(distance, 0, 100);
    }
    public bool CheckAttribute(string attribute, float conditionValue, string comparisonOperator)
    {
        // 獲取屬性值
        float attributeValue = GetAttributeValue(attribute);
        switch (comparisonOperator)
        {
            case ">":
                return attributeValue > conditionValue;
            case "<":
                return attributeValue < conditionValue;
            case "=":
                return Mathf.Approximately(attributeValue, conditionValue);
            default:
                Debug.LogError("Unrecognized comparison operator: " + comparisonOperator);
                return false;
        }
    }
    private float GetAttributeValue(string attribute)
    {
        switch (attribute)
        {
            case "happinessValue":
                return happinessValue;
            case "hungerValue":
                return hungerValue;
            case "excretionValue":
                return excretionValue;
            case "fatigueValue":
                return fatigueValue;
            case "fearValue":
                return fearValue;
            case "distance":
                return distance;
            case "Snickers":
                return Snickers;
            case "EnergyDrink":
                return EnergyDrink;
            case "SupermarketHandRoll":
                return SupermarketHandRoll;
            case "PrayerValue":
                return PrayerValue;
            default:
                Debug.LogError("Unrecognized attribute: " + attribute);
                return 0f;
        }
    }

    // 如有需要，可以添加其他方法來讀取或修改這些屬性
}