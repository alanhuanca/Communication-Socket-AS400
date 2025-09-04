namespace AS400.Core
{

    /// <summary>
    /// Interfaz que define las operaciones del servicio de AS/400.
    /// Esto desacopla la lógica de negocio de la implementación de la infraestructura.
    /// </summary>
    public interface IAs400Service
    {
        Task<List<string>> SendMultipleTransactionsAsync(IEnumerable<string> transactions);
    }
}
