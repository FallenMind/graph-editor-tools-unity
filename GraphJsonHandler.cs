using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GraphJsonHandler
{
    // Структура для сериализации данных узлов
    [System.Serializable]
    public class NodeData
    {
        public string ID;
        public string TEXT;
        public List<string> Connected;

        public NodeData(string id, string text, List<string> connected = null)
        {
            ID = id;
            TEXT = text;
            Connected = connected ?? new List<string>();
        }
    }

    // Структура для сериализации графа
    [System.Serializable]
    public class GraphData
    {
        public List<NodeData> Nodes = new List<NodeData>();
    }

    // Чтение графа из JSON файла
    public static Graph LoadGraphFromJson(string jsonPath)
    {
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"JSON file not found: {jsonPath}");
            return null;
        }

        string json = File.ReadAllText(jsonPath);
        GraphData graphData = JsonUtility.FromJson<GraphData>(json);

        Graph graph = new Graph();
        foreach (var nodeData in graphData.Nodes)
        {
            var node = new Node(nodeData.ID, nodeData.TEXT, nodeData.Connected);
            graph.AddNode(node);
        }

        return graph;
    }

    // Сохранение графа в JSON файл
    public static void SaveGraphToJson(Graph graph, string jsonPath)
    {
        GraphData graphData = new GraphData();

        foreach (var node in graph.Nodes)
        {
            var nodeData = new NodeData(node.ID, node.Text, node.ConnectedNodes);
            graphData.Nodes.Add(nodeData);
        }

        string json = JsonUtility.ToJson(graphData, true);
        File.WriteAllText(jsonPath, json);

        Debug.Log($"Graph saved to {jsonPath}");
    }
}
