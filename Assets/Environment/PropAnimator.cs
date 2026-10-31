using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Enigma.Environment
{
    // Clip corto en un Animator del pivot (puerta / palanca / escotilla).
    public static class PropAnimator
    {
        public static IEnumerator Play(Transform pivot, Quaternion endRotation, float duration)
        {
            if (pivot == null)
                yield break;

            var anim = pivot.GetComponent<Animator>();
            if (anim == null)
                anim = pivot.gameObject.AddComponent<Animator>();
            anim.applyRootMotion = false;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            float dur = Mathf.Max(0.05f, duration);
            Vector3 from = pivot.localEulerAngles;
            Vector3 target = endRotation.eulerAngles;
            float toX = from.x + Mathf.DeltaAngle(from.x, target.x);
            float toY = from.y + Mathf.DeltaAngle(from.y, target.y);
            float toZ = from.z + Mathf.DeltaAngle(from.z, target.z);

            var clip = new AnimationClip { name = "PropMove", legacy = false };
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.x", AnimationCurve.Linear(0f, from.x, dur, toX));
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.y", AnimationCurve.Linear(0f, from.y, dur, toY));
            clip.SetCurve("", typeof(Transform), "localEulerAnglesRaw.z", AnimationCurve.Linear(0f, from.z, dur, toZ));

            var graph = PlayableGraph.Create("PropMove");
            try
            {
                graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
                var output = AnimationPlayableOutput.Create(graph, "Prop", anim);
                var playable = AnimationClipPlayable.Create(graph, clip);
                playable.SetDuration(dur);
                output.SetSourcePlayable(playable);
                graph.Play();

                float t = 0f;
                while (t < dur)
                {
                    t += Time.deltaTime;
                    yield return null;
                }

                pivot.localRotation = endRotation;
            }
            finally
            {
                if (graph.IsValid())
                    graph.Destroy();
            }
        }

        // Corrediza: interpola posición local (ORIGEN).
        public static IEnumerator PlaySlide(Transform pivot, Vector3 endLocalPos, float duration)
        {
            if (pivot == null)
                yield break;

            Vector3 from = pivot.localPosition;
            float dur = Mathf.Max(0.05f, duration);
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                k = k * k * (3f - 2f * k);
                pivot.localPosition = Vector3.Lerp(from, endLocalPos, k);
                yield return null;
            }
            pivot.localPosition = endLocalPos;
        }
    }
}
