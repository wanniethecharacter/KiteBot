using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KiteBot
{
    public class TextMarkovChain
    {
        private readonly Dictionary<string, Chain> _chains;
        private readonly Chain _head;

        public TextMarkovChain()
        {
            _chains = new Dictionary<string, Chain>();
            _head = new Chain("[]");
            _chains.Add("[]", _head);
        }

        public void feed(string s)
        {
            if (!s.EndsWith(".") && !s.EndsWith("!") && !s.EndsWith("?"))
                s += ".";

            s = s.ToLower();
            s = s.Replace('/',' ').Replace(',',' ').Replace("[]", "");
            s = s.Replace(".", " .").Replace("!", " !").Replace("?", " ?");
            string[] splitValues = s.Split(' ');

            addWord("[]", splitValues[0]);

            for (int i = 0; i < splitValues.Length - 1; i++)
            {
                if (splitValues[i] == "." ||
                    splitValues[i] == "?" ||
                    splitValues[i] == "!")
                    addWord("[]", splitValues[i + 1]);
                else
                    addWord(splitValues[i], splitValues[i + 1]);
            }
        }

        private void addWord(string prev, string next)
        {
            if (_chains.ContainsKey(prev) && _chains.ContainsKey(next))
                _chains[prev].addWord(_chains[next]);
            else if (_chains.ContainsKey(prev))
            {
                _chains.Add(next, new Chain(next));
                _chains[prev].addWord(_chains[next]);
            }
        }

        public void feed(XmlDocument xd)
        {
            XmlNode root = xd.ChildNodes[0];
            foreach (XmlNode n in root.ChildNodes)
            {
                //First add all chains that are not there already
                Chain nc = new Chain(n);
                if(!_chains.ContainsKey(nc.word))
                    _chains.Add(nc.word, nc); 
            }

            foreach (XmlNode n in root.ChildNodes)
            {
                //Now that all words have been added, we can add the probabilities
                XmlNode nextChains = n.ChildNodes[0];
                Chain current = _chains[n.Attributes["Word"].Value.ToString()];
                foreach (XmlNode nc in nextChains)
                {
                    Chain c = _chains[nc.Attributes["Word"].Value.ToString()];
                    current.addWord(c, Convert.ToInt32(nc.Attributes["Count"].Value));
                }
            }
        }

        public void save(string path)
        {
            XmlDocument xd = getDataAsXML();
            xd.Save(path);
        }

        public XmlDocument getDataAsXML()
        {
            XmlDocument xd = new XmlDocument();
            XmlElement root = xd.CreateElement("Chains");
            xd.AppendChild(root);

            foreach (string key in _chains.Keys)
                root.AppendChild(_chains[key].getXMLElement(xd));

            return xd;
        }

        public bool readyToGenerate()
        {
            return _head.getNextChain() != null;
        }

        public async Task<string> generateSentence()
        {
            StringBuilder s = new StringBuilder();
            Chain nextString = _head.getNextChain();
            while (nextString.word != "!" && nextString.word != "?" && nextString.word != ".")
            {
                s.Append(nextString.word);
                s.Append(" ");
                nextString = nextString.getNextChain();
                if (nextString == null)
                    return s.ToString();
            }

            s.Append(nextString.word); //Add punctuation at end

            s[0] = char.ToUpper(s[0]);

            return s.ToString().Replace("  "," ").Replace(" .",".");
        }

        public async Task<string> generateSentence(string input)
        {
            StringBuilder s = new StringBuilder();
            Chain nextString;
            if (_chains.ContainsKey(input))
            {
                nextString = _chains[input];
            }
            else
            {
                nextString = _head.getNextChain();
            }
            while (nextString.word != "!" && nextString.word != "?" && nextString.word != ".")
            {
                s.Append(nextString.word);
                s.Append(" ");
                nextString = nextString.getNextChain();
                if (nextString == null)
                    return s.ToString();
            }

            s.Append(nextString.word); //Add punctuation at end

            s[0] = char.ToUpper(s[0]);

            return s.ToString().Replace("  ", " ").Replace(" .", ".");
        }

        private class Chain
        {
            public string word;

            private Dictionary<string, ChainProbability> chains;
            private int _fullCount;

            public Chain(string w)
            {
                word = w;
                chains = new Dictionary<string, ChainProbability>();
                _fullCount = 0;
            }

            public Chain(XmlNode node)
            {
                word = node.Attributes["Word"].Value;
                _fullCount = 0;  //Full Count is stored, but this will be loaded when adding new words to the chain.  Default to 0 when loading XML
                chains = new Dictionary<string, ChainProbability>();
            }

            public void addWord(Chain chain, int increase = 1)
            {
                _fullCount += increase;
                if (chains.ContainsKey(chain.word))
                    chains[chain.word].count += increase;
                else
                    chains.Add(chain.word, new ChainProbability(chain, increase));
            }

            public Chain getNextChain()
            {
                //Randomly get the next chain
                //Trey:  As this gets bigger, this is a remarkably inefficient way to randomly get the next chain.
                //The reason it is implemented this way is it allows new sentences to be read in much faster
                //since it will not need to recalculate probabilities and only needs to add a counter.  I don't
                //believe the tradeoff is worth it in this case.  I need to do a timed evaluation of this and decide.
                int currentCount = RandomHandler.random.Next(_fullCount);
                foreach (string key in chains.Keys)
                {
                    for (int i = 0; i < chains[key].count; i++)
                    {
                        if (currentCount == 0)
                            return chains[key].chain;
                        currentCount--;
                    }
                }
                return null;
            }

            public XmlElement getXMLElement(XmlDocument xd)
            {
                XmlElement e = xd.CreateElement("Chain");
                e.SetAttribute("Word", word);
                e.SetAttribute("FullCount", _fullCount.ToString());

                XmlElement nextChains = xd.CreateElement("NextChains");
                XmlElement nextChain;

                foreach (string key in chains.Keys)
                {
                    nextChain = xd.CreateElement("Chain");
                    nextChain.SetAttribute("Word", chains[key].chain.word);
                    nextChain.SetAttribute("Count", chains[key].count.ToString());
                    nextChains.AppendChild(nextChain);
                }

                e.AppendChild(nextChains);

                return e;
            }
        }

        private class ChainProbability
        {
            public Chain chain;
            public int count;

            public ChainProbability(Chain chain, int count)
            {
                this.chain = chain;
                this.count = count;
            }
        }
    }
}
