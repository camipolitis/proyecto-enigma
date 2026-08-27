/*
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Personaje : MonoBehaviour
{
    public bool interaccion;

    public bool interactuando;

    private int n_inter;

    public Canvas[] canvas;

    public Camera[] Cameras;

    public GameObject c_Radio;

    public Animation a_Radio;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && interaccion)
        {
            if (interactuando)
            {
                Cameras[0].enabled = true;
                Cameras[n_inter].enabled = false;
                interactuando = false;
                canvas[6].enabled = false;
                GetComponent<FirstPersonController>().enabled = true;
                if (n_inter == 6)
                {
                    GetComponent<FirstPersonController>().m_MouseLook.lockCursor = true;
                    Cursor.lockState = CursorLockMode.Confined;
                }
                return;
            }
            GetComponent<FirstPersonController>().enabled = false;
            Cameras[0].enabled = false;
            Cameras[n_inter].enabled = true;
            interactuando = true;
            if (n_inter == 6)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                GetComponent<FirstPersonController>().m_MouseLook.lockCursor = false;
                canvas[6].enabled = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.T) && n_inter == 1 && ControladorEscena.frecuencia < 7)
        {
            MonoBehaviour.print("suma positva");
            ControladorEscena.ElegirFrecuencia(b: true);
        }
        else if (Input.GetKeyDown(KeyCode.R) && n_inter == 1)
        {
            MonoBehaviour.print("suma neg");
            ControladorEscena.ElegirFrecuencia(b: false);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.name == "Radio")
        {
            n_inter = 1;
            canvas[n_inter].enabled = true;
        }
        if (col.gameObject.name == "PapelCancion")
        {
            n_inter = 2;
        }
        if (col.gameObject.name == "Cuadro")
        {
            n_inter = 3;
        }
        if (col.gameObject.name == "Maletin")
        {
            n_inter = 6;
        }
        interaccion = true;
    }

    private void OnTriggerExit(Collider col)
    {
        interaccion = false;
        canvas[n_inter].enabled = false;
        n_inter = 0;
    }
}
*/