using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Web.IntegrationTests.Common; 

public static class UriExtensions
{
    public static UriAssertions Should(this Uri? instance)
    {
        return new UriAssertions(instance);
    }
}

public class UriAssertions(Uri? instance) : ReferenceTypeAssertions<Uri, UriAssertions>(instance!)
{
    protected override string Identifier => "uri";

    [CustomAssertion]
    public AndConstraint<UriAssertions> StartWith(string expected, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .ForCondition(Subject.ToString().StartsWith(expected))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:uri} to start with {0}{reason}, but found {1}.", expected, Subject.ToString());
        return new AndConstraint<UriAssertions>(this);
    }

    [CustomAssertion]
    public AndWhichConstraint<UriAssertions, string> HaveQueryParameter(string name, string because = "", params object[] becauseArgs)
    {
        var queryParams = UriHelpers.GetQueryParams(Subject);
        Execute.Assertion
            .ForCondition(queryParams.ContainsKey(name))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected {context:uri} to have query parameter {0}{reason}, but found {1}.", name, queryParams);
        return new AndWhichConstraint<UriAssertions, string>(this, queryParams[name]);
    }
}
