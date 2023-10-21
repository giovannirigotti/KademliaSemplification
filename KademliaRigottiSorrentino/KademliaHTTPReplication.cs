using System;
using System.Collections.Generic;
using System.Text;

namespace KademliaRigottiSorrentino
{

    // This class emulate the behaviour behind internet. To not compute HTML request i use this HashMap.
    public class KademliaServer
    {
        private Dictionary<ulong, KademliaNode> allNodes;
        private KademliaNode firstNode;

        public KademliaServer()
        {
            allNodes = new Dictionary<ulong, KademliaNode>();

            // Inizializza primo nodo della DHT e lo imposta come nodo iniziale per i nuovi nodi (semplificazione per la join)
            firstNode = new KademliaNode(KademliaHelper.ComputeKademliaId("firstNode"), this);
            AddNode(firstNode);
        }

        public void AddNode(KademliaNode node)
        {
            allNodes[node.NodeId] = node;
        }

        public void RemoveNode(KademliaNode node)
        {
            allNodes.Remove(node.NodeId);
        }

        public bool GetNode(ulong nodeID, out KademliaNode node)
        {
            return allNodes.TryGetValue(nodeID, out node);
        }

        public KademliaNode GetFirstNode()
        {
            return firstNode;
        }
    }
}
