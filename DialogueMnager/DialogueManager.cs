using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For Image
using TMPro; // For TextMeshPro
using System.IO;
using UnityEngine.EventSystems; // 引入事件系统命名空间


public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Configuration")]
    [SerializeField] private string csvFilePath; // CSV文件路径，包含对话数据。
    [SerializeField] private float textTypingSpeed = 0.05f; // 文本打印速度，模拟打字机效果。
    [SerializeField] private float selectionButtonHorizontalOffset = 10f; // 对话选项按钮之间的水平间距。
    [SerializeField] private float autoPlayPauseDuration = 2.0f; // 自动播放模式下，对话之间的停顿时间。

    [Header("UI References")]
    [SerializeField] private Image diaChrImgLRenderer; // 左侧角色的图像渲染器。
    [SerializeField] private Image diaChrImgRRenderer; // 右侧角色的图像渲染器。
    [SerializeField] private Image diaImgBackgrundRenderer; // 对话背景的图像渲染器。
    [SerializeField] private TMP_Text diaChrNameLText; // 显示左侧角色名的文本组件。
    [SerializeField] private TMP_Text diaChrNameRText; // 显示右侧角色名的文本组件。
    [SerializeField] private TMP_Text diaText; // 显示对话内容的文本组件。
    [SerializeField] private GameObject buttonPrefab; // 对话选择按钮的预制体。
    [SerializeField] private Transform buttonContainer; // 存放动态生成的对话选择按钮的容器。
    [SerializeField] private Button autoPlayButton; // 自动播放按钮。
    [SerializeField] private Slider dialogueSliderbarAutoplaySpeed; // 调整自动播放速度的滑动条。
    [SerializeField] private TMP_Text dialogueTextAutoplaySpeed; // 显示当前自动播放速度的文本组件。
    //[SerializeField] private TMP_Text history_Text; // 显示对话历史记录的文本组件。
    //[SerializeField] private BackgroundEffectManager backgroundEffectManager;

    [Header("Audio References")]
    [SerializeField] private AudioSource audioSource; // 掛載播放音訊物件
    [SerializeField] private AudioSource soundEffectSource; // New AudioSource for sound effects


    [Header("Autoplay Settings")]
    [SerializeField] private Color autoPlayActiveColor = Color.red; // 自动播放激活时的按钮颜色。
    private Color autoPlayOriginalColor; // 自动播放按钮的原始颜色，用于恢复默认状态。

    private bool isAutoPlaying = false; // 指示是否启用了自动播放模式。
    private bool allowDialogueProgression = true; // 控制对话是否可以自动进展到下一个条目。
    private List<DialogueEntry> dialogues = new List<DialogueEntry>(); // 存储从CSV文件加载的所有对话条目。
    private int currentDialogueIndex = -1; // 当前显示的对话条目索引。
    private bool isDialogueActive = true; // 指示对话系统当前是否处于活动状态。
    private bool isTextTypingInProgress = false; // 指示文本打字效果是否正在进行。
    private float lastDialogueProgressTime = 0f; // 记录上一次对话进展的时间，用于处理输入冲突。
    private float dialogueProgressDebounce = 0.2f; // 对话进展的去抖时间，防止快速重复触发。

    private IEnumerator autoPlayCoroutine; // 控制自动播放逻辑的协程。

    void Start() // 初始化对话管理器，从 CSV 文件读取对话数据并清理现有对话配置。对话的自动开始被注释以避免在场景启动时立即开始。
    {

        // 初始化时保存按钮的原始颜色，并注册其它必要的初始化逻辑
        autoPlayOriginalColor = autoPlayButton.colors.normalColor;
        // 初始化时隐藏滑动条和文本
        dialogueSliderbarAutoplaySpeed.gameObject.SetActive(false);
        dialogueTextAutoplaySpeed.gameObject.SetActive(false);
        // 添加滑动条的值改变监听器
        dialogueSliderbarAutoplaySpeed.onValueChanged.AddListener(OnAutoplaySpeedChanged);

        List<string> languagesToPreload = new List<string> { "English", "Chinese" };

        PreloadTranslations(); // 预加载翻译
        ReadCSV(csvFilePath);
        ClearDialogue();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    void PreloadTranslations() //載入多語言翻譯數據
    {
        // 假设我们有一个方法来获取所有支持的语言代码
        var supportedLanguages = new List<string> { "English", "Chinese" };
        foreach (var languageCode in supportedLanguages)
        {
            LanguageManager.Instance.LoadLanguage(languageCode);
        }

        // 现在我们预加载当前语言的翻译，确保一开始就加载
        string currentLanguageCode = "English"; // 假设英语是默认语言，您可以根据需要更改
        LanguageManager.Instance.LoadLanguage(currentLanguageCode);
    }
    public void StartDialogue() // 啟動對話系統，重置對話索引，啟動對話系統，並呼叫 ProgressDialogue() 開始展示對話。
    {
        if (dialogues.Count > 0)
        {
            currentDialogueIndex = -1;
            isDialogueActive = true; // Enable dialogue progression
            ProgressDialogue();
        }
    }
    public void StartDialogueFromEditor(int diaID) //從編輯器直接啟動對話
    {
        int defaultDiaScript = 1; // 定义默认的 diaScript 值
        StartDialogueByID(diaID, defaultDiaScript);
    }
    public void StartDialogueByID(int diaID, int diaScript) //透過特定的對話ID與Script號來啟動對話。 尋找符合的對話條目，並顯示它。
    {
        int targetIndex = dialogues.FindIndex(d => d.diaID == diaID && d.diaScript == diaScript);
        if (targetIndex != -1)
        {
            currentDialogueIndex = targetIndex;
            isDialogueActive = true; // Enable dialogue progression
            DisplayDialogue(dialogues[currentDialogueIndex]);
        }
        else
        {
            Debug.LogError($"Dialogue with diaID {diaID} and diaScript {diaScript} not found.");
        }
    }

    void Update() // 每幀調用，處理對話的自動進展和玩家輸入（滑鼠點擊或空格鍵）。 也處理UI上的懸停檢測。
    {
        if (isDialogueActive)
        {
            // 检查鼠标是否悬停在任何对话按钮上
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // 如果是，获取当前悬停的对象
                GameObject hoveredObject = EventSystem.current.currentSelectedGameObject;

                // 检查该对象是否属于对话按钮
                if (hoveredObject != null && hoveredObject.CompareTag("UI")) //如果是UI則不會觸發進入下一段對話
                {
                    // 如果是对话按钮，不允许推进对话
                    return;
                }
            }
            bool requiresSelection = currentDialogueIndex >= 0 && !string.IsNullOrEmpty(dialogues[currentDialogueIndex].diaSelection);

            // 当不在自动播放模式时，仍然允许用户通过鼠标点击或空格键来推进对话
            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && Time.time - lastDialogueProgressTime > dialogueProgressDebounce)
            {
                if (isTextTypingInProgress)
                {
                    CompleteTypewriterEffect();
                    //backgroundEffectManager.CompleteCurrentEffect(); // 立即完成背景特效

                }
                else if (allowDialogueProgression && !requiresSelection) // 当不需要选择时，才推进对话
                {
                    ProgressDialogue();
                    lastDialogueProgressTime = Time.time;
                }
            }
        }
    }
    void CompleteTypewriterEffect() // 立即完成打字机效果，显示完整的对话文本，并处理按钮的位置和显示。
    {
        StopAllCoroutines();

        diaText.text = LanguageManager.Instance.GetTranslation(dialogues[currentDialogueIndex].diaText);
        PositionAndDisplayButtons(dialogues[currentDialogueIndex].diaSelection);
        isTextTypingInProgress = false;
        // Call CompleteCurrentEffect on backgroundEffectManager to instantly complete and cleanup background effects
        //backgroundEffectManager.CompleteCurrentEffect();

        // 立即重置特效，包括Scale值
        //backgroundEffectManager.ResetScaleEffectImmediately();

    }
    IEnumerator DisplayTextTypewriterStyle(string text, string selectionData)// 以打字机风格逐字显示文本的协程。在文本完全显示后，处理选择按钮的定位和显示。
    {
        isTextTypingInProgress = true;
        //string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        //history_Text.text += $"({timestamp}) {text}\n"; // 提前添加到历史记录

        diaText.text = "";
        foreach (char c in text)
        {
            diaText.text += c;
            yield return new WaitForSeconds(textTypingSpeed);
        }

        isTextTypingInProgress = false;
        // 打字效果完成后，处理按钮的位置和显示
        PositionAndDisplayButtons(selectionData);
    }
    void PositionAndDisplayButtons(string selectionData) //根據給定的選擇數據，計算並定位對話選擇按鈕。 按鈕基於文字方塊最後一行文字的位置動態放置。
    {
    // 确保TMP已经更新其布局
    diaText.ForceMeshUpdate();

    // 获取TMP文本的textInfo，其中包含了行的信息
    var textInfo = diaText.textInfo;
    int lastLine = textInfo.lineCount - 1; // 最后一行的索引

    if (lastLine < 0) return; // 如果没有文本，则直接返回

    // 获取最后一行的起始位置
    float lastLineStart = textInfo.lineInfo[lastLine].firstCharacterIndex;
    float lastLineY = textInfo.characterInfo[(int)lastLineStart].bottomLeft.y; // 最后一行首字的Y坐标

    // 计算按钮应该放置的位置，我们需要将它放置在TMP文本下方
    Vector3 startPosition = diaText.transform.TransformPoint(new Vector3(0, lastLineY - 20f, 0)); // 转换为世界坐标

    // 从TMP文本的最后一行首字下方开始，对齐第一个按钮的左边缘
    float currentXPosition = startPosition.x;

    for (int i = 0; i < buttonContainer.childCount; i++)
    {
        GameObject button = buttonContainer.GetChild(i).gameObject;
        RectTransform buttonRectTransform = button.GetComponent<RectTransform>();

        // 设置按钮的新位置
        // 注意：这里假设按钮的父对象（buttonContainer）位于世界坐标的(0,0,0)，如果不是，需要相应调整
        Vector3 newPosition = new Vector3(currentXPosition + (buttonRectTransform.sizeDelta.x / 2), startPosition.y, startPosition.z);
        buttonRectTransform.position = newPosition;

        // 更新下一个按钮的起始X坐标
        currentXPosition += buttonRectTransform.sizeDelta.x + selectionButtonHorizontalOffset;

        // 显示按钮
        button.SetActive(true);
    }
    }

    void ProgressDialogue() // 处理对话的进展。根据当前索引确定接下来显示的对话部分，或者结束对话。还处理基于 nextDiaID 的对话分支。
    {
        if (currentDialogueIndex == -1)
        {
            currentDialogueIndex = 0; // Start from the first dialogue if not started
        }
        else if (!string.IsNullOrEmpty(dialogues[currentDialogueIndex].nextDiaID))
        {
            // 清理 nextDiaID 字段，移除空格和单引号
            string cleanedNextDiaID = dialogues[currentDialogueIndex].nextDiaID.Replace(" ", "").Replace("'", "");

            if (cleanedNextDiaID == "END")
            {
                ClearDialogue();
                return;
            }

            string[] nextParts = cleanedNextDiaID.Split('-');
            int nextID = int.Parse(nextParts[0]);
            int nextScript = nextParts.Length > 1 ? int.Parse(nextParts[1]) : GetMinimumScriptNumberForDiaID(nextID);
            currentDialogueIndex = dialogues.FindIndex(d => d.diaID == nextID && d.diaScript == nextScript);
        }
        else
        {
            // Use the same diaID to continue to the next dialogue
            currentDialogueIndex = FindNextDialogueIndexWithSameDiaID();
        }

        if (currentDialogueIndex >= 0 && currentDialogueIndex < dialogues.Count)
        {
            DisplayDialogue(dialogues[currentDialogueIndex]);
        }
        else
        {
            ClearDialogue();
        }
        // Reset flag for next update
        allowDialogueProgression = buttonContainer.childCount == 0;

    }


    int GetMinimumScriptNumberForDiaID(int diaID) // 返回给定 diaID 的最小脚本编号，用于确定新对话ID的开始点。
    {
        int minScript = int.MaxValue;
        foreach (var entry in dialogues)
        {
            if (entry.diaID == diaID && entry.diaScript < minScript)
            {
                minScript = entry.diaScript;
            }
        }
        return minScript != int.MaxValue ? minScript : 0; // Return 0 if no dialogue found
    }

    int FindNextDialogueIndexWithSameDiaID() // 查找具有相同 diaID 的下一个对话条目索引，用于继续具有多个部分的对话。
    {
        int currentID = dialogues[currentDialogueIndex].diaID;
        for (int i = currentDialogueIndex + 1; i < dialogues.Count; i++)
        {
            if (dialogues[i].diaID == currentID)
            {
                return i;
            }
        }
        return -1; // Return -1 if no next dialogue found
    }

    void ReadCSV(string filePath) // 从 CSV 文件中读取对话数据，将每行转换为 DialogueEntry 对象，并添加到对话列表中。
    {
        string[] lines = File.ReadAllLines(filePath);

        // Start from index 1 to skip the header row
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            string[] fields = line.Split(',');
            if (fields.Length < 17) continue; // Skip lines that don't have enough fields

            DialogueEntry entry = new DialogueEntry();
            if (int.TryParse(fields[0], out int diaID))
            {
                entry.diaID = diaID;
                Debug.Log("DiaID: " + entry.diaID);

            }
            else
            {
                Debug.LogError("Invalid diaID format in line: " + line);
                continue; // Skip this line if diaID is not an integer
            }

            if (int.TryParse(fields[1], out int diaScript))
            {
                entry.diaScript = diaScript;
            }
            else
            {
                Debug.LogError("Invalid diaScript format in line: " + line);
                continue; // Skip this line if diaScript is not an integer
            }

            // Set the remaining fields and add debugging logs
            entry.diaChrImgHightlight = fields[2];
            Debug.Log("diaChrImgHightlight: " + entry.diaChrImgHightlight);

            entry.diaChrNameL = fields[3];
            //entry.diaChrNameL = LanguageManager.Instance.GetTranslation(fields[3]); //使用多語言
            Debug.Log("DiaChrNameL: " + entry.diaChrNameL);

            entry.diaChrImgL = fields[4];
            Debug.Log("DiaChrImgL: " + entry.diaChrImgL);

            entry.diaChrNameR = fields[5];
            //entry.diaChrNameR = LanguageManager.Instance.GetTranslation(fields[5]); //使用多語言
            Debug.Log("DiaChrNameR: " + entry.diaChrNameR);

            entry.diaChrImgR = fields[6];
            Debug.Log("DiaChrImgR: " + entry.diaChrImgR);

            entry.diaText = fields[7];
            //entry.diaText = LanguageManager.Instance.GetTranslation(fields[7]); //使用多語言
            Debug.Log("DiaText: " + entry.diaText);

            entry.diaTextEffect = fields[8];
            Debug.Log("diaTextEffect: " + entry.diaTextEffect);

            entry.diaSelection = fields[9];
            Debug.Log("DiaSelection: " + entry.diaSelection);

            entry.diaConditions = fields[10];
            Debug.Log("DiaConditions: " + entry.diaConditions);

            entry.diaEffects = fields[11];
            Debug.Log("DiaEffects: " + entry.diaEffects);

            entry.diaImgBackground = fields[12];
            Debug.Log("DiaImgBackgrouund: " + entry.diaImgBackground);

            entry.diaImgBackgroundEffects = fields[13];
            Debug.Log("diaImgBackgroundEffects: " + entry.diaImgBackgroundEffects);

            entry.diaBackgroundMusic = fields[14];
            Debug.Log("DiaImgBackgrouund: " + entry.diaBackgroundMusic);

            entry.diaSoundEffect = fields[15];
            Debug.Log("diaSoundEffect: " + entry.diaSoundEffect);

            entry.nextDiaID = fields[16];
            Debug.Log("NextDiaID: " + entry.nextDiaID);

            dialogues.Add(entry);
        }
    }

    void ClearDialogue() // 清除螢幕上的目前對話並停用對話系統。 用於對話結束或需要重置時調用。
    {
        //將名字、對話面板文字清空
        diaChrNameLText.text = "";
        diaChrNameRText.text = "";
        diaText.text = "";
        isDialogueActive = false; // Disable dialogue progression
        // diaImgBackgrundRenderer is not cleared as per requirement
    }
    void DisplayDialogue(DialogueEntry entry) // 显示特定的对话条目，更新 UI 元素（如角色名称、图像和文本）。应用指定的对话效果，并根据需要更新选择按钮。
    {
        //diaChrNameLText.text = entry.diaChrNameL;
        //diaChrNameRText.text = entry.diaChrNameR;
        //StartCoroutine(DisplayTextTypewriterStyle(entry.diaText, entry.diaSelection));

        // 使用翻译后的角色名称

        diaChrNameLText.text = LanguageManager.Instance.GetTranslation(entry.diaChrNameL);
        diaChrNameRText.text = LanguageManager.Instance.GetTranslation(entry.diaChrNameR);
        diaText.text = LanguageManager.Instance.GetTranslation(entry.diaText);        //使用LanguageManager获取对应的翻译文本
        StartCoroutine(DisplayTextTypewriterStyle(diaText.text, entry.diaSelection)); //打字機效果


        //backgroundEffectManager.CompleteCurrentEffect(); // 重置背景特效状态

        // 确保条件被评估，如果有的话
        if (!string.IsNullOrEmpty(entry.diaConditions))
        {
            bool conditionMet = ParseAndEvaluateCondition(entry.diaConditions);
            if (!conditionMet)
            {
                // 如果条件不满足，可以在这里处理逻辑，例如跳转到下一个对话或者其他操作
                return; // 直接返回，不展示当前对话
            }
        }
        // 显示左侧角色图像，如果指定了图像
        if (!string.IsNullOrEmpty(entry.diaChrImgL) && entry.diaChrImgL != "None")
        {
            diaChrImgLRenderer.sprite = LoadchrSpriteByNumber(entry.diaChrImgL);
            diaChrImgLRenderer.gameObject.SetActive(true); // 激活图像对象
        }
        else
        {
            diaChrImgLRenderer.gameObject.SetActive(false); // 隐藏图像对象
        }

        // 显示右侧角色图像，如果指定了图像
        if (!string.IsNullOrEmpty(entry.diaChrImgR) && entry.diaChrImgR != "None")
        {
            diaChrImgRRenderer.sprite = LoadchrSpriteByNumber(entry.diaChrImgR);
            diaChrImgRRenderer.gameObject.SetActive(true);
        }
        else
        {
            diaChrImgRRenderer.gameObject.SetActive(false);
        }

        ApplyHighlighting(entry.diaChrImgHightlight); // 根据diaChrImgHightlight应用高亮逻辑
        PlayBackgroundMusic(entry.diaBackgroundMusic);

        // 检查并设置背景图像
        if (!string.IsNullOrEmpty(entry.diaImgBackground) && entry.diaImgBackground != "None")
        {
            if (diaImgBackgrundRenderer.sprite == null || diaImgBackgrundRenderer.sprite.name != entry.diaImgBackground)
            {
                diaImgBackgrundRenderer.sprite = LoadBackgroundSpriteByNumber(entry.diaImgBackground);
            }
            diaImgBackgrundRenderer.gameObject.SetActive(true);
        }
        else
        {
            diaImgBackgrundRenderer.gameObject.SetActive(false);
        }

        if (int.TryParse(entry.diaImgBackgroundEffects, out int backgroundEffectType))
        {
            //backgroundEffectManager.ApplyEffect(backgroundEffectType, diaImgBackgrundRenderer.rectTransform);
        }
        else
        {
            Debug.LogError("Failed to parse diaImgBackgroundEffects: " + entry.diaImgBackgroundEffects);
        }


        // Re-enable dialogue progression
        allowDialogueProgression = true;

        // Play sound effect if specified
        PlaySoundEffect(entry.diaSoundEffect);
        ApplyDialogueEffects(entry.diaEffects); // Apply effects from diaEffects field

        // Update dialogue selection buttons (if needed)
        UpdateSelectionButtons(entry.diaSelection);
        // Optionally, play background music if specified
        // Implement music playing logic here
    }
    Sprite LoadchrSpriteByNumber(string pictureNumber) // 根据给定的图片编号从资源文件夹加载并返回精灵对象。用于加载角色和背景图像。
    {
        // 清理 pictureNumber 字段，移除空格和單引號
        string cleanedPictureNumber = pictureNumber.Replace(" ", "").Replace("'", "");

        Sprite loadedSprite = Resources.Load<Sprite>("chrImage/" + cleanedPictureNumber);
        if (loadedSprite == null)
        {
            Debug.LogError("Failed to load character sprite with picture number: " + cleanedPictureNumber);
        }
        return loadedSprite;
    }
    void ApplyHighlighting(string highlightSetting) //三元運算結構判斷圖片是否高亮，1為左圖、2為右圖、3為左右一起亮
    {
        // 默认情况下，假设两个角色都需要被反灰
        Color leftColor = Color.gray;
        Color rightColor = Color.gray;

        // 根据highlightSetting决定是否需要调整颜色
        leftColor = (highlightSetting == "1" || highlightSetting == "3") ? Color.white : Color.gray; // 如果右侧角色说话或两者都不高亮，则左侧角色显示正常颜色
        rightColor = (highlightSetting == "2" || highlightSetting == "3") ? Color.white : Color.gray; // 如果左侧角色说话或两者都不高亮，则右侧角色显示正常颜色

        // 应用颜色
        diaChrImgLRenderer.color = leftColor;
        diaChrImgRRenderer.color = rightColor;
    }

    Sprite LoadBackgroundSpriteByNumber(string pictureNumber) // 根据给定的图片编号从资源文件夹加载并返回精灵对象。用于加载角色和背景图像。
    {
        // 清理 pictureNumber 字段，移除空格和单引号
        string cleanedPictureNumber = pictureNumber.Replace(" ", "").Replace("'", "");

        Sprite loadedSprite = Resources.Load<Sprite>("backgroundImage/" + cleanedPictureNumber);
        if (loadedSprite == null)
        {
            Debug.LogError("Failed to load background sprite with picture number: " + cleanedPictureNumber);
        }
        return loadedSprite;
    }

    void UpdateSelectionButtons(string selectionData) // 根据给定的选择数据创建并定位对话选择按钮。这些按钮在水平方向上相互偏移。
    {
        // 清除现有按钮
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        if (string.IsNullOrEmpty(selectionData))
        {
            return; // 如果没有选项数据，直接返回
        }

        // 以 '/' 分割选择数据成独立选项
        string[] options = selectionData.Split('/');
        for (int i = 0; i < options.Length; i++)
        {
            string option = options[i].Trim();
            string[] parts = option.Split('>');
            if (parts.Length != 2) continue; // 跳过格式不正确的选项

            string buttonText = LanguageManager.Instance.GetTranslation(parts[0].Trim());
            string target = parts[1].Trim();

            // 创建并设置按钮
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            TMP_Text buttonTextComponent = buttonObj.GetComponentInChildren<TMP_Text>();
            buttonTextComponent.text = buttonText;

            // 自动调整按钮宽度以匹配文本长度
            RectTransform buttonRectTransform = buttonObj.GetComponent<RectTransform>();
            Vector2 textSize = buttonTextComponent.GetPreferredValues();
            float padding = 20f; // 为按钮文本左右添加一些内边距
            buttonRectTransform.sizeDelta = new Vector2(textSize.x - padding, buttonRectTransform.sizeDelta.y);


            // 检查每个按钮的条件
            bool isButtonEnabled = CheckConditionForButton(target);
            buttonObj.GetComponent<Button>().interactable = isButtonEnabled;

            // 添加按钮点击监听器
            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnSelectionButtonClicked(target));

            // 初始时隐藏按钮
            buttonObj.SetActive(false);
        }
    }
    bool CheckConditionForButton(string target) // 檢查給定目標的按鈕是否滿足條件。 用於根據條件啟用或禁用對話選擇按鈕。
    {
        string[] targetParts = target.Split('-');
        if (targetParts.Length != 2) return false; // Incorrect format

        int targetDiaID = int.Parse(targetParts[0]);
        int targetDiaScript = int.Parse(targetParts[1]);

        DialogueEntry targetEntry = dialogues.Find(d => d.diaID == targetDiaID && d.diaScript == targetDiaScript);
        if (targetEntry != null)
        {
            // Evaluate the condition
            return EvaluateCondition(targetEntry.diaConditions);
        }
        return true; // Default to true if no condition is found
    }
    bool ParseAndEvaluateCondition(string condition) //解析並評估對話系統中定義的條件表達式
    {
        if (string.IsNullOrEmpty(condition)) return true;

        // 检查是否为扩展格式（包含括号）
        int indexOfParenthesis = condition.IndexOf('(');
        if (indexOfParenthesis != -1)
        {
            // 提取基本条件和导向部分
            string baseCondition = condition.Substring(0, indexOfParenthesis).Trim();
            string directionPart = condition.Substring(indexOfParenthesis).Trim(new char[] { '(', ')' });

            // 评估基本条件
            bool conditionResult = EvaluateCondition(baseCondition);

            if (conditionResult)
            {
                // 如果条件满足，则处理导向逻辑
                ProcessDialogueDirection(directionPart);
            }

            return conditionResult;
        }
        else
        {
            // 处理基本格式
            return EvaluateCondition(condition);
        }
    }
    void ProcessDialogueDirection(string directionPart) // 處理對話導向，解析並實現根據對話 ID 和腳本導向特定對話段落的邏輯
    {
        // 解析导向部分，例如 "2-1"
        string[] parts = directionPart.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out int diaID) && int.TryParse(parts[1], out int diaScript))
        {
            // 在这里实现根据diaID和diaScript导向对话段落的逻辑
            StartDialogueByID(diaID, diaScript);
        }
        else
        {
            Debug.LogError("Invalid direction part format: " + directionPart);
        }
    }
    bool EvaluateCondition(string condition) // 評估對話條件。 作為入口點，根據條件的複雜性調用適當的處理邏輯。
    {
        if (string.IsNullOrEmpty(condition)) return true;

        // 利用新的複雜條件評估方法
        return EvaluateComplexCondition(condition);
    }

    bool EvaluateComplexCondition(string condition)// 评估对话条件。支持复杂的条件逻辑，包括逻辑运算符和单个条件的比较。
    {
        condition = condition.Replace(" ", "");

        int openIndex = condition.IndexOf('(');
        while (openIndex != -1)
        {
            int closeIndex = FindClosingParenthesisIndex(condition, openIndex);
            string subCondition = condition.Substring(openIndex + 1, closeIndex - openIndex - 1);
            bool subResult = EvaluateLogicalOperators(subCondition);

            condition = condition.Substring(0, openIndex) + (subResult ? "true" : "false") + condition.Substring(closeIndex + 1);
            openIndex = condition.IndexOf('(');
        }

        return EvaluateLogicalOperators(condition);
    }
    bool EvaluateLogicalOperators(string condition)// 评估对话条件。支持复杂的条件逻辑，包括逻辑运算符和单个条件的比较。
    {
        string[] orParts = condition.Split(new string[] { "or" }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var orPart in orParts)
        {
            bool andResult = true;
            string[] andParts = orPart.Split(new string[] { "and" }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var andPart in andParts)
            {
                bool singleResult;
                if (andPart == "true")
                    singleResult = true;
                else if (andPart == "false")
                    singleResult = false;
                else
                    singleResult = EvaluateSingleCondition(andPart);

                andResult &= singleResult;
                if (!singleResult) break;
            }
            if (andResult) return true;
        }
        return false;
    }

    bool EvaluateSingleCondition(string condition)// 评估对话条件。支持复杂的条件逻辑，包括逻辑运算符和单个条件的比较。
    {
        // 解析單一條件
        string[] parts = condition.Split(new char[] { '>', '<', '=' }, 2);
        if (parts.Length != 2) return false;

        string[] attributeParts = parts[0].Split(':');
        if (attributeParts.Length != 2) return false;

        string target = attributeParts[0];
        string attribute = attributeParts[1];
        float conditionValue = float.Parse(parts[1]);

        characterAttribute character = FindObjectOfType<characterAttribute>();
        if (character != null)
        {
            string comparisonOperator = condition.Substring(parts[0].Length, 1);
            return character.CheckAttribute(attribute, conditionValue, comparisonOperator);
        }
        return false;
    }


    int FindClosingParenthesisIndex(string condition, int openIndex) // 找到给定开括号的对应闭合括号位置。用于处理复杂条件字符串中的嵌套。
    {
        int depth = 0;
        for (int i = openIndex; i < condition.Length; i++)
        {
            if (condition[i] == '(') depth++;
            if (condition[i] == ')')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1; // 沒有找到對應的閉合括號
    }
    void OnSelectionButtonClicked(string target) // 处理对话选择按钮的点击事件。停止当前的打字效果，解析目标对话ID和脚本，更新对话索引以切换到指定的对话。
    {

        //allowDialogueProgression = false; // Disable automatic progression
        StopAllCoroutines(); // 停止任何进行中的打字效果

        string[] targetParts = target.Split('-');
        if (targetParts.Length != 2) return; // 格式不正确

        int targetDiaID = int.Parse(targetParts[0]);
        int targetDiaScript = int.Parse(targetParts[1]);

        int targetIndex = dialogues.FindIndex(d => d.diaID == targetDiaID && d.diaScript == targetDiaScript);
        if (targetIndex >= 0 && targetIndex < dialogues.Count)
        {
            currentDialogueIndex = targetIndex;
            DisplayDialogue(dialogues[targetIndex]);
        }

        // Reactivate automatic dialogue progression
        allowDialogueProgression = true;

        // Check if auto play is enabled and continue auto play dialogue
        if (isAutoPlaying)
        {
            // Restart or continue the auto play coroutine
            if (autoPlayCoroutine != null)
            {
                StopCoroutine(autoPlayCoroutine);
            }
            autoPlayCoroutine = AutoPlayDialogue();
            StartCoroutine(autoPlayCoroutine);
        }

        /*TMP_Text buttonTextComponent = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_Text>();
        string chosenOption = buttonTextComponent.text;
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        history_Text.text += $"({timestamp}) 你選擇了[{chosenOption}]\n";*/

        // Reactivate automatic dialogue progression
        allowDialogueProgression = true;

    }

    void ApplyDialogueEffects(string effects) // 解析并应用对话条目中指定的效果。根据目标和属性改变角色或NPC的属性。
    {
        if (string.IsNullOrEmpty(effects))
        {
            return;
        }

        string[] effectInstructions = effects.Split('/');
        foreach (string instruction in effectInstructions)
        {
            string[] parts = instruction.Split('>');
            if (parts.Length != 2) continue;

            string targetAndAttribute = parts[0].Trim();
            float changeValue = float.Parse(parts[1].Trim());

            // Split target and attribute
            string[] targetAttributeParts = targetAndAttribute.Split(':');
            if (targetAttributeParts.Length != 2) continue;

            string target = targetAttributeParts[0].Trim();
            string attribute = targetAttributeParts[1].Trim();

            // Apply the effect based on target and attribute
            ApplyEffectToTarget(target, attribute, changeValue);
        }
    }
    void ApplyEffectToTarget(string target, string attribute, float changeValue) // 解析并应用对话条目中指定的效果。根据目标和属性改变角色或NPC的属性。
    {
        IAttributeChanger attributeChanger = null;

        if (target.StartsWith("NPC"))
        {
            // 從 NPC 字符串中提取 NPC ID
            string npcId = target.Substring(3);
            NPCManager npcManager = FindObjectOfType<NPCManager>();
            if (npcManager != null)
            {
                attributeChanger = npcManager.GetNPCAttributeChanger(npcId);
            }
            else
            {
                Debug.LogError("NPCManager not found in scene.");
            }
        }
        else if (target == "chr")
        {
            // 玩家角色邏輯
            attributeChanger = FindObjectOfType<characterAttribute>();
        }

        if (attributeChanger != null)
        {
            attributeChanger.ChangeAttribute(attribute, changeValue);
        }
        else
        {
            Debug.LogError("Target for attribute change not found: " + target);
        }
    }
    public void ToggleAutoPlay() // 切換自動播放功能，自動播放將自動推進對話。
    {
        isAutoPlaying = !isAutoPlaying;
        // 根据自动播放状态显示或隐藏滑动条和文本
        dialogueSliderbarAutoplaySpeed.gameObject.SetActive(isAutoPlaying);
        dialogueTextAutoplaySpeed.gameObject.SetActive(isAutoPlaying);

        // 设置按钮颜色来反映自动播放的状态
        SetButtonColor(autoPlayButton, isAutoPlaying ? autoPlayActiveColor : autoPlayOriginalColor);

        if (isAutoPlaying)
        {


            if (autoPlayCoroutine != null)
            {
                StopCoroutine(autoPlayCoroutine);
            }
            autoPlayCoroutine = AutoPlayDialogue();
            StartCoroutine(autoPlayCoroutine);
        }
        else
        {
            dialogueSliderbarAutoplaySpeed.value = 0; // 重置为 1x 对应的值
            if (autoPlayCoroutine != null)
            {
                StopCoroutine(autoPlayCoroutine);
                autoPlayCoroutine = null;
            }
        }
    }

    private IEnumerator AutoPlayDialogue() // 自動播放對話的協程，等待打字效果完成並自動推進對話
    {
        while (isAutoPlaying && isDialogueActive)
        {
            // 等待当前对话打字效果完成
            yield return new WaitWhile(() => isTextTypingInProgress);
            // 等待设定的停顿时间
            yield return new WaitForSeconds(autoPlayPauseDuration);
            // 推进到下一条对话
            if (allowDialogueProgression)
            {
                ProgressDialogue();
            }
        }
    }
    void SetButtonColor(Button button, Color color) // 设置按钮颜色，反映自动播放状态的按钮颜色设置
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color;
        button.colors = colors;
    }
    void OnAutoplaySpeedChanged(float value) //調整自動播放狀態倍率文字顯示
    {
        // 根据滑动条的值调整 autoPlayPauseDuration 并更新倍率显示
        switch ((int)value)
        {
            case 0:
                autoPlayPauseDuration = 2.0f; // 1x
                dialogueTextAutoplaySpeed.text = "1x";
                break;
            case 1:
                autoPlayPauseDuration = 1.5f; // 1.5x
                dialogueTextAutoplaySpeed.text = "1.5x";
                break;
            case 2:
                autoPlayPauseDuration = 1.0f; // 2x
                dialogueTextAutoplaySpeed.text = "2x";
                break;
            case 3:
                autoPlayPauseDuration = 0.5f; // 2.5x
                dialogueTextAutoplaySpeed.text = "2.5x";
                break;
            // 移除2.75x的情况，如果需要可以根据maxValue的调整再添加
            default:
                autoPlayPauseDuration = 2.0f; // 默认值为1x
                dialogueTextAutoplaySpeed.text = "1x";
                break;
        }
    }
    public void AccelerateAutoplaySpeed() //自動播放加速按鈕啟動及自動播放速率調節
    {
        // 如果自动播放未启用，则启用它
        if (!isAutoPlaying)
        {
            ToggleAutoPlay();
        }

        // 提升滑动条的级距，当达到最大值时循环回最小值
        int newValue = (int)dialogueSliderbarAutoplaySpeed.value + 1;
        if (newValue > 3) // 如果超过最大值，重置为0（对应1x速度）
        {
            newValue = 0;
        }
        dialogueSliderbarAutoplaySpeed.value = newValue;

        // 根据新的滑动条值调整播放速率和显示文本
        OnAutoplaySpeedChanged(dialogueSliderbarAutoplaySpeed.value);
    }
    void PlayBackgroundMusic(string musicId) //播放指定編號音源
    {
        // 檢查當前音訊是否正在播放以及音訊源的clip是否與請求播放的音樂ID匹配
        bool isPlayingSameClip = audioSource.isPlaying && audioSource.clip != null && audioSource.clip.name == musicId;
        if (isPlayingSameClip || (string.IsNullOrEmpty(musicId) || musicId.Equals("None", System.StringComparison.OrdinalIgnoreCase)))
        {
            // 如果是相同的音樂正在播放，或沒有指定musicId（或為"None"），則不做任何操作
            if (string.IsNullOrEmpty(musicId) || musicId.Equals("None", System.StringComparison.OrdinalIgnoreCase))
            {
                audioSource.Stop(); // 如果指定停止音樂，則停止播放
            }
            return; // 早期返回避免重新播放相同音樂
        }

        // 載入並播放新的背景音樂
        AudioClip clip = Resources.Load<AudioClip>($"Audio/BackgroundMusic/{musicId}");
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"Music clip not found for ID: {musicId}");
        }
    }
    void PlaySoundEffect(string soundEffectId) //撥放指定編號音效
    {
        if (!string.IsNullOrEmpty(soundEffectId) && soundEffectId != "None")
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/SoundEffects/{soundEffectId}");
            if (clip != null)
            {
                // Use soundEffectSource to play the sound effect
                soundEffectSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"Sound effect clip not found for ID: {soundEffectId}");
            }
        }
    }

}