using UnityEngine;
using System.Collections;

public class CardCollider : MonoBehaviour {

    public delegate void OnCardTapped(int cardIndex);

    private int cardIndex = -1;
    private OnCardTapped onCardTapped = null;

    public void Setup(int cardIndex, OnCardTapped onCardTapped)
    {
        this.cardIndex = cardIndex;
        this.onCardTapped = onCardTapped;
    }

	public void OnMouseDown()
    {
        if (onCardTapped != null)
            onCardTapped(cardIndex);
    }
}
