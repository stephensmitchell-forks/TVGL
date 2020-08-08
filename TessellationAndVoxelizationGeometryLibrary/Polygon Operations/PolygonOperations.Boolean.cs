﻿using System;
using System.Collections.Generic;
using System.Linq;
using TVGL.Numerics;

namespace TVGL.TwoDimensional
{
    /// <summary>
    /// A set of general operation for points and paths
    /// </summary>
    public static partial class PolygonOperations
    {
        static PolygonUnion polygonUnion;
        static PolygonDifference polygonDifference;
        static PolygonExclusiveOR polygonXOR;
        static PolygonIntersection polygonIntersection;
        static PolygonRemoveIntersections polygonRemoveIntersections;

        #region Union Public Methods
        /// <summary>
        /// Returns the list of polygons that exist in either A OR B.
        /// </summary>
        /// <param name="polygonA">The polygon a.</param>
        /// <param name="polygonB">The polygon b.</param>
        /// <returns>System.Collections.Generic.List&lt;TVGL.TwoDimensional.Polygon&gt;.</returns>
        public static List<Polygon> Union(this Polygon polygonA, Polygon polygonB, double minAllowableArea = Constants.BaseTolerance)
        {
            var relationship = GetPolygonRelationshipAndIntersections(polygonA, polygonB, out var intersections);
            return Union(polygonA, polygonB, relationship, intersections, minAllowableArea);
        }
        /// <summary>
        /// Returns the list of polygons that exist in either A OR B.By providing the intersections
        /// between the two polygons, the operation will be performed with less time and memory.
        /// </summary>
        /// <param name="polygonA">The polygon a.</param>
        /// <param name="polygonB">The polygon b.</param>
        /// <param name="polygonRelationship">The polygon relationship.</param>
        /// <param name="intersections">The intersections.</param>
        /// <returns>System.Collections.Generic.List&lt;TVGL.TwoDimensional.Polygon&gt;.</returns>
        /// <exception cref="ArgumentException">A negative polygon (i.e. hole) is provided to Union which results in infinite shape. - polygonA</exception>
        /// <exception cref="ArgumentException">A negative polygon (i.e. hole) is provided to Union which results in infinite shape. - polygonB</exception>
        public static List<Polygon> Union(this Polygon polygonA, Polygon polygonB, PolygonRelationship polygonRelationship, List<IntersectionData> intersections,
             double minAllowableArea = Constants.BaseTolerance)
        {
            if (!polygonA.IsPositive) throw new ArgumentException("A negative polygon (i.e. hole) is provided to Union which results in infinite shape.", "polygonA");
            if (!polygonB.IsPositive) throw new ArgumentException("A negative polygon (i.e. hole) is provided to Union which results in infinite shape.", "polygonB");
            switch (polygonRelationship)
            {
                case PolygonRelationship.Separated:
                case PolygonRelationship.SeparatedButVerticesTouch:
                case PolygonRelationship.AIsInsideHoleOfB:
                case PolygonRelationship.AIsInsideHoleOfBButVerticesTouch:
                case PolygonRelationship.BIsInsideHoleOfA:
                case PolygonRelationship.BIsInsideHoleOfABButVerticesTouch:
                    return new List<Polygon> { polygonA.Copy(), polygonB.Copy() };
                case PolygonRelationship.BIsCompletelyInsideA:
                case PolygonRelationship.BIsInsideAButEdgesTouch:
                case PolygonRelationship.BIsInsideAButVerticesTouch:
                    return new List<Polygon> { polygonA.Copy() };
                case PolygonRelationship.AIsCompletelyInsideB:
                case PolygonRelationship.AIsInsideBButEdgesTouch:
                case PolygonRelationship.AIsInsideBButVerticesTouch:
                    return new List<Polygon> { polygonB.Copy() };
                default:
                    //case PolygonRelationship.SeparatedButEdgesTouch:
                    //case PolygonRelationship.Intersect:
                    //case PolygonRelationship.AIsInsideHoleOfBButEdgesTouch:
                    //case PolygonRelationship.BIsInsideHoleOfABButEdgesTouch:
                    if (polygonUnion == null) polygonUnion = new PolygonUnion();
                    return polygonUnion.Run(polygonA, polygonB, intersections, minAllowableArea);
            }
        }
        /// <summary>
        /// Returns the list of polygons that are the subshapes of ANY of the provided polygons.
        /// </summary>
        /// <param name="polygons">The polygons.</param>
        /// <returns>System.Collections.Generic.List&lt;TVGL.TwoDimensional.Polygon&gt;.</returns>
        public static List<Polygon> Union(this IEnumerable<Polygon> polygons, double minAllowableArea = Constants.BaseTolerance)
        {

            var polygonList = polygons.ToList();
            for (int i = polygonList.Count - 1; i > 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    var polygonRelationship = GetPolygonRelationshipAndIntersections(polygonList[i],
                        polygonList[j], out var intersections);
                    if (polygonRelationship == PolygonRelationship.BIsCompletelyInsideA
                        || polygonRelationship == PolygonRelationship.BIsInsideAButEdgesTouch
                        || polygonRelationship == PolygonRelationship.BIsInsideAButVerticesTouch)
                    {  // remove polygon B
                        polygonList.RemoveAt(j);
                        i--;
                    }
                    else if (polygonRelationship == PolygonRelationship.AIsCompletelyInsideB
                        || polygonRelationship == PolygonRelationship.AIsInsideBButEdgesTouch
                        || polygonRelationship == PolygonRelationship.AIsInsideBButVerticesTouch)
                    {                            // remove polygon A
                        polygonList.RemoveAt(i);
                        break; // to stop the inner loop
                    }
                    else if (polygonRelationship == PolygonRelationship.SeparatedButEdgesTouch
                             || polygonRelationship == PolygonRelationship.Intersection
                             || polygonRelationship == PolygonRelationship.AIsInsideHoleOfBButEdgesTouch
                             || polygonRelationship == PolygonRelationship.BIsInsideHoleOfABButEdgesTouch)
                    {
                        var newPolygons = Union(polygonList[i], polygonList[j], polygonRelationship, intersections, minAllowableArea);
                        polygonList.RemoveAt(i);
                        polygonList.RemoveAt(j);
                        polygonList.AddRange(newPolygons);
                        i = polygonList.Count; // to restart the outer loop
                        break; // to stop the inner loop
                    }
                }
            }
            return polygonList;
        }
        #endregion

        #region Intersect Public Methods
        /// <summary>
        /// Returns the list of polygons that result from the subshapes common to both A and B. 
        /// </summary>
        /// <param name="polygonA">The polygon a.</param>
        /// <param name="polygonB">The polygon b.</param>
        /// <param name="minAllowableArea">The minimum allowable area.</param>
        /// <returns>System.Collections.Generic.List&lt;TVGL.TwoDimensional.Polygon&gt;.</returns>
        public static List<Polygon> Intersect(this Polygon polygonA, Polygon polygonB, double minAllowableArea = Constants.BaseTolerance)
        {
            var relationship = GetPolygonRelationshipAndIntersections(polygonA, polygonB, out var intersections);
            return Intersect(polygonA, polygonB, relationship, intersections, minAllowableArea);
        }

        /// <summary>
        /// Returns the list of polygons that result from the subshapes common to both A and B. By providing the intersections
        /// between the two polygons, the operation will be performed with less time and memory.
        /// </summary>
        /// <param name="polygonA">The polygon a.</param>
        /// <param name="polygonB">The polygon b.</param>
        /// <param name="polygonRelationship">The polygon relationship.</param>
        /// <param name="intersections">The intersections.</param>
        /// <param name="minAllowableArea">The minimum allowable area.</param>
        /// <returns>System.Collections.Generic.List&lt;TVGL.TwoDimensional.Polygon&gt;.</returns>
        public static List<Polygon> Intersect(this Polygon polygonA, Polygon polygonB, PolygonRelationship polygonRelationship, List<IntersectionData> intersections, double minAllowableArea = Constants.BaseTolerance)
        {
            switch (polygonRelationship)
            {
                case PolygonRelationship.Separated:
                case PolygonRelationship.SeparatedButVerticesTouch:
                case PolygonRelationship.SeparatedButEdgesTouch:
                case PolygonRelationship.AIsInsideHoleOfB:
                case PolygonRelationship.AIsInsideHoleOfBButVerticesTouch:
                case PolygonRelationship.AIsInsideHoleOfBButEdgesTouch:
                case PolygonRelationship.BIsInsideHoleOfA:
                case PolygonRelationship.BIsInsideHoleOfABButVerticesTouch:
                case PolygonRelationship.BIsInsideHoleOfABButEdgesTouch:
                    return new List<Polygon>();
                case PolygonRelationship.BIsCompletelyInsideA:
                case PolygonRelationship.BIsInsideAButVerticesTouch:
                case PolygonRelationship.BIsInsideAButEdgesTouch:
                    if (polygonB.IsPositive) return new List<Polygon> { polygonB.Copy() };
                    else
                    {
                        var polygonACopy = polygonA.Copy();
                        polygonACopy.AddHole(polygonB.Copy());
                        return new List<Polygon> { polygonACopy };
                    }
                case PolygonRelationship.AIsCompletelyInsideB:
                case PolygonRelationship.AIsInsideBButVerticesTouch:
                case PolygonRelationship.AIsInsideBButEdgesTouch:
                    if (polygonA.IsPositive) return new List<Polygon> { polygonA.Copy() };
                    else
                    {
                        var polygonBCopy = polygonB.Copy();
                        polygonBCopy.AddHole(polygonA.Copy());
                        return new List<Polygon> { polygonBCopy };
                    }
                default:
                    //case PolygonRelationship.Intersect:
                    if (polygonIntersection == null) polygonIntersection = new PolygonIntersection();
                    return polygonIntersection.Run(polygonA, polygonB, intersections, minAllowableArea);
            }
        }

        /// <summary>
        /// Returns the list of polygons that are the subshapes of ALL of the provided polygons.
        /// </summary>
        /// <param name="polygons">The polygons.</param>
        /// <returns>System.Collections.Generic.List&lt;TVGL.TwoDimensional.Polygon&gt;.</returns>
        public static List<Polygon> Intersect(this IEnumerable<Polygon> polygons, double minAllowableArea = Constants.BaseTolerance)
        {
            var polygonList = polygons.ToList();
            for (int i = polygonList.Count - 1; i > 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    var polygonRelationship = GetPolygonRelationshipAndIntersections(polygonList[i],
                        polygonList[j], out var intersections);
                    if (polygonRelationship == PolygonRelationship.Separated
                        || polygonRelationship == PolygonRelationship.SeparatedButVerticesTouch
                        || polygonRelationship == PolygonRelationship.SeparatedButEdgesTouch
                        || polygonRelationship == PolygonRelationship.AIsInsideHoleOfB
                        || polygonRelationship == PolygonRelationship.AIsInsideHoleOfBButVerticesTouch
                        || polygonRelationship == PolygonRelationship.AIsInsideHoleOfBButEdgesTouch
                        || polygonRelationship == PolygonRelationship.BIsInsideHoleOfA
                        || polygonRelationship == PolygonRelationship.BIsInsideHoleOfABButVerticesTouch
                        || polygonRelationship == PolygonRelationship.BIsInsideHoleOfABButEdgesTouch)
                        return new List<Polygon>();
                    else if (polygonRelationship == PolygonRelationship.BIsCompletelyInsideA
                        || polygonRelationship == PolygonRelationship.BIsInsideAButVerticesTouch
                        || polygonRelationship == PolygonRelationship.BIsInsideAButEdgesTouch)
                    {                            // remove polygon A
                        polygonList.RemoveAt(i);
                        break; // to stop the inner loop
                    }
                    else if (polygonRelationship == PolygonRelationship.AIsCompletelyInsideB
                                         || polygonRelationship == PolygonRelationship.AIsInsideBButVerticesTouch
                                            || polygonRelationship == PolygonRelationship.AIsInsideBButEdgesTouch)
                    {  // remove polygon B
                        polygonList.RemoveAt(j);
                        i--;
                    }
                    else
                    {
                        var newPolygons = Intersect(polygonList[i], polygonList[j], polygonRelationship, intersections, minAllowableArea);
                        polygonList.RemoveAt(i);
                        polygonList.RemoveAt(j);
                        polygonList.AddRange(newPolygons);
                        i = polygonList.Count; // to restart the outer loop
                        break; // to stop the inner loop
                    }
                }
            }
            return polygonList;
        }

        #endregion

        #region Subtract Public Methods
        /// <summary>
        /// Returns the list of polygons that result from A-B (subtracting polygon B from polygon A).
        /// </summary>
        /// <param name="polygonA">The polygon a.</param>
        /// <param name="polygonB">The polygon b.</param>
        /// <param name="minAllowableArea">The minimum allowable area.</param>
        /// <returns>System.Collections.Generic.List&lt;TVGL.TwoDimensional.Polygon&gt;.</returns>
        public static List<Polygon> Subtract(this Polygon polygonA, Polygon polygonB, double minAllowableArea = Constants.BaseTolerance)
        {
            var relationship = GetPolygonRelationshipAndIntersections(polygonA, polygonB, out var intersections);
            return Subtract(polygonA, polygonB, relationship, intersections, minAllowableArea);
        }

        /// <summary>
        /// Returns the list of polygons that result from A-B (subtracting polygon B from polygon A). By providing the intersections
        /// between the two polygons, the operation will be performed with less time and memory.
        /// </summary>
        /// <param name="polygonA">The polygon a.</param>
        /// <param name="polygonB">The polygon b.</param>
        /// <param name="polygonRelationship">The polygon relationship.</param>
        /// <param name="intersections">The intersections.</param>
        /// <param name="minAllowableArea">The minimum allowable area.</param>
        /// <returns>System.Collections.Generic.List&lt;TVGL.TwoDimensional.Polygon&gt;.</returns>
        /// <exception cref="ArgumentException">The minuend is already a negative polygon (i.e. hole). Consider another operation"
        ///                 +" to accopmlish this function, like Intersect. - polygonA</exception>
        /// <exception cref="ArgumentException">The subtrahend is already negative polygon (i.e. hole).Consider another operation"
        ///                 + " to accopmlish this function, like Intersect. - polygonB</exception>
        public static List<Polygon> Subtract(this Polygon polygonA, Polygon polygonB,
                    PolygonRelationship polygonRelationship, List<IntersectionData> intersections,
                    double minAllowableArea = Constants.BaseTolerance)
        {
            if (!polygonA.IsPositive) throw new ArgumentException("The minuend is already a negative polygon (i.e. hole). Consider another operation"
                + " to accopmlish this function, like Intersect.", "polygonA");
            if (!polygonB.IsPositive) throw new ArgumentException("The subtrahend is already a negative polygon (i.e. hole). Consider another operation"
                + " to accopmlish this function, like Intersect.", "polygonB");
            switch (polygonRelationship)
            {
                case PolygonRelationship.Separated:
                case PolygonRelationship.SeparatedButVerticesTouch:
                case PolygonRelationship.SeparatedButEdgesTouch:
                case PolygonRelationship.BIsInsideHoleOfA:
                case PolygonRelationship.BIsInsideHoleOfABButVerticesTouch:
                case PolygonRelationship.BIsInsideHoleOfABButEdgesTouch:
                case PolygonRelationship.AIsInsideHoleOfB:
                case PolygonRelationship.AIsInsideHoleOfBButVerticesTouch:
                case PolygonRelationship.AIsInsideHoleOfBButEdgesTouch:
                    return new List<Polygon> { polygonA.Copy() };
                case PolygonRelationship.BIsCompletelyInsideA:
                    var polygonACopy = polygonA.Copy();
                    var polygonBCopy = polygonB.Copy();
                    polygonBCopy.Reverse();
                    polygonACopy.AddHole(polygonBCopy);
                    return new List<Polygon> { polygonACopy };
                case PolygonRelationship.AIsCompletelyInsideB:
                case PolygonRelationship.AIsInsideBButVerticesTouch:
                case PolygonRelationship.AIsInsideBButEdgesTouch:
                    return new List<Polygon>();
                default:
                    //case PolygonRelationship.Intersect:
                    //case PolygonRelationship.BInsideAButVerticesTouch:
                    //case PolygonRelationship.BInsideAButEdgesTouch:
                    if (polygonDifference == null) polygonDifference = new PolygonDifference();
                    return polygonDifference.Run(polygonA, polygonB, intersections, minAllowableArea);
            }
        }

        #endregion

        #region Exclusive-OR Public Methods
        /// <summary>
        /// Returns the list of polygons that are the Exclusive-OR of the two input polygons. Exclusive-OR are the regions where one polgyon
        /// resides but not both.
        /// </summary>
        /// <param name="polygonA">The polygon a.</param>
        /// <param name="polygonB">The polygon b.</param>
        /// <param name="minAllowableArea">The minimum allowable area.</param>
        /// <returns>System.Collections.Generic.List&lt;TVGL.TwoDimensional.Polygon&gt;.</returns>
        public static List<Polygon> ExclusiveOr(this Polygon polygonA, Polygon polygonB, double minAllowableArea = Constants.BaseTolerance)
        {
            var relationship = GetPolygonRelationshipAndIntersections(polygonA, polygonB, out var intersections);
            return ExclusiveOr(polygonA, polygonB, relationship, intersections, minAllowableArea);
        }

        /// <summary>
        /// Returns the list of polygons that are the Exclusive-OR of the two input polygons. Exclusive-OR are the regions where one polgyon
        /// resides but not both. By providing the intersections between the two polygons, the operation will be performed with less time and memory.
        /// </summary>
        /// <param name="polygonA">The polygon a.</param>
        /// <param name="polygonB">The polygon b.</param>
        /// <param name="polygonRelationship">The polygon relationship.</param>
        /// <param name="intersections">The intersections.</param>
        /// <param name="minAllowableArea">The minimum allowable area.</param>
        /// <returns>System.Collections.Generic.List&lt;TVGL.TwoDimensional.Polygon&gt;.</returns>
        public static List<Polygon> ExclusiveOr(this Polygon polygonA, Polygon polygonB, PolygonRelationship polygonRelationship,
                    List<IntersectionData> intersections, double minAllowableArea = Constants.BaseTolerance)
        {
            switch (polygonRelationship)
            {
                case PolygonRelationship.Separated:
                case PolygonRelationship.SeparatedButVerticesTouch:
                case PolygonRelationship.SeparatedButEdgesTouch:
                case PolygonRelationship.BIsInsideHoleOfA:
                case PolygonRelationship.BIsInsideHoleOfABButVerticesTouch:
                case PolygonRelationship.BIsInsideHoleOfABButEdgesTouch:
                case PolygonRelationship.AIsInsideHoleOfB:
                case PolygonRelationship.AIsInsideHoleOfBButVerticesTouch:
                case PolygonRelationship.AIsInsideHoleOfBButEdgesTouch:
                    return new List<Polygon> { polygonA.Copy(), polygonB.Copy() };
                case PolygonRelationship.BIsCompletelyInsideA:
                    var polygonACopy1 = polygonA.Copy();
                    var polygonBCopy1 = polygonB.Copy();
                    polygonBCopy1.Reverse();
                    polygonACopy1.AddHole(polygonBCopy1);
                    return new List<Polygon> { polygonACopy1 };
                case PolygonRelationship.AIsCompletelyInsideB:
                    var polygonBCopy2 = polygonB.Copy();
                    var polygonACopy2 = polygonA.Copy();
                    polygonACopy2.Reverse();
                    polygonBCopy2.AddHole(polygonACopy2);
                    return new List<Polygon> { polygonBCopy2 };
                //case PolygonRelationship.Intersect:
                //case PolygonRelationship.AIsInsideBButVerticesTouch:
                //case PolygonRelationship.AIsInsideBButEdgesTouch:
                //case PolygonRelationship.BIsInsideAButVerticesTouch:
                //case PolygonRelationship.BIsInsideAButEdgesTouch:
                default:
                    if (polygonXOR == null) polygonXOR = new PolygonExclusiveOR();
                    return polygonXOR.Run(polygonA, polygonB, intersections, minAllowableArea);
            }
        }
        #endregion

        #region RemoveSelfIntersections Public Method
        public static List<Polygon> RemoveSelfIntersections(this Polygon polygon, bool noHoles, out List<Polygon> strayHoles,
            double minAllowableArea = Constants.BaseTolerance)
        {
            polygon = polygon.Simplify(Constants.BaseTolerance);
            var intersections = polygon.GetSelfIntersections();
            if (polygonRemoveIntersections == null) polygonRemoveIntersections = new PolygonRemoveIntersections();

            var intersectionLookup = polygonRemoveIntersections.MakeIntersectionLookupList(intersections, polygon, null, out var positivePolygons,
                out var negativePolygons, isSubtract, isUnion, bothApproachDirections); //store negative in increasing (from -inf to 0) area
            while (GetNextStartingIntersection(intersections, isSubtract, isUnion, bothApproachDirections, out var startingIntersection,
                out var startEdge, out var switchPolygon))
            {
                var polyCoordinates = MakePolygonThroughIntersections(intersectionLookup, intersections, startingIntersection,
                    startEdge, isSubtract, isUnion, bothApproachDirections, switchPolygon).ToList();
                var area = polyCoordinates.Area();
                if (area.IsNegligible(minAllowableArea)) continue;
                if (area < 0 && noHoles)
                {
                    polyCoordinates.Reverse();
                    positivePolygons.Add(area, new Polygon(polyCoordinates));
                }
                else if (area < 0) negativePolygons.Add(area, new Polygon(polyCoordinates));
                else positivePolygons.Add(area, new Polygon(polyCoordinates));
            }

            return CreateShallowPolygonTreesPostBooleanOperation(positivePolygons.Values.ToList(), negativePolygons.Values,
                out strayHoles, false, true);
        }
        #endregion

    }
}