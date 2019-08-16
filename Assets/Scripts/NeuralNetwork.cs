using System;
using UnityEngine;

public class NeuralNetwork
{
    int[] layer;
    Layer[] layers;

    public NeuralNetwork(int[] layer)
    {
        this.layer = new int[layer.Length];
        for (int i = 0; i < layer.Length; i++)
        {
            this.layer[i] = layer[i];
        }

        layers = new Layer[layer.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(layer[i], layer[i + 1]);
        }

        VisualizeNetwork(layers);
    }

    public class Layer
    {
        public int numberOfInputs; //# of neurons in the previous layer
        public int numberOfOutputs; //# of neurons in the current layer

        public float[] outputs; //outputs of this layer
        public float[] inputs; //inputs in into this layer
        public float[,] weights; //weights of this layer

        public static System.Random random = new System.Random(); //Static random class variable

        public Layer(int numberOfInputs, int numberOfOutputs)
        {
            this.numberOfInputs = numberOfInputs;
            this.numberOfOutputs = numberOfOutputs;

            //initilize datastructures
            outputs = new float[numberOfOutputs];
            inputs = new float[numberOfInputs];
            weights = new float[numberOfOutputs, numberOfInputs];


            InitilizeWeights(); //initilize weights
        }

        /// <summary>
        /// Initilize weights between -0.5 and 0.5
        /// </summary>
        public void InitilizeWeights()
        {
            for (int i = 0; i < numberOfOutputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] = (float)random.NextDouble() - 0.5f;
                }
            }
        }

        /// <summary>
        /// Feedforward this layer with a given input
        /// </summary>
        /// <param name="inputs">The output values of the previous layer</param>
        /// <returns></returns>
        public float[] FeedForward(float[] inputs)
        {
            this.inputs = inputs;// keep shallow copy which can be used for back propagation

            //feed forwards
            for (int i = 0; i < numberOfOutputs; i++)
            {
                outputs[i] = 0;
                for (int j = 0; j < numberOfInputs; j++)
                {
                    outputs[i] += inputs[j] * weights[i, j];
                }

                outputs[i] = (float)Math.Tanh(outputs[i]);
            }

            return outputs;
        }

    }

    public void VisualizeNetwork(Layer[] layers)
    {
        var xOffset = 9;
        var yOffset = 6;
        for (int i = 0; i < layers.Length; i++)
        {
            if (i == 0)
            {
                for (int x = 0; x < layers[i].numberOfInputs; x++)
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.position = new Vector3(i * xOffset - xOffset, x * yOffset, 0);
                }
            }
            for (int j = 0; j < layers[i].numberOfOutputs; j++)
            {                
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(i * xOffset, j * yOffset, 0);
                LineRenderer line = go.AddComponent<LineRenderer>();
                line.SetWidth(0.02f, 0.02f);
                line.positionCount = layers[i].numberOfInputs * 2;
                Vector3[] points = new Vector3[layers[i].numberOfInputs * 2];
                for (int k = 0; k < layers[i].numberOfInputs; k++)
                {                    
                    points[k * 2] = new Vector3(i * xOffset, j * yOffset, 0);
                    points[k * 2 + 1] = new Vector3(i * xOffset - xOffset, k * yOffset, 0);                    
                }
                line.SetPositions(points);
            }
        }
    }
}
