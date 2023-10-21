using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace KademliaRigottiSorrentino
{
    public class KademliaNode
    {
        private const int RoutingTableSize = 20;
        private KademliaServer HTTPReplication;
        public ulong NodeId { get; private set; }
        private Dictionary<ulong, string> values;
        private List<RoutingField> routingTable; // Lista di dimensione 20 (controlli futuri) di coppie "distanza hex","nodeId" (Come richiesto nel protocollo Kademlia)


        public KademliaNode(ulong nodeId, KademliaServer server)
        {
            HTTPReplication = server;
            NodeId = nodeId;
            values = new Dictionary<ulong, string>();
            routingTable = new List<RoutingField>(); // Inizializza la routing table
        }

        // Aggiunge un valore associato a una chiave nel dizionario dei valori del nodo con distanza maggiore
        public void StoreValue(ulong key, string value, out KademliaNode nodeWhereAdded)
        {
            int res = FindNode(key, out KademliaNode nodeFounded, 5);

            if (res == 0 || res == 1)
            {
                nodeWhereAdded = nodeFounded;
                nodeFounded.AddValue(key, value);
            }
            else
            {
                nodeWhereAdded = this;
                this.AddValue(key, value);
            }
           
        }

        public void AddValue(ulong key, string value)
        {
            values[key] = value;
        }

        // Ottiene un valore associato a una chiave dal dizionario dei valori del nodo
        private bool GetValue(ulong key, out string value)
        {
            return values.TryGetValue(key, out value);
        }

        // Restituisce stringa con tutti i valori del Dizionario del nodo
        public string GetDictionary()
        {
            StringBuilder dictionaryString = new StringBuilder();
            foreach (var entry in values)
            {
                dictionaryString.Append($"Chiave: {entry.Key}, Valore: {entry.Value}\n");
            }
            return dictionaryString.ToString();
        }

        // Aggiunge un nodo alla routing table del nodo corrente e mantiene una dimensione massima della RoutingTable
        public void AddNodeToRoutingTable(KademliaNode node)
        {
            // Calcola la distanza XOR tra il nodo corrente e il nodo da aggiungere
            ulong xorDistance = KademliaHelper.CalculateXorDistance(this.NodeId, node.NodeId);

            // Inserisci la coppia "distanza", "nodeId" nella routing table all'indice calcolato
            RoutingField newField = new RoutingField(xorDistance, node.NodeId);
            if (!routingTable.Contains(newField))
            {
                routingTable.Add(new RoutingField(xorDistance, node.NodeId));
            }

            // Riordino
            routingTable.Sort();

            if (routingTable.Count >= RoutingTableSize)
            {
                // Rimuovi l'elemento in eccesso
                routingTable.RemoveAt(RoutingTableSize);
            }
        }

        // Metodo per rimuovere un nodo dalla routing table del nodo corrente in base all'indice
        public bool RemoveNodeFromRoutingTable(ulong nodeId)
        {
            int indexToRemove = -1;

            // Trova l'indice del nodo nella routing table
            for (int i = 0; i < routingTable.Count; i++)
            {
                if (routingTable[i].nodeID == nodeId)
                {
                    indexToRemove = i;
                    break;
                }
            }

            // Rimuovi il nodo dalla routing table se è stato trovato
            if (indexToRemove != -1)
            {
                routingTable.RemoveAt(indexToRemove);
                return true;
            }

            return false;
        }

        // Metodo per ottenere la routing table del nodo corrente
        public List<RoutingField> GetRoutingTable()
        {
            return routingTable;
        }

        // Metodo per visualizzare la routing table corrente
        public string RoutingTableToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var r in routingTable)
            {
                sb.Append(r.ToString() + "\n");
            }

            return sb.ToString();
        }

        // METODO PING di Kademlia
        public bool Ping(KademliaNode in_node, out KademliaNode out_node)
        {
            if (HTTPReplication.GetNode(in_node.NodeId, out out_node) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int FindNode(ulong targetID, out KademliaNode outNode, int lookUpCounter)
        {
            // Inizializza il nodo di uscita
            outNode = null;

            // Controlla che non sto cercando me stesso
            if (this.NodeId == targetID)
            {
                outNode = this;
                return 1;
            }

            // Fine ricerca ricorsiva
            if (lookUpCounter == 0)
            {
                // Ritorna il nodo più vicino trovato
                HTTPReplication.GetNode(GetClosestNodes(targetID)[0].nodeID, out outNode);
                return 0;
            }

            // Trova i nodi nella routing table con le distanze XOR più vicine al nodo di destinazione
            List<RoutingField> closestNodes = GetClosestNodes(targetID);

            // Invia richieste ai nodi nella lista closestNodes per ottenere informazioni sul nodo cercato
            foreach (var node in closestNodes)
            {
                // Se il nodo è nella distanza minima, cerca ulteriori informazioni
                if (HTTPReplication.GetNode(node.nodeID, out outNode))
                {
                    // Effettua una ricerca ricorsiva sul nodo trovato
                    int recursiveResearch = outNode.FindNode(targetID, out outNode, --lookUpCounter);

                    switch (recursiveResearch)
                    {
                        case 0:
                            if (lookUpCounter > 0)
                            {
                                AddNodeToRoutingTable(outNode);
                                return outNode.FindNode(targetID, out outNode, --lookUpCounter);
                            }
                            else
                            {
                                AddNodeToRoutingTable(outNode);
                                return 0;
                            }
                        case 1:
                            AddNodeToRoutingTable(outNode);
                            return 1;
                    }
                }
            }

            return -1;
        }

        // Find Valude replication / semplification
        public bool FindValue(ulong key, out string value, out KademliaNode nodeWhereFound)
        {
            nodeWhereFound = null;
            value = null;

            // Controlla se il valore è presente nel nodo corrente
            if (values.TryGetValue(key, out value))
            {
                nodeWhereFound = this;
                return true;
            }

            // Inizializza il nodo di uscita
            KademliaNode outNode = null;

            // Effettua la ricerca nella rete Kademlia
            int lookupCounter = 5; // Numero massimo di ricerche ricorsive
            int result = FindNode(key, out outNode, lookupCounter);

            if (result == 1 || result == 0)
            {
                // Il nodo contenente il valore è stato trovato nella rete Kademlia
                if (outNode.GetValue(key, out value))
                {
                    nodeWhereFound = outNode;
                    return true;
                }
            }

            return false; // Il valore non è stato trovato nella DHT
        }

        // ritorna lista in ordine di distanza crescenti dei 3 nodi più vicini
        private List<RoutingField> GetClosestNodes(ulong targetNodeId)
        {
            List<RoutingField> closestNodes = new List<RoutingField>(routingTable);

            for (int i = 0; i < closestNodes.Count - 1; i++)
            {
                for (int j = i + 1; j < closestNodes.Count; j++)
                {
                    if (KademliaHelper.CalculateXorDistance(targetNodeId, closestNodes[j].nodeID) < KademliaHelper.CalculateXorDistance(targetNodeId, closestNodes[i].nodeID))
                    {
                        // Scambia i nodi se troviamo un nodo più vicino
                        var temp = closestNodes[i];
                        closestNodes[i] = closestNodes[j];
                        closestNodes[j] = temp;
                    }
                }
            }

            // Restituisci i primi 3 nodi ordinati
            List<RoutingField> result = new List<RoutingField>();
            for (int i = 0; i < Math.Min(3, closestNodes.Count); i++)
            {
                result.Add(closestNodes[i]);
            }

            return result;
        }

        public void Join(KademliaNode newNode)
        {
            // Lo rendo "raggiungibile" sarebbe da fare con HTTP a è una semplificazione
            HTTPReplication.AddNode(newNode);

            AddNodeToRoutingTable(newNode);

            // In Kademlia è diverso (sarebbe newNode.FindNode(newNode.nodeId)) ma per come ho implementato
            // il metodo FindNode() la routing table del nuovo nodo rimarrebbe vuota
            newNode.AddNodeToRoutingTable(this);
        }
    }
}
