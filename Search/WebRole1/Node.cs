using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class Node
    {
        //its a node, not much to say about it
        //its for the trie
        //public List<string> suffex = new List<string>(); // if i get to the extra credit
        public bool IsTerminal = false;
        public Dictionary<char, Node> Edges = new Dictionary<char, Node>();
    }
}