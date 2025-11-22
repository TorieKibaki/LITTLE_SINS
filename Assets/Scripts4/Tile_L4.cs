using UnityEngine;

public class Tile_L4 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;

    private Vector3 startPosition;
    private bool isMoving = true;

    void Start()
    {
        // Remember where we started so we can respawn here
        startPosition = transform.position;
    }

    void Update()
    {
        if (isMoving)
        {
            // Move constantly to the right
            // Since we are NOT parenting the player, the platform will slide
            // out from under them if they don't move.
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
    }

    // Called by GameManager when player respawns
    public void ResetTile()
    {
        transform.position = startPosition;
        isMoving = true;
    }
}