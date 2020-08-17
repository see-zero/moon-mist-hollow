using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private int decisionClock = 0;
    private int decisionTime = 260;

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
        decisionClock++;

        if (decisionClock > decisionTime) {
            decisionClock = 0;

            direction = GetNewDirection();
            animator.SetAnimation(direction);
        }
    }


    private void Move() {
        rigidBody.velocity = Time.deltaTime * speed * direction;
    }


    private Vector2 GetNewDirection() {
        System.Random random = new System.Random();

        Vector3 direction = new Vector3(random.Next(-1, 2), random.Next(-1, 2), 0);
        direction = ToIso(dir.normalized);

        return direction;
    }


    private Vector3 ToIso(Vector3 cartesianVec) {
        return new Vector3(
            cartesianVec.x - cartesianVec.y, 
            (cartesianVec.x + cartesianVec.y) / 2,
            0
        ); 
    }
}
