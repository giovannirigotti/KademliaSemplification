using System;

namespace KademliaRigottiSorrentino
{
    class Program
    {
        static void Main(string[] args)
        {
            KademliaServer server = new KademliaServer();
            KademliaNode firstNode = server.GetFirstNode();

            Console.WriteLine("Kademlia DHT Emulator");

            Console.WriteLine(" ");

            Console.Write("First Node ID: ");
            Console.WriteLine(firstNode.NodeId);

            Console.WriteLine(" ");

            Console.WriteLine("Routing table first Node:");
            Console.WriteLine(firstNode.RoutingTableToString());

            Console.WriteLine(" ");

            // Simula l'aggiunta di alcuni nodi
            for (int i = 0; i < 10; i++)
            {
                KademliaNode newNode = new KademliaNode(KademliaHelper.ComputeKademliaId($"Node{i}"), server);
                firstNode.Join(newNode);
            }

            Console.WriteLine("10 Nodi aggiunti alla rete Kademlia.");

            Console.WriteLine(" ");

            Console.WriteLine("Routing table first Node:");
            Console.WriteLine(firstNode.RoutingTableToString());

            Console.WriteLine("Digita 'q' per uscire.");
            Console.WriteLine("Digita 'StoreValue NodeID Chiave Valore' per aggiungere un valore nel DHT.");
            Console.WriteLine("Digita 'GetValues NodeID' per vedere tutte le coppie chiave valore del nodo.");
            Console.WriteLine("Digita 'RoutingTable NodeID' per vedere la routing table del nodo.");
            Console.WriteLine("Digita 'FindNode NodeID TargetNodeID' per trovare un nodo nella rete Kademlia.");
            Console.WriteLine("Digita 'FindValue NodeID Chiave' per trovare un valore nel DHT.");
            Console.WriteLine("Digita 'CreateNode NodeNameString' per creare un nuovo nodo nella rete Kademlia.");
            Console.WriteLine("Digita 'Join NodeID NewNodeID' per far unire un nodo esistente con un nuovo nodo nella rete Kademlia.");

            string input;
            while ((input = Console.ReadLine()) != "q")
            {
                string[] inputParts = input.Split(' ');

                if (inputParts.Length >= 2)
                {
                    if (inputParts[0] == "StoreValue")
                    {
                        ulong nodeID = ulong.Parse(inputParts[1]);
                        ulong key = KademliaHelper.ComputeKademliaId(inputParts[2]);
                        string value = inputParts[3];

                        KademliaNode node;
                        if (server.GetNode(nodeID, out node))
                        {
                            node.StoreValue(key, value, out KademliaNode nodeWhereAdded);
                            Console.WriteLine($"Valore '{value}' aggiunto al nodo {nodeWhereAdded.NodeId} con chiave '{key}' partendo dal nodo {nodeID}.");
                        }
                        else
                        {
                            Console.WriteLine($"Nodo con ID {nodeID} non trovato nella rete Kademlia.");
                        }
                    }
                    else if (inputParts[0] == "GetValues")
                    {
                        ulong nodeID = ulong.Parse(inputParts[1]);
                        KademliaNode node;
                        if (server.GetNode(nodeID, out node))
                        {
                            string dictionary = node.GetDictionary();
                            Console.WriteLine($"Dizionario del nodo {nodeID}:\n{dictionary}");
                        }
                        else
                        {
                            Console.WriteLine($"Nodo con ID {nodeID} non trovato nella rete Kademlia.");
                        }
                    }
                    else if (inputParts[0] == "RoutingTable")
                    {
                        ulong nodeID = ulong.Parse(inputParts[1]);
                        KademliaNode node;
                        if (server.GetNode(nodeID, out node))
                        {
                            string routingTableString = node.RoutingTableToString();
                            Console.WriteLine($"Routing Table del nodo {nodeID}:\n{routingTableString}");
                        }
                        else
                        {
                            Console.WriteLine($"Nodo con ID {nodeID} non trovato nella rete Kademlia.");
                        }
                    }
                    else if (inputParts[0] == "FindNode")
                    {
                        ulong nodeID = ulong.Parse(inputParts[1]);
                        ulong targetNodeID = ulong.Parse(inputParts[2]);

                        KademliaNode node;
                        if (server.GetNode(nodeID, out node))
                        {
                            KademliaNode targetNode;
                            int result = node.FindNode(targetNodeID, out targetNode, 5);

                            if (result == 1)
                            {
                                Console.WriteLine($"Nodo con ID {targetNodeID} trovato nel nodo {nodeID}.");
                            }
                            else if (result == 0)
                            {
                                Console.WriteLine($"Nodo più vicino a {targetNodeID} trovato nel nodo {nodeID} con il seguente ID: {targetNode.NodeId}.");
                            }
                            else
                            {
                                Console.WriteLine($"Nodo con ID {targetNodeID} non trovato nel nodo {nodeID}.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Nodo con ID {nodeID} non trovato nella rete Kademlia.");
                        }
                    }
                    else if (inputParts[0] == "FindValue")
                    {
                        ulong nodeID = ulong.Parse(inputParts[1]);
                        ulong key = KademliaHelper.ComputeKademliaId(inputParts[2]);

                        KademliaNode node;
                        if (server.GetNode(nodeID, out node))
                        {
                            if (node.FindValue(key, out string value, out KademliaNode nodeWhereFound))
                            {
                                Console.WriteLine($"Valore per chiave '{key}' uguale a: '{value}' trovato nel nodo con ID: {nodeWhereFound.NodeId}");
                            }
                            else
                            {
                                Console.WriteLine($"Valore per chiave '{key}' non trovato partendo dal nodo {nodeID}.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Nodo con ID {nodeID} non trovato nella rete Kademlia.");
                        }
                    }
                    else if (inputParts[0] == "CreateNode")
                    {
                        ulong nodeID = KademliaHelper.ComputeKademliaId(inputParts[1]);
                        KademliaNode newNode = new KademliaNode(nodeID, server);
                        server.AddNode(newNode); // NON è stato aggiunto alla rete Kademlia (non ha routing table)
                        Console.WriteLine($"Nodo con ID {nodeID} creato.");
                    }
                    else if (inputParts[0] == "Join")
                    {
                        ulong nodeID = ulong.Parse(inputParts[1]);
                        ulong newNodeID = ulong.Parse(inputParts[2]);

                        KademliaNode node, newNode;
                        if (server.GetNode(nodeID, out node) && server.GetNode(newNodeID, out newNode))
                        {
                            node.Join(newNode);
                            Console.WriteLine($"Nodo con ID {newNodeID} unito al nodo {nodeID} nella rete Kademlia.");
                        }
                        else
                        {
                            Console.WriteLine($"Nodo con ID {nodeID} o {newNodeID} non trovato nella rete Kademlia.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Comando non riconosciuto. Riprova.");
                    }
                }
                else
                {
                    Console.WriteLine("Comando non valido. Riprova.");
                }
            }

            Console.WriteLine("Uscita dalla simulazione.");
        }
    }
}
