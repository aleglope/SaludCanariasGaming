using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [Header("Referencias UI")]
    public Image cardBack;
    public Image cardFace;

    [Header("Sprites")]
    public Sprite matchedSprite;

    private int cardId;
    private bool isFlipped = false;
    private bool isMatched = false;
    private GameController gameController;

    public void SetUp(int id, Sprite faceSprite, Sprite matchedSprite, GameController controller)
    {
        cardId = id;
        cardFace.sprite = faceSprite;
        this.matchedSprite = matchedSprite;
        gameController = controller;
        FlipBackInstant();
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanFlip()) return;
        Flip();
        gameController.CardFlipped(this);
    }

    private bool CanFlip()
    {
        return !isMatched && !isFlipped && !gameController.IsTransitioning();
    }

    public void Flip()
    {
        isFlipped = true;

        LeanTween.rotateY(gameObject, 90f, 0.15f).setOnComplete(() =>
        {
            cardBack.enabled = false;
            cardFace.enabled = true;
            LeanTween.rotateY(gameObject, 0f, 0.15f);
        });
    }

    public void FlipBack()
    {
        StartCoroutine(FlipBackCoroutine());
    }

    private IEnumerator FlipBackCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        isFlipped = false;

        LeanTween.rotateY(gameObject, 90f, 0.15f).setOnComplete(() =>
        {
            cardBack.enabled = true;
            cardFace.enabled = false;
            LeanTween.rotateY(gameObject, 0f, 0.15f);
            LeanTween.moveLocalX(gameObject, transform.localPosition.x + 10f, 0.05f).setLoopPingPong(2);
        });
    }

    public void PlayMatchEffect()
    {
        cardFace.sprite = matchedSprite;

        LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.15f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                LeanTween.scale(gameObject, Vector3.one, 0.1f);
            });
    }

    public void FlipBackInstant()
    {
        isFlipped = false;
        cardBack.enabled = true;
        cardFace.enabled = false;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public int GetCardId() => cardId;

    public void SetMatched(bool matched)
    {
        isMatched = matched;

        if (matched)
            PlayMatchEffect();
    }

    public void PlaySpawnEffect()
    {
        transform.localScale = Vector3.zero;

        LeanTween.scale(gameObject, Vector3.one, 0.4f)
            .setEase(LeanTweenType.easeOutBack)
            .setDelay(Random.Range(0f, 0.3f));
    }

    public bool IsMatched() => isMatched;
}
