using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    [SerializeField] private GameObject Player;
    [SerializeField]  private GameObject Editor;

    private bool modeFlip;

    // Start is called before the first frame update
    void Start()
    {
        modeFlip = true;
        Player.SetActive(modeFlip);
        Editor.SetActive(!modeFlip);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Jump"))
        {
            modeFlip = !modeFlip;
            Player.SetActive(modeFlip);
            Editor.SetActive(!modeFlip);
        }
    }
}
