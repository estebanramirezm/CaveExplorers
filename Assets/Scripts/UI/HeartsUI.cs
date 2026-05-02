using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeartsUI : MonoBehaviour
{
    [Header("Hearts")]
    public Image[] HeartImages;
    public Sprite FullSprite;
    public Sprite EmptySprite;

    [Header("Firefly Counter")]
    public Image FireflyIcon;
    public TextMeshProUGUI FireflyCountText;

    [Header("Fade")]
    public float FadeDuration = 0.25f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        UpdateHearts(3);
        UpdateFireflyCount(0);
    }

    public void UpdateHearts(int currentHearts)
    {
        for (int i = 0; i < HeartImages.Length; i++)
        {
            if (HeartImages[i] == null) continue;
            HeartImages[i].sprite = i < currentHearts ? FullSprite : EmptySprite;
        }
    }

    public void UpdateFireflyCount(int count)
    {
        if (FireflyCountText != null)
            FireflyCountText.text = "x" + count;
    }

    public void Hide() => Fade(0f);
    public void Show() => Fade(1f);

    private void Fade(float target)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTo(target));
    }

    private IEnumerator FadeTo(float target)
    {
        float start   = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / FadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
        canvasGroup.blocksRaycasts = target > 0f;
    }
}
