using UnityEngine;
using System.Collections;

public class CharacterMovement : MonoBehaviour
{
    private float decisionClock = 0;
    private float decisionTime = 3.6f;

    public float speed = 2.4f;
    private Vector2 direction = Vector2.zero;

    private Rigidbody2D rigidBody;
    private CharacterAnimator animator;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<CharacterAnimator>();
    }


    void Update()
    {
        Tick();
        Move(); 
    }


    private void Tick() {
        decisionClock += Time.deltaTime;

        if (decisionClock > decisionTime) {
            decisionClock -= decisionTime;

            direction = GetNewDirection();
            animator.SetAnimation(direction);
        }
    }


    private void Move() {
        rigidBody.velocity = Time.deltaTime * speed * direction;
    }


    private Vector2 GetNewDirection() {
        Vector3 newDirection = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
        newDirection = ToIso(newDirection.normalized);

        return newDirection;
    }


    private Vector2 ToIso(Vector2 cartesianVec) {
        return new Vector2(
            cartesianVec.x - cartesianVec.y,
            (cartesianVec.x + cartesianVec.y) / 2
        ); 
    }
}
