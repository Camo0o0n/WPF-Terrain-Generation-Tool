using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
//using static System.Runtime.InteropServices.JavaScript.JSType;
using Point = System.Windows.Point;

namespace TerrainGeneratorSinglePage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Variables
        private TerrainGeneratorDS TG;
        private GeometryModel3D TerrainGeometryModel;

        private float[,] noiseData;
        private float[,] noiseTerrainMap;

        private float[,] erosionMap;

        private bool mDown;
        private Point mLastPos;

        private Random random = new Random();
        private WriteableBitmap wbp;

        private int GenerationMode = 0;
        private int imageNumber = 0;
        private int MeshNumber = 0;
        #endregion

        #region DiamondSquare Terrain Xaml Data
        private int _details = 9;
        public int Details
        {
            get => _details;
            set
            {
                if (value <= 15) { _details = value; }
                NotifyPropertyChanged("Details");
            }
        }
        private float _roughness = 0.3f;
        public float Roughness
        {
            get => _roughness;
            set
            {
                _roughness = (float)Math.Round(value, 4);
                NotifyPropertyChanged("Roughness");
            }
        }
        #endregion

        #region Simplex Xaml Data
        private FastNoiseLite SimplexNoise;
        private int _simplexSeed;
        public int SimplexSeed
        {
            get => _simplexSeed;
            set
            {
                _simplexSeed = value;
                NotifyPropertyChanged("SimplexSeed");
            }
        }
        private float _simplexFrequency = 0.01f;
        public float SimplexFrequency
        {
            get => _simplexFrequency;
            set
            {
                _simplexFrequency = (float)Math.Round(value, 4);
                NotifyPropertyChanged("SimplexFrequency");
            }
        }
        private bool _simplexEnabled;
        public bool SimplexEnabled
        {
            get => _simplexEnabled;
            set
            {
                _simplexEnabled = value;
                if(_simplexEnabled) {
                    SimplexNoise = new FastNoiseLite();
                    SimplexNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
                    simplexStuff();
                    Button_Click_GenerateNoise(null, null);
                }
                else { SimplexNoise = null; Button_Click_GenerateNoise(null, null); }
                NotifyPropertyChanged("SimplexEnabled");
            }
        }
        public bool _simplexRandomSeed = true;
        public bool SimplexRandomSeed
        {
            get => _simplexRandomSeed;
            set
            {
                _simplexRandomSeed = value;
                NotifyPropertyChanged("SimplexRandomSeed");
            }
        }
        private void simplexStuff()
        {
            SimplexNoise.SetFrequency(_simplexFrequency);
            if (_simplexRandomSeed) { SimplexSeed = random.Next(10000); }
            SimplexNoise.SetSeed(_simplexSeed);
        }
        private void Slider_ValueChangedSimplex(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bool randomSeed1 = false;
            bool randomSeed2 = false;
            bool randomSeed3 = false;
            if (_simplexEnabled)
            {

                if (_simplexRandomSeed) { _simplexRandomSeed = false; randomSeed1 = true; }
                if (_perlinRandomSeed) { _perlinRandomSeed = false; randomSeed2 = true; }
                if (_cellularRandomSeed) { _cellularRandomSeed = false; randomSeed3 = true; }
                simplexStuff();
                Button_Click_GenerateNoise(null, null);
                if (randomSeed1) { _simplexRandomSeed = true; }
                if (randomSeed2) { _perlinRandomSeed = true; }
                if (randomSeed3) { _cellularRandomSeed = true; }
                
            }
        }
        #endregion

        #region Perlin Xaml Data
        private FastNoiseLite PerlinNoise;
        private int _perlinSeed;
        public int PerlinSeed
        {
            get => _perlinSeed;
            set
            {
                _perlinSeed = value;
                NotifyPropertyChanged("PerlinSeed");
            }
        }
        private float _perlinFrequency = 0.01f;
        public float PerlinFrequency
        {
            get => _perlinFrequency;
            set
            {
                _perlinFrequency = (float)Math.Round(value, 4);
                NotifyPropertyChanged("PerlinFrequency");
            }
        }
        private bool _perlinEnabled = false;
        public bool PerlinEnabled
        {
            get => _perlinEnabled;
            set
            {
                _perlinEnabled = value;
                if (_perlinEnabled) {
                    PerlinNoise = new FastNoiseLite();
                    PerlinNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
                    perlinStuff();
                    Button_Click_GenerateNoise(null, null);
                }
                else { PerlinNoise = null; Button_Click_GenerateNoise(null, null); }
                NotifyPropertyChanged("PerlinEnabled");
            }
        }
        private bool _perlinRandomSeed = true;
        public bool PerlinRandomSeed
        {
            get => _perlinRandomSeed;
            set
            {
                _perlinRandomSeed = value;
                NotifyPropertyChanged("PerlinRandomSeed");
            }
        }
        private void perlinStuff()
        {
            PerlinNoise.SetFrequency(_perlinFrequency);
            if (PerlinRandomSeed) { PerlinSeed = random.Next(10000); }
            PerlinNoise.SetSeed(_perlinSeed);
        }
        private void Slider_ValueChangedPerlin(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bool randomSeed1 = false;
            bool randomSeed2 = false;
            bool randomSeed3 = false;
            if (_perlinEnabled)
            {
                if (_perlinRandomSeed) { _perlinRandomSeed = false; randomSeed1 = true; }
                if (_simplexRandomSeed) { _simplexRandomSeed = false; randomSeed2 = true; }
                if (_cellularRandomSeed) { _cellularRandomSeed = false; randomSeed3 = true; }
                perlinStuff();  
                Button_Click_GenerateNoise(null, null);
                if (randomSeed1) { _perlinRandomSeed = true; }
                if (randomSeed2) { _simplexRandomSeed = true; }
                if (randomSeed3) { _cellularRandomSeed = true; }
                
            }
        }
        #endregion
     
        #region Cellular Xaml Data
        private FastNoiseLite CellularNoise;
        private int _cellularSeed;
        public int CellularSeed
        {
            get => _cellularSeed;
            set
            {
                _cellularSeed = value;
                NotifyPropertyChanged("CellularSeed");
            }
        }
        private float _cellularFrequency = 0.01f;
        public float CellularFrequency
        {
            get => _cellularFrequency;
            set
            {
                _cellularFrequency = (float)Math.Round(value, 4);
                NotifyPropertyChanged("CellularFrequency");
            }
        }
        private bool _cellularEnabled = false;
        public bool CellularEnabled
        {
            get => _cellularEnabled;
            set
            {
                _cellularEnabled = value;
                if (_cellularEnabled)
                {
                    CellularNoise = new FastNoiseLite();
                    CellularNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular); 
                    cellularStuff(); 
                    Button_Click_GenerateNoise(null, null); }
                else { CellularNoise = null; Button_Click_GenerateNoise(null, null); }
                NotifyPropertyChanged("CellularEnabled");
            }
        }
        public bool _cellularRandomSeed = true;
        public bool CellularRandomSeed
        {
            get => _cellularRandomSeed;
            set
            {
                _cellularRandomSeed = value;
                NotifyPropertyChanged("CellularRandomSeed");
            }
        }
        private void cellularStuff()
        {
            CellularNoise.SetFrequency(_cellularFrequency);
            if (CellularRandomSeed) { CellularSeed = random.Next(10000); }
            CellularNoise.SetSeed(_cellularSeed);
        }
        private void Slider_ValueChangedCellular(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bool randomSeed1 = false;
            bool randomSeed2 = false;
            bool randomSeed3 = false;
            if (_cellularEnabled) { 
                if (_cellularRandomSeed) { _cellularRandomSeed = false; randomSeed1 = true; } 
                if (_simplexRandomSeed) { _simplexRandomSeed = false; randomSeed2 = true; } 
                if (_perlinRandomSeed) { _perlinRandomSeed = false; randomSeed3 = true; } 
                cellularStuff();
                Button_Click_GenerateNoise(null, null); }
                if (randomSeed1) { _cellularRandomSeed = true; }
                if (randomSeed2) { _simplexRandomSeed = true; }
                if (randomSeed3) { _perlinRandomSeed = true; }
                
        }
        #endregion

        #region Hydro Cycles Xaml Value
        private int _hydroCycles = 50000;
        public int HydroCycles
        {
            get => _hydroCycles;
            set
            {
                if (value > 200000) { _hydroCycles = 200000; }
                else if (value <= 1) { _hydroCycles = 1; }
                else { _hydroCycles = value; }
                NotifyPropertyChanged("HydroCycles");
            }
        }
        #endregion

        #region image Size Xaml Value
        private int _imageSize = 512;
        public int ImageSize
        {
            get => _imageSize;
            set
            {
                _imageSize = value;
                NotifyPropertyChanged("ImageHeight");
            }
        }
        #endregion

        #region Noise Terrain Height Xaml Value
        private int _noiseHeight = 256;
        public int NoiseHeight
        {
            get => _noiseHeight;
            set
            {
                _noiseHeight = value;
                NotifyPropertyChanged("NoiseHeight");
            }
        }
        #endregion

        #region INotifyPropertyChanged Setup
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
        public MainWindow()
        {
            DataContext = this;
            KeyDown += new KeyEventHandler(Grid_KeyDown);
            InitializeComponent();
            Button_Click_GenerateTerrain(null, null);
            SimplexEnabled = true;
            Button_Click_GenerateNoise(null, null);
            Button_Click_Reset(null, null);
        }

        #region WASD mesh movement
        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            Transform3DGroup group = MyModel3DGroup.Transform as Transform3DGroup;
            if (group == null)
            {
                group = new Transform3DGroup();
                MyModel3DGroup.Transform = group;
            }
            if (Keyboard.IsKeyDown(Key.A))
            {
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(new Vector3D(0, 1, 0), -3.0D / Math.PI));
                group.Children.Add(new RotateTransform3D(r));
            }
            if (Keyboard.IsKeyDown(Key.D))
            {
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(new Vector3D(0, 1, 0), 3.0D / Math.PI));
                group.Children.Add(new RotateTransform3D(r));
            }
            if (Keyboard.IsKeyDown(Key.W))
            {
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(new Vector3D(1, 0, 0), -3.0D / Math.PI));
                group.Children.Add(new RotateTransform3D(r));
            }
            if (Keyboard.IsKeyDown(Key.S))
            {
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(new Vector3D(1, 0, 0), 3.0D / Math.PI));
                group.Children.Add(new RotateTransform3D(r));
            }
            if (Keyboard.IsKeyDown(Key.Q))
            {
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(new Vector3D(0, 0, 1), 3.0D / Math.PI));
                group.Children.Add(new RotateTransform3D(r));
            }
            if (Keyboard.IsKeyDown(Key.E))
            {
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(new Vector3D(0, 0, 1), -3.0D / Math.PI));
                group.Children.Add(new RotateTransform3D(r));
            }
        }
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e) 
        { 
            Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z - e.Delta / 2.5D);
        }
        #endregion

        private void Button_Click_GenerateNoise(object sender, EventArgs e)
        {
            if(_imageSize <= 0) { return; }
            noiseData = new float[_imageSize, _imageSize];
            int count = 0;
            if (_simplexEnabled) { count++; if (_simplexRandomSeed) { simplexStuff(); } }
            if (_perlinEnabled) { count++; if (_perlinRandomSeed) { perlinStuff(); } }
            if (_cellularEnabled) { count++; if (_cellularRandomSeed) { cellularStuff(); } }

            if (count == 0) { return; }
            Parallel.For(0, _imageSize, x =>
            {
                for (int y = 0; y < _imageSize; y++)
                {
                    if (_simplexEnabled) { noiseData[x, y] += SimplexNoise.GetNoise(x, y); }
                    if (_perlinEnabled) { noiseData[x, y] += PerlinNoise.GetNoise(x, y); }
                    if (_cellularEnabled) { noiseData[x, y] += CellularNoise.GetNoise(x, y); }
                    noiseData[x, y] = noiseData[x, y] / count;
                }
            });
            WriteableBitmap wb = new WriteableBitmap(_imageSize, _imageSize, 96, 96, PixelFormats.Gray8, null);

            Int32Rect rect = new Int32Rect(0, 0, _imageSize, _imageSize);

            //Width * height *  bytes per pixel aka(32/8)
            byte[] pixels = new byte[_imageSize * _imageSize * (wb.Format.BitsPerPixel / 8)];

            int pixelWidth = wb.PixelWidth;
            int format = wb.Format.BitsPerPixel;
            Parallel.For (0, pixelWidth, y =>
            {
                for (int xX = 0; xX < pixelWidth; xX++)
                {
                    int pixelOffset = (xX + (pixelWidth * y)) * (format / 8);
                    pixels[pixelOffset] = (byte)RatioValueConverter(-1, 1, 1, 255, noiseData[xX, y]);

                }
            });
            
            int stride = wb.PixelWidth * (wb.Format.BitsPerPixel / 8);
            wb.WritePixels(rect, pixels, stride, 0);
            wbp = wb;
            noiseImage.Source = wb;
        }

        private void Button_Click_Reset(object sender, RoutedEventArgs e)
        {
            Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, 2048);
            Transform3DGroup group = MyModel3DGroup.Transform as Transform3DGroup;
            group = new Transform3DGroup();
            MyModel3DGroup.Transform = group;
            QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(new Vector3D(0, 0, 1), -125D));
            group.Children.Add(new RotateTransform3D(r));
            r = new QuaternionRotation3D(new Quaternion(new Vector3D(1, 0, 0), -55D));
            group.Children.Add(new RotateTransform3D(r));
            r = new QuaternionRotation3D(new Quaternion(new Vector3D(0, 1, 0), 4D));
            group.Children.Add(new RotateTransform3D(r));
        }

        private void Button_Click_GenerateTerrain(object sender, RoutedEventArgs e)
        {
            GenerationMode = 1;
            
            TG = new TerrainGeneratorDS(_details);
            TG.Generate(_roughness);

            DrawSetup(TG._map);
        }

        #region Draw Terrain
        private void DrawSetup(float[,] map)
        {
            int length = map.GetLength(0);
            MyModel3DGroup.Children.Clear();

            PointLight pointLight = new PointLight(Colors.White, new Point3D(length / 2, length / 2, length * 3 / 5));

            MyModel3DGroup.Children.Add(pointLight);

            float minHeightValue = 0;
            float maxHeightValue = 0;
            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    float val = map[x, y];
                    if (val < minHeightValue) { minHeightValue = val; }
                    if (val > maxHeightValue) { maxHeightValue = val; }
                }
            }

            for (int x = 0; x < length - 1; x += length)
            {
                for (int y = 0; y < length - 1; y += length)
                {
                    int maxX = x + length + 1;
                    int maxY = y + length + 1;

                    if (maxX > length) { maxX = length; }
                    if (maxY > length) { maxY = length; }

                    DrawTerrain(map, length, minHeightValue, maxHeightValue, x, maxX, y, maxY);

                    System.GC.Collect();
                    System.GC.WaitForFullGCComplete();
                }
            }
        }

        private void DrawTerrain(float[,] map, int size, float minHeight, float maxHeight, int minPosX, int maxPosX, int minPosY, int maxPosY)
        {
            float halfSize = size / 2;
            float halfHeight = (maxHeight - minHeight) / 2;

            TerrainGeometryModel = new GeometryModel3D(new MeshGeometry3D(), new DiffuseMaterial(new SolidColorBrush(Colors.Yellow)));
            Point3DCollection points = new Point3DCollection();
            Int32Collection indicies = new Int32Collection();

            for (int y = minPosY; y < maxPosY; y++) {
                for (int x = minPosX; x < maxPosX; x++) {
                    points.Add(new Point3D((x - halfSize), (y - halfSize), map[x,y]));
                }
            }
            ((MeshGeometry3D)TerrainGeometryModel.Geometry).Positions = points;

            int ind1 = 0;
            int ind2 = 0;
            int xLength = maxPosX;
            for (int y = minPosY; y < maxPosY - 1; y++) {
                for (int x = minPosX; x < maxPosX - 1; x++)
                {
                    ind1 = x + y * xLength;
                    ind2 = ind1 + xLength;

                    indicies.Add(ind1);
                    indicies.Add(ind2 + 1);
                    indicies.Add(ind2);

                    indicies.Add(ind1);
                    indicies.Add(ind1 + 1);
                    indicies.Add(ind2 + 1);
                }
            }
             ((MeshGeometry3D)TerrainGeometryModel.Geometry).TriangleIndices = indicies;

            MyModel3DGroup.Children.Add(TerrainGeometryModel);
        }
        #endregion

        private void Button_Click_GenerateTerrainNoise(object sender, RoutedEventArgs e)
        {
            if (_noiseHeight <= 0) { return; }
            GenerationMode = 2;

            if (noiseData == null) { return; }

            int halfHeight = _noiseHeight/2;
            
            noiseTerrainMap = new float[_imageSize, _imageSize];

            float minHeightNoise = 0;
            float maxHeightNoise = 0;
            Parallel.For(0, noiseData.GetLength(0), y=>
            {
                for (int x = 0; x < noiseData.GetLength(0); x++)
                {
                    float val = noiseData[x, y];
                    if (val < minHeightNoise) { minHeightNoise = val; }
                    if (val > maxHeightNoise) { maxHeightNoise = val; }
                }
            });

            Parallel.For(0, noiseTerrainMap.GetLength(0), x =>
            {
                for (int y = 0; y < noiseTerrainMap.GetLength(0); y++)
                {
                    noiseTerrainMap[x, y] = RatioValueConverter(minHeightNoise, maxHeightNoise, halfHeight * -1, halfHeight, noiseData[x, y]);
                }
            });

            DrawSetup(noiseTerrainMap);
        }

        #region Saving Mesh and Image
        private void Button_Click_SaveMesh(object sender, RoutedEventArgs e)
        {
            float[,] meshSave;
            if (GenerationMode == 1) { meshSave = TG._map; }
            else if (GenerationMode == 2) { meshSave = noiseTerrainMap; }
            else if (GenerationMode == 3) { meshSave = erosionMap; }
            else { return; }

            MeshNumber++;
            using (StreamWriter outputFile = new StreamWriter("MeshFile" + MeshNumber + ".obj"))
            {
                outputFile.WriteLine("# obj file created by Cameron\n#Generated Mesh#\n");
                for(int x = 0; x < meshSave.GetLength(0); x++) {
                    for (int y = 0; y < meshSave.GetLength(0); y++) {
                        outputFile.WriteLine("v " + x + " " + y + " " + meshSave[x,y]);
                    }
                }
                for (int x = 0; x < meshSave.GetLength(0) - 1; x++) {
                    for (int y = 0; y < meshSave.GetLength(0) - 1; y++) {
                        outputFile.WriteLine("f " + (meshSave.GetLength(0) * x+y+1) + " " + (meshSave.GetLength(0) * x+y+2) + " " + (meshSave.GetLength(0) * (x+1)+y+1));
                    }
                }
                for (int x = 0; x < meshSave.GetLength(0) - 1; x++) {
                    for (int y = 0; y < meshSave.GetLength(0) - 1; y++) {
                        outputFile.WriteLine("f " + (meshSave.GetLength(0) * x + y + 2) + " " + (meshSave.GetLength(0) * (x + 1) + y + 2) + " " + (meshSave.GetLength(0) * (x + 1) + y + 1));
                    }
                }

                outputFile.Close();
            }
        }

        private void Button_Click_SaveNoiseImage(object sender, RoutedEventArgs e)
        {
            imageNumber++;
            string filename = "imageFile" + imageNumber + ".png";
            if (filename != string.Empty)
            {
                using (FileStream stream5 = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(wbp));
                    encoder.Save(stream5);
                }
            }
        }
        #endregion

        private void Button_Click_HydraulicErosion(object sender, RoutedEventArgs e)
        {
            
            if (GenerationMode == 1) { erosionMap = TG._map; }
            else if (GenerationMode == 2) { erosionMap = noiseTerrainMap; }
            else if (GenerationMode == 3) { }
            else { return; }

            GenerationMode = 3;

            HydraulicErosion hydraulicErosion = new HydraulicErosion(erosionMap, _hydroCycles);
            erosionMap = hydraulicErosion.erosionMap;

            DrawSetup(erosionMap);
        }

        private float RatioValueConverter(float old_min, float old_max, float new_min, float new_max, float value)
        {
            return (((value - old_min) / (old_max - old_min)) * (new_max - new_min) + new_min);
        }
    }
}
