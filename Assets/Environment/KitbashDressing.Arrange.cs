using UnityEngine;

namespace Enigma.Environment
{
    // Orden de N1 y N2 por zonas: los grises se ubican antes de que nazcan sus modelos
    public partial class KitbashDressing
    {
        private void ArrangeRooms(string sceneName)
        {
            if (Application.isPlaying)
                return;
            if (sceneName == "Level01_Room")
            {
                // Zona médica en el centro: la camilla en diagonal
                Spot("Decor_Stretcher", -2.5f, -1f, 35f);
                // Trabajo: se sienta del lado del cajón y mira al monitor
                Spot("Decor_Chair", 6.25f, 2.75f, 270f);
                Spot("Interactable_Note", 5.62f, 2.05f, 0f);
                Spot("DecorBrokenPC", 5.26f, 2.2f, 0f);
                // Almacenaje: cajas en fila contra la pared sur, junto a la estantería
                Spot("Crate_R0", 6.35f, -9.5f, 8f);
                Spot("Crate_R1", 7.0f, -9.45f, -14f);
                Spot("Crate_R3", 7.65f, -9.5f, 21f);
                Spot("Crate_R2", 8.9f, -4.6f, 33f);
                // Escombros del medio: al rincón de chatarra o junto al estante caído
                Spot("Debris_R0", -8.6f, 5.3f, 12f);
                Spot("Debris_R2", -5.5f, 6.8f, 74f);
                Spot("Debris_R8", -8.9f, 6.4f, 296f);
                Spot("Debris_R7", -4.8f, 9.1f, 259f);
                Spot("Debris_R3", 5.4f, -6.1f, 111f);
                Spot("Debris_R5", 3.8f, -6.3f, 185f);
                Spot("Debris_R6", 5.9f, -3.6f, 222f);
            }
            else if (sceneName == "Level02_Lab")
            {
                Spot("Chair", -3.0f, 0.95f, 0f);
                // Almacenaje noroeste
                Spot("Crate_L2", -6.4f, 3.15f, 10f);
                Spot("Debris_B", -5.4f, 4.9f, 0f);
                // Almacenaje sureste
                Spot("Crate_L1", 3.6f, -5.25f, 28f);
                Spot("Crate_A", 4.6f, -5.3f, -6f);
                Spot("Crate_L3", 5.5f, -5.25f, 70f);
                // La llegada queda libre
                Spot("Debris_L6", 2.3f, -5.0f, 222f);
            }
            else if (sceneName == "Level04_Origen")
            {
                // El casillero se arrima solo a la pared sur y abre hacia la galería, no al foso
                Spot("Interactable_Locker", 5.8f, -5.5f, 0f);
            }
        }

        // Planta y giro absolutos; la altura queda como está
        private void Spot(string name, float x, float z, float yaw)
        {
            var go = FindLocal(name);
            if (go == null)
                return;
            var t = go.transform;
            t.position = new Vector3(x, t.position.y, z);
            var e = t.eulerAngles;
            t.rotation = Quaternion.Euler(e.x, yaw, e.z);
        }
    }
}
