using UnityEngine;

public class VirtualKeyboard : MonoBehaviour
{
    public void OnKeyPress(string key)
    {
        switch (key)
        {
            case "BACKSPACE":
                InputSimulator.SimulateKeyDown(KeyCode.Backspace);
                break;
            case "ENTER":
                InputSimulator.SimulateKeyDown(KeyCode.Return);
                break;
            default:
                if (key.Length == 1)
                {
                    KeyCode code;
                    if (key == "Ñ")
                        code = KeyCode.None; // opción 1: manejar 'Ñ' por separado si lo usas como texto
                    else
                        code = (KeyCode)System.Enum.Parse(typeof(KeyCode), key.ToUpper());

                    InputSimulator.SimulateKeyDown(code);
                }
                break;
        }
    }
}
