using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class Trie
    {
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
                        next.Word = insert;
                    }
                    node.Edges.Add(letter, next);
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
                        fillResults(temp);
                    }

                    else
                    {
                        currentNode = temp;
                    }
                } else if(currentNode.Edges.ContainsKey('_') && Search.ToCharArray()[i] == ' '){
                    Node temp = currentNode.Edges['_'];
                    if (i == Search.Length - 1)
                    {
                        fillResults(temp);
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
        //recursivly finds close words to the search prefix
        private void fillResults(Node node){

            if(node!=null&&results.Count<=10){
                if (node.IsTerminal)
                {
                    results.Add(node.Word);
                }
                foreach (KeyValuePair<char, WebRole1.Node> kvp in node.Edges)
                {
                    fillResults(kvp.Value);
                }          
            }
        }
    }
}