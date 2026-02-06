using CashFlow.Application.UseCases.Login.DoLogin;
using CashFlow.Exception;
using CashFlow.Exception.ExceptionsBase;
using CommonTestUtilities.Cryptography;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Token;
using FluentAssertions;

namespace UseCases.Test.Login.DoLogin;

public class DoLoginUseCaseTest
{
    [Fact]
    public async Task Success()
    {
        var user = UserBuilder.Build();

        var request = RequestLoginJsonBuilder.Build();
        // Como user.Email é gerado aleatoriamente e request.Email também é gerado aleatoriamente, precisamos garantir que o email do request seja o mesmo do usuário para simular um cenário de login bem-sucedido.
        request.Email = user.Email;

        var useCase = CreateUseCase(user, request.Password);

        var result = await useCase.Execute(request);

        result.Should().NotBeNull();
        result.Name.Should().Be(user.Name);
        result.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Error_User_Not_Found()
    {
        var user = UserBuilder.Build();
        var request = RequestLoginJsonBuilder.Build();

        // The email in the request is different from the user's email, so it simulates a user not found scenario.
        // example: user.Email = "Jonas@hotmail.com" and request.Email = "Bento@gmail.com"
        var useCase = CreateUseCase(user, request.Password);

        var act = async () => await useCase.Execute(request);

        var result = await act.Should().ThrowAsync<InvalidLoginException>();

        result.Where(ex => ex.GetErrors().Count == 1 && ex.GetErrors().Contains(ResourceErrorMessages.EMAIL_OR_PASSWORD_INVALID));
    }

    [Fact]
    public async Task Error_Password_Not_Match()
    {
        var user = UserBuilder.Build();

        var request = RequestLoginJsonBuilder.Build();
        request.Email = user.Email;

        // Para testar o cenário de senha incorreta, nao passamos a senha do request para o CreateUseCase. No proprio PasswordEncrypterBuilder.Verify(password) ja vai retornar false pois tem uma validacao de password nulo.
        var useCase = CreateUseCase(user);

        var act = async () => await useCase.Execute(request);

        var result = await act.Should().ThrowAsync<InvalidLoginException>();

        result.Where(ex => ex.GetErrors().Count == 1 && ex.GetErrors().Contains(ResourceErrorMessages.EMAIL_OR_PASSWORD_INVALID));
    }

    private DoLoginUseCase CreateUseCase(CashFlow.Domain.Entities.User user, string? password = null)
    {
        var passwordEncripter = new PasswordEncrypterBuilder().
(password).Build();

        var tokenGenerator = JwtTokenGeneratorBuilder.Build();
        var readRepository = new UserReadOnlyRepositoryBuilder().GetUserByEmail(user).Build();

        return new DoLoginUseCase(readRepository, passwordEncripter, tokenGenerator);
    }
}