# Grasshopperについて
## よく使う手法
GrasshopperのC#ではよく使う手法がいくつかあります。  
サンプルはsrcにおいてあります。
### 3dグラフ
3次元上の複数の点を通るサーフェスを表現する際に便利なのは``NurbsSurface.CreateFromPoints()``関数です。この関数はPoint3dの配列からそれを通るNurbsSurfaceを作ってくれます。
厳密な意味で正しい形状が出来るわけではありませんが、それっぽい形を作るには便利です。  
000.3dGraph.ghxではサンプルを
```
  private void RunScript(Interval x, Interval y, ref object A)
  {
    int uCount = 100;//u方向の解像度
    int vCount = 100;//v方向の解像度
    var points = new List<Point3d>();

    for(int u = 0;u < uCount;u++){
      for(int v = 0;v < vCount;v++){
        double uValue = ((double) u / (double) (uCount - 1)) * x.Length + x.Min;
        double vValue = ((double) v / (double) (vCount - 1)) * y.Length + y.Min;
        points.Add(GetPoint(uValue, vValue));
      }
    }

    A = Rhino.Geometry.NurbsSurface.CreateFromPoints(points.ToArray(), uCount, vCount, 3, 3);
  }
```
003.3dGraphClass.ghxではより実践的なサンプルを載せています。クラスやdelegateといった説明していない複雑な手法を用いています。
時間がない場合にはあまり詳しく読み込む事はお勧めしません。そのままコピーして使ってください。
```
  /// <summary>
  /// 3dグラフを示すクラスです。
  /// </summary>
  public class Graph3d{
    public delegate Point3d Function3d(double u,double v);
    /// <summary>
    /// グラフで描画する関数。
    /// </summary>
    public Function3d F;

    /// <summary>
    /// u方向の描画範囲。
    /// </summary>
    public Interval uInterval = new Interval(-1, 1);
    /// <summary>
    /// v方向の描画範囲。
    /// </summary>
    public Interval vInterval = new Interval(-1, 1);

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    /// <param name="f">グラフで描画する関数。</param>
    public Graph3d(Function3d f){
      this.F = f;
    }

    /// <summary>
    /// 関数が示すグラフをサーフェスとして取得します。
    /// </summary>
    /// <param name="uCount">u方向の解像度</param>
    /// <param name="vCount">v方向の解像度。</param>
    /// <returns>目的の形状。</returns>
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
    /// 関数が示すサーフェスをBakeします。
    /// </summary>
    /// <param name="RhinoDocument">通常はRhinoDocumentと入力してください。</param>
    /// <param name="uCount">u方向の解像度。</param>
    /// <param name="vCount">v方向の解像度。</param>
    /// <returns></returns>
    public void BakeSurface(RhinoDoc RhinoDocument, int uCount, int vCount){
      RhinoDocument.Objects.Add(GetSurface(uCount, vCount));
    }
  }
```
### 自動的にBakeする
通常grasshopperではオブジェクトを追加する場合には手作業でBakeを行わなければならずレイヤー分けも自分で行わなければなりません。
その際に便利なのは、``RhinoDocument.Objects.Add(geometry);``関数です。
この関数はRhinoDocumentというScript_Instanceクラスで使われている関数なのでその外や子クラスでは受け渡しをしないと使えませんし、場合によっては"RhinoDocument"という部分が変わります。
つまり、RunScript関数内か引数にRhinoDoc型を持つ関数内で使える関数です。  
この関数の使い方はとても簡単です。たとえば、C#コンポーネントをつくり入力xの型をGeometryBaseにして、RunScriptの中身を以下のように書き換えてみてください。
```
  RhinoDocument.Objects.Add(x);
```
入力xに適当なジオメトリ、例えば球を入力すれば入力した瞬間にRhinoceros画面上にそのジオメトリが追加されます。  
このやり方で注意が必要なのは入力値が変わるたび毎にジオメトリが追加されることです。
例えば球のサイズをスライダーで入力していれば一度に動かすだけでいくつも球が追加されます。
その対策として、C#コンポーネントの入力のひとつをbool型にしてそれにBooleanToggleコンポーネントをつなぐという手法がよく使われます。
面倒なら右クリックのLock Solverで再計算を無効にしておき、必要になるたびにF5で一括Bakeをするというやり方もあります。
```
  private void RunScript(GeometryBase x, bool y, ref object A)
  {
    if(y){RhinoDocument.Objects.Add(x);}
  }
```
一方で便利な点はオプションでいくつもの設定が可能な点です。それはこの関数が引数にひとつ、ふたつどちらの数の引数でももてるからです。
ふたつ持つ場合にはふたつ目の引数は``ObjectAttributes``型になります。例えば、色を設定したかったら次のようにします。
```
    var objAttr = new ObjectAttributes();
    objAttr.ColorSource = ObjectColorSource.ColorFromObject;
    objAttr.ObjectColor = Color.Red;
    RhinoDocument.Objects.Add(x, objAttr);
```
その他にレイヤーを自動設定することもできます。その場合は既にあるレイヤーの場合``RhinoDocument.Layers.FindByFullPath(レイヤー名, false);``から、まだないレイヤーの場合には``RhinoDocument.Layers.Add(レイヤー名)``によって作成しレイヤー番号を取得できます。
レイヤーがない場合には前者は-1を返すのでifを使って対処できます。
その後上の例なら``objAttr.LayerIndex``に上で得たレイヤー番号を代入してください。  
004.BakeAdvanced.ghxは簡単に色とレイヤー名を指定できるようにしたものです。
```
  private void RunScript(GeometryBase obj, List<Color> color, string layer, bool bake)
  {
    try{
      var objAttr = new ObjectAttributes();
      if(color.Count() != 0){
        objAttr.ColorSource = ObjectColorSource.ColorFromObject;
        objAttr.ObjectColor = color[0];
      }
      if(layer != ""){
        objAttr.LayerIndex = GetLayerNumber(RhinoDocument, layer);
      }
      if(bake){
        RhinoDocument.Objects.Add(obj, objAttr);
      }
    }catch(Exception e){
      Print(e.ToString());
    }
  }

  // <Custom additional code> 

  public int GetLayerNumber(RhinoDoc rd, string layerName){
    int layerCount = rd.Layers.FindByFullPath(layerName, true);
    if(layerCount == -1){
      layerCount = rd.Layers.Add(layerName, Color.White);
    }
    return layerCount;
  }
```
