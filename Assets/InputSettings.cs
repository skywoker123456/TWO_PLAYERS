using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputSettings : MonoBehaviour
{
    [Header("Main Panel")]
    public Text[] Texts;

    public NewInputManager InputManager;                    //ссылка на настраиваемый InputManager

    public KeyCode[] AllKeyCodes;                           //массив всех кейкодов
    public KeyCode[] JoystickKeyCodes = new KeyCode[20];    //массив
    public KeyCode[] Joystick1KeyCodes = new KeyCode[20];   //массив
    public KeyCode[] Joystick2KeyCodes = new KeyCode[20];   //массив
    public KeyCode[] Joystick3KeyCodes = new KeyCode[20];   //массив
    public KeyCode[] Joystick4KeyCodes = new KeyCode[20];   //массив
    public KeyCode[] Joystick5KeyCodes = new KeyCode[20];   //массив
    public KeyCode[] Joystick6KeyCodes = new KeyCode[20];   //массив
    public KeyCode[] Joystick7KeyCodes = new KeyCode[20];   //массив
    public KeyCode[] Joystick8KeyCodes = new KeyCode[20];   //массив


    private int keyCounter = 0;

    [Header("Info Panel")]
    public GameObject InfoPanel;
    private Text controllersList;


    private void Awake()
    {
        controllersList = InfoPanel.transform.Find("ControllersList").GetComponent<Text>();
    }

    void Start()
    {
        //получение массивов кейкодов
        AllKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
        System.Array.Copy(AllKeyCodes, 146, JoystickKeyCodes, 0, 20);
        System.Array.Copy(AllKeyCodes, 166, Joystick1KeyCodes, 0, 20);
        System.Array.Copy(AllKeyCodes, 186, Joystick2KeyCodes, 0, 20);
        System.Array.Copy(AllKeyCodes, 206, Joystick3KeyCodes, 0, 20);
        System.Array.Copy(AllKeyCodes, 226, Joystick4KeyCodes, 0, 20);
        System.Array.Copy(AllKeyCodes, 246, Joystick5KeyCodes, 0, 20);
        System.Array.Copy(AllKeyCodes, 266, Joystick6KeyCodes, 0, 20);
        System.Array.Copy(AllKeyCodes, 286, Joystick7KeyCodes, 0, 20);
        System.Array.Copy(AllKeyCodes, 306, Joystick8KeyCodes, 0, 20);


        for (int i = 0; i < Texts.Length; i++)
        {
            Texts[i].text = InputManager.KeyboardControls[i].Action;
        }


        UpdateInfoPanel();
    }

    void Update()
    {
        if (!InputManager.AllKeysDefined)
        {
            Texts[keyCounter].text = InputManager.KeyboardControls[keyCounter].Action + "\t\t\tPress Button";

            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in AllKeyCodes)    //AllKeyCodes
                {
                    if (Input.GetKey(keyCode))
                    {
                        Debug.Log("KeyCode down: " + keyCode);

                        InputManager.KeyboardControls[keyCounter].Key = keyCode;
                        Texts[keyCounter].text = InputManager.KeyboardControls[keyCounter].Action + "\t\t\t" + InputManager.KeyboardControls[keyCounter].Key.ToString();

                        keyCounter += 1;

                        if (keyCounter == Texts.Length) InputManager.AllKeysDefined = true;
                    }
                }
            }
        }
        else Destroy(gameObject);

        UpdateInfoPanel();

        //JoystickHorizontal
        //Debug.Log(Input.GetAxis("JoystickHorizontal"));
        Joystick.GetJoysticks();

    }

    void UpdateInfoPanel()
    {
        controllersList.text = "";
        for (int i = 0; i < Input.GetJoystickNames().Length; i++)
        {
            if (Input.GetJoystickNames()[i] == "")  controllersList.text += "Joystick" + i + ": NULL" + "\n";
            else                                    controllersList.text += "Joystick" + i + ": " + Input.GetJoystickNames()[i] + "\n";
        }
    }
}
