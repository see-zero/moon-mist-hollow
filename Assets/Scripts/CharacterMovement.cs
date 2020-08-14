using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private Transform xform;
    private Rigidbody2D rigidBody;
    private Vector3 dir = Vector3.zero; 
    private CharacterAnimator animator;
 
    private int decisionClock = 0;
    private int decisionTime = 260;

    public float speed = 2.4f;


    void Start()
    {
        animator = GetComponent<CharacterAnimator>();
        rigidBody = GetComponent<Rigidbody2D>();
        xform = GetComponent<Transform>();
    }


    void Update()
    {
        Tick();
        Move(); 
    }


    private void Tick() {
        decisionClock++;

        if (decisionClock > decisionTime) {
            Decide();
        }
    }


    private void Move() {
        rigidBody.MovePosition(xform.position + Time.deltaTime * speed * dir);
    }


    private void Decide() {
        decisionClock = 0;

        UpdateDirection();
        UpdateAnimation();
    }


    private void UpdateDirection() {
        System.Random ran = new System.Random();

        dir = new Vector3(ran.Next(-1, 2), ran.Next(-1, 2), 0);
        dir = ToIso(dir.normalized);
    }


    private Vector3 ToIso(Vector3 cartesianVec) {
        return new Vector3(
            cartesianVec.x - cartesianVec.y, 
            (cartesianVec.x + cartesianVec.y) / 2,
            0
        ); 
    }


    private void UpdateAnimation() {
        if (dir.x == 0 && dir.y == 0) {
            animator.setActive(false);
        } else if (dir.x > 0 && dir.y > 0) {
            animator.PlayAnimation(Direction.NE);
        } else if (dir.x > 0 && dir.y < 0) {
            animator.PlayAnimation(Direction.SE);
        } else if (dir.x < 0 && dir.y > 0) {
            animator.PlayAnimation(Direction.NW);
        } else if (dir.x < 0 && dir.y < 0) {
            animator.PlayAnimation(Direction.SW);
        } else if (dir.x == 0 && dir.y > 0) {
            animator.PlayAnimation(Direction.NN);
        } else if (dir.x == 0 && dir.y < 0) {
            animator.PlayAnimation(Direction.SS);
        } else if (dir.x > 0 && dir.y == 0) {
            animator.PlayAnimation(Direction.EE);
        } else if (dir.x < 0 && dir.y == 0) {
            animator.PlayAnimation(Direction.WW);
        } else if (dir.x == 0 && dir.y == 0) {
        }
    }

}
