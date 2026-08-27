/*
using UnityEngine;
using UnityEngine.UI;

public class Maletin : MonoBehaviour
{
    public Button boton;

    public int num;

    public Text text;

    private void Start()
    {
        boton.onClick.AddListener(TaskOnClick);
    }

    private void TaskOnClick()
    {
        if (this.num != 9)
        {
            this.num++;
        }
        else
        {
            this.num = 0;
        }
        string text = this.num.ToString();
        this.text.text = text;
        int num = 100;
        if (base.gameObject.name == "Bot1")
        {
            num = 1;
        }
        if (base.gameObject.name == "Bot2")
        {
            num = 2;
        }
        if (base.gameObject.name == "Bot3")
        {
            num = 3;
        }
        if (base.gameObject.name == "Bot4")
        {
            num = 4;
        }
        ControladorEscena.SetMaletin(num, this.num);
    }

    private void Update()
    {
    }
}
*/