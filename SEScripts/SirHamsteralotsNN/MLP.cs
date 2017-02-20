using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;

//using SEScripts.Lib;

    namespace SEScripts.SirHamsteralotsNN
{
    class Program : MyGridProgram
    {

        class Net
        {
            public List<Layer> m_layers = new List<Layer> { };
            public Layer m_resultLayer;

            float m_error = 0;
            //float m_recentAveragedError;
            //float m_recentAveragedSmoothingFactor = 0f;

            public Net(int[] topology)
            {
                List<Neuron> layerNeurons;
                for (int layerNum = 0; layerNum < topology.Length - 2; layerNum++)
                {
                    //create a new layer and add to the m_layer container
                    m_layers.Add(new Layer());

                    layerNeurons = new List<Neuron>();
                    for (int neuronNum = 0; neuronNum < topology[layerNum]; neuronNum++)
                    {
                        layerNeurons.Add(new Neuron(topology[layerNum + 1], neuronNum));    //add a new neuron to the layer container of neurons
                    }
                    m_layers[layerNum].neuronas = layerNeurons;
                    
                }
                //create output layer with linear activation function
                m_layers.Add(new Layer());
                layerNeurons = new List<Neuron>();
                for (int neuronNum = 0; neuronNum < topology[topology.Length - 2]; neuronNum++)
                {
                    layerNeurons.Add(new Neuron(topology[topology.Length - 1], neuronNum));
                }
                m_layers[m_layers.Count - 1].neuronas = layerNeurons;

                m_resultLayer = new Layer();
                layerNeurons = new List<Neuron>();
                for (int neuronNum = 0; neuronNum < topology[topology.Length - 1]; neuronNum++)
                {
                    layerNeurons.Add(new Neuron(0, neuronNum));
                }
                m_resultLayer.neuronas = layerNeurons;
            }

            public void feedForward(List<double> inputValues)
            {
                //P.Echo("Feed");
                int inputNeurons = m_layers[0].neuronas.Count;
                //P.Echo("inputs to first layer");
                //P.Echo("input size: " + inputValues.Count);
                //P.Echo("neurons number: " + m_layers[0].neuronas.Count);
                //inputs to first layer
                for (int i = 0; i < inputNeurons; i++)
                {
                    m_layers[0].neuronas[i].InputValue = (float)inputValues[i];
                }

                //P.Echo("feeding through layers");
                //forward propagation
                for (int layerNum = 1; layerNum < m_layers.Count; layerNum++)
                {//from the first hidden layer on

                    for (int n = 0; n < m_layers[layerNum].neuronas.Count; n++)
                    {
                        m_layers[layerNum].neuronas[n].FeedForward(m_layers[layerNum - 1]);
                    }
                }

                //P.Echo("feeding to result layer");
                for (int n = 0; n < m_resultLayer.neuronas.Count; n++)
                {
                    m_resultLayer.neuronas[n].FeedForward(m_layers[m_layers.Count - 1]);
                }

                /*P.Echo("Feedforward Result:");
                for(int i = 0; i < m_layers.Count; i++)
                {
                    string inputs = "[";
                    foreach(double n in m_layers[i].InputValues)
                    {
                        inputs += n + ",";
                    }
                    P.Echo("layer " + i + ": " + inputs + "]");
                }
                P.Echo("result layer: " + m_resultLayer.InputValues[0]);*/
            }

            public void BackProp(float[] targetValues)
            {
                //Calculate overall net error (RMS)
                
                m_error = 0;
                int numNeuronas = m_layers[m_layers.Count - 1].neuronas.Count;//without bias

                m_layers[m_layers.Count - 1].Delta = new float[numNeuronas];

                for (int n = 0; n < targetValues.Length; n++)
                {
                    float delta = targetValues[n] - m_resultLayer.InputValues[n];
                    m_layers[m_layers.Count - 1].Delta[n] = delta;
                    m_error += delta * delta;
                }//check the error of the network
                m_error /= numNeuronas;
                m_error = m_error;

                //P.Echo("error: " + m_error.ToString());

                //Implement a recent average measurement
                //m_recentAveragedError = (m_recentAveragedError * m_recentAveragedSmoothingFactor + m_error)
                /// (m_recentAveragedSmoothingFactor * 1.0f);
                
                //Calculate gradients on hidden layers
                for (int layerNum = m_layers.Count - 2; layerNum >= 0; layerNum--)
                {           //check if layerNum >0 , o layerNum>=0
                    Layer currentLayer = m_layers[layerNum];
                    Layer nextLayer = m_layers[layerNum + 1];
                    currentLayer.CalculateDeltas(nextLayer);
                }

                //For all layers from output to first hidden layer,
                //update connection weights

                for (int i = 0; i < m_layers.Count; i ++)
                {
                    P.Echo("UpdateWeights of layer [" + i + "]");
                    m_layers[i].UpdateWeights();
                }
            }

            public float[] getResults()
            {
                return m_resultLayer.InputValues;
            }

            public string GetError()
            {
                return m_error.ToString();
            }

            /*public string GetAverageError() {
              return m_recentAveragedError.ToString();
            }
            */

            public string ExportToCSV()
            {
                string neurons = "";

                for (int layerNum = 0; layerNum < m_layers.Count; layerNum++)
                {// for each layer save the neurons weights in string
                 //for each neuron on this layer, including bias, add a row of values
                    for (int neuron = 0; neuron < m_layers[layerNum].neuronas.Count; neuron++)
                    {
                        Neuron currentNeuron = m_layers[layerNum].neuronas[neuron];// get current neuron

                        string weightsString = "";

                        for (int weightNum = 0; weightNum < currentNeuron.neuronWeights.Count; weightNum++)
                        {// all the neuron weights in this neuron
                            weightsString += currentNeuron.neuronWeights[weightNum].weight;
                            if (weightNum < currentNeuron.neuronWeights.Count - 1)
                            {//add separator, unless is the last weight
                                weightsString += ",";
                            }
                        }
                        neurons += weightsString + "\n";// append the line of neuron weights to string//nextline = next neuron
                    }
                }

                return neurons;
            }

            public void ImportWeightsFromCSV(string WeightsCVSstring)
            {
                ///Should receive a string in the form of "w1,1,w1,2,...w1,n \n w2,1,w2,2,...w2,n \n .... wm,1,wm,2,...wm,n \n "

                string[] lines = WeightsCVSstring.Split('\n');
                int neuronCount = 0;

                for (int layer = 0; layer < m_layers.Count - 1; layer++)
                {                    //every layer except for the output

                    for (int n = 0; n < m_layers[layer].neuronas.Count; n++)
                    {                // for each neuron in the layer
                                     //current neuron
                        Neuron currentNeuron = m_layers[layer].neuronas[n];                      // current neuron

                        currentNeuron.LoadCSVWeights(lines[neuronCount]);
                        neuronCount++;
                    }
                }

                P.Echo("Imported weights");
                return;
            }
        }

        public class Neuron
        {
            //learning variables
            float m_gradient, eta = 0.5f, alpha = 0.5f;
            int neuronIndex;
            Layer m_layer;
            public float InputValue;

            public List<Connections> neuronWeights = new List<Connections> { };

            public Neuron(int outputNum, int m_neuronIndex)
            {
                neuronIndex = m_neuronIndex;

                for (int connect = 0; connect < outputNum; connect++)
                {
                    neuronWeights.Add(new Connections());
                    neuronWeights[connect].weight = 1.0f;// RandomWeight();
                }

                P.Echo("Neuron Created!");

            }

            float RandomWeight()
            {
                Random rand = new Random();

                return ((float)rand.NextDouble());
            }

            public void FeedForward(Layer previousLayer)
            {
                //P.Echo("N.Feed");
                float sum = 0;
                //weightsum the inputs of the last layer, including bias
                for (int n = 0; n < previousLayer.InputValues.Length; n++)
                {
                    sum += previousLayer.neuronas[n].InputValue *
                        previousLayer.neuronas[n].neuronWeights[neuronIndex].weight;
                }

                InputValue = previousLayer.TransferFunction(sum);
            }

            public void LoadCSVWeights(string CSVWeights)
            {
                ///should receive a string in the form of "weight1,weight2,...,weightn"
                string[] weightString = CSVWeights.Split(',');                             // separate individual weights

                for (int w = 0; w < weightString.Length; w++)
                {                             //for every weight assign the weight to the network

                    float readWeight = 0;

                    if (!float.TryParse(weightString[w], out readWeight))
                    {                   // try to convert the text to an float number
                        return;
                    }

                    neuronWeights[w].weight = readWeight;                                    //set the read value to the weight number
                }
            }

            public void UpdateWeights()
            {
                for (int i = 0; i < neuronWeights.Count; i++)
                {
                    float deltaWeight = InputValue * m_layer.Delta[i] * eta;
                    neuronWeights[i].weight += deltaWeight;
                    P.Echo("Delta is: " + m_layer.Delta[i]);
                    P.Echo("Input is: " + InputValue);
                    P.Echo("Updated weight [" + i + "] to : " + neuronWeights[i].weight);
                }
            }
            

            public void SetLayer(Layer layer)
            {
                m_layer = layer;
            }
        }

        public class Layer
        {
            public Func<float, float> TransferFunction;
            public Func<float, float> TransferFunctionDerivative;

            public float[] Delta;
            private List<Neuron> m_neurons = new List<Neuron> { };
            public List<Neuron> neuronas
            {
                get { return m_neurons; }
                set
                {
                    m_neurons = new List<Neuron> { };
                    foreach (Neuron neuron in value)
                    {
                        neuron.SetLayer(this);
                        m_neurons.Add(neuron);
                    }
                }
            }
            public float[] InputValues
            {
                get
                {
                    float[] values = new float[m_neurons.Count];
                    for (int i = 0; i < m_neurons.Count; i++)
                    {
                        values[i] = m_neurons[i].InputValue;
                    }
                    return values;
                }
            }

            public Layer() : this(Layer.SigmoidFunction, Layer.SigmoidFunctionDerivative) { }

            public Layer(Func<float, float> transferFunction, Func<float,float> transferFunctionDeriv)
            {
                TransferFunction = transferFunction;
                TransferFunctionDerivative = transferFunctionDeriv;
            }

            public void CalculateDeltas(Layer nextLayer)
            {
                // calculate nextLayer.Weights.Transpose() * nextLayer.Delta
                Delta = new float[nextLayer.neuronas.Count];
                for (int j = 0; j < nextLayer.neuronas.Count; j++)
                {
                    Delta[j] = 0;
                    for (int i = 0; i < nextLayer.neuronas[j].neuronWeights.Count; i++)
                    {
                        Delta[j] += neuronas[i].neuronWeights[j].weight * nextLayer.Delta[i];
                    }
                    Delta[j] = Delta[j] * TransferFunctionDerivative(nextLayer.neuronas[j].InputValue);
                }
            }

            public void UpdateWeights()
            {
                foreach (Neuron neuron in m_neurons)
                {
                    neuron.UpdateWeights();
                }
            }

            public static float SigmoidFunction(float sum)
            {
                //sigmoid function
                return ((float)(1 / (1 + Math.Exp(-sum))));
            }

            public static float SigmoidFunctionDerivative(float sum)
            {

                return (sum * (1 - sum));
            }

            public static float LinearFunction(float sum)
            {
                return sum;
            }

            public static float LinearFunctionDerivative(float sum)
            {
                return sum;
            }
        }

        public class Connections
        {
            public float weight, deltaWeight;
        }

        public class Training
        {
            string[][] InputData;
            string[][] OutputData;

            //expects string in format I I I,O O O\n  1s AND 0s ONLY
            public Training(string data)
            {
                data.Trim();

                List<string> trainingData = new List<string>(data.Split('\n'));

                InputData = new string[trainingData.Count][];
                OutputData = new string[trainingData.Count][];

                //split each value
                for (int i = 0; i < trainingData.Count; i++)
                {
                    string[] temp = trainingData[i].Split(',');
                    OutputData[i] = temp[1].Split('_');
                    InputData[i] = temp[0].Split('_');
                }
            }

            public double[][] GetInputData()
            {
                double[][] tempArr = new double[InputData.Length][];

                for (int i = 0; i < tempArr.Length; i++)
                {
                    tempArr[i] = new double[InputData[0].Length];
                }

                for (int i = 0; i < tempArr.Length; i++)
                {
                    for (int t = 0; t < tempArr[i].Length; t++)
                    {
                        tempArr[i][t] = Double.Parse(InputData[i][t]);
                    }
                }
                return tempArr;
            }

            public int[][] GetOutputData()
            {
                int[][] tempArr = new int[OutputData.Length][];

                for (int i = 0; i < tempArr.Length; i++)
                {
                    tempArr[i] = new int[OutputData[0].Length];
                }

                for (int i = 0; i < tempArr.Length; i++)
                {
                    for (int t = 0; t < tempArr[i].Length; t++)
                    {
                        tempArr[i][t] = Int32.Parse(OutputData[i][t]);
                    }
                }
                return tempArr;
            }
        }
        
        /*=============================================== NON ESSENTIAL SHIT ===============================================*/
        //initialize code
        double[][] InputData;
        int[][] OutputData;

        static MyGridProgram P;
        Net myNet;
        Training myTraining;
        IMyTimerBlock Timer;

        public Program()
        {
            P = this;

            int[] topology = new int[3];
            topology[0] = 2;
            topology[1] = 3;
            topology[2] = 1;
            //topology[1] = 1;
            //topology[3] = 1;
            //topology[4] = 1;
            //topology[5] = 1;
            myNet = new Net(topology);
            Timer = GridTerminalSystem.GetBlockWithName("MLP Timer") as IMyTimerBlock;
        }

        public void Save()
        {
            Storage = myNet.ExportToCSV();
        }

        //run main thing
        int counter = 0;
        bool done = false;
        bool firstRun = true;

        void SubMain(string argument)
        {
            //if(firstRun && Storage != "") {
            //  myNet.ImportWeightsFromCSV(Storage);
            //}

            if (argument == "train" && !done)
            {  /*
                myNet.feedForward(new List<double> { 0d, 0d });
                P.Echo("0,0 => " + myNet.getResults()[0] + " / 0");
                myNet.feedForward(new List<double> { 1d, 0d });
                P.Echo("1,0 => " + myNet.getResults()[0] + " / 0.731");
                myNet.feedForward(new List<double> { 0d, 1d });
                P.Echo("0,1 => " + myNet.getResults()[0] + " / 0.731");
                myNet.feedForward(new List<double> { 1d, 1d });
                P.Echo("1,1 => " + myNet.getResults()[0] + " / 0.919");
                return;*/


                counter++;


                if (firstRun)
                {
                    myTraining = new Training(Me.CustomData);
                    InputData = myTraining.GetInputData();
                    OutputData = myTraining.GetOutputData();
                    firstRun = false;
                    /*Echo("input values:");
                    foreach (double[] ds in InputData)
                    {
                        string line = "  ";
                        foreach (double d in ds)
                        {
                            line += d + ",";
                        }
                        Echo(line);
                    }*/
                }

                float ErrorTotal = 0;
                for (int i = 0; i < InputData.Length; i++)
                {
                    List<double> inputVals = new List<double>();
                    List<float> outputVals = new List<float>();


                    inputVals.AddRange(InputData[i]);

                    myNet.feedForward(inputVals);

                    /*================= echos some SHIT =================*/
                    var sb = new StringBuilder();
                    float[] temp = myNet.getResults();

                    for (int t = 0; t < inputVals.Count; t++)
                    {
                        sb.AppendLine("Input [" + t + "] : " + inputVals[t]);
                    }
                    /*
                    for (int t = 0; t < temp.Length; t++)
                    {
                        sb.AppendLine("Output [" + t + "] : " + temp[t].ToString());
                    }*/
                    sb.AppendLine("Output [0]: " + myNet.getResults()[0] );
                    sb.AppendLine("Expected [" + "] : " + OutputData[i][0]);

                    foreach (int t in OutputData[i])
                        outputVals.Add((float)t);
                    myNet.BackProp(outputVals.ToArray());

                    sb.AppendLine("Error: " + myNet.GetError());
                    sb.AppendLine("TrainingRun: " + counter);
                    Echo(sb.ToString());
                    /*===================================================*/


                    /*Echo total average to lcd*/
                    ErrorTotal += float.Parse(myNet.GetError());

                    var lcd = this.GridTerminalSystem.GetBlockWithName("EXCEPTION DUMP") as IMyTextPanel;
                    lcd?.WritePublicText("average error: " + ErrorTotal / InputData.Length, append: false);
                }
                if (ErrorTotal / InputData.Length < 0.00025) { done = true; }
                P.Echo("Total error: " + ErrorTotal + "\n");
                Timer?.Trigger();
            }
            else if (argument == "train" && done)
            {
                Me.CustomData = myNet.ExportToCSV();
            }
            else if (argument == "ImportCSV" && firstRun)
            {
                myNet.ImportWeightsFromCSV(Me.CustomData);
                firstRun = false;
            }
            else if (argument == "ClearStorage")
            {
                Storage = "";
                Echo("Storage Cleared");
            }
            else if (argument != "")
            {
                string[] tempArr;

                tempArr = argument.Split('_');

                List<double> inputVals = new List<double>();

                foreach (string t in tempArr)
                {
                    inputVals.Add(double.Parse(t));
                }

                myNet.feedForward(inputVals);

                StringBuilder sb = new StringBuilder();

                foreach (float t in myNet.getResults())
                {
                    sb.Append(t.ToString());
                }
                Echo("Output: " + sb.ToString());
            }
        }

        void Main(string argument)
        {
            try
            {
                SubMain(argument);
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();

                sb.AppendLine("Exception Message:");
                sb.AppendLine($"   {e.Message}");
                sb.AppendLine();

                sb.AppendLine("Stack trace:");
                sb.AppendLine(e.StackTrace);
                sb.AppendLine();

                var exceptionDump = sb.ToString();
                var lcd = this.GridTerminalSystem.GetBlockWithName("EXCEPTION DUMP") as IMyTextPanel;

                Echo(exceptionDump);
                lcd?.WritePublicText(exceptionDump, append: false);

                //Optionally rethrow
                throw;
            }
        }
    }
}
