using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;

using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { /* Implementation hidden. */ }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { /* Implementation hidden. */ }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { /* Implementation hidden. */ }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private readonly RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private readonly GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private readonly IGH_Component Component;
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private readonly int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments,
  /// Output parameters as ref arguments. You don't have to assign output parameters,
  /// they will have a default value.
  /// </summary>
  private void RunScript(object x, object y, ref object A)
  {
    {
      var AnntennGraph = new Graph3d((u,v) => {return new Point3d(u, v, u * u - v * v);});
      AnntennGraph.BakeSurface(RhinoDocument, 100, 100);
    }
    {
      var AnntennGraph = new Graph3d((u,v) => {return new Point3d(u, v, u * u * u + v * v);});
      var surf2 = AnntennGraph.GetSurface(100, 100);
      surf2.Translate(2, 0, 0);
      RhinoDocument.Objects.Add(surf2);
    }
  }

  // <Custom additional code> 

  /// <summary>
  /// 3d�O���t�������N���X�ł��B
  /// </summary>
  public class Graph3d{
    public delegate Point3d Function3d(double u,double v);
    /// <summary>
    /// �O���t�ŕ`�悷��֐��B
    /// </summary>
    public Function3d F;

    /// <summary>
    /// u�����̕`��͈́B
    /// </summary>
    public Interval uInterval = new Interval(-1, 1);
    /// <summary>
    /// v�����̕`��͈́B
    /// </summary>
    public Interval vInterval = new Interval(-1, 1);

    /// <summary>
    /// �R���X�g���N�^�B
    /// </summary>
    /// <param name="f">�O���t�ŕ`�悷��֐��B</param>
    public Graph3d(Function3d f){
      this.F = f;
    }

    /// <summary>
    /// �֐��������O���t���T�[�t�F�X�Ƃ��Ď擾���܂��B
    /// </summary>
    /// <param name="uCount">u�����̉𑜓x</param>
    /// <param name="vCount">v�����̉𑜓x�B</param>
    /// <returns>�ړI�̌`��B</returns>
    public Surface GetSurface(int uCount=100, int vCount=100){
      var points = new List<Point3d>();

      for(int u = 0;u < uCount;u++){
        for(int v = 0;v < vCount;v++){
          double uValue = ((double) u / (double) (uCount - 1)) * uInterval.Length + uInterval.Min;
          double vValue = ((double) v / (double) (vCount - 1)) * vInterval.Length + vInterval.Min;
          points.Add(F(uValue, vValue));
        }
      }
      return NurbsSurface.CreateFromPoints(points, uCount, vCount, 3, 3);
    }

    /// <summary>
    /// �֐��������T�[�t�F�X��Bake���܂��B
    /// </summary>
    /// <param name="RhinoDocument">�ʏ��RhinoDocument�Ɠ��͂��Ă��������B</param>
    /// <param name="uCount">u�����̉𑜓x�B</param>
    /// <param name="vCount">v�����̉𑜓x�B</param>
    /// <returns></returns>
    public void BakeSurface(RhinoDoc RhinoDocument, int uCount, int vCount){
      RhinoDocument.Objects.Add(GetSurface(uCount, vCount));
    }
  }

  // </Custom additional code> 
}