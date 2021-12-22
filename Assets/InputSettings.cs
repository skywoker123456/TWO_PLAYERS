using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputSettings : MonoBehaviour
{
    public Text[] Texts;

    public NewInputManager InputManager;

    public KeyCode[] AllKeyCodes;

    private int keyNumber = 0;


    void Start()
    {
        for (int i = 0; i < Texts.Length; i++)
        {
            Texts[i].text = InputManager.KeyboardControls[i].Action;
        }

        AllKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
    }

    void Update()
    {
        if (!InputManager.AllKeysDefined)
        {
            Texts[keyNumber].text = InputManager.KeyboardControls[keyNumber].Action + "\t\t\tPress Button";

            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in AllKeyCodes)
                {
                    if (Input.GetKey(keyCode))
                    {
                        Debug.Log("KeyCode down: " + keyCode);

                        InputManager.KeyboardControls[keyNumber].Key = keyCode;
                        Texts[keyNumber].text = InputManager.KeyboardControls[keyNumber].Action + "\t\t\t" + InputManager.KeyboardControls[keyNumber].Key.ToString();

                        keyNumber += 1;

                        if (keyNumber == Texts.Length) InputManager.AllKeysDefined = true;
                    }
                }
            }
        }
        else Destroy(gameObject);               
    }
}
