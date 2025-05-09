using System.Text.Json.Serialization;

namespace Shared.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SecurityQuestionType
    {
        WhatIsYourFavoriteColor,
        WhatIsYourPetName,
        WhatIsYourBirthCity,
        WhatIsYourMotherMaidenName,
        WhatIsYourFavoriteFood,
        WhatIsYourFavoriteSport,
        WhatIsYourFavoriteMovie,
        WhatIsYourFavoriteBook
    }
}