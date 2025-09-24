namespace EmailWorker.Configuration;

public class RabbitSettings
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string Queue    { get; set; } = "cadastro.em.analise.email";
}

public class EmailSettings
{
    public string FromAddress { get; set; } = "no-reply@pathbit.com";
    public string FromName    { get; set; } = "Equipe PATHBIT";
}
