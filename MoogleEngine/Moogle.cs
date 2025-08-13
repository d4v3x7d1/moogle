using MoogleEngine.Core;
using MoogleEngine.Engines;

namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {
        
        Engine engine = new OldMoogleEngine();

        var result = engine.Search(query).Select(item => new SearchItem(item.title, item.snippet, item.score)).ToArray();
        var suggestions = engine.Suggest(query);

        return new SearchResult(result, suggestions);
    }
}