using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GraphEditorWindow : EditorWindow
{
    private GraphUI graphUI;

    [MenuItem("Window/Graph Editor")]
    public static void OpenWindow()
    {
        GetWindow<GraphEditorWindow>("Graph Editor");
    }

    private void OnEnable()
    {
        // Создание графа из JSON
        var graph = new Graph();
        graph.AddNode(new Node("MAIN_1", "Hello, World 1", new List<string> { "MAIN_2", "MAIN_3" }));
        graph.AddNode(new Node("MAIN_2", "Hello, World 2"));
        graph.AddNode(new Node("MAIN_3", "Hello, World 3", new List<string> { "MAIN_4" }));
        graph.AddNode(new Node("MAIN_4", "Hello, World 4"));

        graphUI = new GraphUI(graph);
    }

    private void LoadGraph()
    {
        string path = EditorUtility.OpenFilePanel("Load Graph from JSON", "", "json");
        if (string.IsNullOrEmpty(path)) return;

        var graph = GraphJsonHandler.LoadGraphFromJson(path);
        if (graph != null)
        {
            graphUI = new GraphUI(graph);
            Debug.Log("Graph successfully loaded from JSON!");
        }
    }
    private void SaveGraph()
    {
        string path = EditorUtility.SaveFilePanel("Save Graph to JSON", "", "graph.json", "json");
        if (string.IsNullOrEmpty(path)) return;

        if (graphUI != null)
        {
            GraphJsonHandler.SaveGraphToJson(graphUI.Graph, path);
            Debug.Log("Graph successfully saved to JSON!");
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Graph"))
        {
            LoadGraph();
        }
        if (GUILayout.Button("Save Graph"))
        {
            SaveGraph();
        }
        GUILayout.EndHorizontal();

        if (graphUI != null)
        {
            DrawNodes();
            DrawConnections();
            ProcessEvents(Event.current);

            GUILayout.Space(10);
            DrawNodeEditor(); // Панель редактирования текста

            if (GUI.changed) Repaint();
        }
    }



    private void DrawNodes()
    {
        foreach (var nodeUI in graphUI.NodeUIs.Values)
        {
            if (nodeUI.IsSelected)
            {
                GUI.color = Color.cyan; // Цвет для выделенной ноды
            }
            else
            {
                GUI.color = Color.white; // Цвет для обычных нод
            }

            GUI.Box(nodeUI.Rect, nodeUI.Node.Text, EditorStyles.helpBox);
            GUI.color = Color.white; // Сбрасываем цвет
        }
    }

    private void DrawNodeEditor()
    {
        if (selectedNode != null)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Edit Node Text", EditorStyles.boldLabel);

            // Поле для ввода текста
            string newText = EditorGUILayout.TextField("Text", selectedNode.Node.Text);

            // Если текст изменился, обновляем содержимое
            if (newText != selectedNode.Node.Text)
            {
                selectedNode.Node.Text = newText;
                GUI.changed = true;
            }

            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.Label("No node selected", EditorStyles.helpBox);
        }
    }


    private void DrawArrow(Vector2 start, Vector2 end)
    {
        // Вычисляем направление стрелки
        Vector2 direction = (end - start).normalized;

        // Основная линия стрелки
        Handles.DrawLine(start, end);

        // Создаем вершины стрелки
        Vector2 arrowHead1 = end - direction * 10f + new Vector2(-direction.y, direction.x) * 5f;
        Vector2 arrowHead2 = end - direction * 10f - new Vector2(-direction.y, direction.x) * 5f;

        // Рисуем треугольник стрелки
        Handles.DrawLine(end, arrowHead1);
        Handles.DrawLine(end, arrowHead2);
    }

    private void DrawConnection(Vector2 start, Vector2 end)
    {
        Handles.DrawBezier(
            start,
            end,
            start + Vector2.left * 50f,
            end - Vector2.left * 50f,
            Color.white,
            null,
            2f
        );
    }

    private NodeUI selectedNode = null; // Храним текущую выделенную ноду

    // Переписанный ProcessEvents и связанные методы

    // Переписанный ProcessEvents и связанные методы

    private NodeUI connectionStartNode = null; // Нода начала новой связи
    private Vector2 rightClickStartPosition;  // Начальная позиция ПКМ для создания новой ноды

    private void ProcessEvents(Event e)
    {
        if (selectedNode != null)
        {
            HandleSelectedNodeEvents(e);
            return;
        }

        switch (e.type)
        {
            case EventType.MouseDown:
                HandleMouseDown(e);
                break;
            case EventType.MouseUp:
                HandleMouseUp(e);
                break;
            case EventType.MouseDrag:
                HandleMouseDrag(e);
                break;
        }
    }

    private void HandleSelectedNodeEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDrag:
                DragSelectedNode(e);
                break;
            case EventType.MouseUp:
                ReleaseSelectedNode(e);
                break;
        }
    }

    private void HandleMouseDown(Event e)
    {
        if (e.button == 0) // Left Click
        {
            NodeUI clickedNode = FindNodeUnderMouse(e.mousePosition);
            if (clickedNode != null)
            {
                SelectNode(clickedNode);
                clickedNode.IsDragged = true;
            }
        }
        else if (e.button == 1) // Right Click
        {
            NodeUI clickedNode = FindNodeUnderMouse(e.mousePosition);
            if (clickedNode != null)
            {
                connectionStartNode = clickedNode;
            }
            else
            {
                rightClickStartPosition = e.mousePosition;
            }
        }
    }

    private void HandleMouseUp(Event e)
    {
        if (e.button == 0) // Left Click
        {
            if (selectedNode != null)
            {
                ReleaseSelectedNode(e);
            }
        }
        else if (e.button == 1) // Right Click
        {
            if (connectionStartNode != null)
            {
                NodeUI targetNode = FindNodeUnderMouse(e.mousePosition);
                if (targetNode != null && connectionStartNode != targetNode)
                {
                    CreateConnection(connectionStartNode, targetNode);
                }
                connectionStartNode = null;
            }
            else
            {
                CreateNewNodeAtPosition(rightClickStartPosition);
            }
        }
    }

    private void HandleMouseDrag(Event e)
    {
        NodeUI draggedNode = FindNodeUnderMouse(e.mousePosition);
        if (draggedNode != null && draggedNode.IsDragged)
        {
            DragNode(draggedNode, e.mousePosition);
        }
    }

    private void DragSelectedNode(Event e)
    {
        if (selectedNode.IsDragged)
        {
            DragNode(selectedNode, e.mousePosition);
        }
    }

    private void ReleaseSelectedNode(Event e)
    {
        if (selectedNode != null)
        {
            selectedNode.IsSelected = false;
            selectedNode = null; // Сбрасываем выделение
        }

    }

    private void DragNode(NodeUI node, Vector2 mousePosition)
    {
        node.Rect.position = mousePosition - new Vector2(node.Rect.width / 2, node.Rect.height / 2);
        GUI.changed = true;
    }

    private NodeUI FindNodeUnderMouse(Vector2 mousePosition)
    {
        foreach (var nodeUI in graphUI.NodeUIs.Values)
        {
            if (nodeUI.Rect.Contains(mousePosition))
            {
                return nodeUI;
            }
        }
        return null;
    }

    private void CreateConnection(NodeUI fromNode, NodeUI toNode)
    {
        fromNode.Node.ConnectedNodes.Add(toNode.Node.ID);
        graphUI.UpdateArrowsForNode(fromNode);
        GUI.changed = true;
    }

    private void CreateNewNodeAtPosition(Vector2 position)
    {
        string newNodeId = $"Node_{graphUI.Graph.Nodes.Count + 1}";
        Node newNode = new Node(newNodeId, "New Node");
        graphUI.Graph.AddNode(newNode);
        graphUI.NodeUIs[newNodeId] = new NodeUI(newNode, position, new Vector2(200, 100));
        graphUI.UpdateArrows();
        GUI.changed = true;
        Debug.Log($"New node created at {position}");
    }


    private void DrawConnections()
    {
        foreach (var nodeUI in graphUI.NodeUIs.Values)
        {
            foreach (var connectedId in nodeUI.Node.ConnectedNodes)
            {
                var connectedNodeUI = graphUI.GetNodeUI(connectedId);
                if (connectedNodeUI != null)
                {
                    Vector2 start = new Vector2(nodeUI.Rect.xMax, nodeUI.Rect.center.y);
                    Vector2 end = new Vector2(connectedNodeUI.Rect.xMin, connectedNodeUI.Rect.center.y);

                    DrawArrow(start, end);
                }
            }
        }

        // Если мы в процессе создания связи, рисуем временную линию
        if (connectionStartNode != null)
        {
            Vector2 start = new Vector2(connectionStartNode.Rect.xMax, connectionStartNode.Rect.center.y);
            Vector2 end = Event.current.mousePosition;

            Handles.DrawAAPolyLine(3f, start, end);
            GUI.changed = true;
        }
    }

    private void SelectNode(NodeUI nodeUI)
    {
        // Снимаем выбор с ранее выбранной ноды
        if (selectedNode != null)
        {
            selectedNode.IsSelected = false;
        }

        // Устанавливаем выбор на текущую ноду
        nodeUI.IsSelected = true;
        selectedNode = nodeUI;

        // Перемещаем выделенную ноду на передний план
        graphUI.BringNodeToFront(nodeUI);
    }



    private void DeselectAllNodes()
    {
        foreach (var nodeUI in graphUI.NodeUIs.Values)
        {
            nodeUI.IsSelected = false;
        }
    }

}
