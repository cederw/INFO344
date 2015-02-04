using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class Node
    {
        //its a node, not much to say about it
        public string Word;
        public bool IsTerminal { get { return Word != null; } }
        public Dictionary<char, Node> Edges = new Dictionary<char, Node>();
    }
}