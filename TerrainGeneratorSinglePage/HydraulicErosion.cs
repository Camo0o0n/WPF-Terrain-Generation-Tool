using GlmSharp;
using System;
using System.Threading.Tasks;

namespace TerrainGeneratorSinglePage
{
    internal class HydraulicErosion
    {
        // adapted code https://nickmcd.me/2020/04/10/simple-particle-based-hydraulic-erosion/

        private double scale = 60.0;
        private float dt = 1.2f;
        private float density = 1.0f;
        private float evapRate = 0.001f;
        private float depositionRate = 0.1f;
        private float minVol = 0.01f;
        private float friction = 0.05f;
        private vec2 dim;
        public float[,] erosionMap;
        private Random random = new Random();

        private vec3 surfaceNormal(int i, int j)
        {
            vec3 n = new vec3(0.15F) * glm.Normalized(new vec3((float)scale * (erosionMap[i, j] - erosionMap[i + 1, j]), 1.0F, 0.0F));
            n += new vec3(0.15F) * glm.Normalized(new vec3((float)scale * (erosionMap[i - 1, j] - erosionMap[i, j]), 1.0F, 0.0F));
            n += new vec3(0.15F) * glm.Normalized(new vec3(0.0F, 1.0F, (float)scale * (erosionMap[i, j] - erosionMap[i, j + 1])));
            n += new vec3(0.15F) * glm.Normalized(new vec3(0.0F, 1.0F, (float)scale * (erosionMap[i, j - 1] - erosionMap[i, j])));

            n += new vec3(0.1F) * glm.Normalized(new vec3((float)scale * (erosionMap[i, j] - erosionMap[i + 1, j + 1]) / glm.Sqrt(2.0F), glm.Sqrt(2.0f), (float)scale * (erosionMap[i, j] - erosionMap[i + 1, j + 1]) / glm.Sqrt(2.0F)));    //Positive Y
            n += new vec3(0.1F) * glm.Normalized(new vec3((float)scale * (erosionMap[i, j] - erosionMap[i + 1, j - 1]) / glm.Sqrt(2.0F), glm.Sqrt(2.0F), (float)scale * (erosionMap[i, j] - erosionMap[i + 1, j - 1]) / glm.Sqrt(2.0F)));    //Positive Y
            n += new vec3(0.1F) * glm.Normalized(new vec3((float)scale * (erosionMap[i, j] - erosionMap[i - 1, j + 1]) / glm.Sqrt(2.0F), glm.Sqrt(2.0F), (float)scale * (erosionMap[i, j] - erosionMap[i - 1, j + 1]) / glm.Sqrt(2.0F)));    //Positive Y
            n += new vec3(0.1F) * glm.Normalized(new vec3((float)scale * (erosionMap[i, j] - erosionMap[i - 1, j - 1]) / glm.Sqrt(2.0F), glm.Sqrt(2.0F), (float)scale * (erosionMap[i, j] - erosionMap[i - 1, j - 1]) / glm.Sqrt(2.0F)));    //Positive Y

            return n;
        }

        struct Particle
        {
            public Particle(vec2 _pos) { pos = _pos; }

            public vec2 pos;
            public vec2 speed = new vec2(0, 0);

            public float volume = 1.0f;
            public float sediment = 0.0f;
        };

        public HydraulicErosion(float[,] erosionMap, int _hydroCycles)
        {

            this.erosionMap = erosionMap;

            dim = new vec2(erosionMap.GetLength(0), this.erosionMap.GetLength(0));

            Parallel.For(0, _hydroCycles, ctr =>
            {
                vec2 newpos = new vec2(random.Next(1, (int)dim.x), random.Next(1, (int)dim.y));
                Particle drop = new Particle(newpos);

                while (drop.volume > minVol)
                {

                    vec2 ipos = drop.pos;
                    if (ipos.x >= dim.x - 1 || ipos.y >= dim.y - 1 || ipos.y <= 1 || ipos.x <= 1) { break; }
                    vec3 n = surfaceNormal((int)ipos.x, (int)ipos.y);

                    drop.speed += dt * new vec2(n.x, n.z) / (drop.volume * density);
                    drop.pos += dt * drop.speed;
                    drop.speed *= (1.0F - dt * friction);

                    if (!glm.All(glm.GreaterThanEqual(drop.pos, new vec2(0))) ||
                       !glm.All(glm.LesserThan(drop.pos, dim))) break;

                    float maxsediment = drop.volume * glm.Length(drop.speed) * (this.erosionMap[(int)ipos.x, (int)ipos.y] - this.erosionMap[(int)drop.pos.x, (int)drop.pos.y]);
                    if (maxsediment < 0.0) { maxsediment = 0.0F; }
                    float sdiff = maxsediment - drop.sediment;

                    drop.sediment += dt * depositionRate * sdiff;
                    this.erosionMap[(int)ipos.x, (int)ipos.y] -= dt * drop.volume * depositionRate * sdiff;

                    drop.volume *= (1.0F - dt * evapRate);
                }
            });
        }
    }
}
