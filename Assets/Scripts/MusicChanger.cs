using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MusicChanger : MonoBehaviour
{
    [SerializeField] private Adrenaline player;

    [SerializeField] AudioSource track1;
    [SerializeField] AudioSource track2;
    [SerializeField] AudioSource track3;
    [SerializeField] AudioSource track4;

    private void Start()
    {
        player = FindFirstObjectByType<Adrenaline>();
        if (track1 != null) player.track1 = track1; else player.track1 = null;
        if (track2 != null) player.track2 = track2; else player.track2 = null;
        if (track3 != null) player.track3 = track3; else player.track3 = null;
        if (track4 != null) player.track4 = track4; else player.track4 = null;

    }

    private void Update()
    {
        if (track1 != null) track1.volume = player.volume;
        if (track2 != null) track2.volume = Mathf.InverseLerp(10f, 30f, player.currentAdrenaline) * player.volume;
        if (track3 != null) track3.volume = Mathf.InverseLerp(30f, 60f, player.currentAdrenaline) * player.volume;
        if (track4 != null) track4.volume = Mathf.InverseLerp(60f, 90f, player.currentAdrenaline) * player.volume;
    }
}
