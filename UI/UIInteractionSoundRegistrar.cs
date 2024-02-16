using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIInteractionSoundRegistrar : MonoBehaviour
{
    public Button[] buttons; // 可以在 Inspector 中手動指定需要註冊互動音效的按鈕

    private void Awake()
    {
        // 為每個按鈕添加 UIInteractionSound 行為
        foreach (var button in buttons)
        {
            var sound = button.gameObject.AddComponent<UIInteractionSound>();
            // 如果需要對 sound 進行額外配置，可以在這裡操作
        }
    }
}
