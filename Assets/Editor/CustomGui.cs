﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR

public class CustomGUI : EditorWindow
{
    public int Trees; // The amount of trees.
    public List<Rect> dialwindows = new List<Rect>(); // A list of prompt windows.
    public List<Rect> responsewindows = new List<Rect>(); // A list of the response windows.
    public List<int> attachedWindows = new List<int>(); //A list of windows that are attacheds
    public Object[] Dialogues; // A list of all Dialogue objects.
    public Dialogue Dialogue; // A specific Dialogue object.
    public List<Dialogue> treeDialogues; // A list of the Dialogues of the current tree
    public List<int> NodeLayer; // The Layer that the corrosponding node in dialogueWindows is in.
    public List<Dialogue> toDestroy; // A lise of Dialogue objects to destory.
    public Dialogue currentNode; // The current Dialogue node.
    public int currentTree; // The current Tree being drawn
    public Vector2 scrollBar; // The List of Trees ScrollBar
    public Vector2 scrollBar2; // The Dialogue Tree ScrollBar
    private List<int> atLayer; // the Amount of nodes at a given layer.
    List<int> found; // A list of found trees.
    List<int> treesToDelete; // A list of trees to delete
    List<Dialogue> nodesToDelete; // A list of dialogues to delete.
    int layers; // The amount of layers in the Tree.

    // Called after all gameObjects are initialized, Used to initialized variables 
    public void Awake()
    {
        layers = 0;

        found = new List<int>();
        Debug.Assert(found != null, "failure to create found");

        treeDialogues = new List<Dialogue>();
        Debug.Assert(treeDialogues != null, "failure to create treesDialogues");

        nodesToDelete = new List<Dialogue>();
        Debug.Assert(nodesToDelete != null, "failure to create nodesToDelete");

        treesToDelete = new List<int>();
        Debug.Assert(treesToDelete != null, "failure to create treesToDelete");

        atLayer = new List<int>();
        Debug.Assert(atLayer != null, "failure to create atlayer");

        // Obtain all of the Dialogue Objects.
        Dialogues = Resources.LoadAll("DialogueTree");
        Debug.Assert(Dialogues != null, "failure loading dialgoues");

        // Find the amount of trees.
        Trees = findTrees();
        Debug.Assert(Trees >= 0, "failure to obtain number of trees");

        // The first tree will be the default tree.
        currentTree = found[0];
        Debug.Assert(currentTree > 0, "failure to set tree to first tree");

        NodeLayer = new List<int>();
        Debug.Assert(NodeLayer != null, "failure to create nodeLayer");

        dialwindows = new List<Rect>();
        Debug.Assert(dialwindows != null, "failure to create dialwindows");

    }

    // Adds the button on the window tab
    [MenuItem("Window/customGUI")]
    static void ShowEditor()
    {
        CustomGUI editor = EditorWindow.GetWindow<CustomGUI>();
        editor.Show();
        Debug.Assert(editor != null, "there is no editor");
    }
     
    // Called several times per frame, used to redraw the GUI
    public void OnGUI()
    {
        Trees = findTrees();
        Debug.Assert(Trees >= 0, "Error in OnGUI, failure to obtain number of trees");


        // Load the dialogue objects for the given tree.
        Dialogues = Resources.LoadAll("DialogueTree/Tree" + currentTree);
        Debug.Assert(Dialogues != null, "Error in OnGUI, failure to obtain Dialogues");

        //Refresh Layer and atLayer
        atLayer.Clear();
        Debug.Assert(atLayer != null, "Error in OnGUI, failure to refresh atLayer");

        EditorGUILayout.BeginHorizontal();
        
        // The scrollbar for the list of trees.
        scrollBar = GUILayout.BeginScrollView(scrollBar, false, true, GUILayout.Width(120));

        // For every tree.
        for(int i = 0; i < Trees; i++)
        {
            GUILayout.BeginHorizontal();

            if (!treesToDelete.Contains(i))
            {
                // Make a button for that tree.
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("Tree " + found[i]))
                {
                    currentTree = found[i];
                    Debug.Log(currentTree);
                }
                // Make a button to delete that tree.
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("x"))
                {
                    GUI.backgroundColor = Color.red;
                    treesToDelete.Add(i);
                }

            }

            GUILayout.EndHorizontal();
        }

        // A button that makes a new tree.
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("Add"))
        { 
            // Make a new folder for the tree.
            int i = 1;
            // While the Tree hasnt been made yet.
            while (Trees == found.Count)
            {
                Debug.Log(i);

                // Make sure that tree doesnt already exist.
                if (!found.Contains(i))
                {
                    // Make the folder.
                    AssetDatabase.CreateFolder("Assets/Resources/DialogueTree", "Tree" + i);
                    
                    //make the head dialogue
                    Dialogue newDial = new Dialogue();
                    newDial.tree = i;
                    newDial.start = true;

                    AssetDatabase.CreateAsset(newDial, "Assets/Resources/DialogueTree/Tree" + i + "/" + "Dialogue.asset");

                    // Add the new tree to found.
                    found.Add(i);
                }
                i++;
            }
            //set the current Tree to the newly created one.
            currentTree = i;
        }
        EditorGUILayout.EndScrollView();

        // Create an arrea for the inport/export buttons.
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("inport"))
        {
            //TODO: do inport
        }

        if(GUILayout.Button("export"))
        {
            //TODO: do export
        }
        EditorGUILayout.EndHorizontal();


        // Create an area for the nodes to be in.
        scrollBar2 = GUILayout.BeginScrollView(scrollBar2, true, true, new
            GUILayoutOption[]{
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true),

        }) ;

        drawTree(currentTree);

        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }


    /* Draws the Dialogue Tree in the customGUI window
     * 
     * PARAM    treeNum - The number of the tree that is being drawn
     * PRE -  The index of the tree being drawn
     * POST - Tree is displayed in the cusotmGUI window
     * RETURN - NULL
     */
    public void drawTree(int treeNum)
    {
        //find the head node
        Dialogue head = ScriptableObject.CreateInstance<Dialogue>();

        dialwindows.Clear();
        NodeLayer.Clear();


        for (int i = 0; i < Dialogues.Length; i++)
        {
            //if this node belongs to the current tree, and is the head of that tree
            if ((((Dialogue)Dialogues[i]).tree == treeNum) && (((Dialogue)Dialogues[i]).start == true))
            {
                head = (Dialogue)Dialogues[i];
                goto Found; 
            }
            else
            {
                //if it cant be found, set it ot the first one that appears.
                head = (Dialogue)Dialogues[0];
            }
        }

        Found:
        atLayer.Add(0);
        drawNode(head, 0);
    }

    /* Draws a node
    * 
    * PARAM:    dial - the dialogue being draw.
    *           layer - the layer of the node
    * PRE -  The node being drawn is not null.
    * POST - Node is displayed in the cusotmGUI window.
    * RETURN - The Rect of the node.
    */
    public Rect drawNode(Dialogue dial, int layer)
    {
        float biggestX = 0;

        if (layer > layers)
        {
            layers = layer;
        }

        Debug.Assert(dial != null, "Error in drawNode, the dialogue input was null.");
        Debug.Assert(layer >= 0, "Error in drawNode, the layer input was less than 0.");

        // Find other nodes at the current layer.
        for(int i = 0; i < dialwindows.Count; i++)
        {
            // Find the hightest x position of the nodes on the same layer.
            if((NodeLayer[i] == layer) && dialwindows[i].x > biggestX)
            {
                biggestX = dialwindows[i].x;
            }
        }


        //if the response list doesnt exist
        if (dial.response == null)
        {
            //Make one
            dial.response = new List<string>();
        }

        //if the next list doesnt exist
        if (dial.next == null)
        {
            //Make one
            dial.next = new List<Dialogue>();
        }

        // Create the position of the node.
        Rect nodeRect = new Rect(dial.response.Count *100 + (atLayer[layer] * 200) + biggestX, layer * 100, 175, 65);
        Rect textRect = new Rect(nodeRect.x, nodeRect.y + 20, nodeRect.width, 20);
        Rect buttonRect = new Rect(nodeRect.x, textRect.y + 25, nodeRect.width, 20);
        Rect exitRect = new Rect(nodeRect.x + nodeRect.width - 15, nodeRect.y, 15, 15);

        dialwindows.Add(nodeRect);
        NodeLayer.Add(layer);

        //if this node is not going to be deleted
        if (!nodesToDelete.Contains(dial))
        {

            // Draw the node.
            EditorGUI.DrawRect(nodeRect, Color.grey);

            // Display the dialogues parameters.
            EditorGUI.LabelField(nodeRect, "NPC Prompt:");
            dial.prompt = EditorGUI.TextField(textRect, dial.prompt);

            // Make a button for creating another node.
            if (GUI.Button(buttonRect, "new child"))
            {                
                dial.response.Add("");
                dial.next.Add(null);
            }
            // If the current node is not the head
            if (layer != 0)
            {
                // Make a button for deleting the node.
                GUI.backgroundColor = Color.red;
                if (GUI.Button(exitRect, "X"))
                {
                    AssetDatabase.DeleteAsset("Assets/Resources/DialogueTree/Tree" + dial.tree + "/" + dial.name + ".asset");
                }
                GUI.backgroundColor = Color.white;
            }

            //if this node has a list of responses
            if (dial.response != null)
            {
                // For each of its responses, create a response node
                for (int i = 0; i < dial.response.Count; i++)
                {
                    // if adding the response would make an new layer... 
                    if (atLayer.Count == layer + 1)
                    {
                        // Make a new layer
                        atLayer.Add(0);
                    }

                    Rect child = drawReponse(dial, layer + 1, i);
                    Debug.Assert(child != null, "error in drawNode, the child was null");
                    DrawNodeCurve(nodeRect, child);
                }
            }

            // Notify the other nodes that this node is at the current layer.
            atLayer[layer] ++;

        }

        return nodeRect;
    }

    /* Draws a node
    * 
    * PARAM:    dial - the dialogue being draw.
    *           layer - the layer of the node
    *           index - the index of the response being drawn
    * PRE -  The id of the node being drawn is not null
    * POST - All of the responses in the node are displayed in the cusotmGUI window
    * RETURN - The rect of the response node
    */
    public Rect drawReponse(Dialogue dial, int layer, int index)
    {
        if(layer > layers)
        {
            layers = layer;
        }

        // Create the position of the node.
        Rect nodeRect = new Rect((atLayer[layer] * 200) + 10, layer * 100, 175, 70);
        Rect textRect = new Rect(nodeRect.x, nodeRect.y + 20, nodeRect.width, 20);
        Rect buttonRect = new Rect(nodeRect.x, textRect.y + 25, nodeRect.width, 20);
        Rect exitRect = new Rect(nodeRect.x + nodeRect.width - 15, nodeRect.y, 15, 15);

        responsewindows.Add(nodeRect);
        

        // Draw the node.
        EditorGUI.DrawRect(nodeRect, Color.white);

        // Display the dialogues parameters.
        EditorGUI.LabelField(nodeRect, "User Response:");
        dial.response[index] = EditorGUI.TextField(textRect, dial.response[index]);

        if (dial.next.Count >= index)
        {
            if (dial.next[index] == null)
            {
                // Make a button for creating another node.
                if (GUI.Button(buttonRect, "new child"))
                {
                    // Make a new Dialogue, and set some of its parameters
                    Dialogue newDial = ScriptableObject.CreateInstance<Dialogue>();
                    newDial.start = false;
                    newDial.tree = dial.tree;
                    newDial.response = new List<string>();
                    AssetDatabase.CreateAsset(newDial, "Assets/Resources/DialogueTree/Tree" + dial.tree + "/Dialogue" + Dialogues.Length + ".asset");

                    //if the current Dialogue doesnt have a next, make one
                    if (dial.next == null)
                    {
                        dial.next = new List<Dialogue>();
                    }

                    Debug.Assert(dial.next != null, "Error in drawResponse, dial.next is null");

                    // If the next[] already has a spot open set it to the new node
                    if (dial.next.Count >= index)
                    {
                        dial.next[index] = newDial;
                    }
                    // Otherwise, add it to the list
                    else
                    {
                        dial.next.Add(newDial);
                    }
                }
            }
        }

        // If the current response has no next, make a delete button.
        if (dial.next[index] == null)
        {
            // Make a button for deleting the response.
            GUI.backgroundColor = Color.red;
            if (GUI.Button(exitRect, "X"))
            {
                dial.response.Remove(dial.response[index]);
                dial.next.Remove(dial.next[index]);
            }
            GUI.backgroundColor = Color.white;
        }
        // Else, if it does have a next, make a new node for it.
        else
        {
            // if adding the next node would make an new layer... 
            if (atLayer.Count == layer + 1)
            {
                // Make a new layer
                atLayer.Add(0);
            }

            Rect child = drawNode(dial.next[index], layer + 1);
            Debug.Assert(child != null, "Error in drawResponse, child was null");
            DrawNodeCurve(nodeRect, child);
        }
        
        // Notify the other nodes that this node is at the current layer.
        atLayer[layer]++;

        return nodeRect;
    }

    /*a helper function to get the index of a given node
    * 
    * PARAM:    node -  dialogue node that exists in the Dialogue[] array
    * PRE - The given node is not null
    * POST - NONE
    * RETURN - the index of the node in the Dialogue[] array
    */
    public int getNodeIndex(Dialogue node)
    {
        return 0;
    }

    /* A helper function that draws a line between nodes.
    * 
    * PARAM:    start - The node that the line is starting from
    *           end - The node that the line is ending from
    * PRE -  start and end are not null
    * POST - Draws a line between two nodes
    * RETURN - NONE
    */
    void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width/2, start.y + start.height, 0);
        Vector3 endPos = new Vector3(end.x + end.width/2, end.y, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Color shadowCol = new Color(0, 0, 0, 0.06f);

        for (int i = 0; i < 3; i++)
        {// Draw a shadow
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
        }

        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }
    
    /* A helper function that finds the current node. if there is more than one current node
     * throw and error. if there is no selected current, set current to the first node.
    * 
    * PRE -  The tree is not null.
    * POST - NONE.
    * RETURN - A dialogue that is asigned as the current node.
    */
    public Dialogue findCurrent()
    {
        return null;
    }

    /* Returns the amount of trees that are currently in the resources folder.
    * 
    * PRE -  NONE
    * POST - NONE.
    * RETURN - the number of trees in the resources folder
    */
    public int findTrees()
    {
        Dialogues = Resources.LoadAll("DialogueTree");

        // For every node
        for (int i = 0; i < Dialogues.Length; i++)
        {
            // If that node belongs to a tree that has not been found yet 
            if(!found.Contains(((Dialogue)Dialogues[i]).tree))
            {
                found.Add(((Dialogue)Dialogues[i]).tree);
            }
        }
        return found.Count;
    }



}


#endif