/*
using UnityEngine;

public class ControladorEscena : MonoBehaviour
{
    public static ControladorEscena Instance;

    public GameObject radio;

    public AudioSource rAudio;

    public AudioClip[] s_Estaciones;

    public static int frecuencia;

    public static bool pRadio;

    public static int[] n_Boton;

    private void Awake()
    {
        Instance = this;
        n_Boton = new int[5];
        n_Boton[1] = 0;
        n_Boton[2] = 0;
        n_Boton[3] = 0;
        n_Boton[4] = 0;
    }

    private void Update()
    {
        if (pRadio)
        {
            rAudio.enabled = false;
        }
        else
        {
            rAudio.enabled = true;
        }
        MonoBehaviour.print(n_Boton[1]);
        if (n_Boton[1] == 5 && n_Boton[2] == 6 && n_Boton[3] == 3 && n_Boton[4] == 9)
        {
            GameObject gameObject = base.gameObject;
            MonoBehaviour.print("ganaste");
            gameObject.GetComponent<Final>().EndGame();
        }
    }

    public static void SetRadio()
    {
        if (pRadio)
        {
            pRadio = false;
        }
        else
        {
            pRadio = true;
        }
    }

    public void EstablecerFrecuencia()
    {
        rAudio.clip = s_Estaciones[frecuencia];
        rAudio.Play();
    }

    public static void ElegirFrecuencia(bool b)
    {
        if (b)
        {
            frecuencia++;
        }
        else if (!b && frecuencia > 1)
        {
            frecuencia--;
        }
        MonoBehaviour.print(frecuencia);
        Instance.EstablecerFrecuencia();
    }

    public static void SetMaletin(int boton, int n)
    {
        n_Boton[boton] = n;
    }

    public void EndGame()
    {
    }
}
*/