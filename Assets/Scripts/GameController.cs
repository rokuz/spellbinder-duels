﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class GameController : MonoBehaviour 
{
    private const int kCardsCount = 12;

    public ServerRequest serverRequest;
    public GameObject[] cards;
    public Sprite fireSprite;
    public Sprite waterSprite;
    public Sprite airSprite;
    public Sprite earthSprite;
    public Sprite natureSprite;
    public Sprite lightSprite;
    public Sprite darknessSprite;
    public Sprite bloodSprite;
    public Sprite illusionSprite;

    private Magic[] gameField = new Magic[kCardsCount] { 
        Magic.WATER, Magic.EARTH, Magic.NATURE, Magic.DARKNESS,
        Magic.EARTH, Magic.AIR, Magic.AIR, Magic.NATURE,
        Magic.WATER, Magic.DARKNESS, Magic.EARTH, Magic.EARTH };

    private bool isBackShowing = true;
    private float swapRequestTime = 0.0f;
    private bool isSwapping = false;

	// Use this for initialization
    public void Start () 
    {
        Vector3 cardSize = Vector3.zero;
        for (int i = 0; i < cards.Length; i++)
        {
            SpriteRenderer renderer = (from r in cards[i].GetComponentsInChildren<SpriteRenderer>() where r.gameObject.name == "front" select r).Single();
            if (renderer != null)
                renderer.sprite = GetSprite(gameField[i]);

            if (i == 0)
                cardSize = renderer.bounds.size;
        }

        const float kGapSize = 2.0f;
        Vector3 origin = new Vector3(-1.5f * cardSize.x - 1.5f * kGapSize, cardSize.y + kGapSize, 0.0f);
        for (int i = 0; i < 3; i++)
        {
            float offsetY = (i == 0) ? 0.0f : kGapSize * i;
            for (int j = 0; j < 4; j++)
            {
                float offsetX = (j == 0) ? 0.0f : kGapSize * j;
                cards[4 * i + j].transform.position = origin + new Vector3(j * cardSize.x + offsetX, -i * cardSize.y - offsetY, 0.0f);
            }
        }
	}
	
	// Update is called once per frame
    public void Update ()
    {
        if (Input.GetMouseButtonDown(0))
            SwapAllCards();
        
        AnimateCardsSwapping();
	}
        
    private void SwapAllCards()
    {
        if (!isSwapping)
        {
            swapRequestTime = Time.time;
            isBackShowing = !isBackShowing;
            for (int i = 0; i < cards.Length; i++)
                cards[i].transform.rotation = Quaternion.AngleAxis(isBackShowing ? 180.0f : 0.0f, Vector3.up);
            isSwapping = true;
        }
    }

    private void AnimateCardsSwapping()
    {
        if (isSwapping)
        {
            float kAnimationTimeSec = 1.0f;
            float delta = Time.time - swapRequestTime;
            if (delta <= kAnimationTimeSec)
            {
                for (int i = 0; i < cards.Length; i++)
                    cards[i].transform.Rotate(new Vector3(0, 180.0f * Time.deltaTime / kAnimationTimeSec, 0));
            }
            else
            {
                for (int i = 0; i < cards.Length; i++)
                    cards[i].transform.rotation = Quaternion.AngleAxis(isBackShowing ? 0.0f : 180.0f, Vector3.up);
                isSwapping = false;
            }
        }
    }

    private Sprite GetSprite(Magic magic) 
    {
        switch (magic) 
        {
            case Magic.FIRE:
                return this.fireSprite;
            case Magic.WATER:
                return this.waterSprite;
            case Magic.AIR:
                return this.airSprite;
            case Magic.EARTH:
                return this.earthSprite;
            case Magic.NATURE:
                return this.natureSprite;
            case Magic.LIGHT:
                return this.lightSprite;
            case Magic.DARKNESS:
                return this.darknessSprite;
            case Magic.BLOOD:
                return this.bloodSprite;
            case Magic.ILLUSION:
                return this.illusionSprite;
        }
        return null;
    }
}
