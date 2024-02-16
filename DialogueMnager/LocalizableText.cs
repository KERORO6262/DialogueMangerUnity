using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizableText : MonoBehaviour, ILocalizable
{
    public string key; // 可以在Inspector中指定

    void Start()
    {
        LanguageManager.Instance.RegisterLocalizable(this);
    }

    void OnDestroy()
    {
        LanguageManager.Instance.UnregisterLocalizable(this);
    }

    public void Localize()
    {
        var textComponent = GetComponent<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = LanguageManager.Instance.GetTranslation(key);
        }
        
    }
}
