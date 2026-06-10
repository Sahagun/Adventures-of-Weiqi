using UnityEngine;

/// <summary>
/// Records that the player has entered the Training Hub, so the New Lobby can play the
/// "after Training Hub" conversation on their next visit. Drop this on any object in
/// the Training Hub scene.
/// </summary>
public class TrainingHubVisitMarker : MonoBehaviour
{
    private void Start()
    {
        StoryFlags.TrainingHubVisited = true;
    }
}
