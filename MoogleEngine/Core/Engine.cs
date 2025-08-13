namespace MoogleEngine.Core
{
    /// <summary>
    /// Clase abstracta que define la interfaz de un motor de búsqueda.
    /// Cada motor implementará la lógica de búsqueda y la generación de sugerencias.
    /// 
    /// El método <see cref="Search"/> devuelve una lista de resultados como tuplas
    /// con tres elementos: título, fragmento (snippet) y score de relevancia.
    /// 
    /// El método <see cref="Suggest"/> devuelve una sugerencia basada en la consulta
    /// si los resultados no son suficientes o la consulta parece incorrecta.
    /// </summary>
    public abstract class Engine
    {
        /// <summary>
        /// Realiza la búsqueda en el conjunto de documentos según la consulta dada.
        /// </summary>
        /// <param name="query">La cadena de búsqueda ingresada por el usuario.</param>
        /// <returns>
        /// Lista de tuplas donde cada tupla contiene:
        /// - title: el título del documento
        /// - snippet: un fragmento del documento que coincide con la consulta
        /// - score: relevancia del resultado como un float entre 0 y 1
        /// </returns>
        public abstract List<(string title, string snippet, float score)> Search(string query);

        /// <summary>
        /// Genera una sugerencia basada en la consulta dada.
        /// Útil para corrección de errores de escritura o consultas poco precisas.
        /// </summary>
        /// <param name="query">La cadena de búsqueda ingresada por el usuario.</param>
        /// <returns>Una sugerencia para mejorar la consulta.</returns>
        public abstract string Suggest(string query);
    }
}
