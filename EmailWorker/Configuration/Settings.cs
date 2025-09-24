namespace EmailWorker.Configuration;

public class RabbitSettings
{
    // Para uso em produção com CloudAMQP:
    public string? Uri { get; set; }

    // Para uso local com Docker:
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Queue    { get; set; } = "cadastro.em.analise.email";
}

public class EmailSettings
{
    public string FromAddress { get; set; } = "no-reply@pathbit.com";
    public string FromName    { get; set; } = "Equipe PATHBIT";

    // Quando true, não chama a API do SendGrid (apenas loga) — útil para depurar sem chave
    public bool DryRun { get; set; } = false;
}
