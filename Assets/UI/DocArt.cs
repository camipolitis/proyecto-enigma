using UnityEngine;

namespace Enigma.UI
{
    // Arte de papel generado por código: tinta, quemaduras, clip y humo.
    public static class DocArt
    {
        static Texture2D _ring;
        static Texture2D _burn1;
        static Texture2D _burn2;
        static Texture2D _clip;
        static Texture2D _smoke;

        static Texture2D NewTex(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            t.wrapMode = TextureWrapMode.Clamp;
            t.filterMode = FilterMode.Bilinear;
            return t;
        }

        static float Fbm(float x, float y)
        {
            float v = 0f, amp = 0.5f, f = 1f;
            for (int i = 0; i < 4; i++)
            {
                v += Mathf.PerlinNoise(x * f, y * f) * amp;
                f *= 2.1f;
                amp *= 0.5f;
            }
            return v;
        }

        // Círculo a mano en tinta roja: dos pasadas que no cierran igual
        public static Texture2D Ring()
        {
            if (_ring != null)
                return _ring;
            const int w = 256, h = 128;
            _ring = NewTex(w, h);
            var px = new Color32[w * h];
            var ink = new Color(0.66f, 0.07f, 0.06f);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w * 2f - 1f;
                float v = (y + 0.5f) / h * 2f - 1f;
                float ang = Mathf.Atan2(v, u);
                float a = 0f;
                for (int pass = 0; pass < 2; pass++)
                {
                    float wob = Mathf.Sin(ang * 3f + pass * 1.7f) * 0.035f + pass * 0.05f;
                    float rx = 0.88f + wob, ry = 0.78f - wob * 0.6f;
                    float d = Mathf.Abs(Mathf.Sqrt((u / rx) * (u / rx) + (v / ry) * (v / ry)) - 1f);
                    float thick = 0.045f + 0.015f * Mathf.Sin(ang * 2f + pass);
                    float s = Mathf.Clamp01(1f - d / thick);
                    // La segunda pasada se corta: trazo que se superpone en un extremo
                    if (pass == 1 && ang > 1.9f)
                        s = 0f;
                    a = Mathf.Max(a, s);
                }
                px[y * w + x] = new Color(ink.r, ink.g, ink.b, a * 0.92f);
            }
            _ring.SetPixels32(px);
            _ring.Apply();
            return _ring;
        }

        // Papel quemado: bordes carbonizados y agujeros; lo intacto queda transparente
        public static Texture2D Burn(int variant)
        {
            if (variant == 2 && _burn2 != null)
                return _burn2;
            if (variant != 2 && _burn1 != null)
                return _burn1;
            const int n = 384;
            var t = NewTex(n, n);
            var px = new Color32[n * n];
            float seed = variant == 2 ? 41.3f : 7.9f;
            Vector2[] holes = variant == 2
                ? new[] { new Vector2(0.2f, 0.22f), new Vector2(0.83f, 0.66f) }
                : new[] { new Vector2(0.8f, 0.3f) };
            float[] radii = variant == 2 ? new[] { 0.11f, 0.08f } : new[] { 0.12f };
            var paper = new Color(0f, 0f, 0f, 0f);
            var scorch = new Color(0.45f, 0.26f, 0.1f);
            var edge = new Color(0.2f, 0.1f, 0.04f);
            var charC = new Color(0.05f, 0.035f, 0.03f);
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n, v = (y + 0.5f) / n;
                float noise = Fbm(u * 5f + seed, v * 5f + seed * 0.5f);
                // Más quemado abajo a la derecha (variante 1) o arriba (variante 2)
                float bias = variant == 2 ? (v - 0.5f) * 0.12f : ((u - 0.5f) - (v - 0.5f)) * 0.08f;
                float d = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                float b = 0.13f + bias - d + (noise - 0.5f) * 0.2f;
                for (int i = 0; i < holes.Length; i++)
                {
                    float hd = Vector2.Distance(new Vector2(u, v), holes[i]);
                    b = Mathf.Max(b, radii[i] - hd + (noise - 0.5f) * 0.09f);
                }
                Color c;
                if (b > 0.025f)
                    c = new Color(charC.r, charC.g, charC.b, 1f);
                else if (b > 0f)
                    c = Color.Lerp(edge, charC, b / 0.025f);
                else if (b > -0.07f)
                {
                    float k = 1f + b / 0.07f;
                    c = Color.Lerp(scorch, edge, k * k);
                    c.a = Mathf.Pow(k, 1.6f) * 0.9f;
                }
                else
                    c = paper;
                if (b > -0.07f && b <= 0f)
                    c.a = Mathf.Clamp01(c.a);
                else if (b > 0f)
                    c.a = 1f;
                px[y * n + x] = c;
            }
            t.SetPixels32(px);
            t.Apply();
            if (variant == 2)
                _burn2 = t;
            else
                _burn1 = t;
            return t;
        }

        // Clip de oficina: alambre doblado en dos óvalos
        public static Texture2D Clip()
        {
            if (_clip != null)
                return _clip;
            const int w = 64, h = 192;
            _clip = NewTex(w, h);
            var px = new Color32[w * h];
            var metal = new Color(0.55f, 0.56f, 0.58f);
            var shine = new Color(0.85f, 0.86f, 0.88f);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float a = 0f;
                a = Mathf.Max(a, Loop(x, y, 32f, 96f, 24f, 88f));
                a = Mathf.Max(a, Loop(x, y, 32f, 84f, 15f, 68f));
                a = Mathf.Max(a, Loop(x, y, 32f, 104f, 7f, 60f));
                float k = Mathf.Clamp01((x - 20f) / 30f);
                var c = Color.Lerp(metal, shine, k * 0.6f);
                c.a = a;
                px[y * w + x] = c;
            }
            _clip.SetPixels32(px);
            _clip.Apply();
            return _clip;
        }

        static float Loop(int x, int y, float cx, float cy, float rx, float ry)
        {
            float dx = (x + 0.5f - cx) / rx, dy = (y + 0.5f - cy) / ry;
            // Rectángulo redondeado: norma 4 para lados rectos
            float d = Mathf.Pow(Mathf.Pow(Mathf.Abs(dx), 4f) + Mathf.Pow(Mathf.Abs(dy), 4f), 0.25f);
            float px = Mathf.Abs(d - 1f) * Mathf.Min(rx, ry);
            return Mathf.Clamp01(1.6f - px);
        }

        // Mancha de humo: marrón grisáceo difuso
        public static Texture2D Smoke()
        {
            if (_smoke != null)
                return _smoke;
            const int n = 192;
            _smoke = NewTex(n, n);
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n - 0.5f, v = (y + 0.5f) / n - 0.5f;
                float r = Mathf.Sqrt(u * u + v * v) * 2f;
                float noise = Fbm(u * 4f + 3.1f, v * 4f + 8.7f);
                float a = Mathf.Clamp01(1f - r + (noise - 0.5f) * 0.9f);
                a = a * a * 0.55f;
                px[y * n + x] = new Color(0.22f, 0.18f, 0.15f, a);
            }
            _smoke.SetPixels32(px);
            _smoke.Apply();
            return _smoke;
        }
    }
}
