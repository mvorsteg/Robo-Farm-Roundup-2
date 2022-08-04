using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class Utility {


    /* given a point x and a line stemming from point t forward, find the point closest to x that lies on t */
    public static Vector3 GetClosestPointOnLine(Transform t, Vector3 x)
    {
        Vector3 p = t.position;
        Vector3 q = t.position + t.forward * 5;
        float k = Vector3.Dot(x - p, q - p) / Vector3.Dot(q - p, q - p);
        return p + k * (q - p);
    }

    /*  returns the closest n gameObjects within a radius with a certain tag 
        the array returned may be fully or partially empty 
        Runs in O(nk)*/
    public static GameObject[] GetNClosestWithTag(Vector3 pos, float radius, int n, string tag)
    {
        Tuple<float, GameObject>[] closest = new Tuple<float, GameObject>[n]; // store distance to object, and the object, in a tuple
        int j = 0;
        Collider[] cols = Physics.OverlapSphere(pos, radius); // get all colliders in radius
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].tag == tag)
            {
                float val = (pos - cols[i].transform.position).sqrMagnitude; // get distance of current object
                // if only 1 object, skip the complex step
                if (n == 1)
                {
                    if (j == 0 || val < closest[0].Item1)
                    {
                        closest[0] = Tuple.Create(val, cols[i].gameObject);
                        j = 1;
                    }
                }
                else
                {
                    if (j < n || val < closest[n - 2].Item1) // if any slot is empty or new val is smaller than largest in closest
                    {
                        // add to correct position
                        int idx = n - 2;
                        for (int k = 0; k < idx; k++)
                        {
                            if (k >= j || val < closest[k].Item1) // if the current slot is open OR new val is smaller than val at k
                            {
                                idx = k; // prevent further searching
                            }
                        }
                        // now shift all entries in array down by 1, starting after idx
                        for (int k = n - 1; k > idx; k--)
                        {
                            closest[k] = closest[k - 1];
                        }
                        closest[idx] = Tuple.Create(val, cols[i].gameObject); // add new object at correct position
                        j++;
                    }
                }
            }
        }
        //return only the 2nd item from each tuple
        GameObject[] keepers = new GameObject[n];
        for (int i = 0; i < n; i++)
        {
            if (closest[i] != null)
            {
                keepers[i] = closest[i].Item2;
            }
        }
        return keepers;
    }

    /*  can be used in place of WaitForSeconds when timeScale != 1.0 */
    public static IEnumerator WaitForRealSeconds(float seconds)
    {
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup-startTime < seconds) {
            yield return null;
        }
    }

#if (UNITY_EDITOR) 
    [MenuItem("Assets/Save RenderTexture to file")]
    public static void SaveRTToFile()
    {
        RenderTexture rt = Selection.activeObject as RenderTexture;

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = tex.EncodeToPNG();
        
        string path = AssetDatabase.GetAssetPath(rt) + ".png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);
        Debug.Log("Saved to " + path);
    }

    [MenuItem("Assets/Save RenderTexture to file", true)]
    public static bool SaveRTToFileValidation()
    {
        return Selection.activeObject is RenderTexture;
    }
#endif

}