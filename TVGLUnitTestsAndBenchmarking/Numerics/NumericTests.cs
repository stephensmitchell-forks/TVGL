using System;
using Xunit;
using TVGL.Numerics;

namespace TVGLUnitTestsAndBenchmarking
{
    public class TVGLNumericsTests
    {
        static Random r = new Random();
        static double r100 => 200.0 * r.NextDouble() - 100.0;

        [Fact]
        public void Vector2Length()
        {
            var vectLength5 = new Vector2(3, 4);
            Assert.Equal(5, vectLength5.Length(), 10);
        }

        [Fact]
        public void Cross2test1()
        {
            Assert.Equal(1, Vector2.UnitX.Cross(Vector2.UnitY), 10);
        }

        [Fact]
        public void Cross2test2()
        {
            var v1 = new Vector2(1, 2);
            var v2 = new Vector2(2, 1);
            Assert.True(v1.Cross(v2) < 0);
        }
        [Fact]
        public void Normalize2()
        {
            var v1 = new Vector2(r100, r100);
            var v2 = v1.Normalize();
            Assert.Equal(v2.Length(), 1.0, 10);
        }
        [Fact]
        public void Matrix3InvertSimple()
        {
            for (int i = 0; i < 5; i++)
            {
                var m = Matrix3x3.Null;
                var mInv = Matrix3x3.Null;
                var v1 = Vector2.Null;
                var v2 = Vector2.Null;
                do
                {
                    m = new Matrix3x3(r100, r100, r100, r100, r100, r100);
                    v1 = new Vector2(r100, r100);
                    v2 = v1.Transform(m);
                }
                while (!Matrix3x3.Invert(m, out mInv));
                Assert.True(v1.IsPracticallySame(v2.Transform(mInv), 1e-10));
            }
        }
        [Fact]
        public void Matrix3InvertFull()
        {
            for (int i = 0; i < 5; i++)
            {
                var m = Matrix3x3.Null;
                var mInv = Matrix3x3.Null;
                var v1 = Vector2.Null;
                var v2 = Vector2.Null;
                do
                {
                    m = new Matrix3x3(r100, r100, r100, r100, r100, r100, r100, r100, r100);
                    v1 = new Vector2(r100, r100);
                    v2 = v1.Transform(m);
                }
                while (!Matrix3x3.Invert(m, out mInv));
                Assert.True(v1.IsPracticallySame(v2.Transform(mInv), 1e-10));
            }
        }
        [Fact]
        public void Matrix4InvertSimple()
        {
            for (int i = 0; i < 5; i++)
            {
                var m = Matrix4x4.Null;
                var mInv = Matrix4x4.Null;
                var v1 = Vector3.Null;
                var v2 = Vector3.Null;
                do
                {
                    m = new Matrix4x4(r100, r100, r100, r100, r100, r100, r100, r100, r100, r100, r100, r100);
                    v1 = new Vector3(r100, r100, r100);
                    v2 = v1.Transform(m);
                }
                while (!Matrix4x4.Invert(m, out mInv));
                Assert.True(v1.IsPracticallySame(v2.Transform(mInv), 1e-10));
            }
        }
        [Fact]
        public void Matrix4InvertFull()
        {
            for (int i = 0; i < 5; i++)
            {
                var m = Matrix4x4.Null;
                var mInv = Matrix4x4.Null;
                var v1 = Vector3.Null;
                var v2 = Vector3.Null;
                do
                {
                    m = new Matrix4x4(r100, r100, r100, r100, r100, r100, r100, r100, r100, r100, r100, r100, r100, r100, r100, r100);
                    v1 = new Vector3(r100, r100, r100);
                    v2 = v1.Transform(m);
                }
                while (!Matrix4x4.Invert(m, out mInv));
                Assert.True(v1.IsPracticallySame(v2.Transform(mInv), 1e-10));
            }
        }


    }
}
