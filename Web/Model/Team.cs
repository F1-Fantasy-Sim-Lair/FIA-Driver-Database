using Web.Types;

namespace Web.Model;

public class Team(Snowflake teamId)
{
    public Snowflake TeamId { get; private set; } = teamId;
    public string Name { get; set; } = string.Empty;
}
