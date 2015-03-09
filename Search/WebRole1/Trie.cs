using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class Trie
    {
        //Builds a trie
        public Node Root = new Node();
        private List<string> results = new List<string>();
        private string Search;
        //starts with just the root
        public Trie()
        {
            Root = new Node();
        }
        //adds a new page title to the trie
        public void AddTitle(string insert)
        {
            string word = insert.ToLower();
            var node = Root;
            for (int len = 1; len <= word.Length; len++)
            {

                var letter = word[len - 1];

                Node next;
                if (!node.Edges.TryGetValue(letter, out next))
                {
                    next = new Node();
                    if (len == word.Length)
                    {
                        next.IsTerminal = true;
                    }
                    if(letter == '_'){
                        //just replace the spaces here
                        if(!node.Edges.ContainsKey(' ')){
                            node.Edges.Add(' ', next);
                        }
                        
                    }
                    else
                    {
                        node.Edges.Add(letter, next);
                    }
                    

                }
                node = next;
             }

            
        }
        //looks for titles that match the search string
        public string[] SearchForPrefix(string search)
        {
            
            results.Clear();
            Search = search.ToLower();
            Node currentNode = Root;
            //finds the search prefix in the trie and then searches off of it
            //if the prefix doesnt exist searchs based on the closest match
            
            for (int i = 0; i < Search.Length;i++ )
            {
                if (currentNode.Edges.ContainsKey(Search.ToCharArray()[i]))
                {
                    Node temp = currentNode.Edges[Search.ToCharArray()[i]];
                    if (i == Search.Length - 1)
                    {
                        fillResults(temp, Search);
                    }

                    else
                    {
                        currentNode = temp;
                    }
                } 
                

            }
            
            if(results.Count==0){
                results.Add("No results :(");
            }
            return results.ToArray();
        }

        private void fillResults(Node node, string temp)
        {
            //The complicated part here is to trace each path seperatly so that whne the BFS finds a terminal node the word can be found
            if(node.IsTerminal){
                results.Add(temp);
            }
            HashSet<Node> disc = new HashSet<Node>();
            Queue<Node> q = new Queue<Node>();
            Dictionary<Node, string> paths = new Dictionary<Node, string>();
            q.Enqueue(node);
            //label
            disc.Add(node);
            paths.Add(node,temp);
            while (q.Count > 0 && results.Count <= 10)
            {
                Node tNode = q.Dequeue();
                foreach (KeyValuePair<char, WebRole1.Node> kvp in tNode.Edges)
                {
                    
                    if(kvp.Value.IsTerminal){
                        results.Add(paths[tNode]+kvp.Key);
                    } 
                        
                    if(!disc.Contains(kvp.Value)){
                       
                        paths.Add(kvp.Value,paths[tNode]+kvp.Key);
                        q.Enqueue(kvp.Value);
                        disc.Add(kvp.Value);
                    }
                } 
            }

            
        }
    }
}