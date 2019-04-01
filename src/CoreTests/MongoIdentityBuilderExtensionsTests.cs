namespace CoreTests
{
	using Microsoft.AspNetCore.Identity.MongoDB;
	using NUnit.Framework;

	[TestFixture]
	public class MongoIdentityBuilderExtensionsTests : AssertionHelper
	{
		private const string FakeConnectionStringWithDatabase = "mongodb://fakehost:27017/database";

		protected class CustomUser : IdentityUser
		{
		}

		protected class CustomRole : IdentityRole
		{
		}

		protected class WrongUser : IdentityUser
		{
		}

		protected class WrongRole : IdentityRole
		{
		}
	}
}