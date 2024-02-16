using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class LanguageManager : MonoBehaviour
{
    [Header("UI Components")]
    public List<TMP_Text> textComponents; // 可以在Inspector中拖拽指定的TextMeshPro组件
    [Header("Translation Files")]
    public List<string> translationFilePaths; // 允许在Inspector中输入文件路径
    private List<ILocalizable> localizables = new List<ILocalizable>();

    public static LanguageManager Instance { get; private set; }
    private Dictionary<string, string> translations = new Dictionary<string, string>();

    public Button englishButton;
    public Button chineseButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        englishButton.onClick.AddListener(() => this.SwitchLanguage("English"));
        chineseButton.onClick.AddListener(() => this.SwitchLanguage("Chinese"));
    }
    public void SwitchLanguage(string languageCode)
    {
        // 直接调用LoadLanguage，传入新的语言代码
        LoadLanguage(languageCode);
        UpdateUIElements(); // 确保此方法更新了所有需要的UI元素
    }

    public void RegisterLocalizable(ILocalizable localizable)
    {
        if (!localizables.Contains(localizable))
        {
            localizables.Add(localizable);
            localizable.Localize(); // 立即更新文本
        }
    }
    public void UnregisterLocalizable(ILocalizable localizable)
    {
        localizables.Remove(localizable);
    }
    public void OnLanguageChange(string newLanguageCode)
    {
        LanguageManager.Instance.LoadLanguage(newLanguageCode);
        // 可能还需要刷新所有当前显示的文本以显示新的翻译
    }
    public void LoadLanguage(string languageCode)
    {
        translations.Clear();

        foreach (var filePath in translationFilePaths)
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, filePath);
            if (File.Exists(fullPath))
            {
                string[] lines = File.ReadAllLines(fullPath);

                int languageColumn = -1;
                string[] headers = lines[0].Split(',');
                for (int i = 0; i < headers.Length; i++)
                {
                    if (headers[i].Trim('"').Equals(languageCode, StringComparison.OrdinalIgnoreCase))
                    {
                        languageColumn = i;
                        break;
                    }
                }

                if (languageColumn == -1) continue;

                for (int i = 1; i < lines.Length; i++)
                {
                    // 使用正则表达式匹配逗号分隔的值，支持双引号内的逗号
                    var matches = Regex.Matches(lines[i], "(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)");
                    List<string> parts = new List<string>();
                    foreach (Match match in matches)
                    {
                        parts.Add(match.Value.TrimStart(',').Trim('"'));
                    }

                    if (parts.Count > languageColumn)
                    {
                        string key = parts[0].Trim();
                        string translation = parts[languageColumn].Trim();
                        translations[key] = translation;
                    }
                }
            }
            else
            {
                Debug.LogError("Translation file not found: " + fullPath);
            }
        }

        UpdateUIElements();
    }

    public void UpdateUIElements()
    {
        foreach (var localizable in localizables)
        {
            localizable.Localize();
        }
    }
    public string GetTranslation(string key)
    {
        if (translations.TryGetValue(key, out string translation))
        {
            return translation;
        }
        return key; // Return the key itself if no translation is found
    }
}