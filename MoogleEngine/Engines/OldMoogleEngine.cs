

using MoogleEngine.Core;

namespace MoogleEngine.Engines{

    public class OldMoogleEngine : Engine
    {
        private static char[] _separators = new char[] { ' ', '=', '|', '\\', '–', '_', '/',
            '.', ',', ';', ':', '(', ')', '{', '}', '\r', '\n', '?', '¿', '¡', '"', '[',
            ']', '@', '+', '-', '#', '&', '$', '%',};

      
        private static DirDocuments DirFile = new DirDocuments(Path.GetDirectoryName(Directory.GetCurrentDirectory()) + @"/Content");
                public static float CosineSimilarity(List<float> vectorDoc, List<float> vectorQuery)
        {
            float dotProduct = 0;
            float normA = 0;
            float normB = 0;
            for (int i = 0; i < vectorQuery.Count; i++)
            {
                dotProduct += vectorQuery[i] * vectorDoc[i];
                normA += (float)Math.Pow(vectorDoc[i], 2);
                normB += (float)Math.Pow(vectorQuery[i], 2);
            }
            return dotProduct / ((float)(Math.Sqrt(normA) * Math.Sqrt(normB)) + float.Epsilon);
        }

        private static WordWeigther[] GetWordWeigthers(string[] terms)
        {

            List<WordWeigther> result = new List<WordWeigther>();

            for (int i = 0; i < terms.Length; i++)
            {
                string word = terms[i];
                char Operator = ' ';
                float weigth = 0f;

                if (terms[i].StartsWith("^"))
                {
                    word = terms[i].Substring(1);
                    Operator = '^';
                }

                if (terms[i].StartsWith("!"))
                {
                    word = terms[i].Substring(1);
                    Operator = '!';
                }
                if (terms[i].StartsWith("*"))
                {
                    var count = terms[i].Split('*').Length - 1;
                    word = terms[i].Remove(0, count);
                    Operator = '*';
                    weigth = count;
                }
                if (terms[i] == "~")
                {
                    result[result.Count - 1].Operator = '~';
                }
                else if ((terms[i].EndsWith("~") || terms[i].StartsWith("~")) && terms[i] != "~")
                {
                    if (terms[i].EndsWith("~"))
                    {
                        word = terms[i].Substring(0, terms[i].Length - 1);
                    }
                    if (terms[i].StartsWith("~"))
                    {
                        word = terms[i].Substring(1);
                    }
                    Operator = ' ';
                }
                if (terms[i] != "~")
                {
                    result.Add(new WordWeigther(word.ToLower(), weigth, Operator));
                }
            }
            return result.ToArray();
        }

        public static float GetScore(Document document, List<float> vectorDoc, List<float> vectorQuery, WordWeigther[] terms)
        {
            int pos = 0;
            float score = 0f;
            foreach (var term in terms)
            {
                if (term.Operator == '!')
                {
                    if (document.Words.Contains(term.Word))
                    {
                        return 0;
                    }
                }
                if (term.Operator == '^')
                {
                    if (!document.Words.Contains(term.Word))
                    {
                        return 0;
                    }
                }

                if (term.Operator == '~')
                {
                    if (document.Words.Contains(term.Word))
                    {
                        if (document.Frecuency.ContainsKey(term.Word) && document.Frecuency.ContainsKey(terms[pos + 1].Word))
                        {
                            int value = MergeDistantce(document.Frecuency[term.Word], document.Frecuency[terms[pos + 1].Word]).Distance;
                            score += 1.0f / (float)value;
                        }
                    }
                }

                if (term.Operator == '*')
                {
                    if (document.Words.Contains(term.Word))
                    {
                        score += term.Weigth * 1.0f / terms.Length;
                    }
                }
                pos++;
            }
            float similar = CosineSimilarity(vectorDoc, vectorQuery);
            return similar + score;
        }
        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // pos.PositionAialize
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }

        public static string FainalySugestion(Document query, List<string> dirwords)
        {
            string rtn = "";
            foreach (var word in query.Words)
            {
                if (!DirFile.StopWords(word.ToLower()))
                {
                    rtn += " " + ChangeSugestion(word, dirwords);
                }
                else
                    rtn += " " + word;
            }
            return rtn;
        }
         public static string ChangeSugestion(string word, List<string> dirwords)
        {
            int temp = 0;
            int max = int.MaxValue;
            string change = "";
            foreach (var str in dirwords)
            {
                temp = LevenshteinDistance(word, str);
                if (temp < max)
                {
                    change = str;
                    max = temp;
                }
            }
            return change;
        }
                public static Dictionary<string, float> DicQuery(List<string> words, List<float> vector)
        {
            Dictionary<string, float> result = new Dictionary<string, float>();
            int count = 0;
            foreach (var item in words)
            {
                if (!DirFile.StopWords(item))
                {
                    result.Add(item, vector[count]);
                    count++;
                }
            }
            return result;
        }

         public static PosAndDist MergeDistantce(List<int> poswA, List<int> poswB)
        {
            int temp = 0;
            int countA = 0;
            int countB = 0;
            int posA = 0;
            int posB = 0;
            int distance = int.MaxValue;
            while (countA < poswA.Count && countB < poswB.Count)
            {
                if (poswA[countA] < poswB[countB])
                {
                    temp = Math.Abs(poswA[countA] - poswB[countB]);
                    if (distance > temp)
                    {
                        posA = poswA[countA];
                        posB = poswB[countB];
                        distance = temp;
                    }
                    countA++;
                }
                else
                {
                    temp = Math.Abs(poswA[countA] - poswB[countB]);
                    if (distance > temp)
                    {
                        posA = poswA[countA];
                        posB = poswB[countB];
                        distance = temp;
                    }
                    countB++;
                }
            }
            return new PosAndDist(posA, posB, distance);
        }

        public static string GetSnippet(Document document, Dictionary<string, float> query)
        {
            string result = "";
            TuplaString word = WordsMax(query);
            string word1 = word.WordI;
            string word2 = word.WordF;

            if (word1 != "" && word2 != "")
            {
                if (document.Frecuency.ContainsKey(word1) && document.Frecuency.ContainsKey(word2))
                {
                    PosAndDist pos = MergeDistantce(document.Frecuency[word1], document.Frecuency[word2]);
                    if (pos.Distance < 30)
                    {
                        if (pos.PositionB + 30 - pos.Distance < document.Words.Count)
                        {
                            result = GetText(document.Words, pos.PositionA, pos.PositionB + 30 - pos.Distance);
                        }
                        else result = GetText(document.Words, pos.PositionA, document.Words.Count);
                    }
                    else if (pos.Distance > 30)
                    {
                        result = GetText(document.Words, pos.PositionA, pos.PositionA + 30);
                    }
                }
                else if (document.Frecuency.ContainsKey(word1))
                {
                    if (document.Frecuency[word1][0] + 30 < document.Words.Count())
                    {
                        result = GetText(document.Words, document.Frecuency[word1][0], document.Frecuency[word1][0] + 30);
                    }
                    else result = GetText(document.Words, document.Frecuency[word1][0], document.Words.Count());
                }
                else if (document.Frecuency.ContainsKey(word2))
                {
                    if (document.Frecuency[word2][0] + 30 < document.Words.Count())
                    {
                        result = GetText(document.Words, document.Frecuency[word2][0], document.Frecuency[word2][0] + 30);
                    }
                    else result = GetText(document.Words, document.Frecuency[word2][0], document.Words.Count());
                }
            }
            else if (word1 != "")
            {
                if (document.Frecuency.ContainsKey(word1))
                {
                    if (document.Frecuency[word1][0] + 30 < document.Words.Count())
                    {
                        result = GetText(document.Words, document.Frecuency[word1][0], document.Frecuency[word1][0] + 30);
                    }
                    else result = GetText(document.Words, document.Frecuency[word1][0], document.Words.Count());
                }

            }
            if (!document.Frecuency.ContainsKey(word1) && !document.Frecuency.ContainsKey(word2))
            {
                if (document.Words.Count > 30)
                {
                    result = GetText(document.Words, 0, 30);
                }
                else result = GetText(document.Words, 0, document.Words.Count);
            }
            return result;
        }

        public static string GetText(List<string> words, int init, int final)
        {
            string text = "";
            for (int i = init; i < final; i++)
            {
                text += " " + words[i];
            }
            return text;
        }
        // busca  la dos palabras con mas importancia del la query
        public static TuplaString WordsMax(Dictionary<string, float> wordr)
        {
            Dictionary<string, float> dic = wordr;
            float max1 = 0;
            float max2 = 0;
            string wordmax1 = "";
            string wordmax2 = "";
            foreach (var value in dic)
            {
                if (value.Value > max1)
                {
                    max1 = value.Value;
                    wordmax1 = value.Key;
                }
            }
            dic[wordmax1] = 0;
            foreach (var value in dic)
            {
                if (value.Value > max2)
                {

                    max2 = value.Value;
                    wordmax2 = value.Key;

                }
            }
            return new TuplaString(wordmax1, wordmax2);
        }

        public override List<(string title, string snippet, float score)> Search(string query)
        {
             float score = 0f;
            var docQuery = new Document(query);
            List<(string title, string snippet, float score)> results = new List<(string title, string snippet, float score)>();
            var vectorQuery = DirFile.BuildVectorQuery(docQuery);
            var terms = GetWordWeigthers(query.ToLower().Split(_separators, StringSplitOptions.RemoveEmptyEntries));

            foreach (var document in DirFile.Documents)
            {
                var vectorDoc = DirFile.BuildVector(document, docQuery.Words);
                score = GetScore(document, vectorDoc, vectorQuery, terms);
                if (score != 0)
                {
                    results.Add((document.Title, GetSnippet(document, DicQuery(docQuery.Words, vectorQuery)), score));
                }
            }
            return results.OrderByDescending(x => x.score).ToList();
        }

        public override string Suggest(string query)
        {
            return FainalySugestion(new Document(query), DirFile.DocRelevance.Keys.ToList());
        }
    }
}


 