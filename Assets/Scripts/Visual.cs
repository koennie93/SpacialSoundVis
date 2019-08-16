using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visual : MonoBehaviour
{

    public SpeakerInput input;
    public GameObject[] bars;
    private float[] visualScale;
    private Transform[] visualList;

    // Start is called before the first frame update
    void Start()
    {
        MakeCircle();
        //NeuralNetwork test = new NeuralNetwork(new int[] { 10, 5, 5, 10 });
    }

    void MakeBars()
    {
        bars = new GameObject[input.numBars];
        for (int i = 0; i < bars.Length; i++)
        {
            GameObject visiBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visiBar.transform.position = new Vector3(i, 0, 0);
            visiBar.transform.parent = transform;
            visiBar.name = "VisiBar " + i;
            bars[i] = visiBar;
        }
    }

    void MakeCircle()
    {
        visualScale = new float[input.numBars];
        bars = new GameObject[input.numBars];

        Vector3 center = Vector3.zero;
        float radius = 4.0f;

        for (int i = 0; i < input.numBars; i++)
        {
            float ang = i * 1.0f / input.numBars;
            ang = ang * Mathf.PI * 2;

            float x = center.x + Mathf.Cos(ang) * radius;
            float y = center.y + Mathf.Sin(ang) * radius;

            Vector3 pos = center + new Vector3(x, y, 0);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
            go.name = "Bar " + i;
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.05f, 1, 1);
            go.transform.rotation = Quaternion.LookRotation(Vector3.forward, pos);
            go.transform.parent = transform;
            bars[i] = go;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bars.Length != input.numBars)
        {
            foreach (GameObject bar in bars)
            {
                Destroy(bar);
            }

            MakeCircle();
        }

        // Since this is being changed on a seperate thread we do this to be safe
        lock (input.barData)
        {
            for (int i = 0; i < input.barData.Length; i++)
            {
                // Don't make the bars too short
                float curData = Mathf.Max(0.01f, input.barData[i]);
                if (i == 9) Debug.Log(curData);

                // Set offset so they stretch off the ground instead of expand in the air
                //bars[i].transform.position = new Vector3(i, curData / 2.0f * 10.0f, 0);
                bars[i].transform.localScale = new Vector3(0.05f, curData * 10.0f, 1);
            }
        }
    }
}
