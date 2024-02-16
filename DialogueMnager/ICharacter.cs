public interface ICharacter
{
    void ChangeAttribute(string attribute, float changeValue);
    bool CheckAttribute(string attribute, float value, string comparisonOperator);
}