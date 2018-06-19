﻿// ***********************************************************************
// Assembly         : TessellationAndVoxelizationGeometryLibrary
// Author           : Design Engineering Lab
// Created          : 02-27-2015
//
// Last Modified By : Matt Campbell
// ***********************************************************************
// <copyright file="Voxel.cs" company="Design Engineering Lab">
//     Copyright ©  2017
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;

namespace TVGL.Voxelization
{
    /// <summary>
    /// Interface IVoxel
    /// </summary>
    public interface IVoxel
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        long ID { get; }
        // int[] CoordinateIndices { get; }
        /// <summary>
        /// Gets the bottom coordinate.
        /// </summary>
        /// <value>The bottom coordinate.</value>
        double[] BottomCoordinate { get; }
        /// <summary>
        /// Gets the length of the side.
        /// </summary>
        /// <value>The length of the side.</value>
        double SideLength { get; }
        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <value>The role.</value>
        VoxelRoleTypes Role { get; }
        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <value>The level.</value>
        int Level { get; }
        /// <summary>
        /// Gets or sets a value indicating whether [BTM coord is inside].
        /// </summary>
        /// <value><c>true</c> if [BTM coord is inside]; otherwise, <c>false</c>.</value>
        bool BtmCoordIsInside { get; set; }
        /// <summary>
        /// Gets the coordinate indices.
        /// </summary>
        /// <value>The coordinate indices.</value>
        int[] CoordinateIndices { get; }
    }

    /// <summary>
    /// Struct Voxel
    /// </summary>
    /// <seealso cref="TVGL.Voxelization.IVoxel" />
    public struct Voxel : IVoxel
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public long ID { get; internal set; }
        // public int[] CoordinateIndices { get; internal set; }
        /// <summary>
        /// Gets the length of the side.
        /// </summary>
        /// <value>The length of the side.</value>
        public double SideLength { get; internal set; }
        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <value>The role.</value>
        public VoxelRoleTypes Role { get; internal set; }
        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <value>The level.</value>
        public int Level { get; internal set; }
        /// <summary>
        /// Gets the bottom coordinate.
        /// </summary>
        /// <value>The bottom coordinate.</value>
        public double[] BottomCoordinate { get; internal set; }
        /// <summary>
        /// Gets or sets a value indicating whether [BTM coord is inside].
        /// </summary>
        /// <value><c>true</c> if [BTM coord is inside]; otherwise, <c>false</c>.</value>
        public bool BtmCoordIsInside { get; set; }

        /// <summary>
        /// Gets the coordinate indices.
        /// </summary>
        /// <value>The coordinate indices.</value>
        public int[] CoordinateIndices => Constants.GetCoordinateIndices(ID, Level);

        /// <summary>
        /// Initializes a new instance of the <see cref="Voxel"/> struct.
        /// </summary>
        /// <param name="ID">The identifier.</param>
        /// <param name="solid">The solid.</param>
        internal Voxel(long ID, VoxelizedSolid solid)
        {
            this.ID = ID;
            Constants.GetRoleFlags(ID, out var level, out var role, out var btmIsInside);
            Role = role;
            Level = level;
            BtmCoordIsInside = btmIsInside;
            SideLength = solid.VoxelSideLengths[Level];
            BottomCoordinate = solid.GetRealCoordinates(ID, Level);
        }
        //todo: get rid of level as inputarg - maybe remove above constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Voxel"/> struct.
        /// </summary>
        /// <param name="ID">The identifier.</param>
        /// <param name="level">The level.</param>
        internal Voxel(long ID, int level)
        {
            this.ID = ID;
            Constants.GetRoleFlags(ID, out var leveldummy, out var role, out var btmIsInside);
            Role = role;
            Level = level;
            BtmCoordIsInside = btmIsInside;
            SideLength = double.NaN;
            BottomCoordinate = null;
        }
        //internal Voxel(int x, int y, int z, int level,int inputCoordLevel, VoxelRoleTypes role, bool btmIsInside)
        //{
        //    this.ID = Constants.MakeIDFromCoordinates(level,x, y, z, inputCoordLevel);
        //    Role = role;
        //    Level = level;
        //    BtmCoordIsInside = btmIsInside;
        //    SideLength = double.NaN;
        //    BottomCoordinate = null;
        //}
    }
    /// <summary>
    /// Class VoxelWithTessellationLinks.
    /// </summary>
    /// <seealso cref="TVGL.Voxelization.IVoxel" />
    public abstract class VoxelWithTessellationLinks : IVoxel
    {
        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <value>The level.</value>
        public abstract int Level { get; }
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public long ID { get; internal set; }

        /// <summary>
        /// Gets the bottom coordinate.
        /// </summary>
        /// <value>The bottom coordinate.</value>
        public double[] BottomCoordinate { get; internal set; }
        /// <summary>
        /// Gets the length of the side.
        /// </summary>
        /// <value>The length of the side.</value>
        public double SideLength { get; internal set; }
        /// <summary>
        /// Gets or sets a value indicating whether [BTM coord is inside].
        /// </summary>
        /// <value><c>true</c> if [BTM coord is inside]; otherwise, <c>false</c>.</value>
        public bool BtmCoordIsInside { get; set; }
        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <value>The role.</value>
        public VoxelRoleTypes Role { get; internal set; }
        /// <summary>
        /// The tessellation elements
        /// </summary>
        internal HashSet<TessellationBaseClass> TessellationElements;
        /// <summary>
        /// Gets the coordinate indices.
        /// </summary>
        /// <value>The coordinate indices.</value>
        public int[] CoordinateIndices => Constants.GetCoordinateIndices(ID, Level);

        /// <summary>
        /// Gets the faces.
        /// </summary>
        /// <value>The faces.</value>
        internal List<PolygonalFace> Faces => TessellationElements.Where(te => te is PolygonalFace).Cast<PolygonalFace>().ToList();
        /// <summary>
        /// Gets the edges.
        /// </summary>
        /// <value>The edges.</value>
        internal List<Edge> Edges => TessellationElements.Where(te => te is Edge).Cast<Edge>().ToList();
        /// <summary>
        /// Gets the vertices.
        /// </summary>
        /// <value>The vertices.</value>
        internal List<Vertex> Vertices => TessellationElements.Where(te => te is Vertex).Cast<Vertex>().ToList();
    }

    /// <summary>
    /// Class Voxel_Level0_Class.
    /// </summary>
    /// <seealso cref="TVGL.Voxelization.VoxelWithTessellationLinks" />
    public class Voxel_Level0_Class : VoxelWithTessellationLinks
    {
        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <value>The level.</value>
        public override int Level => 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Voxel_Level0_Class"/> class.
        /// </summary>
        /// <param name="ID">The identifier.</param>
        /// <param name="voxelRole">The voxel role.</param>
        /// <param name="solid">The solid.</param>
        public Voxel_Level0_Class(long ID, VoxelRoleTypes voxelRole, VoxelizedSolid solid)
        {
            this.ID = Constants.ClearFlagsFromID(ID);
            Role = voxelRole;
            if (Role == VoxelRoleTypes.Partial) this.ID += 1;
            else if (Role == VoxelRoleTypes.Partial) this.ID += 3;
            if (Role == VoxelRoleTypes.Full) BtmCoordIsInside = true;
            SideLength = solid.VoxelSideLengths[0];
            var coordinateIndices = Constants.GetCoordinateIndices(ID, 0);
            BottomCoordinate = solid.GetRealCoordinates(0, coordinateIndices[0], coordinateIndices[1], coordinateIndices[2]);
            if (Role == VoxelRoleTypes.Partial)
                InnerVoxels = new VoxelHashSet[solid.discretizationLevel];
        }
        /// <summary>
        /// The inner voxels
        /// </summary>
        internal VoxelHashSet[] InnerVoxels;
    }


    /// <summary>
    /// Class Voxel_Level1_Class.
    /// </summary>
    /// <seealso cref="TVGL.Voxelization.VoxelWithTessellationLinks" />
    public class Voxel_Level1_Class : VoxelWithTessellationLinks
    {
        /// <summary>
        /// Gets the level.
        /// </summary>
        /// <value>The level.</value>
        public override int Level => 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Voxel_Level1_Class"/> class.
        /// </summary>
        /// <param name="ID">The identifier.</param>
        /// <param name="voxelRole">The voxel role.</param>
        /// <param name="solid">The solid.</param>
        public Voxel_Level1_Class(long ID, VoxelRoleTypes voxelRole, VoxelizedSolid solid)
        {
            this.ID = Constants.ClearFlagsFromID(ID) + 16; //adding 10000 which indicates level1 although it cuts in on the 4th level of
            // the x-position.
            Role = voxelRole;
            if (Role == VoxelRoleTypes.Partial) this.ID += 1;
            else if (Role == VoxelRoleTypes.Partial) this.ID += 3;
            if (Role == VoxelRoleTypes.Full) BtmCoordIsInside = true;
            SideLength = solid.VoxelSideLengths[1];
            var coordinateIndices = Constants.GetCoordinateIndices(ID, 1);
            BottomCoordinate = solid.GetRealCoordinates(1, coordinateIndices[0], coordinateIndices[1], coordinateIndices[2]);
        }
    }
}
