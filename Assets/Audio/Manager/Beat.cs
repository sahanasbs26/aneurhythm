using System;
using UnityEngine;


[Serializable]
public class Beat : MonoBehaviour
{
    public BeatData data;
    public SongManager songManager;
    public BeatManager beatManager;

    void Update()
    {
        Vector3 currentPos = GetComponent<RectTransform>().anchoredPosition;
        float x = data.beatsUntilPlay * beatManager.beatSpacing * (data.isPlayer2 ? -1 : 1) + beatManager.screenHalfWidth;
        currentPos = new Vector3(x, currentPos.y, currentPos.z);
    }        
}
