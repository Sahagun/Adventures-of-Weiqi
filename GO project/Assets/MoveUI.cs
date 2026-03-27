using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIElementMoveY
{
    public RectTransform uiElement;

    public bool captureStartYOnPlay = true;

    public float startY;
    public float endY;
}

public class MoveUI : MonoBehaviour
{
    [Header("Button")]
    public Button moveButton;

    [Header("Move Settings")]
    public List<UIElementMoveY> elements = new List<UIElementMoveY>();
    public float glideSpeed = 10f;

    private void Start()
    {
        if (moveButton != null)
            moveButton.onClick.AddListener(OnMoveButtonPressed);

        foreach (var e in elements)
        {
            if (e.uiElement == null) continue;

            if (e.captureStartYOnPlay)
                e.startY = e.uiElement.anchoredPosition.y;

            var p = e.uiElement.anchoredPosition;
            e.uiElement.anchoredPosition = new Vector2(p.x, e.startY);
        }
    }

    private void OnMoveButtonPressed()
    {
        foreach (var e in elements)
        {
            if (e.uiElement == null) continue;
            StartCoroutine(GlideY(e.uiElement, e.endY));
        }
    }

    private IEnumerator GlideY(RectTransform rt, float targetY)
    {
        while (Mathf.Abs(rt.anchoredPosition.y - targetY) > 0.5f)
        {
            Vector2 p = rt.anchoredPosition;
            float newY = Mathf.Lerp(p.y, targetY, Time.deltaTime * glideSpeed);
            rt.anchoredPosition = new Vector2(p.x, newY);
            yield return null;
        }

        Vector2 final = rt.anchoredPosition;
        rt.anchoredPosition = new Vector2(final.x, targetY);
    }
}