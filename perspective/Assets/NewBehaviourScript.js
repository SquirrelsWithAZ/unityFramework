#pragma strict
 
public var scrollSpeed : float = 0.01;
private var mesh : Mesh;
 
function Start() 
{
    mesh = this.transform.GetComponent(MeshFilter).mesh;
}
 
function Update() 
{
    SwapUVs();
}
 
function SwapUVs()
{
    var uvSwap : Vector2[] = mesh.uv;
 
    for (var b:int = 0; b < uvSwap.length; b ++)
    {
       uvSwap[b] += Vector2( scrollSpeed * Time.deltaTime, 0 );
    }
 
    mesh.uv = uvSwap;
}