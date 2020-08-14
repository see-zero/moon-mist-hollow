using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public Sprite[] walkSSFrames;
    public Sprite[] walkSWFrames;
    public Sprite[] walkWWFrames;
    public Sprite[] walkNWFrames;
    public Sprite[] walkNNFrames;
    public Sprite[] walkNEFrames;
    public Sprite[] walkEEFrames;
    public Sprite[] walkSEFrames;
    
    private int curFrame;
    private float timer;
    private float framerate = .1f;
    private bool active = false;

    private Sprite[] curFrames;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        PlayAnimation(Direction.SE);
    }


    void Update()
    {
        if (active) {
            UpdateAnimation();
        }
    }


    private void UpdateAnimation() {
        timer += Time.deltaTime;

        if (timer >= framerate) {
            timer -= framerate;
            curFrame = (curFrame + 1) % curFrames.Length;
            spriteRenderer.sprite = curFrames[curFrame];
        }
    }


    public void PlayAnimation(Direction dir) {
        timer = 0;
        curFrame = 0;
        active = true;

        switch (dir) {
            case Direction.EE: {
                curFrames = walkEEFrames;
                break;
            }
            case Direction.NE: {
                curFrames = walkNEFrames;
                break;
            }
            case Direction.NN: {
                curFrames = walkNNFrames;
                break;
            }
            case Direction.NW: {
                curFrames = walkNWFrames;
                break;
            }
            case Direction.SE: {
                curFrames = walkSEFrames;
                break;
            }
            case Direction.SS: {
                curFrames = walkSSFrames;
                break;
            }
            case Direction.SW: {
                curFrames = walkSWFrames;
                break;
            }
            case Direction.WW: {
                curFrames = walkWWFrames;
                break;
            }
        }
    }


    public void setActive(bool _active) => active = _active;
}
