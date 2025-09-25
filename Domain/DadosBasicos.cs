namespace DESAFIO2.Domain;

public record DadosBasicos(
    string Nome,
    System.DateOnly DataNascimento, // usar System.DateOnly evita precisar do using System;
    string Cpf,
    string Email,
    string Telefone
);
