
using System.Text.RegularExpressions;

public class Tokenizer
{

    public List<string> GetTokens(string text)
    {
        var tokens = Regex.Split(text, @"[\W_]+").Where(token => !string.IsNullOrEmpty(token)).ToList();
        return tokens;
    }

    public List<string> Normalize(IEnumerable<string> tokens)
    {
        return tokens.Select(token => token.ToLowerInvariant()).ToList();
    }

    // IEnumerable<string> RemoveStopWords(IEnumerable<string> tokens)
    // {
    //     return tokens.Where(token => !stopwords.Contains(token));
    // }

    public List<(string, int)> CountTokenOccurrences(IEnumerable<string> tokens)
    {
        return tokens.GroupBy(token => token).Select(group => (group.Key, group.Count())).ToList();
    }
    

}