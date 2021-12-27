using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewInputManager : MonoBehaviour
{
	[Header("Controllers Settings")]
	public INPUTTYPE InputType;													//тип ввода
	public List<InputControl> KeyboardControls = new List<InputControl>();      //массив кнопок клавиатуры
	public List<InputControl> JoystickControls = new List<InputControl>();        //массив кнопок джойстика
	public int JoystickDeviceNumber = 0;
	//public string GamepadHorizontalAxis = "Horizontal";
	//public string GamepadVerticalAxis = "Vertical";

	[Header("Double Tap Settings")]
	public float doubleTapSpeed = 0.3f;
	private float lastInputTime = 0f;
	private string lastInputAction = "";

	//Делегаты
	public delegate void DirectionInputEventHandler(Vector2 dir, bool doubleTapActive);
	public static event DirectionInputEventHandler onDirectionInputEvent;
	public delegate void InputEventHandler(string action, BUTTONSTATE buttonState);
	public static event InputEventHandler onInputEvent;

	public static bool defendKeyDown;
	private float doubleTapTime;

    [Header("Other Settings")]
    public GameObject InputSettingsPrefab;
    private GameObject inputSettingsPanel;

    public bool AllKeysDefined = false;

	
	

    void Start()
    {

		//если кнопки не назначены
		if (!AllKeysDefined)
        {
            //создаем панель настройки
            inputSettingsPanel = Instantiate(InputSettingsPrefab, GameObject.Find("Canvas").transform);
            inputSettingsPanel.GetComponent<InputSettings>().InputManager = this;

        }
    }

	void Update()
	{
		//выбор обработки ввода
		if (InputType == INPUTTYPE.KEYBOARD) UpdateKeyboardControls();
		if (InputType == INPUTTYPE.JOYSTICK) UpdateJoystickControls();
	}

	public static void DirectionEvent(Vector2 dir, bool doubleTapActive)
	{
		if (onDirectionInputEvent != null) onDirectionInputEvent(dir, doubleTapActive);
	}

	void UpdateKeyboardControls()
	{
		float x = 0;
		float y = 0;
		bool doubleTapState = false;

		//Перебор массива кнопок
		foreach (InputControl inputControl in KeyboardControls)
		{
			//если никто не подписан на событие - выход
			if (onInputEvent == null) return;

			//если кнопка нажата
			if (Input.GetKeyDown(inputControl.Key))
			{
				doubleTapState = DetectDoubleTap(inputControl.Action);
				onInputEvent(inputControl.Action, BUTTONSTATE.PRESS);

				Debug.Log(inputControl.Action + BUTTONSTATE.PRESS);
			}

			//если кнопка отжата
			if (Input.GetKeyUp(inputControl.Key))
			{
				onInputEvent(inputControl.Action, BUTTONSTATE.RELEASE);
			}

			//convert keyboard direction keys to x,y values (every frame)
			if (Input.GetKey(inputControl.Key))
			{
				if (inputControl.Action == "Left") x = -1f;
				else if (inputControl.Action == "Right") x = 1f;
				else if (inputControl.Action == "Up") y = 1f;
				else if (inputControl.Action == "Down") y = -1f;
			}

			//defend key exception (checks the defend state every frame)
			if (inputControl.Action == "Defend") defendKeyDown = Input.GetKey(inputControl.Key);
		}

		//send a direction event
		DirectionEvent(new Vector2(x, y), doubleTapState);
		//if (onDirectionInputEvent != null) onDirectionInputEvent(new Vector2(x, y), doubleTapState);
	}

	void UpdateJoystickControls()
	{
		if (onInputEvent == null) return;

		//on Joypad button press
		foreach (InputControl inputControl in JoystickControls)
		{
			if (Input.GetKeyDown(inputControl.Key)) onInputEvent(inputControl.Action, BUTTONSTATE.PRESS);

			//defend key exception (checks the defend state every frame)
			if (inputControl.Action == "Defend") defendKeyDown = Input.GetKey(inputControl.Key);
		}

		//get Joypad  direction axis
		float x = Input.GetAxis("Joypad Left-Right");
		float y = Input.GetAxis("Joypad Up-Down");

		//send a direction event
		DirectionEvent(new Vector2(x, y).normalized, false);
	}

	//returns true if a key double tap is detected
	bool DetectDoubleTap(string action)
	{
		bool doubleTapDetected = ((Time.time - lastInputTime < doubleTapSpeed) && (lastInputAction == action));
		lastInputAction = action;
		lastInputTime = Time.time;
		return doubleTapDetected;
	}
}


//---------------
//    ENUMS
//---------------
public enum INPUTTYPE
{
	KEYBOARD = 0,
	JOYSTICK = 5,
	TOUCHSCREEN = 10,		//удалить
}

public enum BUTTONSTATE
{
	PRESS = 0,
	RELEASE = 5,
	HOLD = 10,				//реализовать
}


//---------------
//    CLASSES
//---------------
[System.Serializable]
public class InputControl
{
	public string Action;
	public KeyCode Key;
}

public class Joystick
{
	public string Name;
	public int DeviceNumber;

	public static Joystick[] GetJoysticks()
	{
		string[] joysticskNames = Input.GetJoystickNames();
		Joystick[] joysticks = new Joystick[joysticskNames.Length];
		int joysticksCount = 0;

		for (int i = 0; i < joysticskNames.Length; i++)
		{
			if (!string.IsNullOrEmpty(joysticskNames[i]))	//IsNullOrWhiteSpace
			{
				joysticks[joysticksCount] = new Joystick();
				joysticks[joysticksCount].Name = joysticskNames[i];
				joysticks[joysticksCount].DeviceNumber = i;

				joysticksCount += 1;
			}	
		}

		Debug.Log("Connected Joysticks: " + joysticksCount);

		if (joysticksCount > 0)
		{
			System.Array.Resize(ref joysticks, joysticksCount);
			return joysticks;
		}
		else return null;
	}
}