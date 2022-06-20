using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class SpectatorSystem : NetworkBehaviour
{
    [Header("Spectator camera")]
    public Camera specCam;

    [Header("Screen die animator")]
    [SerializeField] private Animator screenDieAnimator;

    private static List<Camera> specCameras = new List<Camera>();
    private int index = 0;

    private void Update()
    {
        if (hasAuthority)
        {
            if (!PlayerHpManager.isDead || GamePlayer.escaped)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    ChangeCameraView();
                    ChangeFadeScreen();
                }
            }
        }
    }

    public void RemoveCamFromList(string camName)
    {
        var cam = specCameras.Find(c => c.name == camName);

        if (cam != null)
        {
            if (cam.depth == 2)
            {
                ChangeCameraView();
            }
            specCameras.Remove(cam);
        }
    } 

    public void SetSpecCameras()
    {
        specCameras.AddRange(PlayerSpawnSystem.playersSpecCameras.Where(c => c.name != specCam.name));
    }

    private void ChangeCameraView()
    {
        if (index >= specCameras.Count - 1)
            index = 0;

        if (index == 0)
        {
            specCameras[specCameras.Count - 1].depth = 1;
            ChangeAudioListener(specCameras.Count - 1, false);

            ChangeAudioListener(index, true);
            specCameras[index].depth = 2;
        }
        else
        {
            specCameras[index - 1].depth = 1;
            ChangeAudioListener(index - 1, false);

            ChangeAudioListener(index, true);
            specCameras[index].depth = 2;
        }
        index++;
    }

    private void ChangeAudioListener(int index, bool state)
    {
        specCameras[index].transform.parent.GetComponent<AudioListener>().gameObject.SetActive(state);
    }
    private void ChangeFadeScreen()
    {
        screenDieAnimator.SetTrigger("ChangeSpec");
    }

}
