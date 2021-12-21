using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSettings : MonoBehaviour
{
    public KeyCode Left;
    public KeyCode Right;
    public KeyCode Up;
    public KeyCode Down;
    public KeyCode Punch;
    public KeyCode Kick;
    public KeyCode Defend;
    public KeyCode Jump;

    public bool AllKeysDefined = false;

    public KeyCode[] AllKeyCodes;

    public NewInputManager InputManager;


    void Start()
    {
        AllKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));

    }

    void Update()
    {
        
        
        if (!AllKeysDefined)
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in AllKeyCodes)
                {
                    if (Input.GetKey(keyCode))
                    {
                        Left = keyCode;

                        Debug.Log("KeyCode down: " + keyCode);
                    }
                }
            }
        }
        
                
    }

}
