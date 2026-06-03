using Headsoft.Core;
using HeadBet.Core.Domain.Interfaces;

namespace HeadBet.Core.Infrastructure.Localization;

public sealed class NotificationTranslator : INotificationTranslator
{
    private static readonly Dictionary<string, string> _dict = new(StringComparer.OrdinalIgnoreCase)
    {
        [GenericMessages.SUCCESS]              = "Sucesso",
        [GenericMessages.ERROR]                = "Erro",
        [GenericMessages.WARNING]              = "Aviso",
        [GenericMessages.INFORMATION]          = "Informação",
        [GenericMessages.UNAUTHORIZED]         = "Não autorizado",
        [GenericMessages.ITEM_NOT_FOUND]       = "Item não encontrado",
        [GenericMessages.ERROR_ADDING]         = "Erro ao adicionar",
        [GenericMessages.ERROR_UPDATING]       = "Erro ao atualizar",
        [GenericMessages.ERROR_DELETING]       = "Erro ao remover",
        [GenericMessages.ERROR_SAVE]           = "Erro ao salvar",
        [GenericMessages.INVALID_OPERATION]    = "Operação inválida",
        [GenericMessages.INVALID_OBJECT]       = "Objeto inválido",
        [GenericMessages.INVALID_XML]          = "XML inválido",
        [GenericMessages.INVALID_SCHEMA]       = "Schema inválido",
        [GenericMessages.SCHEMA_READING_ERROR] = "Erro de leitura de schema",
        [GenericMessages.FIELD_REQUIRED]       = "Campo obrigatório",
        [GenericMessages.FIELD_INVALID]        = "Campo inválido",
        [GenericMessages.FIELD_UNIQUE]         = "Valor já cadastrado",
        [GenericMessages.FIELD_LENGTH]         = "Tamanho inválido",
        [GenericMessages.FIELD_FORMAT]         = "Formato inválido",
        [GenericMessages.FIELD_RANGE]          = "Valor fora do intervalo",
        [GenericMessages.FIELDS_CONFLICT]      = "Campos em conflito",
        [GenericMessages.ITEM_REQUIRED]        = "Item obrigatório",
        [GenericMessages.ITEM_DUPLICATE]       = "Item duplicado",
    };

    public string Translate(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        return _dict.GetValueOrDefault(key, key);
    }
}
