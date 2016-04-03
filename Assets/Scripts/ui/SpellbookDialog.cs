using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpellbookDialog : MonoBehaviour
{
    public GameObject spellInfoPrefab;
    public Image splash;
    public GameObject content;

    public delegate void OnClose();
    private OnClose onCloseHandler;

    public void Start()
    {
        gameObject.SetActive(false);

        int count = 20;

        const float kSpacing = 10.0f;
        float startOffsetY = -kSpacing;
        float height = 0;
        for (int i = 0; i < count; i++)
        {
            GameObject spellInfo = Instantiate(spellInfoPrefab);
            spellInfo.transform.SetParent(content.transform, false);
            Rect r = spellInfo.GetComponent<RectTransform>().rect;
            float h = r.height + kSpacing;
            spellInfo.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(kSpacing, -i * h + startOffsetY, 0.0f);
            height += h;
        }
        height += kSpacing;
        content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    public void Update()
    {
        CloseIfClickedOutside(this.gameObject);
    }

    public void Open(OnClose onCloseHandler)
    {
        this.onCloseHandler = onCloseHandler;

        ScrollRect scroll = gameObject.GetComponentInChildren<ScrollRect>();
        scroll.verticalNormalizedPosition = 1.0f;

        gameObject.SetActive(true);
        if (splash != null && !splash.IsActive())
            splash.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (splash != null && splash.IsActive())
            splash.gameObject.SetActive(false);

        gameObject.SetActive(false);

        if (onCloseHandler != null)
            onCloseHandler();
    }

    private void CloseIfClickedOutside(GameObject panel)
    {
         if (Input.GetMouseButtonDown(0) && panel.activeSelf && 
             !RectTransformUtility.RectangleContainsScreenPoint(
                 panel.GetComponent<RectTransform>(), 
                 Input.mousePosition, 
                 Camera.main)) {
            Close();
         }
     }
}