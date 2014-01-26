using UnityEngine;
using System.Collections;

public class Spin : MonoBehaviour {
    public Vector3[] spin_axis;
    public float[] spin_period;

    // Update is called once per frame
    void Update () {
        int max = Mathf.Min(spin_axis.Length, spin_period.Length);
        for (int i = 0; i < max; i++)
            transform.Rotate(spin_axis[i], Time.deltaTime * spin_period[i], Space.World);
    }
}
