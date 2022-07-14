using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Pathfinding : NetworkBehaviour
{
  [SerializeField]
  public List<Node> ClosedList = new List<Node>();

  [SerializeField]
  public List<Node> OpenList = new List<Node>();

  [SerializeField]
  public List<List<Node>> Nodes = new List<List<Node>>();

  public static Pathfinding Instance;

  public bool flip = false;


  public override void OnStartClient()
  {
    if (Instance == null)
      Instance = this;
  }

  [System.Serializable]
  public class Node
  {
    public Node(GameObject obj, int firstIndex, int secondIndex)
    {
      this.obj = obj;
      this.firstIndex = firstIndex;
      this.secondIndex = secondIndex;
    }

    public bool walkable = true;
    public GameObject obj;
    public int firstIndex = 0;
    public int secondIndex = 0;
    /// <summary>
    /// distance from end node
    /// </summary>
    public int Hcost = 0;
    /// <summary>
    /// distance from starting node
    /// </summary>
    public int Gcost = 0;

    public int FCost {
      get {
        return Hcost + Gcost;
      }
      set {
        return;
      }
    }

    public Node parentNode;
  }

  public void StartPathfinding()
  {
    List<List<GameObject>> cubeAll = GrilleManager.Instance.getMap();
    Transform current = SelectionManager.Instance.selectedPersonnage.GetComponent<PersoData>().persoCase.transform;
    Transform target = HoverManager.Instance.hoveredCase.transform;

    if (current == null || target == null)
      {
        return;
      }


    Nodes.Clear();
    BakePathfinding(cubeAll);
    RequestPathfinding(current, target);
  }

  public void BakePathfinding(List<List<GameObject>> cubeAll)
  {
    if (Nodes.Count != 0)
      {
        return;
      }
    int i = 0;
    foreach (List<GameObject> obj in cubeAll)
      {
        Nodes.Add(new List<Node>());
        int j = 0;
        foreach (GameObject o in obj)
          {
            Nodes[i].Add(new Node(o.transform.gameObject, i, j));
            o.name = i + " " + j;
            Nodes[i][j].walkable = cubeAll[i][j].GetComponent<CaseData>().casePathfinding == PathfindingCase.Walkable;
            ++j;
          }
        ++i;
      }
  }

  public void RequestPathfinding(Transform current, Transform target)
  {
    Node nodeCurrent = null;
    Node targetCurrent = null;

    foreach (List<Node> nodelist in Nodes)
      {
        foreach (Node node in nodelist)
          {
            if (node.obj.transform.position.x == current.transform.position.x && Mathf.Approximately(node.obj.transform.position.y, current.transform.position.y))
              {
                nodeCurrent = node;
              }
            if (node.obj.transform.position.x == target.transform.position.x && Mathf.Approximately(node.obj.transform.position.y, target.transform.position.y))
              {
                targetCurrent = node;
              }
          }
      }

    if (nodeCurrent == null || targetCurrent == null)
      return;

    APlusLoop(nodeCurrent, targetCurrent);
  }

  public void APlusLoop(Node current, Node target)
  {
    ClosedList.Add(current);
    OpenList.Remove(current);

    if (current == target)
      {
        Pathdone(current, target);
        return;
      }

    GetAllNearPoints(current);

    foreach (Node openNode in OpenList)
      {
        GenerateHcost(openNode, target);
        GenerateGcost(openNode, openNode.parentNode, target);
      }

    if (flip == true)
      {
        OpenList.Reverse();
        if (OpenList.Count != 0)
          {
            OpenList.Add(OpenList[0]);
            OpenList.Remove(OpenList[0]);
          }
      }
    flip = !flip;

    if (OpenList.Count == 0)
      return;

    Node nextNode = OpenList[0];
    foreach (Node openNode in OpenList)
      {
        if (openNode.FCost < nextNode.FCost)
          {
            nextNode = openNode;
          }
      }
    //              Debug.Log(nextNode.obj.name + " || Hcost : " + nextNode.Hcost + "Gcost : " + nextNode.Gcost + "Fcost : " + nextNode.FCost);
    APlusLoop(nextNode, target);
  }

  public void GenerateHcost(Node current, Node target)
  {
    current.Hcost = Mathf.Abs(target.firstIndex - current.firstIndex) + Mathf.Abs(target.secondIndex - current.secondIndex);
    current.Hcost *= 10;
  }

  public void GenerateGcost(Node current, Node parentNode, Node target)
  {
    if (parentNode == null)
      return;
    current.Gcost = parentNode.Gcost;
    current.Gcost += (Mathf.Abs(target.firstIndex - current.firstIndex) + Mathf.Abs(target.secondIndex - current.secondIndex)) * 10;
  }

  void GetAllNearPoints(Node centerNode)
  {
    if (centerNode.firstIndex + 1 < Nodes.Count)
      {
        validateNode(centerNode, Nodes[centerNode.firstIndex + 1][centerNode.secondIndex]);
      }
    if (centerNode.firstIndex - 1 >= 0)
      {
        validateNode(centerNode, (Nodes[centerNode.firstIndex - 1][centerNode.secondIndex]));
      }
    if (centerNode.secondIndex + 1 < Nodes[centerNode.firstIndex].Count)
      {
        validateNode(centerNode, (Nodes[centerNode.firstIndex][centerNode.secondIndex + 1]));
      }
    if (centerNode.secondIndex - 1 >= 0)
      {
        validateNode(centerNode, (Nodes[centerNode.firstIndex][centerNode.secondIndex - 1]));
      }
  }

  void validateNode(Node centerNode, Node node)
  {
    if (node.firstIndex == -1 || node.secondIndex == -1)
      return;
    if (node.walkable == true && !ClosedList.Contains(node) && !OpenList.Contains(node))
      {
        node.parentNode = centerNode;
        OpenList.Add(node);
      }
  }

  void Pathdone(Node current, Node target)
  {
    List<GameObject> pathList = new List<GameObject>();

    Node remonte = current;
    do
      {
        if (current == null || target == null)
          {
            pathList = null;
            break;
          }

        pathList.Insert(0, remonte.obj);
        if (remonte.parentNode != null)
          remonte = remonte.parentNode;
        else
          break;
      } while (remonte.obj.name != target.obj.name);

    MoveBehaviour.Instance.GoPathes = pathList;

    OpenList.Clear();
    ClosedList.Clear();
  }
}
