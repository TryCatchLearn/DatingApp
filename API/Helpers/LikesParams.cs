using System;

namespace API.Helpers;

public class LikesParams : PagingParams
{
    public string MemberId { get; set; } = "";
    public string Predicate { get; set; } = "liked";
}
