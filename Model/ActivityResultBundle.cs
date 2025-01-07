using System.Collections.Generic;

namespace Viento.UnityActivity
{
    public record ActivityResultBundle(
    string ResultKey,
    Dictionary<string, object> Attributes)
    {
        public static ActivityResultBundle CreateEmptyResult(string resultKey) =>
            new(ResultKey: resultKey, Attributes: new());

        public static string RESULT_KEY_EMPTY_RESULT = "EmptyResult";

        public static ActivityResultBundle EMPTY_RESULT =
            new(ResultKey: RESULT_KEY_EMPTY_RESULT, Attributes: new());
    }
}