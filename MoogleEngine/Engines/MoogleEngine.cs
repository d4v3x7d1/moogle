using MoogleEngine.Core;


namespace MoogleEngine.Engines;

class MoogleEngine : Engine
{


    public MoogleEngine()
    {
        //TODO: llamar a una funcion que obtenga un doc luego lo tokenize 
        // 
    }

    public override List<(string title, string snippet, float score)> Search(string query)
    {
        throw new NotImplementedException();
    }



    public override string Suggest(string query)
    {
        throw new NotImplementedException();
    }

}


class Indexer
{

    private readonly Tokenizer _tokenizer = new();
    private readonly DocumentRepository _documentRepository;

    private int _totalDocuments => _maxTermFrequencyPerDoc.Count;
    private readonly Dictionary<string, Dictionary<string, int>> invertedIndex = new();

    private readonly Dictionary<string, int> _maxTermFrequencyPerDoc = new();

    public Indexer(string documentsFolder = "./Content")
    {
        _documentRepository = new DocumentRepository(documentsFolder);
        BuildOrUpdateIndex();
    }

    public void BuildOrUpdateIndex()
    {
        var documents = _documentRepository.GetNextDocuments();

        foreach (var (docId, content) in documents)
        {
            var tokens = _tokenizer.GetTokens(content);
            var normalized = _tokenizer.Normalize(tokens);
            var tokenCounts = _tokenizer.CountTokenOccurrences(normalized);

            _maxTermFrequencyPerDoc[docId] = tokenCounts.Max(t => t.Item2);
            foreach (var (token, count) in tokenCounts)
            {
                if (!invertedIndex.ContainsKey(token))
                {
                    invertedIndex[token] = new Dictionary<string, int>();
                }
                invertedIndex[token][docId] = count;
            }

        }
    }

    public float ComputeTFxIDF(string term, string docId)
    {
        if (!invertedIndex.ContainsKey(term) || !invertedIndex[term].ContainsKey(docId))
            return 0;

        int termFrequency = invertedIndex[term][docId];
        int maxFrequencyInDoc = _maxTermFrequencyPerDoc[docId];
        float tf = ComputeTf(termFrequency, maxFrequencyInDoc);

        int docFrequency = invertedIndex[term].Count;
        float idf = ComputeIdf(_totalDocuments, docFrequency);
        return tf * idf;
    }

    /// <summary>
    /// <para>Calcula el TF (Term Frequency):</para>
    /// 
    /// <para>TF(t, d) = 0.5 + 0.5 * (f(t, d) / max(f(t', d)))</para>
    /// 
    /// <para>Donde:</para>
    /// <list type="bullet">
    ///   <item><description>f(t, d): número de veces que el término <c>t</c> aparece en el documento <c>d</c>.</description></item>
    ///   <item><description>max(f(t', d)): frecuencia del término con más ocurrencias en el documento <c>d</c>.</description></item>
    /// </list>
    /// </summary>
    /// <param name="termFrequency">Número de veces que el término aparece en el documento.</param>
    /// <param name="maxFrequencyInDoc">Frecuencia del término con más ocurrencias en el documento.</param>
    /// <returns>Valor de TF normalizado, en el rango [0.5, 1.0].</returns>
    public float ComputeTf(int termFrequency, int maxFrequencyInDoc)
    {
        if (maxFrequencyInDoc == 0) return 0;
        return 0.5f + 0.5f * (termFrequency / (float)maxFrequencyInDoc);
    }

    /// <summary>
    /// <para>Calcula el IDF (Inverse Document Frequency) de un término en el corpus:</para>
    ///
    /// <para>IDF(t) = log((1 + N) / (1 + df_t)) + 1</para>
    ///
    /// <para>Donde:</para>
    /// <list type="bullet">
    ///   <item><description>N: número total de documentos en el corpus.</description></item>
    ///   <item><description>df_t: número de documentos que contienen el término <c>t</c>.</description></item>
    /// </list>
    /// </summary>
    /// <param name="totalDocs">Número total de documentos en el corpus (N).</param>
    /// <param name="docFrequency">Número de documentos que contienen el término t (df_t).</param>
    /// <returns>Valor de IDF siempre positivo. Valores más altos indican que el término es más raro en el corpus.</returns>
    public float ComputeIdf(int totalDocs, int docFrequency)
    {
        return (float)(Math.Log((1.0 + totalDocs) / (1.0 + docFrequency)) + 1.0);
    }



}