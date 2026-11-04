using Enigma.Interaction;
using UnityEngine;

namespace Enigma.Environment
{
    // Animación de intento: qué se mueve cuando un intento falla, grabado en la escena
    public partial class KitbashDressing
    {
        private const float DoorKnock = 1.2f;
        private const float PadlockShake = 10f;

        private void DressAttempts()
        {
            if (Application.isPlaying)
                return;

            foreach (var name in new[] { "Interactable_Door", "Interactable_DoorN3", "Interactable_OrigenDoor" })
            {
                var go = FindLocal(name);
                var door = go != null ? go.GetComponent<ConditionalDoorInteractable>() : null;
                if (door == null || door.DoorPivot == null)
                    continue;
                ClearAttempts(go);
                var pivot = door.DoorPivot;
                if (name == "Interactable_OrigenDoor")
                {
                    Vector3 slide = pivot.parent != null ? pivot.parent.TransformDirection(Vector3.right) : Vector3.right;
                    AddAttempt(go, pivot, AttemptMotion.Kind.Push, slide * Mathf.Sign(door.SlideX), 0.012f, 0.4f);
                    var padlock = FindLocal("OrigenPadlock");
                    if (padlock != null)
                        AddAttempt(go, padlock.transform, AttemptMotion.Kind.Shake, Vector3.forward, PadlockShake, 0.45f);
                }
                else
                    AddAttempt(go, pivot, AttemptMotion.Kind.Knock, Vector3.up, DoorKnock * Mathf.Sign(door.OpenYaw), 0.4f);
            }

            var lockGo = FindLocal("Interactable_Lock");
            var lockerPadlock = FindLocal("LockerPadlock");
            if (lockGo != null && lockerPadlock != null)
            {
                ClearAttempts(lockGo);
                AddAttempt(lockGo, lockerPadlock.transform, AttemptMotion.Kind.Shake, Vector3.forward, PadlockShake, 0.45f);
            }

            var hatchGo = FindLocal("Interactable_Hatch");
            var hatch = hatchGo != null ? hatchGo.GetComponent<ArrowLockInteractable>() : null;
            if (hatch != null && hatch.HatchPivot != null)
            {
                ClearAttempts(hatchGo);
                AddAttempt(hatchGo, hatch.HatchPivot, AttemptMotion.Kind.Knock, Vector3.right, 1.5f * Mathf.Sign(hatch.OpenPitch), 0.4f);
            }

            var readerGo = FindLocal("Interactable_CardReader");
            var led = readerGo != null ? FindNamed(readerGo.transform, "Kit_ReaderLed") : null;
            if (led != null)
            {
                ClearAttempts(readerGo);
                AddAttempt(readerGo, led, AttemptMotion.Kind.Blink, Vector3.zero, 2f, 0.5f);
            }

            // El cuadro no falla nunca: su collider se prende recién cuando el imán corrió las chapas
            var paint = FindLocal("Interactable_Painting");
            if (paint != null)
                ClearAttempts(paint);

            var lockerGo = FindLocal("Interactable_Locker");
            var lockerDoor = FindLocal("Kit_TapeLockerDoor");
            var hinge = lockerDoor != null ? lockerDoor.GetComponent<FlagHinge>() : null;
            if (lockerGo != null && hinge != null)
            {
                ClearAttempts(lockerGo);
                AddAttempt(lockerGo, lockerDoor.transform, AttemptMotion.Kind.Knock, Vector3.up, Mathf.Sign(hinge.OpenAngle), 0.45f);
            }

            for (int i = 0; i < 4; i++)
            {
                var panel = FindLocal("Interactable_Panel_" + i);
                var holder = panel != null ? FindNamed(panel.transform, "Kit_FloorButton") : null;
                var button = holder != null ? FindNamed(holder, "button") : null;
                if (button == null)
                    continue;
                ClearAttempts(panel);
                AddAttempt(panel, button, AttemptMotion.Kind.Push, Vector3.down, 0.012f, 0.3f);
            }
        }

        private static void ClearAttempts(GameObject host)
        {
            foreach (var m in host.GetComponents<AttemptMotion>())
                Object.DestroyImmediate(m);
        }

        private static void AddAttempt(GameObject host, Transform target, AttemptMotion.Kind kind, Vector3 axis, float amount, float seconds)
        {
            host.AddComponent<AttemptMotion>().Configure(target, kind, axis, amount, seconds);
        }
    }
}
