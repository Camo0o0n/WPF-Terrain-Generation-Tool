﻿using System;

namespace TerrainGeneratorSinglePage
{
    internal class TerrainGeneratorDS
    {
        private int _size;
        private int _max;
        public float[,] _map;
        private float _roughness;
        private Random random = new Random();

        public TerrainGeneratorDS(int detail)
        {
            _size = 1 + (int)Math.Pow(2, detail);
            _max = _size - 1;
            _map = new float[_size, _size];
        }

        public void Generate(float roughness)
        {
            _roughness = roughness;
            SetMap(0, 0, _max);
            SetMap(0, _max, _max/2);
            SetMap(_max, 0, _max/2);
            SetMap(_max, _max, 0);
            divide(_max);
            for (int x = 0; x < _size; x++)
            {
                for (int y = 0; y < _size; y++)
                {
                    _map[x, y] = _map[x, y] - _size / 2;
                }
            }
        }

        private void SetMap(int x, int y, float value)
        {
            _map[x, y] = value;
        }

        private float GetMap(int x, int y)
        {
            if (x < 0 || x > _max || y < 0 || y > _max) { return -1; }
            return _map[x, y];
        }

        private void divide(int jumpSize)
        {
            int half = jumpSize / 2;
            if (half < 1) { return; }
            float offset = _roughness * jumpSize;

            for (int x = half; x < _max; x += jumpSize) { 
                for (int y = half; y < _max; y += jumpSize) {
                    Square(x, y, half, (float)random.NextDouble() * offset * 2 - offset);
                }
            }

            int choiceNumber = 0;
            for(int x = 0; x <= _max; x += half) {
                if (choiceNumber == 0) 
                {
                    for (int y = half; y < _max; y += jumpSize)
                    {
                        Diamond(x, y, half, (float)random.NextDouble() * offset * 2 - offset);
                    }
                    choiceNumber = 1;
                }
                else if (choiceNumber == 1)
                {
                    for (int y = 0; y <= _max; y += jumpSize)
                    {
                        Diamond(x, y, half, (float)random.NextDouble() * offset * 2 - offset);
                    }
                    choiceNumber = 2;
                }
                else if (choiceNumber == 2)
                {
                    for (int y = half; y <= _max; y += jumpSize)
                    {
                        Diamond(x, y, half, (float)random.NextDouble() * offset * 2 - offset);
                    }
                    choiceNumber = 1;
                }
            }
            divide(jumpSize / 2);
        }

        private void Square(int x, int y, int half, float offset)
        {
            float average = averageList(new float[]
            {
                GetMap(x-half, y-half),
                GetMap(x-half, y+half),
                GetMap(x+half, y+half),
                GetMap(x+half, y-half)
            });
            SetMap(x, y, (average + offset));
        }

        private void Diamond(int x, int y, int half, float offset)
        {
            float average = averageList(new float[]
            {
                GetMap(x, y-half),
                GetMap(x, y+half),
                GetMap(x+half, y),
                GetMap(x-half, y)
            });
            SetMap(x, y, (average + offset));
        }

        private float averageList(float[] array)
        {
            float total = 0;
            int length = 0;
            foreach (float value in array)
            {
                if (value != -1)
                {
                    total += value;
                    length++;
                }
            }
            return (total / length);
        }
    }
}
