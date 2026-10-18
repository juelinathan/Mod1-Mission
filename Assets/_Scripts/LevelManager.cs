using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runs the first-playable loop: builds a level, counts shots, cleans up the
/// previous level, and advances when a Goal is hit.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private const int LevelCount = 3;
    private GameObject castleTemplate;
    private GameObject activeCastle;
    private int currentLevel;
    private int shots;
    private bool changingLevel;
    private string status = "Drag the slingshot back, then release.";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateManager()
    {
        if (FindFirstObjectByType<LevelManager>() == null)
        {
            new GameObject("Level Manager").AddComponent<LevelManager>();
        }
    }

    private void Awake()
    {
        Instance = this;
        castleTemplate = GameObject.Find("Castle");
        if (castleTemplate == null)
        {
            Debug.LogError("LevelManager could not find the Castle scene object.");
            enabled = false;
            return;
        }

        castleTemplate.name = "Castle Template";
        castleTemplate.SetActive(false);
        LoadLevel(0);
    }

    public void RegisterShot()
    {
        if (changingLevel) return;
        shots++;
        status = "Shot " + shots + " — knock down the green goal!";
    }

    public void GoalReached()
    {
        if (!changingLevel) StartCoroutine(AdvanceAfterDelay());
    }

    private IEnumerator AdvanceAfterDelay()
    {
        changingLevel = true;
        status = "Level complete!";
        yield return new WaitForSeconds(1.5f);

        if (currentLevel + 1 < LevelCount)
        {
            LoadLevel(currentLevel + 1);
        }
        else
        {
            status = "You cleared all three castles in " + shots + " shots!";
            changingLevel = false;
        }
    }

    private void LoadLevel(int level)
    {
        CleanupLevelObjects();
        currentLevel = level;
        Goal.goalMet = false;
        FollowCam.POI = null;

        activeCastle = Instantiate(castleTemplate);
        activeCastle.name = "Castle Level " + (level + 1);
        activeCastle.SetActive(true);

        // The supplied castle is the level building block. Each level changes
        // its location, scale, and orientation to create a distinct challenge.
        Vector3[] positions = { new Vector3(43.98f, -9.5f, 0f), new Vector3(55f, -9.5f, 0f), new Vector3(66f, -9.5f, 0f) };
        Vector3[] scales = { Vector3.one, new Vector3(1.15f, 1.15f, 1.15f), new Vector3(1.3f, 1.3f, 1.3f) };
        float[] rotations = { 0f, 8f, -10f };
        activeCastle.transform.position = positions[level];
        activeCastle.transform.localScale = scales[level];
        activeCastle.transform.rotation = Quaternion.Euler(0f, rotations[level], 0f);

        status = "Level " + (level + 1) + " of " + LevelCount + " — aim for the goal.";
        changingLevel = false;
    }

    private void CleanupLevelObjects()
    {
        if (activeCastle != null) Destroy(activeCastle);

        // Projectile trails are children of projectiles, so destroying each
        // projectile removes both the ball and its trajectory line.
        foreach (Projectile projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
        {
            Destroy(projectile.gameObject);
        }
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(16, 16, 330, 94), "Mission Demolition");
        GUI.Label(new Rect(30, 43, 290, 24), "Level: " + (currentLevel + 1) + " / " + LevelCount + "    Shots: " + shots);
        GUI.Label(new Rect(30, 67, 300, 24), status);
        if (GUI.Button(new Rect(260, 84, 72, 20), "Restart"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
