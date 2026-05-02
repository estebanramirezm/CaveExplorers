using System.Collections;
using UnityEngine;

/// <summary>
/// Spawned by FireflySwarm when the player presses the scout key with no GrappleBat nearby.
/// Flies forward in the player's facing direction to ScoutRange, then flies back and returns
/// the firefly to the swarm.
/// </summary>
public class ScoutFirefly : MonoBehaviour
{
    public float MoveSpeed  = 5f;
    public float ScoutRange = 8f;

    private Transform player;
    private FireflySwarm swarm;

    public void Launch(Transform playerTransform, FireflySwarm fireflySwarm)
    {
        player = playerTransform;
        swarm  = fireflySwarm;

        float facingDir = Mathf.Sign(playerTransform.localScale.x);
        Vector3 target  = playerTransform.position + new Vector3(facingDir * ScoutRange, 0f, 0f);

        StartCoroutine(ScoutRoutine(target));
    }

    private IEnumerator ScoutRoutine(Vector3 target)
    {
        // Fly out
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, MoveSpeed * Time.deltaTime);
            yield return null;
        }

        // Brief pause at destination
        yield return new WaitForSeconds(0.5f);

        // Fly back
        while (player != null && Vector3.Distance(transform.position, player.position) > 0.3f)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, MoveSpeed * 1.5f * Time.deltaTime);
            yield return null;
        }

        swarm?.AddFireflyVisualOnly(FireflyType.White);
        Destroy(gameObject);
    }
}
