using UnityEngine;
using System.Collections;

public class OurCamera : MonoBehaviour {

    public Camera camera;
    public float gravity;
    public Vector3 cameraOffset;
    public float yScale;
    public float minY;

    public Bounds currentAreaOfInterest;
    public Vector3 idealPosition;

    public void Update()
    {
        // Figure out the content we want on-screen.
        Bounds b = new Bounds();
        bool first = true;

        foreach (Avatar a in Game.instance.grid.players)
        {
            if (first) { b = FindCollider(a.gameObject).bounds; first = false; }
            else b.Encapsulate(FindCollider(a.gameObject).bounds);
        }

        foreach (Prop p in Game.instance.grid.getProp(typeof(Capsule)))
        {
            Capsule c = p as Capsule;
            if (c != null)
            {
                b.Encapsulate(FindCollider(c.gameObject).bounds);
                if (c.Owner != null)
                    foreach (Prop p2 in Game.instance.grid.getProp(typeof(Spawn)))
                    {
                        Spawn s = p2 as Spawn;
                        if (s != null)
                        {
                            if (s.GoalForPlayerType == c.Owner.currentType)
                                b.Encapsulate(FindCollider(s.gameObject).bounds);
                        }
                    }
            }
        }

        currentAreaOfInterest = b;

        // Debug render.
        Debug.DrawLine(
            new Vector3(b.min.x, b.max.y, b.min.z),
            new Vector3(b.min.x, b.max.y, b.max.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(b.min.x, b.max.y, b.max.z),
            new Vector3(b.max.x, b.max.y, b.max.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(b.max.x, b.max.y, b.max.z),
            new Vector3(b.max.x, b.max.y, b.min.z),
            Color.red
        );
        Debug.DrawLine(
            new Vector3(b.max.x, b.max.y, b.min.z),
            new Vector3(b.min.x, b.max.y, b.min.z),
            Color.red
        );

        // Calculate our idealized position
        idealPosition = new Vector3(currentAreaOfInterest.center.x, 0, currentAreaOfInterest.center.z);
        idealPosition += Vector3.up * yScale * CalculateCameraZoomFromBounds(currentAreaOfInterest, camera);

        if (idealPosition.y < minY) idealPosition.y = minY;

        // Move towards our idealized position
        Vector3 d = idealPosition - camera.transform.position;
        if (d.magnitude > 0.0001f)
        {
            d *= gravity * Time.deltaTime;

            camera.transform.position += d;
            camera.transform.position += cameraOffset;
        }
    }

    public static Collider FindCollider(GameObject g)
    {
        Collider c = FindColliderR(g);
        if (c == null) throw new System.InvalidOperationException();
        return c;
    }

    private static Collider FindColliderR(GameObject g)
    {
        if (g.collider != null) return g.collider;
        for (int i = 0; i < g.transform.childCount; i++)
        {
            Collider c = FindCollider(g.transform.GetChild(i).gameObject);
            if (c != null) return c;
        }
        return null;
    }

    public static float CalculateCameraZoomFromBounds(Bounds targetBounds, Camera cam)
    {
        RadianFoV fov = new RadianFoV(cam.fieldOfView, cam.aspect);

        float xBound = targetBounds.extents.x / Mathf.Tan(fov._vertFoV);
        float zBound = targetBounds.extents.z / Mathf.Tan(fov._horizFoV);

        return Mathf.Max(xBound, zBound);
    }

    private struct RadianFoV
    {
        public float _horizFoV;
        public float _vertFoV;

        public RadianFoV(float fov, float aspect)
        {
            _horizFoV = fov * Mathf.Deg2Rad * 0.5f;
            _vertFoV = Mathf.Atan(Mathf.Tan(_horizFoV) * aspect);
        }
    }
}
