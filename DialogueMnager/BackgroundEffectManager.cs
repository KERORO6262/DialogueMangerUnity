using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BackgroundEffectManager : MonoBehaviour
{
    public Component backgroundComponent; // Reference to the background component, can be any component
    private Dictionary<int, Coroutine> activeEffects = new Dictionary<int, Coroutine>();
    private List<GameObject> effectObjects = new List<GameObject>(); // Store effect-related objects for cleanup
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Color initialColor;
    private RectTransform rectTransform;
    public Graphic targetGraphic;

    void Awake()
    {
        // Determine if the background component is a UI Image or a Sprite Renderer and save initial states accordingly
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.localPosition;
        initialScale = rectTransform.localScale;

        // Try to get a color from the component if it supports it
        if (backgroundComponent is Image image)
        {
            initialColor = image.color;
        }
        else if (backgroundComponent is SpriteRenderer spriteRenderer)
        {
            initialColor = spriteRenderer.color;
        }
        else
        {
            // Default color in case the component doesn't have a color property
            initialColor = Color.white;
        }
    }
    void Start()
    {

    }
    public void ApplyEffect(int effectType, RectTransform targetRectTransform)
    {
        // Reset to initial states before applying a new effect
        CompleteCurrentEffect();

        // Start the corresponding coroutine based on the effectType
        Coroutine newEffectCoroutine = null;
        switch (effectType)
        {
            case 1:
                newEffectCoroutine = StartCoroutine(SweepTransitionEffect());
                break;
            case 2:
                newEffectCoroutine = StartCoroutine(SlightShakeEffect(targetRectTransform));
                break;
            case 3:
                newEffectCoroutine = StartCoroutine(ColorChangeEffect(targetGraphic));
                break;
            case 4:
                newEffectCoroutine = StartCoroutine(FadeOutEffect(graphic: targetGraphic));
                break;
            case 5:
                newEffectCoroutine = StartCoroutine(FadeInEffect(graphic: targetGraphic));
                break;
            case 6:
                newEffectCoroutine = StartCoroutine(ScaleEffect(targetRectTransform));
                break;
            case 7:
                newEffectCoroutine = StartCoroutine(TVSnowflakeEntranceEffect());
                break;
            case 8:
                newEffectCoroutine = StartCoroutine(TVSnowflakeAppearanceEffect());
                break;
            // Add more cases for additional effects as needed
            default:
                Debug.LogWarning($"Unknown effect type: {effectType}");
                break;
        }

        // Update the dictionary of active effects
        if (newEffectCoroutine != null)
        {
            activeEffects[effectType] = newEffectCoroutine;
        }
    }
    public void CompleteCurrentEffect()
    {

        // 停止所有活动的特效协程并清除字典
        foreach (var kvp in activeEffects)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
        }
        activeEffects.Clear();

        // 清理与特效相关的对象
        foreach (var obj in effectObjects)
        {
            Destroy(obj);
        }
        effectObjects.Clear();

        // 重置背景组件属性到初始值
        rectTransform.localPosition = initialPosition;
        rectTransform.localScale = initialScale;

        // 重新应用初始颜色，包括Alpha值
        if (targetGraphic != null)
        {
            targetGraphic.color = initialColor;
        }
        else if (backgroundComponent is Image image)
        {
            image.color = initialColor;
        }
        else if (backgroundComponent is SpriteRenderer spriteRenderer)
        {
            spriteRenderer.color = initialColor;
        }
    }
    public void ResetScaleEffectImmediately()
    {
        // 检查是否有需要立即重置Scale的对象
        if (targetGraphic != null && targetGraphic.GetComponent<RectTransform>() != null)
        {
            targetGraphic.GetComponent<RectTransform>().localScale = initialScale;
        }
        else if (rectTransform != null) // Fallback to rectTransform if targetGraphic is not set
        {
            rectTransform.localScale = initialScale;
        }
    }
    IEnumerator SweepTransitionEffect() //掃掠效果
    {
        float duration = 60.0f;
        int maskCount = 10;
        List<GameObject> masks = new List<GameObject>();

        RectTransform backgroundRectTransform = backgroundComponent.GetComponent<RectTransform>();
        float imageWidth = backgroundRectTransform.rect.width;
        float imageHeight = backgroundRectTransform.rect.height;

        float remainingHeight = imageHeight;
        List<float> heights = new List<float>();

        for (int i = 0; i < maskCount; i++)
        {
            float height = (i == maskCount - 1) ?
                remainingHeight :
                Random.Range(50, remainingHeight - 50 * (maskCount - i - 1));
            remainingHeight -= height;
            heights.Add(height);
        }

        float currentY = -imageHeight / 2;

        for (int i = 0; i < maskCount; i++)
        {
            GameObject mask = new GameObject($"Mask{i}", typeof(Image));
            mask.transform.SetParent(backgroundRectTransform, false);
            Image maskImage = mask.GetComponent<Image>();
            maskImage.color = Color.black;

            float width = Random.Range(imageWidth, imageWidth * 1.5f);
            float height = heights[i];
            mask.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
            float anchoredPosX = (imageWidth / 2) - (width / 2);
            mask.GetComponent<RectTransform>().anchoredPosition = new Vector2(anchoredPosX, currentY + height / 2);
            currentY += height;

            effectObjects.Add(mask);
            masks.Add(mask);
        }

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            foreach (GameObject mask in masks)
            {
                RectTransform rect = mask.GetComponent<RectTransform>();
                float targetX = Mathf.Lerp(rect.anchoredPosition.x, imageWidth / 2 + rect.sizeDelta.x / 2, progress);
                rect.anchoredPosition = new Vector2(targetX, rect.anchoredPosition.y);
            }
            yield return null;
        }

        foreach (GameObject mask in masks)
        {
            Destroy(mask);
        }
    }

    IEnumerator SlightShakeEffect(RectTransform targetRectTransform) //震動效果
    {
        float duration = 1f; // 减少震动持续时间以模拟被击打效果
        float magnitude = 4f; // 增加震动幅度以增强效果

        Vector3 originalPos = targetRectTransform.localPosition; // 使用targetRectTransform引用
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = originalPos.x + Random.Range(-magnitude, magnitude);
            float y = originalPos.y + Random.Range(-magnitude, magnitude);

            targetRectTransform.localPosition = new Vector3(x, y, originalPos.z); // 使用targetRectTransform设置新位置
            elapsed += Time.deltaTime;

            yield return null; // 控制震动频率
        }

        // 震动结束后，逐渐将位置恢复到初始状态
        float restoreDuration = 0.1f;
        float restoreElapsed = 0.0f;
        while (restoreElapsed < restoreDuration)
        {
            targetRectTransform.localPosition = Vector3.Lerp(targetRectTransform.localPosition, originalPos, restoreElapsed / restoreDuration);
            restoreElapsed += Time.deltaTime;
            yield return null;
        }

        targetRectTransform.localPosition = originalPos; // 确保最终位置正确设置
    }

    IEnumerator ColorChangeEffect(Graphic targetGraphic)
    {
        int flashCount = 2; // 设定闪烁次数
        float flashDuration = 0.1f; // 单次闪烁持续时间
        Color startColor = targetGraphic.color; // 获取起始颜色
        Color hitColor = new Color(1 - startColor.r, 1 - startColor.g, 1 - startColor.b, startColor.a); // 使用反色作为击中颜色

        // 重复执行闪烁效果
        for (int i = 0; i < flashCount; i++)
        {
            // 从原色变到hitColor
            for (float t = 0; t <= flashDuration; t += Time.deltaTime)
            {
                targetGraphic.color = Color.Lerp(startColor, hitColor, t / flashDuration);
                yield return null;
            }
            // 立刻变回原色，为下一次闪烁做准备
            targetGraphic.color = startColor;

            // 如果不是最后一次闪烁，则短暂等待再次闪烁
            if (i < flashCount - 1)
            {
                yield return new WaitForSeconds(flashDuration);
            }
        }

        // 确保最终颜色设置为原始颜色
        targetGraphic.color = startColor;
    }
    IEnumerator FadeOutEffect(Graphic graphic = null, Renderer renderer = null) //淡出效果
    {
        float duration = 1.0f; // 淡出持续时间
        Color originalColor = graphic != null ? graphic.color : renderer.material.color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);

        for (float t = 0; t <= duration; t += Time.deltaTime)
        {
            Color newColor = Color.Lerp(originalColor, transparentColor, t / duration);
            if (graphic != null)
            {
                graphic.color = newColor;
            }
            else if (renderer != null)
            {
                renderer.material.color = newColor;
            }
            yield return null;
        }
    }
    IEnumerator FadeInEffect(Graphic graphic = null, Renderer renderer = null) //淡入效果
    {
        float duration = 1.0f; // 淡入持续时间
        Color transparentColor = graphic != null ? new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0) : new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 0);
        Color originalColor = new Color(transparentColor.r, transparentColor.g, transparentColor.b, 1); // Assuming original color is fully opaque

        for (float t = 0; t <= duration; t += Time.deltaTime)
        {
            Color newColor = Color.Lerp(transparentColor, originalColor, t / duration);
            if (graphic != null)
            {
                graphic.color = newColor;
            }
            else if (renderer != null)
            {
                renderer.material.color = newColor;
            }
            yield return null;
        }
    }
    IEnumerator ScaleEffect(RectTransform target) //拉伸模擬物理效果
    {
        float initialStretchDuration = 0.1f;
        float initialShrinkDuration = 0.1f;
        Vector3 originalScale = target.localScale;
        Vector3 maxStretchedScale = new Vector3(1.3f, 0.7f, 1);
        Vector3 maxShrunkenScale = new Vector3(0.8f, 1.2f, 1);

        // 第一次拉伸和收缩
        yield return StretchAndShrink(target, maxStretchedScale, maxShrunkenScale, initialStretchDuration, initialShrinkDuration);

        // 弹跳效果
        float bounceDuration = 0.08f;
        Vector3[] bounceScales = {
        new Vector3(0.85f, 1.15f, 1),
        new Vector3(1.05f, 0.95f, 1),
        new Vector3(0.95f, 1.05f, 1),
        new Vector3(1.02f, 0.98f, 1),
        new Vector3(0.99f, 1.01f, 1),
        originalScale
    };

        foreach (var targetScale in bounceScales)
        {
            yield return StretchAndShrink(target, targetScale, originalScale, bounceDuration, bounceDuration);
            bounceDuration *= 0.9f;
        }

        target.localScale = originalScale; // 恢复原始尺寸
    }

    IEnumerator StretchAndShrink(RectTransform target, Vector3 targetStretch, Vector3 targetShrink, float stretchDuration, float shrinkDuration)
    {
        Vector3 originalScale = target.localScale;

        for (float t = 0; t <= stretchDuration; t += Time.deltaTime)
        {
            target.localScale = Vector3.Lerp(originalScale, targetStretch, t / stretchDuration);
            yield return null;
        }

        for (float t = 0; t <= shrinkDuration; t += Time.deltaTime)
        {
            target.localScale = Vector3.Lerp(targetStretch, targetShrink, t / shrinkDuration);
            yield return null;
        }
    }
    IEnumerator TVSnowflakeEntranceEffect()//雪花轉場入場效果
    {
        RectTransform targetRectTransform = backgroundComponent as RectTransform; // 将backgroundComponent强制转换为RectTransform
        if (targetRectTransform == null) yield break; // 如果转换失败，则退出

        float effectDuration = 2.0f; // 效果持续时间
        Vector2 size = targetRectTransform.rect.size; // 获取目标尺寸
        int snowflakePerRow = 50; // 每行的雪花数量
        float snowflakeSize = size.x / snowflakePerRow; // 根据每行雪花数量计算单个雪花大小
        int snowflakePerColumn = Mathf.CeilToInt(size.y / snowflakeSize); // 计算每列的雪花数量

        for (int y = 0; y < snowflakePerColumn; y++)
        {
            for (int x = 0; x < snowflakePerRow; x++)
            {
                GameObject snowflake = new GameObject("Snowflake", typeof(Image));
                snowflake.transform.SetParent(targetRectTransform, false);
                Image snowflakeImage = snowflake.GetComponent<Image>();
                snowflakeImage.color = Color.white;
                RectTransform rect = snowflake.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(x * snowflakeSize - size.x / 2 + snowflakeSize / 2, y * snowflakeSize - size.y / 2 + snowflakeSize / 2);
                rect.sizeDelta = new Vector2(snowflakeSize, snowflakeSize);
                effectObjects.Add(snowflake); // Add snowflake to effectObjects for later cleanup
            }
        }

        // 逐渐移除雪花
        float elapsedTime = 0;
        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / effectDuration;
            int toRemove = Mathf.CeilToInt(effectObjects.Count * Mathf.Pow(progress, 2));

            for (int i = 0; i < toRemove && effectObjects.Count > 0; i++)
            {
                int index = Random.Range(0, effectObjects.Count);
                GameObject snowflakeToRemove = effectObjects[index];
                effectObjects.RemoveAt(index);
                Destroy(snowflakeToRemove);
            }

            yield return null;
        }
    }
    IEnumerator TVSnowflakeAppearanceEffect()//雪花轉場出場效果
    {
        RectTransform targetRectTransform = backgroundComponent as RectTransform; // 将backgroundComponent强制转换为RectTransform
        if (targetRectTransform == null) yield break; // 如果转换失败，则退出

        float effectDuration = 2.0f; // 效果持续时间
        Vector2 size = targetRectTransform.rect.size; // 获取目标尺寸
        int snowflakePerRow = 50; // 每行的雪花数量
        float snowflakeSize = size.x / snowflakePerRow; // 根据每行雪花数量计算单个雪花大小
        int snowflakePerColumn = Mathf.CeilToInt(size.y / snowflakeSize); // 计算每列的雪花数量

        // 清除之前所有的雪花
        foreach (var obj in effectObjects)
        {
            Destroy(obj);
        }
        effectObjects.Clear();

        // 预创建所有雪花，但初始状态为不可见，并确保雪花均匀分布
        for (int y = 0; y < snowflakePerColumn; y++)
        {
            for (int x = 0; x < snowflakePerRow; x++)
            {
                GameObject snowflake = new GameObject("Snowflake", typeof(Image));
                snowflake.transform.SetParent(targetRectTransform, false);
                Image snowflakeImage = snowflake.GetComponent<Image>();
                snowflakeImage.color = new Color(1, 1, 1, 0); // 初始为完全透明
                RectTransform rect = snowflake.GetComponent<RectTransform>();
                float posX = (-size.x / 2) + (snowflakeSize * x) + (snowflakeSize / 2);
                float posY = (-size.y / 2) + (snowflakeSize * y) + (snowflakeSize / 2);
                rect.anchoredPosition = new Vector2(posX, posY);
                rect.sizeDelta = new Vector2(snowflakeSize, snowflakeSize);
                effectObjects.Add(snowflake);
            }
        }

        // 随机化effectObjects列表，使雪花随机出现
        System.Random rng = new System.Random();
        int n = effectObjects.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = effectObjects[k];
            effectObjects[k] = effectObjects[n];
            effectObjects[n] = value;
        }

        // 逐步使雪花可见
        float elapsedTime = 0;
        int shownCount = 0;
        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.deltaTime;
            int targetShownCount = Mathf.FloorToInt((elapsedTime / effectDuration) * effectObjects.Count);

            for (int i = shownCount; i < targetShownCount; i++)
            {
                if (i < effectObjects.Count)
                {
                    Image snowflakeImage = effectObjects[i].GetComponent<Image>();
                    snowflakeImage.color = Color.white; // 使雪花可见
                }
            }
            shownCount = targetShownCount;

            yield return null;
        }

        // 确保所有雪花都被显示
        foreach (var obj in effectObjects)
        {
            Image snowflakeImage = obj.GetComponent<Image>();
            snowflakeImage.color = Color.white;
        }
    }
}