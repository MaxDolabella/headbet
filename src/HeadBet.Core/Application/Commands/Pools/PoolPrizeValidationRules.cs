using FluentValidation;
using Headsoft.Core;
using Headsoft.Core.Extensions;
using HeadBet.Core.Domain.Enums;
using HeadBet.Core.Models;

namespace HeadBet.Core.Application.Commands;

/// <summary>
/// Contrato comum a comandos que carregam premiação de bolão (Create/Update).
/// </summary>
internal interface IPoolPrizeForm
{
    bool IsPaid { get; }
    PrizeMode PrizeMode { get; }
    List<PrizeItemViewModel> Prizes { get; }
}

internal static class PoolPrizeValidationRules
{
    public const decimal PERCENTAGE_TOTAL = 100m;
    public const decimal PERCENTAGE_TOLERANCE = 0.01m;
    public const int MAX_PRIZES = 10;
    private const string FIELD_PRIZES = "Prizes";

    public static void ApplyTo<T>(AbstractValidator<T> validator) where T : IPoolPrizeForm
    {
        validator.When(x => x.IsPaid, () =>
        {
            validator.RuleFor(x => x.Prizes)
                .NotEmpty()
                    .WithNotification(GenericMessages.FIELD_REQUIRED, "Informe ao menos uma posição de premiação.", FIELD_PRIZES)
                .Must(p => p.Count <= MAX_PRIZES)
                    .WithNotification(GenericMessages.FIELD_LENGTH, $"Máximo de {MAX_PRIZES} posições.", FIELD_PRIZES)
                .Must(HasSequentialPositions)
                    .WithNotification(GenericMessages.FIELD_FORMAT, "Posições devem ser sequenciais 1..N.", FIELD_PRIZES);

            validator.When(x => x.PrizeMode == PrizeMode.Percentage, () =>
            {
                validator.RuleFor(x => x.Prizes)
                    .Must(p => p.All(x => x.Percentage is > 0 && x.FixedAmount == null))
                        .WithNotification(GenericMessages.FIELD_FORMAT, "Em modo percentual, cada posição exige percentual maior que zero e sem valor fixo.", FIELD_PRIZES)
                    .Must(p => Math.Abs(p.Sum(x => x.Percentage ?? 0) - PERCENTAGE_TOTAL) <= PERCENTAGE_TOLERANCE)
                        .WithNotification(GenericMessages.FIELD_FORMAT, "Soma das porcentagens deve ser 100%.", FIELD_PRIZES);
            });

            validator.When(x => x.PrizeMode == PrizeMode.Fixed, () =>
            {
                validator.RuleFor(x => x.Prizes)
                    .Must(p => p.All(x => x.FixedAmount is > 0 && x.Percentage == null))
                        .WithNotification(GenericMessages.FIELD_FORMAT, "Em modo valor fixo, cada posição exige valor maior que zero e sem percentual.", FIELD_PRIZES);
            });
        });
    }

    private static bool HasSequentialPositions(IList<PrizeItemViewModel> prizes)
    {
        var sorted = prizes.OrderBy(p => p.Position).ToList();
        for (var i = 0; i < sorted.Count; i++)
        {
            if (sorted[i].Position != i + 1)
                return false;
        }
        return true;
    }
}
