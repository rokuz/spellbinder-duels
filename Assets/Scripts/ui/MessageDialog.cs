using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MessageDialog : MonoBehaviour
{
    public Text text;
    public Button okButton;
    public Image splash;

    public delegate void OnClose();
    private OnClose onCloseHandler;

	public void Start()
    {
        gameObject.SetActive(false);
	}

	public void Update()
    {
	}

    public void Open(string message, OnClose onCloseHandler)
    {
        text.text = message;
        this.onCloseHandler = onCloseHandler;

        gameObject.SetActive(true);
        if (!splash.IsActive())
            splash.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (splash.IsActive())
            splash.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    public void OnOkButtonClicked()
    {
        Close();
        if (onCloseHandler != null)
            onCloseHandler();
    }
}
