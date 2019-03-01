using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TVGL;
using TVGL.CUDA;
using TVGL.IOFunctions;
using TVGL.Voxelization;

namespace TVGLPresenterDX
{
    internal class Program
    {

        static readonly Stopwatch stopwatch = new Stopwatch();

        private static readonly string[] FileNames = {
           //"../../../TestFiles/Binary.stl",
         //   "../../../TestFiles/ABF.ply",
          //   "../../../TestFiles/Beam_Boss.STL",
         "../../../TestFiles/Beam_Clean.STL",

        "../../../TestFiles/bigmotor.amf",
        "../../../TestFiles/DxTopLevelPart2.shell",
        "../../../TestFiles/Candy.shell",
        "../../../TestFiles/amf_Cube.amf",
        "../../../TestFiles/train.3mf",
        "../../../TestFiles/Castle.3mf",
        "../../../TestFiles/Raspberry Pi Case.3mf",
       //"../../../TestFiles/shark.ply",
       "../../../TestFiles/bunnySmall.ply",
        "../../../TestFiles/cube.ply",
        "../../../TestFiles/airplane.ply",
        "../../../TestFiles/TXT - G5 support de carrosserie-1.STL.ply",
        "../../../TestFiles/Tetrahedron.STL",
        "../../../TestFiles/off_axis_box.STL",
           "../../../TestFiles/Wedge.STL",
        "../../../TestFiles/Mic_Holder_SW.stl",
        "../../../TestFiles/Mic_Holder_JR.stl",
        "../../../TestFiles/3_bananas.amf",
        "../../../TestFiles/drillparts.amf",  //Edge/face relationship contains errors
        "../../../TestFiles/wrenchsns.amf", //convex hull edge contains a concave edge outside of tolerance
        "../../../TestFiles/hdodec.off",
        "../../../TestFiles/tref.off",
        "../../../TestFiles/mushroom.off",
        "../../../TestFiles/vertcube.off",
        "../../../TestFiles/trapezoid.4d.off",
        "../../../TestFiles/ABF.STL",
        "../../../TestFiles/Pump-1repair.STL",
        "../../../TestFiles/Pump-1.STL",
        "../../../TestFiles/SquareSupportWithAdditionsForSegmentationTesting.STL",
        "../../../TestFiles/Beam_Clean.STL",
        "../../../TestFiles/Square_Support.STL",
        "../../../TestFiles/Aerospace_Beam.STL",
        "../../../TestFiles/Rook.amf",
       "../../../TestFiles/bunny.ply",

        "../../../TestFiles/piston.stl",
        "../../../TestFiles/Z682.stl",
        "../../../TestFiles/sth2.stl",
        "../../../TestFiles/Cuboide.stl", //Note that this is an assembly 
        "../../../TestFiles/new/5.STL",
       "../../../TestFiles/new/2.stl", //Note that this is an assembly 
        "../../../TestFiles/new/6.stl", //Note that this is an assembly  //breaks in slice at 1/2 y direction
       "../../../TestFiles/new/4.stl", //breaks because one of its faces has no normal
        "../../../TestFiles/radiobox.stl",
        "../../../TestFiles/brace.stl",  //Convex hull fails in MIconvexHull
        "../../../TestFiles/G0.stl",
        "../../../TestFiles/GKJ0.stl",
        "../../../TestFiles/testblock2.stl",
        "../../../TestFiles/Z665.stl",
        "../../../TestFiles/Casing.stl", //breaks because one of its faces has no normal
        "../../../TestFiles/mendel_extruder.stl",

       "../../../TestFiles/MV-Test files/holding-device.STL",
       "../../../TestFiles/MV-Test files/gear.STL"
        };

        [STAThread]
        private static void Main(string[] args)
        {
            //Difference2();
            var writer = new TextWriterTraceListener(Console.Out);
            Debug.Listeners.Add(writer);
            TVGL.Message.Verbosity = VerbosityLevels.OnlyCritical;
            DirectoryInfo dir;
            if (Directory.Exists("../../../../TestFiles"))
            {
                //x64
                dir = new DirectoryInfo("../../../../TestFiles");
            }
            else
            {
                //x86
                dir = new DirectoryInfo("../../../TestFiles");
            }
            var random = new Random();
            //var fileNames = dir.GetFiles("*").OrderBy(x => random.Next()).ToArray();
            var fileNames = dir.GetFiles("*SquareSupport*").ToArray();
            //Casing = 18
            //SquareSupport = 75
            for (var i = 0; i < fileNames.Count(); i++)
            {
                //var filename = FileNames[i];
                var filename = fileNames[i].FullName;
                Console.WriteLine("Attempting: " + filename);
                Stream fileStream;
                TessellatedSolid ts;
                if (!File.Exists(filename)) continue;
                using (fileStream = File.OpenRead(filename))
                    IO.Open(fileStream, filename, out ts);
                if (ts.Errors != null) continue;
                Color color = new Color(KnownColors.AliceBlue);
                ts.SolidColor = new Color(KnownColors.MediumSeaGreen)
                {
                    Af = 0.25f
                };
                //Presenter.ShowAndHang(ts);
                TestVoxelization(ts);

                // var stopWatch = new Stopwatch();
                // Color color = new Color(KnownColors.AliceBlue);
                //ts[0].SetToOriginAndSquare(out var backTransform);
                //ts[0].Transform(new double[,]
                //  {
                //{1,0,0,-(ts[0].XMax + ts[0].XMin)/2},
                //{0,1,0,-(ts[0].YMax+ts[0].YMin)/2},
                //{0,0,1,-(ts[0].ZMax+ts[0].ZMin)/2},
                //  });
                // stopWatch.Restart();
                //PresenterShowAndHang(ts);
                // Console.WriteLine("Voxelizing Tesselated File " + filename);
                //  var vs1 = new VoxelizedSolid(ts[0], VoxelDiscretization.Coarse, false);//, bounds);
                // Presenter.ShowAndHang(vs1);
                //TestVoxelization(ts[0]);
                //bounds = vs1.Bounds;
            }

            Console.WriteLine("Completed.");
            Console.ReadKey();
        }

        public static void TestVoxelization(TessellatedSolid ts)
        {
            var res = 8;

            //stopwatch.Restart();
            //var vs = new VoxelizedSolid(ts, res);
            //stopwatch.Stop();
            //Console.WriteLine("Original voxelization: {0}", stopwatch.Elapsed);

            stopwatch.Restart();
            var vs_cuda = new VoxelizedSolidCUDA(ts, res);
            stopwatch.Stop();
            Console.WriteLine("CUDA voxelization    : {0}\n", stopwatch.Elapsed);

            //Console.WriteLine(ts.SurfaceArea);
            //Console.WriteLine(vs_cuda.SurfaceArea);
            //Console.WriteLine();

            //Console.WriteLine(ts.Volume);
            ////Console.WriteLine(vs.Volume);
            //Console.WriteLine(vs_cuda.Volume);

            //Presenter.ShowAndHang(vs_cuda);

            //stopwatch.Restart();
            ///*var bb = */vs.CreateBoundingSolid();
            //stopwatch.Stop();
            //Console.WriteLine("Original bounding solid: {0}", stopwatch.Elapsed);

            //stopwatch.Restart();
            ///*var bb_cuda = */vs_cuda.CreateBoundingSolid/*_GPU*/();
            //stopwatch.Stop();
            //Console.WriteLine("CUDA bounding solid    : {0}\n", stopwatch.Elapsed);

            //Presenter.ShowAndHang(bb_cuda);

            //stopwatch.Restart();
            //var neg = vs.InvertToNewSolid();
            //stopwatch.Stop();
            //Console.WriteLine("Original inversion: {0}", stopwatch.Elapsed);

            stopwatch.Restart();
            var neg_cuda = vs_cuda.InvertToNewSolid_GPU();
            stopwatch.Stop();
            Console.WriteLine("CUDA inversion    : {0}\n", stopwatch.Elapsed);

            Presenter.ShowAndHang(neg_cuda);

            //stopwatch.Restart();
            //var draft = vs.ExtrudeToNewSolid(VoxelDirections.ZPositive);
            //stopwatch.Stop();
            //Console.WriteLine("Original draft: {0}", stopwatch.Elapsed);

            //stopwatch.Restart();
            //var draft_cuda = vs_cuda.DraftToNewSolid(VoxelDirections.ZPositive);
            //stopwatch.Stop();
            //Console.WriteLine("CUDA draft    : {0}\n", stopwatch.Elapsed);


            //stopwatch.Restart();
            ///*var intersect = */neg.IntersectToNewSolid(draft);
            //stopwatch.Stop();
            //Console.WriteLine("Original intersect: {0}", stopwatch.Elapsed);

            //stopwatch.Restart();
            ///*var intersect_cuda = */neg_cuda.IntersectToNewSolid_GPU(draft_cuda);
            //stopwatch.Stop();
            //Console.WriteLine("CUDA intersect    : {0}\n", stopwatch.Elapsed);

            ////Presenter.ShowAndHang(intersect_cuda);

            //var draft1 = vs.ExtrudeToNewSolid(VoxelDirections.YNegative);
            //stopwatch.Restart();
            ///*var union = */draft.UnionToNewSolid(draft1);
            //stopwatch.Stop();
            //Console.WriteLine("Original union: {0}", stopwatch.Elapsed);

            //var draft1_cuda = vs_cuda.DraftToNewSolid(VoxelDirections.YNegative);
            //stopwatch.Restart();
            ///*var union_cuda = */draft_cuda.UnionToNewSolid_GPU(draft1_cuda);
            //stopwatch.Stop();
            //Console.WriteLine("CUDA union    : {0}\n", stopwatch.Elapsed);

            ////Presenter.ShowAndHang(union_cuda);

            //stopwatch.Restart();
            ///*var subtract = */draft.SubtractToNewSolid(draft1);
            //stopwatch.Stop();
            //Console.WriteLine("Original subtract: {0}", stopwatch.Elapsed);

            //stopwatch.Restart();
            ///*var subtract_cuda = */draft_cuda.SubtractToNewSolid_GPU(draft1_cuda);
            //stopwatch.Stop();
            //Console.WriteLine("CUDA subtract    : {0}\n", stopwatch.Elapsed);

            //Presenter.ShowAndHang(subtract_cuda);

            //Presenter.ShowAndHang(intersect);
            //Presenter.ShowAndHang(intersect_cuda);
        }

        public static void TestSegmentation(TessellatedSolid ts)
        {
            var obb = MinimumEnclosure.OrientedBoundingBox(ts);
            var startTime = DateTime.Now;

            var averageNumberOfSteps = 500;

            //Do the average # of slices slices for each direction on a box (l == w == h).
            //Else, weight the average # of slices based on the average obb distance
            var obbAverageLength = (obb.Dimensions[0] + obb.Dimensions[1] + obb.Dimensions[2]) / 3;
            //Set step size to an even increment over the entire length of the solid
            var stepSize = obbAverageLength / averageNumberOfSteps;

            foreach (var direction in obb.Directions)
            {
                Dictionary<int, double> stepDistances;
                Dictionary<int, double> sortedVertexDistanceLookup;
                var segments = DirectionalDecomposition.UniformDirectionalSegmentation(ts, direction,
                    stepSize, out stepDistances, out sortedVertexDistanceLookup);
                //foreach (var segment in segments)
                //{
                //    var vertexLists = segment.DisplaySetup(ts);
                //    Presenter.ShowVertexPathsWithSolid(vertexLists, new List<TessellatedSolid>() { ts });
                //}
            }

            // var segments = AreaDecomposition.UniformDirectionalSegmentation(ts, obb.Directions[2].multiply(-1), stepSize);
            var totalTime = DateTime.Now - startTime;
            Debug.WriteLine(totalTime.TotalMilliseconds + " Milliseconds");
            //CheckAllObjectTypes(ts, segments);
        }

        private static void CheckAllObjectTypes(TessellatedSolid ts, IEnumerable<DirectionalDecomposition.DirectionalSegment> segments)
        {
            var faces = new HashSet<PolygonalFace>(ts.Faces);
            var vertices = new HashSet<Vertex>(ts.Vertices);
            var edges = new HashSet<Edge>(ts.Edges);


            foreach (var face in faces)
            {
                face.Color = new Color(KnownColors.Gray);
            }

            foreach (var segment in segments)
            {
                foreach (var face in segment.ReferenceFaces)
                {
                    faces.Remove(face);
                }
                foreach (var edge in segment.ReferenceEdges)
                {
                    edges.Remove(edge);
                }
                foreach (var vertex in segment.ReferenceVertices)
                {
                    vertices.Remove(vertex);
                }
            }

            ts.HasUniformColor = false;
            //Turn the remaining faces red
            foreach (var face in faces)
            {
                face.Color = new Color(KnownColors.Red);
            }
            Presenter.ShowAndHang(ts);

            //Make sure that every face, edge, and vertex is accounted for
            //Assert.That(!edges.Any(), "edges missed");
            //Assert.That(!faces.Any(), "faces missed");
            //Assert.That(!vertices.Any(), "vertices missed");
        }

        public static void TestSilhouette(TessellatedSolid ts)
        {
            var silhouette = TVGL.Silhouette.Run(ts, new[] { 0.5, 0.0, 0.5 });
            Presenter.ShowAndHang(silhouette);
        }

        private static void TestOBB(string InputDir)
        {
            var di = new DirectoryInfo(InputDir);
            var fis = di.EnumerateFiles();
            var numVertices = new List<int>();
            var data = new List<double[]>();
            foreach (var fileInfo in fis)
            {
                try
                {
                    IO.Open(fileInfo.Open(FileMode.Open), fileInfo.Name, out TessellatedSolid tessellatedSolid);
                    List<double> times, volumes;
                    MinimumEnclosure.OrientedBoundingBox_Test(tessellatedSolid, out times, out volumes);//, out VolumeData2);
                    data.Add(new[] { tessellatedSolid.ConvexHull.Vertices.Count(), tessellatedSolid.Volume,
                            times[0], times[1],times[2], volumes[0],  volumes[1], volumes[2] });
                }
                catch { }
            }
            // TVGLTest.ExcelInterface.PlotEachSeriesSeperately(VolumeData1, "Edge", "Angle", "Volume");
            TVGLTest.ExcelInterface.CreateNewGraph(new[] { data }, "", "Methods", "Volume", new[] { "PCA", "ChanTan" });
        }

        private static void TestSimplify(TessellatedSolid ts)
        {
            ts.Simplify(.9);
            Debug.WriteLine("number of vertices = " + ts.NumberOfVertices);
            Debug.WriteLine("number of edges = " + ts.NumberOfEdges);
            Debug.WriteLine("number of faces = " + ts.NumberOfFaces);
            TVGL.Presenter.ShowAndHang(ts);
        }
    }
}