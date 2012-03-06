/*
    Copyright 2012 Kuribo64

    This file is part of SM64DSe.

    SM64DSe is free software: you can redistribute it and/or modify it under
    the terms of the GNU General Public License as published by the Free
    Software Foundation, either version 3 of the License, or (at your option)
    any later version.

    SM64DSe is distributed in the hope that it will be useful, but WITHOUT ANY 
    WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
    FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along 
    with SM64DSe. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SM64DSe
{
    // not quite the right place but whatever
    public class LevelSettings
    {
        public LevelSettings(NitroOverlay ovl)
        {
            m_Overlay = ovl;
            Background = (byte)(m_Overlay.Read8(0x78) >> 4);
            ObjectBanks = new uint[8];
            ObjectBanks[0] = m_Overlay.Read8(0x54);
            ObjectBanks[1] = m_Overlay.Read8(0x55);
            ObjectBanks[2] = m_Overlay.Read8(0x56);
            ObjectBanks[3] = m_Overlay.Read8(0x57);
            ObjectBanks[4] = m_Overlay.Read8(0x58);
            ObjectBanks[5] = m_Overlay.Read8(0x59);
            ObjectBanks[6] = m_Overlay.Read8(0x5A);
            ObjectBanks[7] = m_Overlay.Read32(0x5C);

            BMDFileID = m_Overlay.Read16(0x68);
            KCLFileID = m_Overlay.Read16(0x6A);
            MinimapTsetFileID = m_Overlay.Read16(0x6C);
            MinimapPalFileID = m_Overlay.Read16(0x6E);
        }

        public void SaveChanges()
        {
            m_Overlay.Write8(0x78, (byte)(0xF | (Background << 4)));
            m_Overlay.Write8(0x54, (byte)ObjectBanks[0]);
            m_Overlay.Write8(0x55, (byte)ObjectBanks[1]);
            m_Overlay.Write8(0x56, (byte)ObjectBanks[2]);
            m_Overlay.Write8(0x57, (byte)ObjectBanks[3]);
            m_Overlay.Write8(0x58, (byte)ObjectBanks[4]);
            m_Overlay.Write8(0x59, (byte)ObjectBanks[5]);
            m_Overlay.Write8(0x5A, (byte)ObjectBanks[6]);
            m_Overlay.Write32(0x5C, (uint)ObjectBanks[7]);
        }

        public NitroOverlay m_Overlay;
        public byte Background;
        public uint[] ObjectBanks;
        public ushort BMDFileID, KCLFileID, MinimapTsetFileID, MinimapPalFileID;
    }


    public class LevelObject
    {
        public static string[] k_Layers = { "(all stars)", "(star 1)", "(star 2)", "(star 3)", "(star 4)", "(star 5)", "(star 6)", "(star 7)" };

        public LevelObject(NitroOverlay ovl, uint offset, int layer)
        {
            m_Overlay = ovl;
            m_Offset = offset;
            m_Layer = layer;
            m_Area = 0;

            //m_TestMatrix = new Matrix4(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
        }

        public virtual void GenerateProperties() { }

        public virtual string GetDescription() { return "LevelObject"; }

        public virtual bool SupportsRotation() { return true; }
        // return value: bit0=refresh display, bit1=refresh propertygrid, bit2=refresh object list
        public virtual int SetProperty(string field, object newval) { return 0; }
        public virtual void SaveChanges() { }

        public virtual void Render(RenderMode mode)
        {
            if (!m_Renderer.GottaRender(mode))
                return;

            GL.PushMatrix();

            GL.Translate(Position);
            if (YRotation != 0f)
                GL.Rotate(YRotation, 0f, 1f, 0f);

            //GL.MultMatrix(ref m_TestMatrix);

            m_Renderer.Render(mode);

            GL.PopMatrix();
        }

        //public Matrix4 m_TestMatrix;

        public virtual void Release()
        {
            m_Renderer.Release();
        }
        

        public ushort ID;
        public Vector3 Position;
        public float YRotation;

        // object specific parameters
        // for standard objects: [0] = 16bit object param, [1] and [2] = what should be X and Z rotation
        // for simple objects: [0] = 7bit object param
        public ushort[] Parameters;

        public NitroOverlay m_Overlay;
        public uint m_Offset;
        public int m_Layer;
        public int m_Area;
        public uint m_UniqueID;
        public int m_Type;

        public ObjectRenderer m_Renderer;
        public PropertyTable m_Properties;
    }


    public class PseudoParameter
    {
        public PseudoParameter(ObjectDatabase.ObjectInfo.ParamInfo pinfo, ushort val)
        { m_ParamInfo = pinfo; m_ParamValue = val; }

        public ObjectDatabase.ObjectInfo.ParamInfo m_ParamInfo;
        public ushort m_ParamValue;
    }


    public class StandardObject : LevelObject
    {
        //private Hashtable m_PParams;

        public StandardObject(NitroOverlay ovl, uint offset, int num, int layer, int area)
            : base(ovl, offset, layer)
        {
            m_Area = area;
            m_UniqueID = (uint)(0x10000000 | num);
            m_Type = 0;

            ID = m_Overlay.Read16(m_Offset);
            Position.X = (float)((short)m_Overlay.Read16(m_Offset + 0x2)) / 1000f;
            Position.Y = (float)((short)m_Overlay.Read16(m_Offset + 0x4)) / 1000f;
            Position.Z = (float)((short)m_Overlay.Read16(m_Offset + 0x6)) / 1000f;
            YRotation = ((float)((short)m_Overlay.Read16(m_Offset + 0xA)) / 4096f) * 22.5f;

            Parameters = new ushort[3];
            Parameters[0] = m_Overlay.Read16(m_Offset + 0xE);
            Parameters[1] = m_Overlay.Read16(m_Offset + 0x8);
            Parameters[2] = m_Overlay.Read16(m_Offset + 0xC);

            m_Renderer = ObjectRenderer.FromLevelObject(this);
           // m_PParams = new Hashtable();
            m_Properties = new PropertyTable(); 
            GenerateProperties();
        }

        public override string GetDescription()
        {
            return String.Format("{0} - {1} {2}", ID, ObjectDatabase.m_ObjectInfo[ID].m_Name, k_Layers[m_Layer]);
        }

        public override void GenerateProperties()
        {
            m_Properties.Properties.Clear();

            m_Properties.Properties.Add(new PropertySpec("Star", typeof(int), "General", "Which star(s) the object appears for. (all or one)", m_Layer, "", typeof(LayerTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Area", typeof(int), "General", "Which level area the object works in.", m_Area));
            m_Properties.Properties.Add(new PropertySpec("Object ID", typeof(ushort), "General", "What the object will be.", ID, typeof(ObjectIDTypeEditor), typeof(ObjectIDTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("X position", typeof(float), "General", "The object's position along the X axis.", Position.X, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y position", typeof(float), "General", "The object's position along the Y axis.", Position.Y, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Z position", typeof(float), "General", "The object's position along the Z axis.", Position.Z, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y rotation", typeof(float), "General", "The angle in degrees the object is rotated around the Y axis.", YRotation, "", typeof(FloatTypeConverter)));

            /*foreach (ObjectDatabase.ObjectInfo.ParamInfo oparam in ObjectDatabase.m_ObjectInfo[ID].m_ParamInfo)
            {
                uint pmask = (uint)(Math.Pow(2, oparam.m_Length) - 1);
                ushort val = (ushort)((Parameters[oparam.m_Offset >> 4] >> (oparam.m_Offset & 0xF)) & pmask);
                PseudoParameter pparam = new PseudoParameter(oparam, val);

                m_PParams.Add(oparam.m_Name, pparam);

                m_Properties.Properties.Add(new PropertySpec(oparam.m_Name, typeof(PseudoParameter), "Object-specific", oparam.m_Description, pparam, "", typeof(UIntParamTypeConverter)));
                m_Properties[oparam.m_Name] = pparam;
            }*/

            m_Properties.Properties.Add(new PropertySpec("Parameter 1", typeof(ushort), "Object-specific (raw)", "", Parameters[0], "", typeof(HexNumberTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 2", typeof(ushort), "Object-specific (raw)", "", Parameters[1], "", typeof(HexNumberTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 3", typeof(ushort), "Object-specific (raw)", "", Parameters[2], "", typeof(HexNumberTypeConverter)));

            m_Properties["Star"] = m_Layer;
            m_Properties["Area"] = m_Area;
            m_Properties["Object ID"] = ID;
            m_Properties["X position"] = Position.X;
            m_Properties["Y position"] = Position.Y;
            m_Properties["Z position"] = Position.Z;
            m_Properties["Y rotation"] = YRotation;
            m_Properties["Parameter 1"] = Parameters[0];
            m_Properties["Parameter 2"] = Parameters[1];
            m_Properties["Parameter 3"] = Parameters[2];
        }

        public override int SetProperty(string field, object newval)
        {
            /*if (m_PParams.Contains(field))
            {
                PseudoParameter pparam = (PseudoParameter)m_PParams[field];
                uint pmask = (uint)(Math.Pow(2, pparam.m_ParamInfo.m_Length) - 1);
                Parameters[pparam.m_ParamInfo.m_Offset >> 4] &= (ushort)(~(pmask << (pparam.m_ParamInfo.m_Offset & 0xF)));
                Parameters[pparam.m_ParamInfo.m_Offset >> 4] |= (ushort)((pparam.m_ParamValue & pmask) << (pparam.m_ParamInfo.m_Offset & 0xF));
                
                m_Properties["Parameter 1"] = Parameters[0];
                m_Properties["Parameter 2"] = Parameters[1];
                m_Properties["Parameter 3"] = Parameters[2];

                m_Renderer.Release();
                m_Renderer = ObjectRenderer.FromLevelObject(this);

                return 3;
            }
            else*/
            {
                switch (field)
                {
                    case "Object ID": ID = (ushort)newval; break;
                    case "X position": Position.X = (float)newval; break;
                    case "Y position": Position.Y = (float)newval; break;
                    case "Z position": Position.Z = (float)newval; break;
                    case "Y rotation": YRotation = (float)newval; break;
                    case "Parameter 1": Parameters[0] = (ushort)newval; break;
                    case "Parameter 2": Parameters[1] = (ushort)newval; break;
                    case "Parameter 3": Parameters[2] = (ushort)newval; break;
                }

                if ((field == "Object ID") || (field.IndexOf("Parameter ") != -1))
                {
                    m_Renderer.Release();
                    m_Renderer = ObjectRenderer.FromLevelObject(this);
                    //return 3;
                }

                if (field == "Object ID")
                    return 5;
            }

            return 1;
        }

        public override void SaveChanges()
        {
            m_Overlay.Write16(m_Offset, ID);
            m_Overlay.Write16(m_Offset + 0x2, (ushort)((short)(Position.X * 1000f)));
            m_Overlay.Write16(m_Offset + 0x4, (ushort)((short)(Position.Y * 1000f)));
            m_Overlay.Write16(m_Offset + 0x6, (ushort)((short)(Position.Z * 1000f)));
            m_Overlay.Write16(m_Offset + 0xA, (ushort)((short)((YRotation / 22.5f) * 4096f)));

            m_Overlay.Write16(m_Offset + 0xE, Parameters[0]);
            m_Overlay.Write16(m_Offset + 0x8, Parameters[1]);
            m_Overlay.Write16(m_Offset + 0xC, Parameters[2]);
        }
    }

    public class SimpleObject : LevelObject
    {
        public SimpleObject(NitroOverlay ovl, uint offset, int num, int layer, int area)
            : base(ovl, offset, layer)
        {
            m_Area = area;
            m_UniqueID = (uint)(0x10000000 | num);
            m_Type = 5;

            ushort idparam = m_Overlay.Read16(m_Offset);
            ID = (ushort)(idparam & 0x1FF);
            Position.X = (float)((short)m_Overlay.Read16(m_Offset + 0x2)) / 1000f;
            Position.Y = (float)((short)m_Overlay.Read16(m_Offset + 0x4)) / 1000f;
            Position.Z = (float)((short)m_Overlay.Read16(m_Offset + 0x6)) / 1000f;
            YRotation = 0.0f;

            Parameters = new ushort[1];
            Parameters[0] = (ushort)(idparam >> 9);

            m_Renderer = ObjectRenderer.FromLevelObject(this);
            m_Properties = new PropertyTable();
            GenerateProperties();
        }

        public override string GetDescription()
        {
            if (ID == 511) return "511 - Minimap change " + k_Layers[m_Layer];
            return String.Format("{0} - {1} {2}", ID, ObjectDatabase.m_ObjectInfo[ID].m_Name, k_Layers[m_Layer]);
        }

        public override void GenerateProperties()
        {
            m_Properties.Properties.Clear();

            m_Properties.Properties.Add(new PropertySpec("Star", typeof(int), "General", "Which star(s) the object appears for. (all or one)", m_Layer, "", typeof(LayerTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Area", typeof(int), "General", "Which level area the object works in.", m_Area));
            m_Properties.Properties.Add(new PropertySpec("Object ID", typeof(ushort), "General", "What the object will be.", ID, typeof(ObjectIDTypeEditor), typeof(ObjectIDTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("X position", typeof(float), "General", "The object's position along the X axis.", Position.X, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y position", typeof(float), "General", "The object's position along the Y axis.", Position.Y, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Z position", typeof(float), "General", "The object's position along the Z axis.", Position.Z, "", typeof(FloatTypeConverter)));

            m_Properties.Properties.Add(new PropertySpec("Parameter 1", typeof(ushort), "Object-specific (raw)", "", Parameters[0], "", typeof(HexNumberTypeConverter)));

            m_Properties["Star"] = m_Layer;
            m_Properties["Area"] = m_Area;
            m_Properties["Object ID"] = ID;
            m_Properties["X position"] = Position.X;
            m_Properties["Y position"] = Position.Y;
            m_Properties["Z position"] = Position.Z;
            m_Properties["Parameter 1"] = Parameters[0];
        }
        
        public override bool SupportsRotation() { return false; }
        
        public override int SetProperty(string field, object newval)
        {
            switch (field)
            {
                case "Object ID": ID = (ushort)newval; break;
                case "X position": Position.X = (float)newval; break;
                case "Y position": Position.Y = (float)newval; break;
                case "Z position": Position.Z = (float)newval; break;
                case "Parameter 1": Parameters[0] = (ushort)newval; break;
            }

            if ((field == "Object ID") || (field == "Parameter 1"))
            {
                m_Renderer.Release();
                m_Renderer = ObjectRenderer.FromLevelObject(this);
            }

            if (field == "Object ID")
                return 5;

            return 1;
        }

        public override void SaveChanges()
        {
            ushort idparam = (ushort)((ID & 0x1FF) | (Parameters[0] << 9));
            m_Overlay.Write16(m_Offset, idparam);
            m_Overlay.Write16(m_Offset + 0x2, (ushort)((short)(Position.X * 1000f)));
            m_Overlay.Write16(m_Offset + 0x4, (ushort)((short)(Position.Y * 1000f)));
            m_Overlay.Write16(m_Offset + 0x6, (ushort)((short)(Position.Z * 1000f)));
        }
    }

    public class EntranceObject : LevelObject
    {
        public int m_EntranceID;

        public EntranceObject(NitroOverlay ovl, uint offset, int num, int layer, int id)
            : base(ovl, offset, layer)
        {
            m_UniqueID = (uint)(0x20000000 | num);
            m_EntranceID = id;
            m_Type = 1;

            ID = 0;
            Position.X = (float)((short)m_Overlay.Read16(m_Offset + 0x2)) / 1000.0f;
            Position.Y = (float)((short)m_Overlay.Read16(m_Offset + 0x4)) / 1000.0f;
            Position.Z = (float)((short)m_Overlay.Read16(m_Offset + 0x6)) / 1000.0f;
            YRotation = ((float)((short)m_Overlay.Read16(m_Offset + 0xA)) / 4096f) * 22.5f;

            Parameters = new ushort[5];
            Parameters[0] = m_Overlay.Read16(m_Offset + 0x0);
            Parameters[1] = m_Overlay.Read16(m_Offset + 0x8);
            Parameters[2] = m_Overlay.Read16(m_Offset + 0xA);
            Parameters[3] = m_Overlay.Read16(m_Offset + 0xC);
            Parameters[4] = m_Overlay.Read16(m_Offset + 0xE);

            m_Renderer = new ColorCubeRenderer(Color.FromArgb(0, 255, 0), Color.FromArgb(0, 64, 0), true);
            m_Properties = new PropertyTable(); 
            GenerateProperties();
        }

        public override string GetDescription()
        {
            // TODO describe better
            return string.Format("[{0}] Entrance", m_EntranceID);
        }

        public override void GenerateProperties()
        {
            m_Properties.Properties.Clear();

            m_Properties.Properties.Add(new PropertySpec("X position", typeof(float), "General", "The entrance's position along the X axis.", Position.X, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y position", typeof(float), "General", "The entrance's position along the Y axis.", Position.Y, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Z position", typeof(float), "General", "The entrance's position along the Z axis.", Position.Z, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y rotation", typeof(float), "General", "The angle in degrees the entrance is rotated around the Y axis.", YRotation, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 1", typeof(ushort), "Specific", "Purpose unknown.", Parameters[0], "", typeof(HexNumberTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 2", typeof(ushort), "Specific", "Purpose unknown.", Parameters[1], "", typeof(HexNumberTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 3", typeof(ushort), "Specific", "Purpose unknown.", Parameters[2], "", typeof(HexNumberTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 4", typeof(ushort), "Specific", "Purpose unknown.", Parameters[3], "", typeof(HexNumberTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 5", typeof(ushort), "Specific", "Purpose unknown.", Parameters[4], "", typeof(HexNumberTypeConverter)));

            m_Properties["X position"] = Position.X;
            m_Properties["Y position"] = Position.Y;
            m_Properties["Z position"] = Position.Z;
            m_Properties["Y rotation"] = YRotation;
            m_Properties["Parameter 1"] = Parameters[0];
            m_Properties["Parameter 2"] = Parameters[1];
            m_Properties["Parameter 3"] = Parameters[2];
            m_Properties["Parameter 4"] = Parameters[3];
            m_Properties["Parameter 5"] = Parameters[4];
        }

        public override int SetProperty(string field, object newval)
        {
            switch (field)
            {
                case "X position": Position.X = (float)newval; return 1;
                case "Y position": Position.Y = (float)newval; return 1;
                case "Z position": Position.Z = (float)newval; return 1;
                case "Y rotation": YRotation = (float)newval; return 1;

                case "Parameter 1": Parameters[0] = (ushort)newval; return 0;
                case "Parameter 2": Parameters[1] = (ushort)newval; return 0;
                case "Parameter 3": Parameters[2] = (ushort)newval; return 0;
                case "Parameter 4": Parameters[3] = (ushort)newval; return 0;
                case "Parameter 5": Parameters[4] = (ushort)newval; return 0;
            }

            return 0;
        }

        public override void SaveChanges()
        {
            m_Overlay.Write16(m_Offset + 0x2, (ushort)((short)(Position.X * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x4, (ushort)((short)(Position.Y * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x6, (ushort)((short)(Position.Z * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0xA, (ushort)((short)((YRotation / 22.5f) * 4096f)));

            m_Overlay.Write16(m_Offset + 0x0, Parameters[0]);
            m_Overlay.Write16(m_Offset + 0x8, Parameters[1]);
            m_Overlay.Write16(m_Offset + 0xA, Parameters[2]);
            m_Overlay.Write16(m_Offset + 0xC, Parameters[3]);
            m_Overlay.Write16(m_Offset + 0xE, Parameters[4]);
        }
    }

    public class ExitObject : LevelObject
    {
        public int LevelID, EntranceID;
        public ushort Param1, Param2;

        public ExitObject(NitroOverlay ovl, uint offset, int num, int layer)
            : base(ovl, offset, layer)
        {
            m_UniqueID = (uint)(0x20000000 | num);
            m_Type = 10;

            Position.X = (float)((short)m_Overlay.Read16(m_Offset)) / 1000.0f;
            Position.Y = (float)((short)m_Overlay.Read16(m_Offset + 0x2)) / 1000.0f;
            Position.Z = (float)((short)m_Overlay.Read16(m_Offset + 0x4)) / 1000.0f;
            YRotation = ((float)((short)m_Overlay.Read16(m_Offset + 0x8)) / 4096f) * 22.5f;

            LevelID = m_Overlay.Read8(m_Offset + 0xA);
            EntranceID = m_Overlay.Read8(m_Offset + 0xB);
            Param1 = m_Overlay.Read16(m_Offset + 0x6);
            Param2 = m_Overlay.Read16(m_Offset + 0xC);

            m_Renderer = new ColorCubeRenderer(Color.FromArgb(255, 0, 0), Color.FromArgb(64, 0, 0), true);
            m_Properties = new PropertyTable(); 
            GenerateProperties();
        }

        public override string GetDescription()
        {
            return string.Format("Exit ({0}, entrance {1}) {2}", Strings.LevelNames[LevelID], EntranceID, k_Layers[m_Layer]);
        }

        public override void GenerateProperties()
        {
            m_Properties.Properties.Clear();

            m_Properties.Properties.Add(new PropertySpec("Star", typeof(int), "General", "Which star(s) the exit works for. (all or one)", m_Layer, "", typeof(LayerTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("X position", typeof(float), "General", "The exit's position along the X axis.", Position.X, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y position", typeof(float), "General", "The exit's position along the Y axis.", Position.Y, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Z position", typeof(float), "General", "The exit's position along the Z axis.", Position.Z, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y rotation", typeof(float), "General", "The angle in degrees the exit is rotated around the Y axis.", YRotation, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Destination level", typeof(int), "Specific", "The level the exit leads to.", LevelID, "", typeof(LevelIDTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Destination entrance", typeof(int), "Specific", "The ID of the entrance in the destination level, the exit is connected to.", EntranceID));
            m_Properties.Properties.Add(new PropertySpec("Parameter 1", typeof(ushort), "Specific", "Purpose unknown.", Param1, "", typeof(HexNumberTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 2", typeof(ushort), "Specific", "Purpose unknown.", Param2, "", typeof(HexNumberTypeConverter)));

            m_Properties["Star"] = m_Layer;
            m_Properties["X position"] = Position.X;
            m_Properties["Y position"] = Position.Y;
            m_Properties["Z position"] = Position.Z;
            m_Properties["Y rotation"] = YRotation;
            m_Properties["Destination level"] = LevelID;
            m_Properties["Destination entrance"] = EntranceID;
            m_Properties["Parameter 1"] = Param1;
            m_Properties["Parameter 2"] = Param2;
        }

        public override int SetProperty(string field, object newval)
        {
            switch (field)
            {
                case "X position": Position.X = (float)newval; return 1;
                case "Y position": Position.Y = (float)newval; return 1;
                case "Z position": Position.Z = (float)newval; return 1;
                case "Y rotation": YRotation = (float)newval; return 1;
                case "Destination level":
                    if (newval is string) LevelID = int.Parse(((string)newval).Substring(0, ((string)newval).IndexOf(" - ")));
                    else LevelID = (int)newval; 
                    return 4;
                case "Destination entrance": EntranceID = (int)newval; return 4;
                case "Parameter 1": Param1 = (ushort)newval; return 0;
                case "Parameter 2": Param2 = (ushort)newval; return 0;
            }

            return 0;
        }

        public override void SaveChanges()
        {
            m_Overlay.Write16(m_Offset + 0x0, (ushort)((short)(Position.X * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x2, (ushort)((short)(Position.Y * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x4, (ushort)((short)(Position.Z * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x8, (ushort)((short)((YRotation / 22.5f) * 4096f)));

            m_Overlay.Write8(m_Offset + 0xA, (byte)LevelID);
            m_Overlay.Write8(m_Offset + 0xB, (byte)EntranceID);
            m_Overlay.Write16(m_Offset + 0x6, Param1);
            m_Overlay.Write16(m_Offset + 0xC, Param2);
        }
    }

    public class DoorObject : LevelObject
    {
        public int DoorType, OutAreaID, InAreaID, PlaneSizeX, PlaneSizeY;

        public DoorObject(NitroOverlay ovl, uint offset, int num, int layer)
            : base(ovl, offset, layer)
        {
            m_UniqueID = (uint)(0x20000000 | num);
            m_Type = 9;

            ID = 0;
            Position.X = (float)((short)m_Overlay.Read16(m_Offset + 0x0)) / 1000f;
            Position.Y = (float)((short)m_Overlay.Read16(m_Offset + 0x2)) / 1000f;
            Position.Z = (float)((short)m_Overlay.Read16(m_Offset + 0x4)) / 1000f;
            YRotation = ((float)((short)m_Overlay.Read16(m_Offset + 0x6)) / 4096f) * 22.5f;

            DoorType = m_Overlay.Read8(m_Offset + 0xA);
            if (DoorType > 0x17) DoorType = 0x17;

            InAreaID = m_Overlay.Read8(m_Offset + 0x9);
            OutAreaID = InAreaID >> 4;
            InAreaID &= 0xF;

            PlaneSizeX = m_Overlay.Read8(m_Offset + 0x8);
            PlaneSizeY = PlaneSizeX >> 4;
            PlaneSizeX &= 0xF;
            
            m_Renderer = new DoorRenderer(this);
            m_Properties = new PropertyTable(); 
            GenerateProperties();
        }

        public override string GetDescription()
        {
            return string.Format("{1}, areas {2}/{3} {0}", k_Layers[m_Layer], Strings.DoorTypes[DoorType], OutAreaID, InAreaID);
        }

        public override void GenerateProperties()
        {
            m_Properties.Properties.Clear();

            m_Properties.Properties.Add(new PropertySpec("Star", typeof(int), "General", "Which star(s) the door appears for. (all or one)", m_Layer, "", typeof(LayerTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("X position", typeof(float), "General", "The door's position along the X axis.", Position.X, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y position", typeof(float), "General", "The door's position along the Y axis.", Position.Y, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Z position", typeof(float), "General", "The door's position along the Z axis.", Position.Z, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y rotation", typeof(float), "General", "The angle in degrees the door is rotated around the Y axis.", YRotation, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Door type", typeof(int), "Specific", "What the door will look like.", DoorType, "", typeof(DoorTypeTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Outside area", typeof(int), "Specific", "The ID of the 'outside' area.", OutAreaID));
            m_Properties.Properties.Add(new PropertySpec("Inside area", typeof(int), "Specific", "The ID of the 'inside' area.", InAreaID));
            m_Properties.Properties.Add(new PropertySpec("Plane width", typeof(int), "Specific", "For virtual doors, the width of the door plane.", PlaneSizeX, "", typeof(Size16TypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Plane height", typeof(int), "Specific", "For virtual doors, the height of the door plane.", PlaneSizeY, "", typeof(Size16TypeConverter)));

            m_Properties["Star"] = m_Layer;
            m_Properties["X position"] = Position.X;
            m_Properties["Y position"] = Position.Y;
            m_Properties["Z position"] = Position.Z;
            m_Properties["Y rotation"] = YRotation;
            m_Properties["Door type"] = DoorType;
            m_Properties["Outside area"] = OutAreaID;
            m_Properties["Inside area"] = InAreaID;
            m_Properties["Plane width"] = PlaneSizeX;
            m_Properties["Plane height"] = PlaneSizeY;
        }

        public override int SetProperty(string field, object newval)
        {
            switch (field)
            {
                case "X position": Position.X = (float)newval; return 1;
                case "Y position": Position.Y = (float)newval; return 1;
                case "Z position": Position.Z = (float)newval; return 1;
                case "Y rotation": YRotation = (float)newval; return 1;

                case "Door type":
                    if (newval is int) DoorType = (int)newval;
                    else DoorType = int.Parse(((string)newval).Substring(0, ((string)newval).IndexOf(" - ")));
                    m_Renderer.Release();
                    m_Renderer = new DoorRenderer(this);
                    return 5;

                case "Outside area": OutAreaID = Math.Max(0, Math.Min(15, (int)newval)); return 1;
                case "Inside area": InAreaID = Math.Max(0, Math.Min(15, (int)newval)); return 1;

                case "Plane width":
                    if (newval is int) PlaneSizeX = (int)newval;
                    else PlaneSizeX = int.Parse(((string)newval).Substring(0, ((string)newval).IndexOf("/"))) - 1;
                    return 1;

                case "Plane height":
                    if (newval is int) PlaneSizeY = (int)newval;
                    else PlaneSizeY = int.Parse(((string)newval).Substring(0, ((string)newval).IndexOf("/"))) - 1;
                    return 1;
            }

            return 0;
        }

        public override void SaveChanges()
        {
            m_Overlay.Write16(m_Offset + 0x0, (ushort)((short)(Position.X * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x2, (ushort)((short)(Position.Y * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x4, (ushort)((short)(Position.Z * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x6, (ushort)((short)((YRotation / 22.5f) * 4096f)));

            m_Overlay.Write8(m_Offset + 0x8, (byte)(PlaneSizeX | (PlaneSizeY << 4)));
            m_Overlay.Write8(m_Offset + 0x9, (byte)(InAreaID | (OutAreaID << 4)));
            m_Overlay.Write8(m_Offset + 0xA, (byte)DoorType);
        }
    }

    public class PathPointObject : LevelObject
    {
        public PathPointObject(NitroOverlay ovl, uint offset, int num)
            : base(ovl, offset, 0)
        {
            m_UniqueID = (uint)(0x30000000 | num);
            m_Type = 2;

            //ID = m_Overlay.Read16(m_Offset);
            Position.X = (float)((short)m_Overlay.Read16(m_Offset)) / 1000f;
            Position.Y = (float)((short)m_Overlay.Read16(m_Offset + 0x2)) / 1000f;
            Position.Z = (float)((short)m_Overlay.Read16(m_Offset + 0x4)) / 1000f;
            YRotation = 0.0f;

            Parameters = new ushort[1];
            // Parameters[0] = (ushort)(idparam >> 9);

            m_Renderer = new ColorCubeRenderer(Color.FromArgb(0, 255, 255), Color.FromArgb(0, 64, 64), false);
        }

        public override string GetDescription()
        {
            return "PathPointObject";
        }

        public override void SaveChanges()
        {
            //  m_Overlay.Write16(m_Offset, ID);
            //  m_Overlay.Write16(m_Offset + 0x2, (ushort)((short)(Position.X * 1000.0f)));
            // m_Overlay.Write16(m_Offset + 0x4, (ushort)((short)(Position.Y * 1000.0f)));
            // m_Overlay.Write16(m_Offset + 0x6, (ushort)((short)(Position.Z * 1000.0f)));
        }
    }

    public class PathObject : LevelObject
    {
        public PathObject(NitroOverlay ovl, uint offset, int num)
            : base(ovl, offset, 0)
        {
            m_UniqueID = (uint)(0x30000000 | num);
            m_Type = 3;
        }

        public override string GetDescription()
        {
            return "PathObject lol";
        }

        public override void SaveChanges()
        {
            //  m_Overlay.Write16(m_Offset, ID);
            //  m_Overlay.Write16(m_Offset + 0x2, (ushort)((short)(Position.X * 1000.0f)));
            // m_Overlay.Write16(m_Offset + 0x4, (ushort)((short)(Position.Y * 1000.0f)));
            // m_Overlay.Write16(m_Offset + 0x6, (ushort)((short)(Position.Z * 1000.0f)));
        }

        public override void Render(RenderMode mode) { }
        public override void Release() { }
    }

    public class ViewObject : LevelObject
    {
        public ViewObject(NitroOverlay ovl, uint offset, int num)
            : base(ovl, offset, 0)
        {
            m_UniqueID = (uint)(0x40000000 | num);
            m_Type = 4;

            Position.X = (float)((short)m_Overlay.Read16(m_Offset + 0x2)) / 1000.0f;
            Position.Y = (float)((short)m_Overlay.Read16(m_Offset + 0x4)) / 1000.0f;
            Position.Z = (float)((short)m_Overlay.Read16(m_Offset + 0x6)) / 1000.0f;
            YRotation = ((float)((short)m_Overlay.Read16(m_Offset + 0xA)) / 4096f) * 22.5f;

            Parameters = new ushort[3];
            Parameters[0] = m_Overlay.Read16(m_Offset + 0x0);
            Parameters[1] = m_Overlay.Read16(m_Offset + 0x8);
            Parameters[2] = m_Overlay.Read16(m_Offset + 0xC);

            m_Renderer = new ColorCubeRenderer(Color.FromArgb(255, 255, 0), Color.FromArgb(64, 64, 0), true);
            m_Properties = new PropertyTable(); 
            GenerateProperties();
        }

        public override string GetDescription()
        {
            return "View";
        }

        public override void GenerateProperties()
        {
            m_Properties.Properties.Clear();

            m_Properties.Properties.Add(new PropertySpec("X position", typeof(float), "General", "The view's position along the X axis.", Position.X, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y position", typeof(float), "General", "The view's position along the Y axis.", Position.Y, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Z position", typeof(float), "General", "The view's position along the Z axis.", Position.Z, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y rotation", typeof(float), "General", "The angle in degrees the view is rotated around the Y axis.", YRotation, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 1", typeof(ushort), "Specific", "Purpose unknown.", Parameters[0], "", typeof(HexNumberTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 2", typeof(ushort), "Specific", "Purpose unknown.", Parameters[1], "", typeof(HexNumberTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter 3", typeof(ushort), "Specific", "Purpose unknown.", Parameters[2], "", typeof(HexNumberTypeConverter)));

            m_Properties["X position"] = Position.X;
            m_Properties["Y position"] = Position.Y;
            m_Properties["Z position"] = Position.Z;
            m_Properties["Y rotation"] = YRotation;
            m_Properties["Parameter 1"] = Parameters[0];
            m_Properties["Parameter 2"] = Parameters[1];
            m_Properties["Parameter 3"] = Parameters[2];
        }

        public override int SetProperty(string field, object newval)
        {
            switch (field)
            {
                case "X position": Position.X = (float)newval; return 1;
                case "Y position": Position.Y = (float)newval; return 1;
                case "Z position": Position.Z = (float)newval; return 1;
                case "Y rotation": YRotation = (float)newval; return 1;

                case "Parameter 1": Parameters[0] = (ushort)newval; return 0;
                case "Parameter 2": Parameters[1] = (ushort)newval; return 0;
                case "Parameter 3": Parameters[2] = (ushort)newval; return 0;
            }

            return 0;
        }

        public override void SaveChanges()
        {
             m_Overlay.Write16(m_Offset + 0x2, (ushort)((short)(Position.X * 1000.0f)));
             m_Overlay.Write16(m_Offset + 0x4, (ushort)((short)(Position.Y * 1000.0f)));
             m_Overlay.Write16(m_Offset + 0x6, (ushort)((short)(Position.Z * 1000.0f)));
             m_Overlay.Write16(m_Offset + 0xA, (ushort)((short)((YRotation / 22.5f) * 4096f)));

             m_Overlay.Write16(m_Offset + 0x0, Parameters[0]);
             m_Overlay.Write16(m_Offset + 0x8, Parameters[1]);
             m_Overlay.Write16(m_Offset + 0xC, Parameters[2]);
        }
    }

    public class TpSrcObject : LevelObject
    {
        public TpSrcObject(NitroOverlay ovl, uint offset, int num, int layer)
            : base(ovl, offset, layer)
        {
            m_UniqueID = (uint)(0x20000000 | num);
            m_Type = 6;

            Position.X = (float)((short)m_Overlay.Read16(m_Offset)) / 1000.0f;
            Position.Y = (float)((short)m_Overlay.Read16(m_Offset + 0x2)) / 1000.0f;
            Position.Z = (float)((short)m_Overlay.Read16(m_Offset + 0x4)) / 1000.0f;
            YRotation = 0.0f;

            Parameters = new ushort[1];
            Parameters[0] = m_Overlay.Read8(m_Offset + 0x6);

            m_Renderer = new ColorCubeRenderer(Color.FromArgb(255, 0, 255), Color.FromArgb(64, 0, 64), false);
            m_Properties = new PropertyTable();
            GenerateProperties();
        }

        public override string GetDescription()
        {
            return "Teleport source " + k_Layers[m_Layer];
        }

        public override void GenerateProperties()
        {
            m_Properties.Properties.Clear();

            m_Properties.Properties.Add(new PropertySpec("X position", typeof(float), "General", "The teleport source's position along the X axis.", Position.X, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y position", typeof(float), "General", "The teleport source's position along the Y axis.", Position.Y, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Z position", typeof(float), "General", "The teleport source's position along the Z axis.", Position.Z, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter", typeof(ushort), "Specific", "Purpose unknown.", Parameters[0], "", typeof(HexNumberTypeConverter)));

            m_Properties["X position"] = Position.X;
            m_Properties["Y position"] = Position.Y;
            m_Properties["Z position"] = Position.Z;
            m_Properties["Parameter"] = Parameters[0];
        }

        public override int SetProperty(string field, object newval)
        {
            switch (field)
            {
                case "X position": Position.X = (float)newval; return 1;
                case "Y position": Position.Y = (float)newval; return 1;
                case "Z position": Position.Z = (float)newval; return 1;

                case "Parameter": Parameters[0] = (ushort)newval; return 0;
            }

            return 0;
        }

        public override void SaveChanges()
        {
            m_Overlay.Write16(m_Offset + 0x0, (ushort)((short)(Position.X * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x2, (ushort)((short)(Position.Y * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x4, (ushort)((short)(Position.Z * 1000.0f)));

            m_Overlay.Write16(m_Offset + 0x6, Parameters[0]);
        }
    }

    public class TpDstObject : LevelObject
    {
        public TpDstObject(NitroOverlay ovl, uint offset, int num, int layer)
            : base(ovl, offset, layer)
        {
            m_UniqueID = (uint)(0x20000000 | num);
            m_Type = 7;

            Position.X = (float)((short)m_Overlay.Read16(m_Offset)) / 1000.0f;
            Position.Y = (float)((short)m_Overlay.Read16(m_Offset + 0x2)) / 1000.0f;
            Position.Z = (float)((short)m_Overlay.Read16(m_Offset + 0x4)) / 1000.0f;
            YRotation = 0.0f;

            Parameters = new ushort[1];
            Parameters[0] = m_Overlay.Read16(m_Offset + 0x6);

            m_Renderer = new ColorCubeRenderer(Color.FromArgb(255, 128, 0), Color.FromArgb(64, 32, 0), false);
            m_Properties = new PropertyTable();
            GenerateProperties();
        }

        public override string GetDescription()
        {
            return "Teleport destination " + k_Layers[m_Layer];
        }

        public override void GenerateProperties()
        {
            m_Properties.Properties.Clear();

            m_Properties.Properties.Add(new PropertySpec("X position", typeof(float), "General", "The teleport destination's position along the X axis.", Position.X, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Y position", typeof(float), "General", "The teleport destination's position along the Y axis.", Position.Y, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Z position", typeof(float), "General", "The teleport destination's position along the Z axis.", Position.Z, "", typeof(FloatTypeConverter)));
            m_Properties.Properties.Add(new PropertySpec("Parameter", typeof(ushort), "Specific", "Purpose unknown.", Parameters[0], "", typeof(HexNumberTypeConverter)));

            m_Properties["X position"] = Position.X;
            m_Properties["Y position"] = Position.Y;
            m_Properties["Z position"] = Position.Z;
            m_Properties["Parameter"] = Parameters[0];
        }

        public override int SetProperty(string field, object newval)
        {
            switch (field)
            {
                case "X position": Position.X = (float)newval; return 1;
                case "Y position": Position.Y = (float)newval; return 1;
                case "Z position": Position.Z = (float)newval; return 1;

                case "Parameter": Parameters[0] = (ushort)newval; return 0;
            }

            return 0;
        }

        public override void SaveChanges()
        {
            m_Overlay.Write16(m_Offset + 0x0, (ushort)((short)(Position.X * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x2, (ushort)((short)(Position.Y * 1000.0f)));
            m_Overlay.Write16(m_Offset + 0x4, (ushort)((short)(Position.Z * 1000.0f)));

            m_Overlay.Write16(m_Offset + 0x6, Parameters[0]);
        }
    }


    public class LevelTexAnim
    {
        public LevelTexAnim(NitroOverlay ovl, uint tbloffset, uint offset, int num, int area)
        {
            m_Overlay = ovl;
            m_Offset = offset;
            m_UniqueID = (uint)num;
            m_Area = area;

            m_ScaleTblOffset = m_Overlay.ReadPointer(tbloffset + 0x4) + (uint)(m_Overlay.Read16(m_Offset + 0xE) * 4);
            m_RotTblOffset = m_Overlay.ReadPointer(tbloffset + 0x8) + (uint)(m_Overlay.Read16(m_Offset + 0x12) * 2);
            m_TransTblOffset = m_Overlay.ReadPointer(tbloffset + 0xC) + (uint)(m_Overlay.Read16(m_Offset + 0x16) * 4);
            m_ScaleTblSize = m_Overlay.Read16(m_Offset + 0xC);
            m_RotTblSize = m_Overlay.Read16(m_Offset + 0x10);
            m_TransTblSize = m_Overlay.Read16(m_Offset + 0x14);

            m_MatNameOffset = m_Overlay.ReadPointer(m_Offset + 0x4);
            m_MatName = m_Overlay.ReadString(m_MatNameOffset, 0);
        }

        public string GetDescription()
        {
            return string.Format("{0} (S:{1} R:{2} T:{3})", m_MatName, m_ScaleTblSize, m_RotTblSize, m_TransTblSize);
        }

        public void SaveChanges()
        {
        }

        public NitroOverlay m_Overlay;
        public uint m_Offset;
        public int m_Area;
        public uint m_UniqueID;

        public uint m_ScaleTblOffset;
        public uint m_RotTblOffset;
        public uint m_TransTblOffset;
        public ushort m_ScaleTblSize;
        public ushort m_RotTblSize;
        public ushort m_TransTblSize;

        public uint m_MatNameOffset;
        public string m_MatName;
    }
}
