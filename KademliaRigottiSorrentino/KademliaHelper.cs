using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;

namespace KademliaRigottiSorrentino
{
    public class KademliaHelper
    {
        // -----------------------------------------------------------------------------------------
        //  Dato un nodo o un valore (della rete Kademlia) torni l'identificatore univoco di 64 bit.
        // -----------------------------------------------------------------------------------------
        public static ulong ComputeKademliaId(string input)
        {
            // OSS: Non uso SHA per tenere tutto più semplice
            return CityHash.CityHash.CityHash64(input);
        }

        // --------------------------------------------------------------------
        // Metodo per calcolare la distanza XOR tra due identificatori Kademlia
        // --------------------------------------------------------------------
        public static ulong CalculateXorDistance(ulong nodeIdA, ulong nodeIdB)
        {
            // Calcola la distanza XOR
            ulong xorDistance = nodeIdA ^ nodeIdB;

            return xorDistance;
        }
    }

    public struct RoutingField : IComparable<RoutingField>
    {
        public ulong xorDistance;
        public ulong nodeID;

        public RoutingField(ulong xorDistance, ulong nodeID)
        {
            this.xorDistance = xorDistance;
            this.nodeID = nodeID;
        }

        // Implementa il metodo CompareTo per l'interfaccia IComparable
        public int CompareTo(RoutingField other)
        {
            // Confronta le xorDistance per l'ordinamento
            return this.xorDistance.CompareTo(other.xorDistance);
        }

        public override bool Equals(object obj)
        {
            return obj is RoutingField field &&
                   xorDistance == field.xorDistance &&
                   nodeID == field.nodeID;
        }

        // Override toString()
        public override string ToString()
        {
            return "Distance: " + xorDistance.ToString() + " - Node ID: " + nodeID.ToString();
        }


    }
}
